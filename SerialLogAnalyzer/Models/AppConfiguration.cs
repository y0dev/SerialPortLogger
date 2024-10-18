using System.Collections.Generic;
using System.Globalization;
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

		[XmlArray("ComputerConfigs")]
		[XmlArrayItem("ComputerConfig")]
		public List<ComputerConfig> ComputerConfigs { get; set; }

		[XmlElement("LastFileParsed")]
		public LastFileParsed LastFileParsed { get; set; }
	}

	public class Item
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlArray("Modes")]
		[XmlArrayItem("Mode")]
		public List<Mode> Modes { get; set; }

		// Add a property to return the formatted name
		public string FormattedName
		{
			get
			{
				return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Name.Replace("_", " ").ToLower());
			}
		}
	}

	public class Mode
	{
		[XmlAttribute("name")]
		public string Name { get; set; }
		
		[XmlArray("KeywordGroups")]
		[XmlArrayItem("KeywordGroup")]
		public List<KeywordGroup> KeywordGroups { get; set; }
		
	}

	public class KeywordGroup
	{
		[XmlArray("Keywords")]
		[XmlArrayItem("Keyword")]
		public List<string> Keywords { get; set; }

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

	public class ComputerConfig
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlArray("SerialConsoleConfigs")]
		[XmlArrayItem("SerialConsoleConfig")]
		public List<SerialConsoleConfig> SerialConsoleConfigs { get; set; }
	}

	public class SerialConsoleConfig
	{
		[XmlAttribute("name")]
		public string Name { get; set; }

		[XmlElement("Title")]
		public string Title { get; set; }

		[XmlElement("ColorScheme")]
		public string ColorScheme { get; set; }

		[XmlElement("FontSize")]
		public ushort FontSize { get; set; }
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
