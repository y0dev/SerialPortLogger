using System.Collections.Generic;
using System.Xml.Serialization;

namespace SerialLogAnalyzer.Models
{
	[XmlRoot("Configuration")]
	public class AppConfiguration
	{
		[XmlArray("Items")]
		[XmlArrayItem("Item")]
		public List<Item> Items { get; set; }

		[XmlElement("Settings")]
		public Settings Settings { get; set; }

		[XmlElement("LastFileParsed")]
		public LastFileParsed LastFileParsed { get; set; }
	}

	public class Item
	{
		[XmlElement("Name")]
		public string Name { get; set; }

		[XmlArray("Modes")]
		[XmlArrayItem("Mode")]
		public List<Mode> Modes { get; set; }
	}

	public class Mode
	{
		[XmlElement("Keyword")]
		public string Keyword { get; set; }

		[XmlElement("OutputFile")]
		public string OutputFile { get; set; }
	}

	public class Settings
	{
		[XmlElement("Theme")]
		public string Theme { get; set; }

		[XmlElement("FontSize")]
		public int FontSize { get; set; }
	}

	public class LastFileParsed
	{
		[XmlArray("LastFiles")]
		[XmlArrayItem("File")]
		public List<string> LastFiles { get; set; }

		[XmlElement("Status")]
		public string Status { get; set; }

		[XmlElement("TotalFailureCount")]
		public int TotalFailureCount { get; set; }
	}
}
