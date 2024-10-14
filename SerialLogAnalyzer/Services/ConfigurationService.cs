using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using SerialLogAnalyzer.Models;

namespace SerialLogAnalyzer.Services
{
	public class ConfigurationService
	{
		private string configFilePath;

		public ConfigurationService(string filePath)
		{
			configFilePath = filePath;
		}

		public AppConfiguration LoadConfiguration()
		{
			XmlSerializer serializer = new XmlSerializer(typeof(AppConfiguration));

			using (FileStream fileStream = new FileStream(configFilePath, FileMode.Open))
			{
				return (AppConfiguration)serializer.Deserialize(fileStream);
			}
		}

		public void SaveConfiguration(AppConfiguration config)
		{
			XmlSerializer serializer = new XmlSerializer(typeof(AppConfiguration));

			using (FileStream fileStream = new FileStream(configFilePath, FileMode.Create))
			{
				serializer.Serialize(fileStream, config);
			}
		}
	}
}
