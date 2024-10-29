using System;
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
		public string Title { get; set; }
		public string VariableName { get; set; }
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
			int index = 0;
			int lastArrayIndex = 0;

			// Read the file line by line
			foreach (var line in File.ReadLines(FilePath))
			{
				List<int> currentIntArray = null;
				List<double> currentDoubleArray = null;

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

										// Initialize or continue the array
										if (currentIntArray == null)
										{
											if (index - lastArrayIndex == 0 || index - lastArrayIndex > 1)
											{
												currentIntArray = new List<int>();
												currentIntArray.AddRange(numbers);
												parseData.VariableName = subKeyword.VariableName;
												parseData.IntArray = currentIntArray;
												currentIntArray = null; // Reset for next use
												dataList.Add(parseData);
											}
											else if (index - lastArrayIndex == 1)
											{
												dataList[dataList.Count - 1].IntArray.AddRange(numbers);
												currentIntArray = null; // Reset for next use
											}
											lastArrayIndex = index;
											index++;
											break;
										}
									}
								}
								else if (subKeyword.DataType == "Double")
								{
									var match = subKeyword.Regex.Match(line);
									if (match.Success)
									{
										// Initialize or continue the array
										if (currentDoubleArray == null)
										{
											var numbers = match.Value.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
												.Select(n => double.Parse(n.Trim()))
												.ToList();
											if (index - lastArrayIndex == 0 || index - lastArrayIndex > 1)
											{
												currentDoubleArray = new List<double>();
												currentDoubleArray.AddRange(numbers);
												parseData.VariableName = subKeyword.VariableName;
												parseData.IntArray = currentIntArray;
												currentDoubleArray = null; // Reset for next use
												dataList.Add(parseData);
											}
											else if (index - lastArrayIndex == 1)
											{
												dataList[dataList.Count - 1].DoubleArray.AddRange(numbers);
												currentDoubleArray = null; // Reset for next use
											}
											lastArrayIndex = index;
											index++;
											break;
										}
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
										parseData.VariableName = subKeyword.VariableName;
									}
								}
								else if (subKeyword.DataType == "Double")
								{
									var match = Regex.Match(line, @"-?\d+(\.\d+)?");
									if (match.Success)
									{
										parseData.SingleDouble = double.Parse(match.Value);
										parseData.VariableName = subKeyword.VariableName;
									}
								}
							}

							dataList.Add(parseData);
							index++;
							break;
						}
					} // End foreach subKeyword
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
				// Sort the keywordData dictionary
				var sortedKeywordData = keywordData
					.OrderBy(entry => entry.Value.FirstOrDefault(), new ParseDataComparer());

				// Get all keys and entries as a list so we can check the next entry type
				var sortedEntries = sortedKeywordData.ToList();

				// Create a list to hold the output lines for the header row
				var headerRow = new List<string>();
				// Create a list to hold lists of values for each keyword
				var keywordValues = new List<List<string>>();
				
				// Iterate through sorted keyword data
				foreach (var entry in sortedKeywordData)
				{
					// Add the keyword to the header
					if(entry.Value[0].IntArray == null && entry.Value[0].DoubleArray == null)
					{
						headerRow.Add(entry.Key);
					}
					else
					{
						headerRow.Add("");
						keywordValues.Add(new List<string>());
					}

					// Create a list for values under this keyword
					var valuesList = new List<string>();

					int intArrayCount = 0;   
					int doubleArrayCount = 0;

					// Go through each ParseData object
					foreach (var parseData in entry.Value)
					{
						// Check for single values
						if (parseData.SingleInt.HasValue)
						{
							valuesList.Add(parseData.SingleInt.Value.ToString());
						}
						else if (parseData.SingleDouble.HasValue)
						{
							valuesList.Add(parseData.SingleDouble.Value.ToString());
						}

						// Track the maximum length for arrays and increment array counters
						if (parseData.IntArray?.Any() == true)
						{
							valuesList = new List<string>();
							for (int i = 0; i < parseData.IntArray.Count; i++)
							{
								valuesList.Add(parseData.IntArray[i].ToString());
							}
							keywordValues.Add(valuesList);
							intArrayCount++;
						}

						if (parseData.DoubleArray?.Any() == true)
						{
							valuesList = new List<string>();
							for (int i = 0; i < parseData.DoubleArray.Count; i++)
							{
								valuesList.Add(parseData.DoubleArray[i].ToString());
							}
							keywordValues.Add(valuesList);
							doubleArrayCount++;
						}
					}

					// Add placeholders in the header for each array element
					for (int i = 0; i < intArrayCount; i++)
					{
						headerRow.Add($"{entry.Key}_{i + 1}");

					}
					for (int i = 0; i < doubleArrayCount; i++)
					{
						headerRow.Add($"{entry.Key}_{i + 1}");
					}

					if((intArrayCount == 0) && (doubleArrayCount == 0))
					{
						// Add the list of values for the keyword to the main list
						keywordValues.Add(valuesList);
					}
				}

				// Write the header row to the output
				string headerLine = string.Join(",", headerRow);
				// Console.WriteLine(headerLine);
				writer.WriteLine(headerLine);

				// Write values row by row
				for (int row = 0; row < keywordValues.Max(v => v.Count); row++)
				{
					var rowValues = new List<string>();

					for (int col = 0; col < keywordValues.Count; col++)
					{
						// Add the value for the current row or an empty cell if out of range
						rowValues.Add(row < keywordValues[col].Count ? keywordValues[col][row] : "");
					}

					// Write the row values to the output
					string rowLine = string.Join(",", rowValues);
					// Console.WriteLine(rowLine);
					writer.WriteLine(rowLine);
				}

			}
		} // End of WriteCsv()

		private void WriteTxt(string outputFilePath, Dictionary<string, List<ParseData>> keywordData)
		{
			using (var writer = new StreamWriter(outputFilePath))
			{

				// Sort the keywordData dictionary
				var sortedKeywordData = keywordData
					.OrderBy(entry => entry.Value.FirstOrDefault(), new ParseDataComparer());

				// Get all keys and entries as a list so we can check the next entry type
				var sortedEntries = sortedKeywordData.ToList();
				foreach (var entry in sortedKeywordData)
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

				// Sort the keywordData dictionary
				var sortedKeywordData = keywordData
					.OrderBy(entry => entry.Value.FirstOrDefault(), new ParseDataComparer());

				// Get all keys and entries as a list so we can check the next entry type
				var sortedEntries = sortedKeywordData.ToList();

				foreach (var entry in keywordData)
				{
					foreach (var data in entry.Value)
					{
						if (data.IntArray != null)
						{
							writer.WriteLine($"const int {data.VariableName}[] = {{ {string.Join(", ", data.IntArray)} }};");
						}
						else if (data.DoubleArray != null)
						{
							writer.WriteLine($"const double {data.VariableName}[] = {{ {string.Join(", ", data.DoubleArray)} }};");
						}
						else if (data.SingleInt.HasValue)
						{
							writer.WriteLine($"const int {data.VariableName} = {data.SingleInt.Value};");
						}
						else if (data.SingleDouble.HasValue)
						{
							writer.WriteLine($"const double {data.VariableName} = {data.SingleDouble.Value};");
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
		public string VariableName { get; set; }
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
							Keyword = "Total Possessions",
							Regex = new Regex(@"Total Possessions:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "total_poss",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Dribble Average Per Possession",
							Regex = new Regex(@"Dribble Average Per Possession:\s*([\d.]+)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "ave_dribble_per_poss",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Most Dribbles in a Single Possession",
							Regex = new Regex(@"Most Dribbles in a Single Possession:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "max_dribble_in_poss",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Least Dribbles in a Single Possession",
							Regex = new Regex(@"Least Dribbles in a Single Possession:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "min_dribble_in_poss",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Max Dribbles Occurred",
							Regex = new Regex(@"Number of times the Max Dribbles Occurred:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "max_dribbles",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Least Dribbles Occurred",
							Regex = new Regex(@"Number of times the Least Dribbles Occurred:\s*(\d+)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "min_dribbles",
							IsArray = false
						},
						new KeywordRegex
						{
							Keyword = "Dribble Numbers Game",
							Regex = new Regex(@"(\d+(?:,\s*\d+)*)", RegexOptions.Multiline),
							DataType = "Integer",
							VariableName = "dribbles_per_poss",
							IsArray = true
						},
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

	public class ParseDataComparer : IComparer<ParseData>
	{
		public int Compare(ParseData x, ParseData y)
		{
			// Prioritize SingleInt/SingleDouble over IntArray/DoubleArray
			if (x.SingleInt.HasValue || x.SingleDouble.HasValue)
			{
				return y.SingleInt.HasValue || y.SingleDouble.HasValue ? 0 : -1;
			}
			else if (y.SingleInt.HasValue || y.SingleDouble.HasValue)
			{
				return 1;
			}

			// Compare IntArray/DoubleArray based on their presence
			if (x.IntArray?.Any() == true && y.IntArray?.Any() != true)
			{
				return -1;
			}
			else if (x.IntArray?.Any() != true && y.IntArray?.Any() == true)
			{
				return 1;
			}
			else if (x.DoubleArray?.Any() == true && y.DoubleArray?.Any() != true)
			{
				return -1;
			}
			else if (x.DoubleArray?.Any() != true && y.DoubleArray?.Any() == true)
			{
				return 1;
			}

			// If all else fails, return 0 (equal)
			return 0;
		} // End of Compare()
	} // End of class ParseDataComparer
}
