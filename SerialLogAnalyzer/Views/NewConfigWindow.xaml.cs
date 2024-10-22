using Microsoft.Win32;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.Services;
using System;
using System.Collections.Generic;
using System.Windows;

namespace SerialLogAnalyzer
{
	public partial class NewConfigWindow : Window
	{
		public bool NewConfigCreated;
		public AppConfiguration appConfiguration;
		

		public NewConfigWindow()
		{
			InitializeComponent();
			appConfiguration = new AppConfiguration
			{
				ComputerConfigs = new List<ComputerConfig>()
			};
			NewConfigCreated = false;
		}

		// Handler for creating a new PC configuration
		private void CreatePcConfigButton_Click(object sender, RoutedEventArgs e)
		{
			string pcName = pcNameTextBox.Text.Trim();
			if (string.IsNullOrEmpty(pcName))
			{
				MessageBox.Show("Please enter a valid PC name.");
				return;
			}

			var computerConfig = new ComputerConfig
			{
				Name = pcName,
				SerialConsoleConfigs = new List<SerialConsoleConfig>()
			};
			
			// After creating the PC config, enable the serial console config button
			createSerialConsoleConfigButton.IsEnabled = true;

			appConfiguration.ComputerConfigs.Add(computerConfig);
			pcNameTextBox.Clear();
			UpdateSerialConsoleConfigList();
		}

		// Handler for creating a new Serial Console Config
		private void CreateSerialConsoleConfigButton_Click(object sender, RoutedEventArgs e)
		{
			if (appConfiguration.ComputerConfigs.Count == 0)
			{
				MessageBox.Show("Please create a PC Configuration first.");
				return;
			}

			// Open the new window to create a Serial Console Config
			SerialConsoleConfigWindow configWindow = new SerialConsoleConfigWindow();
			bool? result = configWindow.ShowDialog();

			if (result == true)
			{
				// Retrieve the new SerialConsoleConfig from the window
				SerialConsoleConfig newConfig = configWindow.Tag as SerialConsoleConfig;

				// Add the new config to the selected PC Configuration
				var selectedPcConfig = appConfiguration.ComputerConfigs[0]; // Default to the first one
				selectedPcConfig.SerialConsoleConfigs.Add(newConfig);

				// Update the list to reflect the new config
				UpdateSerialConsoleConfigList();
				NewConfigCreated = true;
			}
		}

		// Handler for saving the configuration
		private void SaveConfigurationButton_Click(object sender, RoutedEventArgs e)
		{
			// Open SaveFileDialog for the user to specify the file location and name
			SaveFileDialog saveFileDialog = new SaveFileDialog
			{
				Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*",
				DefaultExt = ".xml",
				FileName = "Configuration" // Default file name
			};

			// Show the dialog and get the result
			bool? result = saveFileDialog.ShowDialog();

			if (result == true)
			{
				string fileName = saveFileDialog.FileName;
				ConfigurationService configurationService = new ConfigurationService(Properties.Resources.CONFIG_PATH);

				// Call the SaveCustomConfiguration from ConfigurationService
				configurationService.SaveCustomConfiguration(appConfiguration, fileName);
			}
		} // End of SaveConfigurationButton_Click()


		// Helper method to update the ListBox showing Serial Console Configs
		private void UpdateSerialConsoleConfigList()
		{
			var serialConfigs = new List<SerialConsoleConfig>();

			foreach (var computerConfig in appConfiguration.ComputerConfigs)
			{
				foreach (var consoleConfig in computerConfig.SerialConsoleConfigs)
				{
					serialConfigs.Add(consoleConfig);
				}
			}

			serialConsoleConfigDataGrid.ItemsSource = serialConfigs;
		} // End of UpdateSerialConsoleConfigList()

		private void ImportXmlButton_Click(object sender, RoutedEventArgs e)
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				Filter = "XML Files (*.xml)|*.xml",
				Title = "Import Configuration XML"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					string xmlFilePath = openFileDialog.FileName;
					// Logic to parse and load XML file
					ConfigurationService configurationService = new ConfigurationService();
					appConfiguration = configurationService.LoadCustomConfiguration(xmlFilePath);
					List<ComputerConfig> configs = appConfiguration.ComputerConfigs;

					// Populate ComboBox with configuration options
					xmlConfigComboBox.ItemsSource = configs;
					xmlConfigComboBox.DisplayMemberPath = "Name";
					xmlConfigComboBox.IsEnabled = true;
					
				}
				catch (Exception ex)
				{
					MessageBox.Show($"Error importing XML file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
		} // End of ImportXmlButton_Click()

		private void XmlConfigComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
		{
			ComputerConfig computerConfig = (ComputerConfig)xmlConfigComboBox.SelectedItem;
			if(computerConfig != null)
			{
				// Update the DataGrid with the selected config's SerialConsoleConfigs
				serialConsoleConfigDataGrid.ItemsSource = computerConfig.SerialConsoleConfigs;
			}
		} // End of XmlConfigComboBox_SelectionChanged()
	}
}
