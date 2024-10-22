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

		public KeywordParser(string filePath)
		{
			FilePath = filePath;
		}

		public Dictionary<string, List<object>> ParseFile(List<string> keywords)
		{
			var keywordData = new Dictionary<string, List<object>>();

			if (!File.Exists(FilePath))
			{
				throw new FileNotFoundException($"The file {FilePath} was not found.");
			}

			// Get the dictionary of keyword regexes
			var keywordRegexDict = KeywordRegexList.GetKeywordRegexDictionary();
			var currentHeaderKeyword = string.Empty;

			// Read the file line by line
			foreach (var line in File.ReadLines(FilePath))
			{
				if (string.IsNullOrWhiteSpace(line)) continue; // Skip blank lines

				if (string.IsNullOrEmpty(currentHeaderKeyword))
				{
					// Check for header keywords
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

				// Check for subkeywords based on the current header keyword
				if (!string.IsNullOrEmpty(currentHeaderKeyword))
				{
					var headerKeywordRegex = keywordRegexDict[currentHeaderKeyword];

					foreach (var subKeyword in headerKeywordRegex.SubKeywordsRegex)
					{
						if (subKeyword.Regex.IsMatch(line))
						{
							List<object> dataList;
							if (!keywordData.TryGetValue(subKeyword.Keyword, out dataList))
							{
								dataList = new List<object>();
								keywordData[subKeyword.Keyword] = dataList;
							}

							// Handle different data types
							if (subKeyword.IsArray)
							{
								if (subKeyword.DataType == "Integer")
								{
									// Extract integers from the line
									var match = subKeyword.Regex.Match(line);
									if (match.Success)
									{
										var numbers = match.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
											.Select(n => int.Parse(n.Trim()))
											.ToList();
										dataList.Add(numbers); // Store as an array
									}
								}
								else if (subKeyword.DataType == "Coordinate")
								{
									// Extract coordinates
									var matches = Regex.Matches(line, @"\((-?\d+\.\d+),\s*(-?\d+\.\d+)\)");
									var coordinates = new List<Tuple<double, double>>();
									foreach (Match match in matches)
									{
										coordinates.Add(new Tuple<double, double>(
											double.Parse(match.Groups[1].Value),
											double.Parse(match.Groups[2].Value)));
									}
									dataList.Add(coordinates); // Store as an array
								}
							}
							else
							{
								// Handle single values
								if (subKeyword.DataType == "Double")
								{
									var match = Regex.Match(line, @"-?\d+(\.\d+)?");
									if (match.Success)
									{
										double value = double.Parse(match.Value);
										dataList.Add(value);
									}
								}
								else if (subKeyword.DataType == "Integer")
								{
									var match = Regex.Match(line, @"-?\d+");
									if (match.Success)
									{
										int value = int.Parse(match.Value);
										dataList.Add(value);
									}
								}
							}
						}
					}
				}
			}
			return keywordData;
		} // End of ParseFile()

		public void WriteOutput(string outputFilePath, Dictionary<string, List<object>> keywordData)
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

		private void WriteCsv(string outputFilePath, Dictionary<string, List<object>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				// Write header
				writer.WriteLine("Keyword,Value Type,Value");

				foreach (var entry in keywordData)
				{
					foreach (var item in entry.Value)
					{
						if (item is List<int> intList)
						{
							// Store arrays in a single column
							writer.WriteLine($"{entry.Key},Integer,{string.Join(",", intList)}");
						}
						else if (item is List<Tuple<double, double>> coordinates)
						{
							var coordString = string.Join(" | ", coordinates.Select(c => $"({c.Item1}, {c.Item2})"));
							writer.WriteLine($"{entry.Key},Coordinate,{coordString}");
						}
						else if (item is double)
						{
							// Store single double values
							writer.WriteLine($"{entry.Key},Double,{item}");
						}
						else if (item is int)
						{
							// Store single integer values
							writer.WriteLine($"{entry.Key},Integer,{item}");
						}
					}
				}
			}
		} // End of WriteCsv()

		private void WriteTxt(string outputFilePath, Dictionary<string, List<object>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				foreach (var entry in keywordData)
				{
					writer.WriteLine($"Keyword: {entry.Key}");
					foreach (var item in entry.Value)
					{
						if (item is List<int> intList)
						{
							// Store arrays in a similar format
							writer.WriteLine($" - Values (Integer): {string.Join(", ", intList)}");
						}
						else if (item is List<Tuple<double, double>> coordinates)
						{
							var coordString = string.Join(" | ", coordinates.Select(c => $"({c.Item1}, {c.Item2})"));
							writer.WriteLine($" - Values (Coordinates): {coordString}");
						}
						else
						{
							// Handle single values and format with a new line every 100 characters
							var formattedValue = item.ToString();
							for (int i = 0; i < formattedValue.Length; i += 100)
							{
								writer.WriteLine($" - Value: {formattedValue.Substring(i, Math.Min(100, formattedValue.Length - i))}");
							}
						}
					}
					writer.WriteLine(); // Add a blank line between keywords
				}
			}
		} // End of WriteTxt()

		private void WriteHeaderFile(string outputFilePath, Dictionary<string, List<object>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				writer.WriteLine("// Header File for Parsed Data");
				writer.WriteLine("// This file contains keyword definitions and expected data types.");

				foreach (var entry in keywordData)
				{
					writer.WriteLine($"// Keyword: {entry.Key}");
					if (entry.Value.Any())
					{
						// Assuming the first item determines the data type
						var typeName = entry.Value.First().GetType().Name;
						if (typeName == "List`1") // Checking if it's a list type
						{
							// Get the type of the list's contents
							var innerType = entry.Value.First() is List<int> ? "int[]" : "Coordinate[]"; // Modify as needed
							writer.WriteLine($"const {innerType} {entry.Key}_Data = {{ /* Array values here */ }};");
						}
						else
						{
							writer.WriteLine($"const {typeName} {entry.Key} = /* Value here */;");
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
