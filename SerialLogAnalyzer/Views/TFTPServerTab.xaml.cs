using SerialLogAnalyzer.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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
	public partial class TFTPServerTab : UserControl
	{

		public string SelectedRootDir { get; set; }

		private TftpServer tftpServer;
		private Logger logger;
		private Logger tftpLogger;


		public TFTPServerTab()
		{
			InitializeComponent();

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
				tftpServer = new TftpServer(ipAddress, rootDirectory, tftpLogger);
				logger.Log($"TFTP Server has been initialized with IP: '{ipAddress}' and root directory '{rootDirectory}'.", LogLevel.Info);
				tftpServer.Start();

				// Update button states
				StartTftpServerButton.IsEnabled = false;
				StopTftpServerButton.IsEnabled = true;

				logger.Log($"TFTP Server started at IP: {ipAddress} and Port: {port}.", LogLevel.Info);
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to start the TFTP server. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				logger.Log($"Error starting TFTP Server: {ex.Message}", LogLevel.Error);
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
					logger.Log("TFTP Server has been stopped.", LogLevel.Info);
				}
				else
				{
					logger.Log("Attempted to stop the server, but it was not running.", LogLevel.Debug);
				}

				// Update button states
				StopTftpServerButton.IsEnabled = false;
				StartTftpServerButton.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to stop the TFTP server. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
				MessageBox.Show($"Failed to clear logs. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				logger.Log($"Error clearing logs: {ex.Message}", LogLevel.Error);
			}
		}

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
						logger.Log($"TFTP Server logs have been exported to {filePath}.", LogLevel.Info);

						MessageBox.Show($"Logs have been exported successfully to {filePath}.", "Export Success", MessageBoxButton.OK, MessageBoxImage.Information);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show($"Failed to export logs. Error: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
				logger.Log($"Error exporting logs: {ex.Message}", LogLevel.Error);
			}
		}

		private void UploadFileButton_Click(object sender, RoutedEventArgs e)
		{

		}

		private void DownloadFileButton_Click(object sender, RoutedEventArgs e)
		{

		}
	}
}
