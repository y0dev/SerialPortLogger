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
		private AppConfiguration appConfiguration;

		public NewConfigWindow()
		{
			InitializeComponent();
			appConfiguration = new AppConfiguration
			{
				ComputerConfigs = new List<ComputerConfig>()
			};
		}

		// Handler for creating a new PC configuration
		private void CreatePcConfigButton_Click(object sender, RoutedEventArgs e)
		{
			string pcName = PcNameTextBox.Text.Trim();
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
			CreateSerialConsoleConfigButton.IsEnabled = true;

			appConfiguration.ComputerConfigs.Add(computerConfig);
			PcNameTextBox.Clear();
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
			}
			else
			{
				// The user canceled the creation
				MessageBox.Show("Serial Console Config creation was canceled.");
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
		}
		

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

			SerialConsoleConfigDataGrid.ItemsSource = serialConfigs;
		}

	}
}
