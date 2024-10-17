using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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

			// Read the file line by line
			foreach (var line in File.ReadLines(FilePath))
			{
				foreach (var keyword in Keywords)
				{
					if (line.Trim().ToLower().Contains(keyword.Trim().ToLower()))
					{
						// If the keyword is found, add the line to the keyword data
						if (!keywordData.ContainsKey(keyword))
						{
							keywordData[keyword] = new List<string>();
						}
						keywordData[keyword].Add(line);
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
	}
}
