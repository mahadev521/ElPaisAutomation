using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTasksBS
{
	public abstract class TranslationService
	{
		private const string ApiUrl = "https://rapid-translate-multi-traduction.p.rapidapi.com/t";
		private const string ApiKey = "5111d50f79mshaa0bc0c92b66267p174d79jsn651a690c8290";
		private const string ApiHost = "rapid-translate-multi-traduction.p.rapidapi.com";
		private const string FromLanguage = "es";
		private const string ToLanguage = "en";

		public static List<string> TranslateHeaders(List<Article> articles)
		{
			var translatedHeaders = new List<string>();

			foreach (var article in articles)
			{
				var client = new RestClient(ApiUrl);
				var request = new RestRequest
				{
					Method = Method.Post
				};
				request.AddHeader("x-rapidapi-key", ApiKey);
				request.AddHeader("x-rapidapi-host", ApiHost);
				request.AddHeader("Content-Type", "application/json");

				var jsonBody = new
				{
					from = FromLanguage,
					to = ToLanguage,
					q = article.Header
				};

				request.AddParameter("application/json", jsonBody, ParameterType.RequestBody);
				var response = client.Execute(request);
				var translatedContent = response.Content?.Substring(2, response.Content.Length - 4);
				if (translatedContent != null) translatedHeaders.Add(translatedContent);
			}

			return translatedHeaders;
		}
	}
}
