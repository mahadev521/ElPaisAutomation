using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeleniumTasksBS
{
	public abstract class WordFrequencyService
	{
		public static Dictionary<string, int> GetWordsOccuredAtleastTwice(List<string> headers)
		{
			var frequencies = new Dictionary<string, int>();

			foreach (var word in headers.SelectMany(header => header.Split()))
			{
				if (frequencies.TryGetValue(word, out var count))
				{
					frequencies[word] = count + 1;
				}
				else
				{
					frequencies[word] = 1;
				}
			}

			return frequencies.Where(pair => pair.Value >= 2)
				.ToDictionary(pair => pair.Key, pair => pair.Value);
		}
	}
}
