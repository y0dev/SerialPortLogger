using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for SerialLoggerTab.xaml
	/// </summary>
	public partial class SerialLoggerView : UserControl
	{
		public ObservableCollection<string> AvailablePorts { get; private set; }
		public ObservableCollection<int> BaudRates { get; set; }
		public int SelectedBaudRate { get; set; }
		public ObservableCollection<string> OpenedPorts { get; private set; } // Collection to track opened ports

		private MainViewModel viewModel;
		private Logger logger;
		private bool isLogging;

		// Dictionary to keep track of the logger threads for each port
		private Dictionary<string, ConsoleLogger> consolelLoggers = new Dictionary<string, ConsoleLogger>();
		private Dictionary<string, SerialLoggerTabItem> serialLoggers = new Dictionary<string, SerialLoggerTabItem>();
		private Dictionary<string, Thread> loggerThreads = new Dictionary<string, Thread>();
		
		private Thread portWatcherThread;
		private bool portWatcherRunning = true;

		public SerialLoggerView(MainViewModel viewModel)
		{
			InitializeComponent();

			this.viewModel = viewModel;
			AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());
			OpenedPorts = new ObservableCollection<string>();

			// Initialize with common baud rates
			BaudRates = new ObservableCollection<int> { 9600, 14400, 19200, 38400, 57600, 115200, 230400 };
			SelectedBaudRate = 115200; // Default baud rate

			logger = Logger.GetInstance("slate_app.log", false);

			// Set the DataContext to itself for binding
			this.DataContext = this;

			// Start the port watcher thread
			StartPortWatcher();
		}

		private void UpdateLoggingButtons()
		{
			createLoggerButton.IsEnabled = !isLogging;
			stopAllLoggersButton.IsEnabled = isLogging;
		}


		private void ConsoleOutputCheckBox_Checked(object sender, RoutedEventArgs e)
		{
			if (consoleOutputListView != null && consoleOutputTabControl != null)
			{
				// Show the ListView
				consoleOutputListView.Visibility = Visibility.Visible;

				// Hide the TabControl
				consoleOutputTabControl.Visibility = Visibility.Collapsed;
				logger.Log("Console Output Checkbox was checked.", LogLevel.Info);
			}
		}

		private void ConsoleOutputCheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if (consoleOutputListView != null && consoleOutputTabControl != null)
			{
				// Hide the ListView
				consoleOutputListView.Visibility = Visibility.Collapsed;

				// Show the TabControl
				consoleOutputTabControl.Visibility = Visibility.Visible;
				logger.Log("Console Output Checkbox was unchecked.", LogLevel.Info);
			}
		}

		private void CreateLoggerButton_Click(object sender, RoutedEventArgs e)
		{
			string selectedPort = serialPortComboBox.SelectedItem as string;
			int selectedBaudRate = Convert.ToInt32(baudRateComboBox.SelectedValue); // Get the selected baud rate

			// Access the MainViewModel instance which contains the config settings
			var viewModel = (MainViewModel)this.FindResource("MainViewModel");

			if (!string.IsNullOrEmpty(selectedPort))
			{
				string logData = $"Data from {selectedPort} at {DateTime.Now}";
				logger.Log($"Creating a logger instance for port: '{selectedPort}'.", LogLevel.Info);
				if (consoleOutputCheckBox.IsChecked == true)
				{
					string consoleTitle = $"Console {selectedPort}";
					string colorScheme = "Default"; // Default color scheme
					ushort fontSize = 14;
					string currentPcName = Environment.MachineName; // Get the current PC name
					bool configFound = false; // Flag to check if PC config was found

					// Loop through each computer configuration
					foreach (var computerConfig in viewModel.Config.ComputerConfigs)
					{
						// Check if the ComputerConfig name matches the current PC name
						if (computerConfig.Name.Trim().Equals(currentPcName.Trim(), StringComparison.OrdinalIgnoreCase))
						{
							// If it matches, check the SerialConsoleConfigs
							foreach (var serialConfig in computerConfig.SerialConsoleConfigs)
							{
								if (serialConfig.Name.Equals(selectedPort, StringComparison.OrdinalIgnoreCase))
								{
									// Found the matching configuration, retrieve the serial port information
									consoleTitle = serialConfig.Title;
									colorScheme = serialConfig.ColorScheme;
									fontSize = serialConfig.FontSize;
									configFound = true; // Set flag to true
									logger.Log($"Found a configuration for the port: '{selectedPort}' on PC '{currentPcName}'.", LogLevel.Info);

									// Optionally break the loop if you only want the first match
									break;
								}
							}
							if (configFound)
							{
								break; // Exit the outer loop if config is found
							}
						}
					}

					string baseDirectory = AppDomain.CurrentDomain.BaseDirectory; // Set base directory to the current application directory
					string logFileName = $"log_{selectedPort}.txt"; // Create a unique log file name based on the selected port

					logger.Log($"Starting a ConsoleLogger thread for port: '{selectedPort}'.", LogLevel.Debug);
					// Create a console for this port in a separate thread
					Thread consoleThread = new Thread(() =>
					{
						ConsoleLogger consoleLogger = new ConsoleLogger(consoleTitle, fontSize, selectedPort, selectedBaudRate, colorScheme, baseDirectory, logFileName);
						consoleLogger.OutputToConsole(logData);
						consolelLoggers[selectedPort] = consoleLogger;

						// Keep the console thread alive
						while (!consoleLogger.IsConsoleClosed)
						{
							Thread.Sleep(100); // Adjust this to prevent busy-waiting
						}
					});

					consoleThread.IsBackground = true;
					consoleThread.Start();

				}
				else
				{
					// Create a new logger tab for the selected port
					SerialLoggerTabItem loggerTab = new SerialLoggerTabItem(selectedPort);
					consoleOutputTabControl.Items.Add(loggerTab);
					serialLoggers[selectedPort] = loggerTab;
				}

				// Remove the selected port from the available ports
				AvailablePorts.Remove(selectedPort);

				// Add the selected port to the list of opened ports
				if (!OpenedPorts.Contains(selectedPort))
				{
					OpenedPorts.Add(selectedPort);
				}

				isLogging = true;
				stopAllLoggersButton.IsEnabled = true;
				createLoggerButton.IsEnabled = AvailablePorts.Count > 0;
				logger.Log($"Created a console thread for port '{selectedPort}' at baud rate '{selectedBaudRate}'", LogLevel.Info);
				logger.Log($"Adding '{selectedPort}' to recent activity for PC '{Environment.MachineName}'.", LogLevel.Debug);
				ConfigHelper.SaveConfigWithRecentActivities(viewModel, 
					new Models.Activity
					{
						ComputerName = Environment.MachineName,
						Type = "Serial Logger",
						SerialPort = selectedPort,
						ActivityDateTime = DateTime.Now
					});
			}
			else
			{
				logger.Log("COM port was selected.", LogLevel.Warning);
				MessageBox.Show("Please select a COM port.");
			}
		}

		private void StopAllLoggersButton_Click(object sender, RoutedEventArgs e)
		{
			isLogging = false;

			// Close all opened serial ports
			foreach (var port in OpenedPorts.ToList()) // Using ToList() to avoid modifying the collection while iterating
			{
				if (consolelLoggers.ContainsKey(port))
				{
					// Close the ConsoleLogger for the port
					ConsoleLogger consoleLogger = consolelLoggers[port];
					consoleLogger.StopReading(); // Assuming the ConsoleLogger class has a ClosePort method
					SetLogFileReadOnly(consoleLogger.LogFileName); // Set log file to read-only
					consolelLoggers.Remove(port);
				}
				else if (serialLoggers.ContainsKey(port))
				{
					// Close the SerialLoggerTabItem for the port
					SerialLoggerTabItem serialLogger = serialLoggers[port];
					// serialLogger.ClosePort(); // Assuming SerialLoggerTabItem class has a ClosePort method
					// SetLogFileReadOnly(serialLogger.LogFileName); // Set log file to read-only
					// serialLoggers.Remove(port);
				}

				logger.Log($"Closed serial port: {port} and set its log file to read-only.", LogLevel.Info);
			}


			// Clear the opened ports list when stopping loggers
			OpenedPorts.Clear();

			stopAllLoggersButton.IsEnabled = false;
			UpdateLoggingButtons();

		}

		private void StartPortWatcher()
		{
			portWatcherThread = new Thread(() =>
			{
				while (portWatcherRunning)
				{
					// Get the currently available ports
					string[] currentPorts = SerialPort.GetPortNames();

					// Compare with the AvailablePorts collection
					var newPorts = currentPorts.Except(AvailablePorts).ToList(); // Find newly added ports
					var removedPorts = AvailablePorts.Except(currentPorts).ToList(); // Find removed ports

					// If there are new ports, add them to the AvailablePorts collection
					if (newPorts.Any())
					{
						Dispatcher.Invoke(new Action(delegate
						{
							foreach (var port in newPorts)
							{
								AvailablePorts.Add(port);
								logger.Log($"New port detected: {port}", LogLevel.Info);
							}
						}));
					}

					// If there are removed ports, remove them from the AvailablePorts collection
					if (removedPorts.Any())
					{
						Dispatcher.Invoke(new Action(delegate
						{
							foreach (var port in newPorts)
							{
								AvailablePorts.Remove(port);
								logger.Log($"Port removed: {port}", LogLevel.Info);
							}
						}));
					}

					// Sleep for 10 seconds before checking again
					Thread.Sleep(10000);
				}
			});

			portWatcherThread.IsBackground = true;
			portWatcherThread.Start();
		} // End of StartPortWatcher()

		// Method to restart a logger thread if it stops unexpectedly
		private void RestartLoggerThread(string port)
		{
			Thread newLoggerThread = new Thread(() =>
			{
				while (isLogging)
				{
					string logData = $"Data from {port} at {DateTime.Now}";
					if (consoleOutputCheckBox.IsChecked == true)
					{
						// ConsoleLogger.OutputToConsole(logData);
					}
					else
					{
						Dispatcher.Invoke(new Action(AppendLogToTabMethod(port, logData)));
					}
					Thread.Sleep(1000);
				}
			});

			newLoggerThread.IsBackground = true;
			newLoggerThread.Start();

			// Replace the old thread with the new one
			loggerThreads[port] = newLoggerThread;
		} // End of RestartLoggerThread()

		// Method to be called to append log to tab
		private Action AppendLogToTabMethod(string port, string logData)
		{
			return delegate
			{
				AppendLogToTab(port, logData); // Call the method that appends log data to the UI
			};
		} // End of AppendLogToTabMethod()

		// Method to append log data to the correct tab
		private void AppendLogToTab(string port, string logData)
		{
			foreach (TabItem item in consoleOutputTabControl.Items)
			{
				if (item.Header.ToString() == port)
				{
					// ((SerialLoggerTabItem)item).AppendLog(logData); // Assuming SerialLoggerTabItem has AppendLog method
					break;
				}
			}
		} // End of AppendLogToTab()

		private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			// Stop the port watcher thread when the view is unloaded
			portWatcherRunning = false;
			if (portWatcherThread != null && portWatcherThread.IsAlive)
			{
				portWatcherThread.Join();
			}
		} // End of OnUnloaded()

		// Helper method to set the log file to read-only
		private void SetLogFileReadOnly(string logFileName)
		{
			try
			{
				string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, logFileName);
				if (File.Exists(filePath))
				{
					FileInfo fileInfo = new FileInfo(filePath);
					fileInfo.IsReadOnly = true; // Set the file as read-only
					logger.Log($"Log file '{logFileName}' set to read-only.", LogLevel.Info);
				}
			}
			catch (Exception ex)
			{
				logger.Log($"Error setting log file '{logFileName}' to read-only: {ex.Message}", LogLevel.Error);
			}
		} // End of SetLogFileReadOnly()
	}
}
