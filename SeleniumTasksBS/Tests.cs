using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;

namespace SeleniumTasksBS;
public class Tests
{
	private const string Url = "https://elpais.com";
	private const int WaitTimeInSeconds = 10;

	private static IWebDriver? _driver;
	private static WebDriverWait? _wait;
	private static ArticleService? _articleService;

	[SetUp]
	public void Setup()
	{
		_driver = new ChromeDriver();
		_driver.Manage().Window.Maximize();
		_driver.Navigate().GoToUrl(Url);
		_wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(WaitTimeInSeconds));
		_articleService = new ArticleService(_driver, _wait);
	}

	[Test]
	public void Test()
	{
		_articleService.AcceptCookies();
		_articleService.CheckPageLanguage();
		var articles = _articleService.FetchArticles();

		Console.WriteLine("------------------------------------------------");
		foreach (var article in articles)
		{
			Console.WriteLine(article.Header);
			Console.WriteLine(article.Content);
			Console.WriteLine("------------------------------------------------");
		}

		var translatedHeaders = TranslationService.TranslateHeaders(articles);

		foreach (var header in translatedHeaders)
		{
			Console.WriteLine(header);
		}

		Console.WriteLine("------------------------------------------------");

		var wordsOccuredAtleastTwice = WordFrequencyService.GetWordsOccuredAtleastTwice(translatedHeaders);

		foreach (var pair in wordsOccuredAtleastTwice)
		{
			Console.WriteLine($"{pair.Key} : {pair.Value}");
		}
	}

	[TearDown]
	public void Teardown()
	{
		_driver?.Quit();
		_driver?.Dispose();
	}
}