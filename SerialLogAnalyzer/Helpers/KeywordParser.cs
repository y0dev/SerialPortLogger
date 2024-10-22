﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SerialLogAnalyzer.Helpers
{
	public class ParseData
	{
		public string Type { get; set; } // Type of data: "Integer", "Double", "Array"
		public string Title { get; set; } // Type of data: "Integer", "Double", "Array"
		public List<int> IntArray { get; set; } // Nullable integer array (List<int> is already nullable)
		public List<double> DoubleArray { get; set; } // Nullable double array (List<double> is already nullable)
		public int? SingleInt { get; set; } // Single nullable integer value
		public double? SingleDouble { get; set; } // Single nullable double value
	}



	public class KeywordParser
	{
		public string FilePath { get; set; }
		public List<string> Keywords { get; set; }

		public KeywordParser(string filePath)
		{
			FilePath = filePath;
		}

		public Dictionary<string, List<ParseData>> ParseFile(List<string> keywords)
		{
			var keywordData = new Dictionary<string, List<ParseData>>();

			if (!File.Exists(FilePath))
			{
				throw new FileNotFoundException($"The file {FilePath} was not found.");
			}

			var keywordRegexDict = KeywordRegexList.GetKeywordRegexDictionary();
			var currentHeaderKeyword = string.Empty;

			// Read the file line by line
			foreach (var line in File.ReadLines(FilePath))
			{
				if (string.IsNullOrWhiteSpace(line)) continue;

				if (string.IsNullOrEmpty(currentHeaderKeyword))
				{
					foreach (var headerKeyword in keywordRegexDict.Keys)
					{
						if (keywords.Contains(headerKeyword) && keywordRegexDict[headerKeyword].Regex.IsMatch(line))
						{
							currentHeaderKeyword = headerKeyword;
							break;
						}
					}
					if (!string.IsNullOrEmpty(currentHeaderKeyword)) continue;
				}

				if (!string.IsNullOrEmpty(currentHeaderKeyword))
				{
					var headerKeywordRegex = keywordRegexDict[currentHeaderKeyword];

					foreach (var subKeyword in headerKeywordRegex.SubKeywordsRegex)
					{
						if (subKeyword.Regex.IsMatch(line))
						{
							List<ParseData> dataList;
							if (!keywordData.TryGetValue(subKeyword.Keyword, out dataList))
							{
								dataList = new List<ParseData>();
								keywordData[subKeyword.Keyword] = dataList;
							}

							var parseData = new ParseData { Title = subKeyword.Keyword, Type = subKeyword.DataType };

							if (subKeyword.IsArray)
							{
								if (subKeyword.DataType == "Integer")
								{
									var match = subKeyword.Regex.Match(line);
									if (match.Success)
									{
										var numbers = match.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
											.Select(n => int.Parse(n.Trim()))
											.ToList();
										parseData.IntArray = numbers;
									}
								}
								else if (subKeyword.DataType == "Double")
								{
									var match = subKeyword.Regex.Match(line);
									if (match.Success)
									{
										var numbers = match.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
											.Select(n => double.Parse(n.Trim()))
											.ToList();
										parseData.DoubleArray = numbers;
									}
								}
							}
							else
							{
								if (subKeyword.DataType == "Integer")
								{
									var match = Regex.Match(line, @"-?\d+");
									if (match.Success)
									{
										parseData.SingleInt = int.Parse(match.Value);
									}
								}
								else if (subKeyword.DataType == "Double")
								{
									var match = Regex.Match(line, @"-?\d+(\.\d+)?");
									if (match.Success)
									{
										parseData.SingleDouble = double.Parse(match.Value);
									}
								}
							}

							dataList.Add(parseData);
						}
					}
				}
			}
			return keywordData;
		} // End of ParseFile()

		public void WriteOutput(string outputFilePath, Dictionary<string, List<ParseData>> keywordData)
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
				case ".h":
					WriteHeaderFile(outputFilePath, keywordData);
					break;
				default:
					throw new NotSupportedException($"The output file format {extension} is not supported.");
			}
		} // End of WriteOutput()

		private void WriteCsv(string outputFilePath, Dictionary<string, List<ParseData>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("Keyword,Type,Value");

				foreach (var entry in keywordData)
				{
					foreach (var data in entry.Value)
					{
						if (data.IntArray != null)
						{
							writer.WriteLine($"{data.Title},Integer Array,{string.Join(",", data.IntArray)}");
						}
						else if (data.DoubleArray != null)
						{
							writer.WriteLine($"{data.Title},Double Array,{string.Join(",", data.DoubleArray)}");
						}
						else if (data.SingleInt.HasValue)
						{
							writer.WriteLine($"{data.Title},Integer,{data.SingleInt.Value}");
						}
						else if (data.SingleDouble.HasValue)
						{
							writer.WriteLine($"{data.Title},Double,{data.SingleDouble.Value}");
						}
					}
				}
			}
		} // End of WriteCsv()

		private void WriteTxt(string outputFilePath, Dictionary<string, List<ParseData>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				foreach (var entry in keywordData)
				{
					writer.WriteLine($"Keyword: {entry.Key}");

					foreach (var data in entry.Value)
					{
						if (data.IntArray != null)
						{
							writer.WriteLine($" - Values (Integer Array): {string.Join(", ", data.IntArray)}");
						}
						else if (data.DoubleArray != null)
						{
							writer.WriteLine($" - Values (Double Array): {string.Join(", ", data.DoubleArray)}");
						}
						else if (data.SingleInt.HasValue)
						{
							writer.WriteLine($" - Value (Integer): {data.SingleInt.Value}");
						}
						else if (data.SingleDouble.HasValue)
						{
							writer.WriteLine($" - Value (Double): {data.SingleDouble.Value}");
						}
					}
					writer.WriteLine(); // Blank line between keywords
				}
			}
		} // End of WriteTxt()

		private void WriteHeaderFile(string outputFilePath, Dictionary<string, List<ParseData>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("// Header File for Parsed Data");

				foreach (var entry in keywordData)
				{
					foreach (var data in entry.Value)
					{
						if (data.IntArray != null)
						{
							writer.WriteLine($"const int {data.Title}[] = {{ {string.Join(", ", data.IntArray)} }};");
						}
						else if (data.DoubleArray != null)
						{
							writer.WriteLine($"const double {data.Title}[] = {{ {string.Join(", ", data.DoubleArray)} }};");
						}
						else if (data.SingleInt.HasValue)
						{
							writer.WriteLine($"const int {data.Title} = {data.SingleInt.Value};");
						}
						else if (data.SingleDouble.HasValue)
						{
							writer.WriteLine($"const double {data.Title} = {data.SingleDouble.Value};");
						}
					}
				}
			}
		} // End of WriteHeaderFile()
	} // End of class KeywordParser

	public class KeywordRegex
	{
		public string Keyword { get; set; }
		public Regex Regex { get; set; }
		public string DataType { get; set; } // Type: "Integer", "Double", "Coordinate", etc.
		public bool IsArray { get; set; } // Indicates if it's an array of data
	} // End of class KeywordRegex

	public class HeaderKeywordRegex : KeywordRegex
	{
		public List<KeywordRegex> SubKeywordsRegex { get; set; } = new List<KeywordRegex>();
	} // End of class HeaderKeywordRegex

	public static class KeywordRegexList
	{
		// Converts the list into a dictionary for fast lookup
		public static Dictionary<string, HeaderKeywordRegex> GetKeywordRegexDictionary()
		{
			// .NET 3.5 does not support StringComparer.OrdinalIgnoreCase in the dictionary constructor
			// We use StringComparer.InvariantCultureIgnoreCase instead
			var keywordRegexDict = new Dictionary<string, HeaderKeywordRegex>(StringComparer.InvariantCultureIgnoreCase)
		{
            // Header keyword: "Dribble Number Test" with subkeywords
            { "Dribble Number Test", new HeaderKeywordRegex
				{
					Keyword = "Dribble Number Test",
					Regex = new Regex(@"^Dribble Number Test$", RegexOptions.Multiline),
					SubKeywordsRegex = new List<KeywordRegex>
					{
						new KeywordRegex
						{
							Keyword = "Dribble Numbers Game",
							Regex = new Regex(@"(\d+(?:,\s*\d+)*)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = true
						},
						new KeywordRegex
						{
							Keyword = "Total Possessions",
							Regex = new Regex(@"Total Possessions:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Dribble Average Per Possession",
							Regex = new Regex(@"Dribble Average Per Possession:\s*([\d.]+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Most Dribbles in a Single Possession",
							Regex = new Regex(@"Most Dribbles in a Single Possession:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Least Dribbles in a Single Possession",
							Regex = new Regex(@"Least Dribbles in a Single Possession:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Max Dribbles Occurred",
							Regex = new Regex(@"Number of times the Max Dribbles Occurred:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Least Dribbles Occurred",
							Regex = new Regex(@"Number of times the Least Dribbles Occurred:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						}
					}
				}
			},

			// Matches the "Shot Attempts Test" heading
            { "Shot Attempts Test", new HeaderKeywordRegex
				{
					Keyword = "Shot Attempts Test",
					Regex = new Regex(@"^Shot Attempts Test$", RegexOptions.Multiline),
					SubKeywordsRegex = new List<KeywordRegex>
					{
						new KeywordRegex
						{
							Keyword = "Shot Coordinates",
							Regex = new Regex(@"\((-?\d+\.\d+),\s*(-?\d+\.\d+)\)", RegexOptions.Multiline),
							DataType = "Coordinate",
							IsArray = true
						},
						new KeywordRegex
						{
							Keyword = "Total Attempts",
							Regex = new Regex(@"Total Attempts:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Average Distance from Basket",
							Regex = new Regex(@"Average Distance from Basket:\s*([\d.]+)", RegexOptions.Multiline),
							DataType = "Double",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Max Distance from Basket",
							Regex = new Regex(@"Max Distance from Basket:\s*([\d.]+)", RegexOptions.Multiline),
							DataType = "Double",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Min Distance from Basket",
							Regex = new Regex(@"Min Distance from Basket:\s*([\d.]+)", RegexOptions.Multiline),
							DataType = "Double",
							IsArray = false
						}
					}
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
