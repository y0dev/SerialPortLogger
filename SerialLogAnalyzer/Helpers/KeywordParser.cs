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

		public Dictionary<string, List<object>> ParseFile()
		{
			var keywordData = new Dictionary<string, List<object>>();

			if (!File.Exists(FilePath))
			{
				throw new FileNotFoundException($"The file {FilePath} was not found.");
			}

			// Get the dictionary of keyword regexes
			var keywordRegexDict = KeywordRegexList.GetKeywordRegexDictionary();

			// A dictionary to track the current keyword context based on the detected header
			var currentHeaderKeyword = string.Empty;

			int arrayNum = 0;
			List<int> intergerArr = new List<int>();

			// Read the file line by line
			foreach (var line in File.ReadLines(FilePath))
			{
				// Skip blank lines
				if (string.IsNullOrWhiteSpace(line))
				{
					continue; // Skip to the next iteration of the loop
				}

				if (string.IsNullOrEmpty(currentHeaderKeyword))
				{
					// Check if the line matches any header keywords
					foreach (var headerKeyword in keywordRegexDict.Keys)
					{
						if (keywordRegexDict[headerKeyword].Regex.IsMatch(line))
						{
							// Set the current header keyword when a header is found
							currentHeaderKeyword = headerKeyword;
							break;
						}
					}
					if (!string.IsNullOrEmpty(currentHeaderKeyword))
					{
						continue;
					}
				}

				// Now check for subkeywords based on the current header keyword
				if (!string.IsNullOrEmpty(currentHeaderKeyword))
				{
					var headerKeywordRegex = keywordRegexDict[currentHeaderKeyword];

					// Check subkeywords
					foreach (var subKeyword in headerKeywordRegex.SubKeywordsRegex)
					{
						if (subKeyword.Regex.IsMatch(line))
						{
							List<object> dataList;
							if (!keywordData.TryGetValue(subKeyword.Keyword, out dataList))
							{
								dataList = new List<object>(); // Use List<object> here
								keywordData[subKeyword.Keyword] = dataList;
							}

							// Handle different data types
							if (subKeyword.IsArray)
							{
								if (subKeyword.DataType == "Integer")
								{
									// Check if the line contains a game header like "Game #1"
									if (line.Contains("Game #"))
									{
										if (intergerArr.Count != 0)
										{
											keywordData[$"{subKeyword.DataType} Array {arrayNum - 1}"].Add(intergerArr); // Store the list of integers
										}

										// Start a new dataList for this new game
										dataList = new List<object>();
										keywordData[$"{subKeyword.DataType} Array {arrayNum}"] = dataList;
										arrayNum += 1;
										intergerArr = new List<int>();										
									}

									// Extract the entire string of numbers (across multiple lines if needed)
									var match = subKeyword.Regex.Match(line);
									if (match.Success)
									{
										// Split the string by commas and convert to a list of integers
										var numbers = match.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
																 .Select(n => int.Parse(n.Trim()))
																 .ToList();
										if (numbers.Count > 1)
										{
											foreach (int value in numbers)
											{
												intergerArr.Add(value);
											}
										}
									}
								}
								else if (subKeyword.DataType == "Coordinate")
								{
									// Extract coordinates in format (x, y)
									var matches = Regex.Matches(line, @"\((-?\d+\.\d+),\s*(-?\d+\.\d+)\)");
									var coordinates = new List<Tuple<double, double>>();
									foreach (Match match in matches)
									{
										coordinates.Add(new Tuple<double, double>(
											double.Parse(match.Groups[1].Value),
											double.Parse(match.Groups[2].Value)
										));
									}
									dataList.Add(coordinates); // Store list of coordinate tuples
								}
							}
							else
							{
								if (subKeyword.DataType == "Double")
								{
									// Use regex to find the first double value in the line
									var match = Regex.Match(line, @"-?\d+(\.\d+)?");
									if (match.Success)
									{
										double value = double.Parse(match.Value); // Parse the numeric portion
										dataList.Add(value); // Add the parsed double value to the data list
									}
								}
								else if (subKeyword.DataType == "Integer")
								{
									// Use regex to find the first integer value in the line
									var match = Regex.Match(line, @"-?\d+");
									if (match.Success)
									{
										int value = int.Parse(match.Value); // Parse the numeric portion
										dataList.Add(value); // Add the parsed integer value to the data list
									}
								}
							}
						}
					}
				}
			}
			return keywordData;
		}
		// End of ParseFile()

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
				default:
					throw new NotSupportedException($"The output file format {extension} is not supported.");
			}
		} // End of WriteOutput()

		private void WriteCsv(string outputFilePath, Dictionary<string, List<object>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{
				// Write header
				writer.WriteLine("Keyword,Value");

				foreach (var entry in keywordData)
				{
					foreach (var item in entry.Value)
					{
						if (item is List<int> intList)
						{
							// Join the list of integers as a comma-separated string
							writer.WriteLine($"{entry.Key},{string.Join(",", intList)}");
						}
						else
						{
							// Handle other types as needed (e.g., doubles, tuples, etc.)
							writer.WriteLine($"{entry.Key},{item}");
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
							// Join the list of integers and format it
							writer.WriteLine($" - Values: {string.Join(", ", intList)}");
						}
						else
						{
							// Handle other types as needed (e.g., doubles, tuples, etc.)
							writer.WriteLine($" - Value: {item}");
						}
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
