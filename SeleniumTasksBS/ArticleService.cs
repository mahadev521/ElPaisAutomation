using Microsoft.VisualStudio.TestPlatform.TestHost;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium;
using SeleniumExtras.WaitHelpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTasksBS
{
	public class ArticleService
	{
		private readonly IWebDriver? driver;
		private readonly WebDriverWait? wait;
		private const string DownloadDirectory = "DownloadedImages";

		public ArticleService(IWebDriver? driver, WebDriverWait? wait)
		{
			this.driver = driver;
			this.wait = wait;
		}

		public void AcceptCookies()
		{
			try
			{
				var cookieButton = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Configs.CookieButtonId)));
				cookieButton.Click();
			}
			catch (WebDriverTimeoutException)
			{
				Console.WriteLine("Cookie consent button did not appear.");
			}
			catch (NoSuchElementException)
			{
				Console.WriteLine("Cookie consent button not found in the DOM.");
			}
		}

		public void CheckPageLanguage()
		{
			var htmlLang = driver.FindElement(By.TagName(Configs.HtmlTag)).GetAttribute(Configs.LanguageAttribute);
			if (htmlLang == Configs.SpanishLanguageCode || htmlLang.StartsWith($"{Configs.SpanishLanguageCode}-"))
			{
				Console.WriteLine("The content on the page is in Spanish.");
			}
			else
			{
				Console.WriteLine("The content on the page is not in Spanish.");
			}
		}

		public List<Article> FetchArticles()
		{
			var articles = new List<Article>();

			try
			{
				NavigateToSection(Configs.SectionOpinion);
				NavigateToSection(Configs.SectionEditorials);

				var articlesList = driver.FindElements(By.TagName(Configs.ArticleTag)).Take(5).ToList();
				Directory.CreateDirectory(DownloadDirectory);

				foreach (var articleElement in articlesList)
				{
					var article = ProcessArticle(articleElement);
					if (article != null)
					{
						articles.Add(article);
					}
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine($"An error occurred: {ex.Message}");
			}

			return articles;
		}

		private void NavigateToSection(string sectionName)
		{
			var sectionLink = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath($"//a[text()='{sectionName}']")));
			sectionLink.Click();
		}

		private Article? ProcessArticle(IWebElement articleElement)
		{
			var success = false;
			var retries = 0;

			while (!success && retries < Configs.MaxRetries)
			{
				try
				{
					var articleLink = articleElement.FindElement(By.TagName("a"));

					articleLink.Click();

					var header = wait.Until(d => d.FindElement(By.TagName(Configs.HeaderTag))).Text;
					var contentElements = wait.Until(d => d.FindElements(By.XPath(Configs.ContentXpath)));

					var content = string.Join(Environment.NewLine, contentElements.Select(e => e.Text));
					var coverImageElement = wait.Until(d => d.FindElement(By.XPath(Configs.CoverImageXpath)));
					var imageUrl = coverImageElement.GetAttribute("src");

					var imageFileName = $"{header}{Guid.NewGuid()}{Configs.ImageExtension}";
					var imagePath = Path.Combine(DownloadDirectory, imageFileName);

					using (var httpClient = new HttpClient())
					{
						var imageBytes = httpClient.GetByteArrayAsync(imageUrl).Result;
						File.WriteAllBytesAsync(imagePath, imageBytes);
					}

					var article = new Article
					{
						Header = header,
						Content = content,
						CoverImagePath = imagePath
					};

					driver.Navigate().Back();
					wait.Until(ExpectedConditions.ElementIsVisible(By.TagName(Configs.ArticleTag)));

					success = true;
					return article;
				}
				catch (StaleElementReferenceException)
				{
					retries++;
					Console.WriteLine($"Retrying due to Stale Element Reference (Attempt {retries})...");
					Thread.Sleep(Configs.RetryDelayMilliseconds);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Failed to process article: {ex.Message}");
					break;
				}
			}

			return default;
		}
	}
}
