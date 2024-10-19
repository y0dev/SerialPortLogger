using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for SerialLoggerTab.xaml
	/// </summary>
	public partial class SerialLoggerTab : UserControl
	{
		public ObservableCollection<string> AvailablePorts { get; private set; }
		public ObservableCollection<int> BaudRates { get; set; }
		public int SelectedBaudRate { get; set; }

		private bool isLogging;

		// Dictionary to keep track of the logger threads for each port
		private Dictionary<string, ConsoleLogger> consolelLoggers = new Dictionary<string, ConsoleLogger>();
		private Dictionary<string, SerialLoggerTabItem> serialLoggers = new Dictionary<string, SerialLoggerTabItem>();
		private Dictionary<string, Thread> loggerThreads = new Dictionary<string, Thread>();
		
		private Thread watchdogThread;
		private bool watchdogRunning = true;

		public SerialLoggerTab()
		{
			InitializeComponent();

			AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());

			// Initialize with common baud rates
			BaudRates = new ObservableCollection<int> { 9600, 14400, 19200, 38400, 57600, 115200, 230400 };
			SelectedBaudRate = 115200; // Default baud rate

			// Set the DataContext to itself for binding
			this.DataContext = this;
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

					loggerThreads[selectedPort] = consoleThread;
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

				isLogging = true;
				stopAllLoggersButton.IsEnabled = true;
				createLoggerButton.IsEnabled = AvailablePorts.Count > 0;
			}
			else
			{
				MessageBox.Show("Please select a COM port.");
			}
		}

		private void StopAllLoggersButton_Click(object sender, RoutedEventArgs e)
		{
			isLogging = false;
			watchdogRunning = false;

			// Stop all logger threads
			foreach (var loggerThread in loggerThreads.Values)
			{
				if (loggerThread.IsAlive)
				{
					loggerThread.Join();
				}
			}

			// Stop the watchdog thread
			if (watchdogThread != null && watchdogThread.IsAlive)
			{
				watchdogThread.Join();
			}

			stopAllLoggersButton.IsEnabled = false;
			UpdateLoggingButtons();

		}

		// Method to start the watchdog thread
		private void StartWatchdog()
		{
			watchdogThread = new Thread(new ThreadStart(delegate
			{
				while (watchdogRunning)
				{
					foreach (var portThread in loggerThreads)
					{
						string port = portThread.Key;
						Thread loggerThread = portThread.Value;

						// Check if the logger thread is still alive
						if (!loggerThread.IsAlive)
						{
							// Use Dispatcher to show the message box on the UI thread
							Dispatcher.Invoke(new Action(delegate
							{
								MessageBox.Show($"Logger thread for {port} has stopped unexpectedly.");
							}));

							// Optionally: Restart the thread or handle it based on your use case
							RestartLoggerThread(port);
						}
					}

					Thread.Sleep(2000); // Check every 2 seconds
				}
			}));

			watchdogThread.IsBackground = true; // Make it a background thread
			watchdogThread.Start();
		}

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
		

	}
}
