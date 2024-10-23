using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for TFTPServerTab.xaml
	/// </summary>
	public partial class TFTPServerView : UserControl
	{

		public string SelectedRootDir { get; set; }

		private TftpServer tftpServer;
		private Logger logger;
		private Logger tftpLogger;
		private MainViewModel viewModel;
		private int filesTransfered = 0;


		public TFTPServerView(MainViewModel viewModel)
		{
			InitializeComponent();

			this.viewModel = viewModel;
			SelectedRootDir = AppDomain.CurrentDomain.BaseDirectory;

			logger = Logger.GetInstance("slate_app.log", false);
			tftpLogger = Logger.GetInstance("tftp_server.log");

		}

		private void StartServerButton_Click(object sender, RoutedEventArgs e)
		{
			// Retrieve the values from the TextBoxes
			string ipAddress = tftpServerIPAddresTextBox.Text;
			string port = tftpServerPortTextBox.Text;
			string rootDirectory = tftpRootDirectoryTextBox.Text; // Get the root directory text

			// Check if the IP address and port fields are empty
			if (string.IsNullOrWhiteSpace(ipAddress))
			{
				MessageBox.Show("Please enter a valid IP address.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				logger.Log("IP address: is empty.", LogLevel.Warning);
				return; // Exit the method if validation fails
			}

			if (string.IsNullOrWhiteSpace(port))
			{
				MessageBox.Show("Please enter a valid port number.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				logger.Log("Port Number is empty.", LogLevel.Warning);
				return; // Exit the method if validation fails
			}

			// Check if the root directory field is empty
			if (string.IsNullOrWhiteSpace(rootDirectory))
			{
				MessageBox.Show("Please enter a valid root directory.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				logger.Log("Root directory is empty.", LogLevel.Warning);
				return; // Exit the method if validation fails
			}

			// Validate if the provided root directory is a valid path
			if (!Directory.Exists(rootDirectory))
			{
				MessageBox.Show("The specified root directory does not exist. Please enter a valid path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
				logger.Log($"Root directory: '{rootDirectory}' is invalid", LogLevel.Warning);
				return; // Exit the method if validation fails
			}

			try
			{
				// Initialize and start the TFTP server
				tftpServer = new TftpServer(ipAddress, rootDirectory, tftpLogger, tftpServerListView);
				logger.Log($"TFTP Server has been initialized with IP: '{ipAddress}' and root directory '{rootDirectory}'.", LogLevel.Info);
				tftpServer.Start();

				tftpServerStatusTextBlock.Text = "Status: Started";
				tftpServerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Green);

				// Update button states
				StartTftpServerButton.IsEnabled = false;
				StopTftpServerButton.IsEnabled = true;

				// Log success message
				tftpServer.AddLogEntry($"TFTP Server started at IP: {ipAddress} and Port: {port}.");
				logger.Log($"TFTP Server started at IP: {ipAddress} and Port: {port}.", LogLevel.Info);

			}
			catch (Exception ex)
			{
				tftpServer.AddLogEntry($"Error starting TFTP Server: {ex.Message}");
				logger.Log($"Error starting TFTP Server: {ex.Message}", LogLevel.Error);
				tftpServerStatusTextBlock.Text = "Status: Error Starting";
				tftpServerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);
			}

		} // End of StartServerButton_Click()

		private void StopServerButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Stop the TFTP server if it is running
				if (tftpServer != null)
				{
					tftpServer.Stop();
					tftpServer.AddLogEntry("TFTP Server has been stopped.");
					logger.Log("TFTP Server has been stopped.", LogLevel.Info);
					filesTransfered = tftpServer.FilesTransfered;
				}
				else
				{
					logger.Log("Attempted to stop the server, but it was not running.", LogLevel.Debug);
				}

				// Update button states
				StopTftpServerButton.IsEnabled = false;
				StartTftpServerButton.IsEnabled = true;


				tftpServerStatusTextBlock.Text = "Status: Stopped";
				tftpServerStatusTextBlock.Foreground = new SolidColorBrush(Colors.Red);

				ConfigHelper.SaveConfigWithRecentActivities(viewModel,
							new Activity
							{
								ComputerName = Environment.MachineName,
								Type = "TFTP Server",
								FilesTransferred = filesTransfered,
								IPAddress = tftpServerIPAddresTextBox.Text,
								ActivityDateTime = DateTime.Now
							});
			}
			catch (Exception ex)
			{
				tftpServer.AddLogEntry($"Error stopping TFTP Server: {ex.Message}");
				logger.Log($"Error stopping TFTP Server: {ex.Message}", LogLevel.Error);
			}

		} // End of StartServerButton_Click()

		private void ClearLogsButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Clear the logs in the logger
				if (tftpLogger != null)
				{
					tftpLogger.ClearLog();
					logger.Log("TFTP Server logs have been cleared.", LogLevel.Info);
				}

				// Clear the log display in the ListView (if using ListView to show logs)
				if (tftpServerListView != null)
				{
					tftpServerListView.Items.Clear(); // Clear the ListView that displays logs
				}
				
			}
			catch (Exception ex)
			{
				tftpServer.AddLogEntry($"Failed to clear logs. Error: {ex.Message}");
				logger.Log($"Error clearing logs: {ex.Message}", LogLevel.Error);
			}
		} // End of ClearLogsButton_Click()

		private void ExportLogsButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Open a SaveFileDialog for the user to choose the export location
				var saveFileDialog = new Microsoft.Win32.SaveFileDialog
				{
					FileName = "TftpServerLogs", // Default file name
					DefaultExt = ".txt", // Default file extension
					Filter = "Text documents (.txt)|*.txt" // Filter files by extension
				};

				bool? result = saveFileDialog.ShowDialog();

				// If the user chooses a file location
				if (result == true)
				{
					string filePath = saveFileDialog.FileName;

					// Write logs to the selected file
					if (tftpLogger != null)
					{
						tftpLogger.Export(filePath); // Assuming the logger class has an Export method to write logs to file
						tftpServer.AddLogEntry($"TFTP Server logs have been exported to {filePath}.");
						logger.Log($"TFTP Server logs have been exported to {filePath}.", LogLevel.Info);

						MessageBox.Show($"Logs have been exported successfully to {filePath}.", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
			}
			catch (Exception ex)
			{
				tftpServer.AddLogEntry($"Failed to export logs. Error: {ex.Message}");
				logger.Log($"Error exporting logs: {ex.Message}", LogLevel.Error);
			}
		} // End of ExportLogsButton_Click()

		private void BrowseDirButton_Click(object sender, RoutedEventArgs e)
		{
			// Create an instance of FolderBrowserDialog
			using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
			{
				// Set the initial selected path (optional, starts with the current directory)
				dialog.SelectedPath = tftpRootDirectoryTextBox.Text;

				// Show the dialog and check if a folder was selected
				System.Windows.Forms.DialogResult result = dialog.ShowDialog();

				if (result == System.Windows.Forms.DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
				{
					// Update the TextBox with the selected directory path
					tftpRootDirectoryTextBox.Text = dialog.SelectedPath;
				}
			}
		} // End of BrowseDirButton_Click()

		private void TftpServerIPAddresTextBox_LostFocus(object sender, RoutedEventArgs e)
		{
			var textBox = sender as TextBox;
			string ipAddress = textBox.Text;

			if (!IsValidIPAddress(ipAddress))
			{
				MessageBox.Show("Please enter a valid IP address.", "Invalid IP Address", MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		// Function to validate the IP address
		private bool IsValidIPAddress(string ipAddress)
		{
			// Regex pattern for matching a valid IP address
			string pattern = @"^((25[0-5]|(2[0-4][0-9])|([01]?[0-9][0-9]?))\.){3}(25[0-5]|(2[0-4][0-9])|([01]?[0-9][0-9]?))$";
			var regex = new Regex(pattern);

			// Check if the input matches the IP address pattern
			return regex.IsMatch(ipAddress);
		} // End of IsValidIPAddress()
	}
}
