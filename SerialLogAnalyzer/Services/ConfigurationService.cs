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

		// Load a custom AppConfiguration that only contains Settings and ComputerConfigs
		public AppConfiguration LoadCustomConfiguration(string customConfigFilePath)
		{
			if (!File.Exists(customConfigFilePath))
			{
				return new AppConfiguration(); // Return a new instance if file doesn't exist
			}

			XmlSerializer serializer = new XmlSerializer(typeof(AppConfiguration));

			using (FileStream fileStream = new FileStream(customConfigFilePath, FileMode.Open))
			{
				// Deserialize the file and load the custom config
				return (AppConfiguration)serializer.Deserialize(fileStream);
			}
		}

		// Save a custom AppConfiguration that only contains Settings and ComputerConfigs
		public void SaveCustomConfiguration(AppConfiguration config, string customConfigFilePath)
		{
			// Create a new instance of AppConfiguration containing only Settings and ComputerConfigs
			AppConfiguration customConfig = new AppConfiguration
			{
				Settings = config.Settings,
				ComputerConfigs = config.ComputerConfigs
			};

			XmlSerializer serializer = new XmlSerializer(typeof(AppConfiguration));

			using (FileStream fileStream = new FileStream(customConfigFilePath, FileMode.Create))
			{
				// Serialize the custom configuration to file
				serializer.Serialize(fileStream, customConfig);
			}
		}

		// Merge custom config into default config
		private void MergeConfigurations(AppConfiguration defaultConfig, AppConfiguration customConfig)
		{
			// Merge Settings: Custom overrides default
			if (customConfig.Settings != null)
			{
				defaultConfig.Settings = customConfig.Settings;
			}

			// Merge ComputerConfigs: Custom overrides or adds to default
			if (customConfig.ComputerConfigs != null && customConfig.ComputerConfigs.Count > 0)
			{
				foreach (var customConfigPC in customConfig.ComputerConfigs)
				{
					var defaultConfigPC = defaultConfig.ComputerConfigs
						.Find(pc => pc.Name == customConfigPC.Name);

					if (defaultConfigPC != null)
					{
						// Update the settings in defaultConfigPC with the values from customConfigPC
						defaultConfigPC.SerialConsoleConfigs = customConfigPC.SerialConsoleConfigs;
					}
					else
					{
						// If custom PC is not found in default, add it
						defaultConfig.ComputerConfigs.Add(customConfigPC);
					}
				}
			}
		}
	}
}
