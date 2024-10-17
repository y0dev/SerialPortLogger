using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SerialLogAnalyzer.Helpers
{
	public class KeywordParser
	{
		public string FilePath { get; set; }
		public List<string> Keywords { get; set; }

		public KeywordParser(string filePath, List<string> keywords)
		{
			FilePath = filePath;
			Keywords = keywords ?? new List<string>();
		}

		public Dictionary<string, List<string>> ParseFile()
		{
			var keywordData = new Dictionary<string, List<string>>();

			if (!File.Exists(FilePath))
			{
				throw new FileNotFoundException($"The file {FilePath} was not found.");
			}

			// Get the dictionary of keyword regexes
			var keywordRegexDict = KeywordRegexList.GetKeywordRegexDictionary();

			// Read the file line by line
			foreach (var line in File.ReadLines(FilePath))
			{
				foreach (var keyword in Keywords)
				{
					if (keywordRegexDict.TryGetValue(keyword, out var keywordRegex))
					{
						// Use the regex to match the keyword in the line
						if (keywordRegex.Regex.IsMatch(line))
						{
							// If the keyword is found, add the line to the keyword data
							if (!keywordData.ContainsKey(keyword))
							{
								keywordData[keyword] = new List<string>();
							}

							// Add the matching line to the result list for the keyword
							keywordData[keyword].Add(line);
						}
					}
					else
					{
						throw new KeyNotFoundException($"Keyword '{keyword}' not found in the keyword regex dictionary.");
					}
				}
			}


			return keywordData;
		} // End of ParseFile()

		public void WriteOutput(string outputFilePath, Dictionary<string, List<string>> keywordData)
		{
			string extension = Path.GetExtension(outputFilePath).ToLowerInvariant();

			switch (extension)
			{
				case ".csv":
					WriteCsv(outputFilePath, keywordData);
					break;
				case ".txt":
					WriteTxt(outputFilePath, keywordData);
					break;
				default:
					throw new NotSupportedException($"The output file format {extension} is not supported.");
			}
		} // End of WriteOutput()

		private void WriteCsv(string outputFilePath, Dictionary<string, List<string>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				// Write header
				writer.WriteLine("Keyword,Line");

				foreach (var entry in keywordData)
				{
					foreach (var line in entry.Value)
					{
						// Format: Keyword,Line
						writer.WriteLine($"{entry.Key},{line}");
					}
				}
			}
		} // End of WriteCsv()

		private void WriteTxt(string outputFilePath, Dictionary<string, List<string>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				foreach (var entry in keywordData)
				{
					writer.WriteLine($"Keyword: {entry.Key}");
					foreach (var line in entry.Value)
					{
						writer.WriteLine($" - {line}");
					}
					writer.WriteLine(); // Add a blank line between keywords
				}
			}
		} // End of WriteTxt()
	} // End of class KeywordParser

	public class KeywordRegex
	{
		public string Keyword { get; set; }
		public Regex Regex { get; set; }
	} // End of class KeywordRegex

	public static class KeywordRegexList
	{
		// Converts the list into a dictionary for fast lookup
		public static Dictionary<string, KeywordRegex> GetKeywordRegexDictionary()
		{
			// .NET 3.5 does not support StringComparer.OrdinalIgnoreCase in the dictionary constructor
			// We use StringComparer.InvariantCultureIgnoreCase instead
			var keywordRegexDict = new Dictionary<string, KeywordRegex>(StringComparer.InvariantCultureIgnoreCase)
		{
            // Phone number in format (123) 456-7890
            { "Phone Numbers", new KeywordRegex
				{
					Keyword = "Phone Numbers",
					Regex = new Regex(@"\(\d{3}\) \d{3}-\d{4}", RegexOptions.Compiled)
				}
			},

            // Email addresses
            { "Email Addresses", new KeywordRegex
				{
					Keyword = "Email Addresses",
					Regex = new Regex(@"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}", RegexOptions.Compiled)
				}
			},

            // Dates in format MM/DD/YYYY or DD/MM/YYYY
            { "Dates", new KeywordRegex
				{
					Keyword = "Dates",
					Regex = new Regex(@"\b(0[1-9]|1[0-2])/[0-3][0-9]/(19|20)\d{2}\b|\b[0-3][0-9]/(0[1-9]|1[0-2])/(19|20)\d{2}\b", RegexOptions.Compiled)
				}
			},

            // IP addresses
            { "IP Addresses", new KeywordRegex
				{
					Keyword = "IP Addresses",
					Regex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b", RegexOptions.Compiled)
				}
			},

            // Credit card numbers (basic pattern for Visa, MasterCard, etc.)
            { "Credit Card Numbers", new KeywordRegex
				{
					Keyword = "Credit Card Numbers",
					Regex = new Regex(@"\b(?:\d[ -]*?){13,16}\b", RegexOptions.Compiled)
				}
			},

            // Social security numbers in format XXX-XX-XXXX
            { "Social Security Numbers", new KeywordRegex
				{
					Keyword = "Social Security Numbers",
					Regex = new Regex(@"\b\d{3}-\d{2}-\d{4}\b", RegexOptions.Compiled)
				}
			},

            // Specific word "Error" (case-insensitive)
            { "Error", new KeywordRegex
				{
					Keyword = "Error",
					Regex = new Regex(@"\b(error)\b", RegexOptions.IgnoreCase | RegexOptions.Compiled)
				}
			},

            // URLs
            { "URLs", new KeywordRegex
				{
					Keyword = "URLs",
					Regex = new Regex(@"https?://[^\s/$.?#].[^\s]*", RegexOptions.Compiled)
				}
			}
		};

			return keywordRegexDict;
		}

		// Retrieve a KeywordRegex by keyword
		public static KeywordRegex GetKeywordRegex(string keyword)
		{
			var keywordRegexDict = GetKeywordRegexDictionary();

			if (keywordRegexDict.TryGetValue(keyword, out var keywordRegex))
			{
				return keywordRegex;
			}
			else
			{
				throw new KeyNotFoundException($"Keyword '{keyword}' not found in the dictionary.");
			}
		}
	} // End of class KeywordRegexList
}
