using SerialLogAnalyzer.Models;
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
			MessageBox.Show($"PC Configuration '{pcName}' created!");
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

			// For simplicity, you can ask for more details through inputs or a separate window.
			var serialConsoleConfig = new SerialConsoleConfig
			{
				Name = "New Serial Console",
				Title = "Default Title",
				ColorScheme = "Default Color",
				FontSize = 12 // Adjust as necessary
			};

			// Here, you can implement logic to specify which ComputerConfig to add this to
			var selectedPcConfig = appConfiguration.ComputerConfigs[0]; // Default to the first one

			selectedPcConfig.SerialConsoleConfigs.Add(serialConsoleConfig);
			UpdateSerialConsoleConfigList();
			MessageBox.Show($"Serial Console Config created for '{selectedPcConfig.Name}'!");
		}

		// Handler for saving the configuration
		private void SaveConfigurationButton_Click(object sender, RoutedEventArgs e)
		{
			// You may want to implement a file dialog here to specify where to save.
			MessageBox.Show("Configuration saved! (Implement save logic)");
		}

		// Helper method to update the ListBox showing Serial Console Configs
		private void UpdateSerialConsoleConfigList()
		{
			SerialConsoleConfigList.Items.Clear();
			foreach (var computerConfig in appConfiguration.ComputerConfigs)
			{
				foreach (var consoleConfig in computerConfig.SerialConsoleConfigs)
				{
					SerialConsoleConfigList.Items.Add(consoleConfig.Name);
				}
			}
		}
	}
}
