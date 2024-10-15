using SerialLogAnalyzer.Helpers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SerialLogAnalyzer.ViewModels
{
	class MainViewTabItem : TabItem
	{
		// Custom properties
		public static readonly DependencyProperty TabHeaderProperty =
			DependencyProperty.Register("TabHeader", typeof(string), typeof(MainViewTabItem),
			new PropertyMetadata(string.Empty, OnTabHeaderChanged));

		public ObservableCollection<string> AvailablePorts { get; private set; }

		public string TabHeader
		{
			get { return (string)GetValue(TabHeaderProperty); }
			set { SetValue(TabHeaderProperty, value); }
		}
		

		// Fields for buttons
		private Button analyzeButton;
		private Button cancelButton;
		private TextBox directoryTextBox;
		private Button logButton;
		private Button stopLoggingButton;

		// Fields for ComboBox
		private ComboBox portComboBox = new ComboBox();
		private ComboBox baudRateComboBox = new ComboBox();

		private TabControl serialTabControl;
		private CheckBox consoleOutputCheckBox;
		private ListView consoleOutputListView;

		// Common button width and height
		private double buttonWidth = 120; // Set desired width
		private double buttonHeight = 30; // Set desired height
		private bool isAnalyzing = false;
		private List<string> selectedFiles = new List<string>();
		private bool isLogging;

		// Dictionary to keep track of the logger threads for each port
		private Dictionary<string, Thread> loggerThreads = new Dictionary<string, Thread>();

		// Watchdog thread
		private Thread watchdogThread;
		private bool watchdogRunning = true;

		// This method will be triggered when the TabHeader changes
		private static void OnTabHeaderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var tabItem = d as MainViewTabItem;
			if (tabItem != null)
			{
				tabItem.UpdateUI();
			}
		}

		// Override OnInitialized to setup initial content
		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			AvailablePorts = new ObservableCollection<string>(SerialPort.GetPortNames());
			UpdateUI(); // Call the method to setup the content
		}

		// Dynamically update UI components based on TabHeader
		private void UpdateUI()
		{
			// Create a Grid as content
			Grid grid = new Grid();

			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the header label


			// Add a Label for the header
			var headerLabel = new Label
			{
				Content = $"Serial {TabHeader}",
				FontSize = 16,
				Margin = new Thickness(10, 0, 0, 10) // Margin for left and bottom spacing
			};
			Grid.SetRow(headerLabel, 0); // Place the label in the first row
			Grid.SetColumnSpan(headerLabel, 2); // Span across both columns
			grid.Children.Add(headerLabel);


			// Add UI for the Logger tab
			if (this.TabHeader == "Logger")
			{
				Console.WriteLine("Logger tab content");
				ConfigureLoggerUI(grid);
			}
			else if (this.TabHeader == "Analyzer")
			{
				Console.WriteLine("Analyzer tab content");
				ConfigureAnalyzerUI(grid);
			}

			// Set grid as the content of the TabItem
			this.Content = grid;
		} // End of UpdateUI()

		// Helper method to configure Logger UI
		private void ConfigureLoggerUI(Grid grid)
		{
			// Create the rows for the grid
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for Serial Port
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for Baud Rate
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for Buttons
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Fill remaining space


			// Define columns for ComboBoxes and Labels
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Column for Labels
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Column for ComboBoxes
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Column for Checkbox

			// Add Label for Serial Port
			var portLabel = new Label
			{
				Content = "Serial Port:",
				Margin = new Thickness(0, 0, 10, 0)
			};
			Grid.SetRow(portLabel, 1);
			Grid.SetColumn(portLabel, 0);
			grid.Children.Add(portLabel);

			// Add ComboBox for Serial Port selection
			portComboBox = new ComboBox
			{
				ItemsSource = AvailablePorts,
				Margin = new Thickness(10, 0, 10, 10) // Margin for left and bottom spacing
			};
			Grid.SetRow(portComboBox, 1);
			Grid.SetColumn(portComboBox, 1);
			grid.Children.Add(portComboBox);

			// Add Label for Baud Rate
			var baudRateLabel = new Label { Content = "Baud Rate:" };
			Grid.SetRow(baudRateLabel, 2);
			Grid.SetColumn(baudRateLabel, 0);
			grid.Children.Add(baudRateLabel);

			// Add ComboBox for Baud Rate selection
			baudRateComboBox = new ComboBox
			{
				ItemsSource = new[] { 9600, 19200, 38400, 57600, 115200 },
				SelectedItem = 115200, // Set a default value
				Margin = new Thickness(10, 0, 10, 10) // Margin for left and bottom spacing
			};

			Grid.SetRow(baudRateComboBox, 2);
			Grid.SetColumn(baudRateComboBox, 1);
			grid.Children.Add(baudRateComboBox);



			// Add CheckBox for a feature, e.g., "Enable Auto-Scroll"
			var checkBoxStackPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Margin = new Thickness(10, 0, 10, 0) // Add spacing around the checkbox
			};

			// Label for the CheckBox
			var checkBoxLabel = new Label
			{
				Content = "Console Output:",
				Margin = new Thickness(0, 0, 5, 0) // Add a small space between label and checkbox
			};
			checkBoxStackPanel.Children.Add(checkBoxLabel);

			// The CheckBox itself
			CheckBox consoleOutputCheckBox = new CheckBox
			{
				IsChecked = true, // Default to checked
				VerticalAlignment = VerticalAlignment.Center
			};
			checkBoxStackPanel.Children.Add(consoleOutputCheckBox);

			// Add the StackPanel to the Grid (3rd column)
			Grid.SetRow(checkBoxStackPanel, 1);
			Grid.SetColumn(checkBoxStackPanel, 2);
			grid.Children.Add(checkBoxStackPanel);

			// Add dynamic content: ListView or TabControl based on the CheckBox state
			UIElement dynamicContent = null;

			void UpdateDynamicContent()
			{
				if (dynamicContent != null)
				{
					grid.Children.Remove(dynamicContent); // Remove existing UI element
				}

				if (consoleOutputCheckBox.IsChecked == true)
				{
					// Add ListView for Console Output
					ListView consoleOutputListView = new ListView
					{
						Height = 235,
						Margin = new Thickness(10, 0, 10, 5) // Margin for left and bottom spacing
					};
					Grid.SetRow(consoleOutputListView, 3);
					Grid.SetColumnSpan(consoleOutputListView, 3); // Span across both columns
					dynamicContent = consoleOutputListView;

					this.consoleOutputCheckBox = consoleOutputCheckBox;
					this.consoleOutputListView = consoleOutputListView;
				}
				else
				{
					// Add TabControl for Serial Port Loggers
					TabControl serialTabControl = new TabControl
					{
						Height = 230,
						Margin = new Thickness(10, 0, 10, 5) // Margin for left and bottom spacing
					};
					Grid.SetRow(serialTabControl, 3);
					Grid.SetColumnSpan(serialTabControl, 3); // Span across both columns
					dynamicContent = serialTabControl;
				}

				grid.Children.Add(dynamicContent); // Add the new UI element to the grid
			}

			// Initial UI setup based on the checkbox state
			UpdateDynamicContent();

			// Listen to checkbox state change to update the UI
			consoleOutputCheckBox.Checked += (s, e) => UpdateDynamicContent();
			consoleOutputCheckBox.Unchecked += (s, e) => UpdateDynamicContent();

			// Create a StackPanel for buttons and align to the right
			StackPanel buttonPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0, 5, 0, 0) // Margin for top spacing
			};

			// Add Start Logging Button
			Button logButton = new Button
			{
				Content = "Create Logger",
				Width = buttonWidth,
				Height = buttonHeight,
				Margin = new Thickness(0, 0, 5, 0) // Right margin for spacing between buttons
			};
			logButton.Click += CreateLoggerButton_Click; // Assuming you have a method to handle logging
			buttonPanel.Children.Add(logButton);

			// Add Stop Logging Button
			Button stopLoggingButton = new Button
			{
				Content = "Stop All Loggers",
				Width = buttonWidth,
				Height = buttonHeight,
				IsEnabled = false, // Initially disabled
				Margin = new Thickness(0, 0, 0, 0) // No margin
			};
			stopLoggingButton.Click += StopLoggingButton_Click; // Assuming you have a method to handle logging
			buttonPanel.Children.Add(stopLoggingButton);

			// Add the button panel to the grid
			Grid.SetRow(buttonPanel, 4); // Place in the third row
			Grid.SetColumnSpan(buttonPanel, 3); // Span across both columns
			grid.Children.Add(buttonPanel);

			// Store the button references for later use
			this.logButton = logButton;
			this.stopLoggingButton = stopLoggingButton;
		} // End of ConfigureLoggerUI()

		// Helper method to configure Analyzer UI
		private void ConfigureAnalyzerUI(Grid grid)
		{
			// Define rows for the grid
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the TextBox and Browse Button
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the ListView
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the Buttons

			// Create a StackPanel for the TextBox and Browse Button
			StackPanel browsePanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Left,
				VerticalAlignment = VerticalAlignment.Top,
				Margin = new Thickness(10, 0, 10, 10) // Margin for bottom spacing
			};

			// Add TextBox for displaying the current directory
			TextBox directoryTextBox = new TextBox
			{
				Width = 300, // Set width as needed
				Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
			};
			browsePanel.Children.Add(directoryTextBox);

			// Browse Button next to the TextBox
			Button browseButton = new Button
			{
				Content = "Browse",
				Width = buttonWidth,
				Height = buttonHeight,
				Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
			};
			browseButton.Click += BrowseButton_Click; // Add click event handler
			browsePanel.Children.Add(browseButton);

			// Add the StackPanel to the grid
			Grid.SetRow(browsePanel, 1);
			Grid.SetColumnSpan(browsePanel, 2); // Span across both columns
			grid.Children.Add(browsePanel);

			// Add ListView for displaying selected files
			var listView = new ListView
			{
				ItemsSource = selectedFiles,
				Height = 250,
				Margin = new Thickness(10, 0, 10, 5)
			};
			Grid.SetRow(listView, 2); // Place the ListView in the second row
			Grid.SetColumnSpan(listView, 2); // Span across both columns
			grid.Children.Add(listView);

			// Create a StackPanel for buttons and align to the right
			StackPanel buttonPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0, 10, 0, 0) // Margin for top spacing
			};

			// Analyze Button
			Button analyzeButton = new Button
			{
				Content = "Analyze",
				IsEnabled = !isAnalyzing,
				Width = buttonWidth,
				Height = buttonHeight,
				Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
			};
			analyzeButton.Click += AnalyzeButton_Click;
			buttonPanel.Children.Add(analyzeButton);

			// Cancel Button (Initially Disabled)
			Button cancelButton = new Button
			{
				Content = "Cancel",
				IsEnabled = isAnalyzing,
				Width = buttonWidth,
				Height = buttonHeight,
				Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
			};
			cancelButton.Click += CancelButton_Click;
			buttonPanel.Children.Add(cancelButton);

			// Add Button Panel to grid
			Grid.SetRow(buttonPanel, 3); // Place the button panel in the third row
			Grid.SetColumnSpan(buttonPanel, 2); // Span across both columns
			grid.Children.Add(buttonPanel);

			// Store the button references for later use
			this.analyzeButton = analyzeButton;
			this.cancelButton = cancelButton;
			this.directoryTextBox = directoryTextBox; // Store a reference to the directory TextBox
		} // End of ConfigureAnalyzerUI()

		// Browse Button Click - To open file dialog and select files
		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			// Open a file dialog to select files
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Multiselect = true; // Allow multiple file selection
			if (dlg.ShowDialog() == true)
			{
				selectedFiles.AddRange(dlg.FileNames); // Add selected files to the list
				UpdateUI(); // Refresh the UI to show selected files
			}
		}

		// Analyze Button Click - To start analysis
		private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedFiles.Count > 0)
			{
				isAnalyzing = true;
				analyzeButton.IsEnabled = false; // Disable analyze button
				cancelButton.IsEnabled = true; // Enable cancel button

				// Simulate analysis process (replace with actual logic)
				Console.WriteLine("Analyzing files...");

				// Start a new thread for file analysis
				Thread analysisThread = new Thread(() =>
				{
					foreach (var file in selectedFiles)
					{
						Console.WriteLine($"Analyzing {file}...");
						Thread.Sleep(1000); // Simulate time taken for analyzing each file
					}

					// Analysis complete, update UI
					isAnalyzing = false;

					// Use Dispatcher to update UI controls
					Dispatcher.Invoke(new Action(delegate
					{
						analyzeButton.IsEnabled = true; // Enable analyze button
						cancelButton.IsEnabled = false; // Disable cancel button
						Console.WriteLine("Analysis complete.");
					}));
				});

				analysisThread.IsBackground = true; // Make the thread a background thread
				analysisThread.Start(); // Start the analysis thread
			}
		} // End of AnalyzeButton_Click()


		// Cancel Button Click - To stop the analysis process
		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (isAnalyzing)
			{
				Console.WriteLine("Cancelling analysis...");
				isAnalyzing = false;
				analyzeButton.IsEnabled = true; // Re-enable analyze button
				cancelButton.IsEnabled = false; // Disable cancel button
			}
		}

		private void CreateLoggerButton_Click(object sender, RoutedEventArgs e)
		{
			string selectedPort = portComboBox.SelectedItem as string;

			if (!string.IsNullOrEmpty(selectedPort))
			{
				if (consoleOutputCheckBox.IsChecked == true)
				{
					// Create a console for this port
					ConsoleLogger.CreateConsoleForPort(selectedPort, ConsoleColor.Green, ConsoleColor.Black);
				}
				else
				{
					// Create a new logger tab for the selected port
					SerialLoggerTabItem loggerTab = new SerialLoggerTabItem(selectedPort);
					serialTabControl.Items.Add(loggerTab);
				}

				// Start the logger thread
				Thread loggerThread = new Thread(delegate ()
				{
					while (isLogging)
					{
						string logData = $"Data from {selectedPort} at {DateTime.Now}";

						// Check if output should be directed to the console or the tab
						if (consoleOutputCheckBox.IsChecked == true)
						{
							// Update to call OutputToConsole with a single argument
							ConsoleLogger.OutputToConsole(logData);
						}
						else
						{
							// Find the logger tab for this port
							SerialLoggerTabItem loggerTab = null;

							// Iterate through tab items to find the right one
							foreach (var item in serialTabControl.Items)
							{
								loggerTab = item as SerialLoggerTabItem;
								if (loggerTab != null && loggerTab.portName == selectedPort)
								{
									break;
								}
							}

							// If the tab is found, append the log data to it
							if (loggerTab != null)
							{
								// Use Dispatcher to update UI element
								Application.Current.Dispatcher.Invoke(new Action(() =>
								{
									loggerTab.AppendLog(logData);
								}));
							}
						}

						Thread.Sleep(1000); // Simulate data logging interval
					}
				});

				loggerThread.IsBackground = true;
				loggerThread.Start();

				// Add the thread to the dictionary for tracking
				loggerThreads[selectedPort] = loggerThread;

				// Start the watchdog if it's not already running
				if (watchdogThread == null)
				{
					StartWatchdog();
				}

				// Remove the selected port from the available ports
				AvailablePorts.Remove(selectedPort);

				isLogging = true;
				stopLoggingButton.IsEnabled = true;
				logButton.IsEnabled = AvailablePorts.Count > 0;
			}
			else
			{
				MessageBox.Show("Please select a COM port.");
			}
		}


		private void StopLoggingButton_Click(object sender, RoutedEventArgs e)
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

			stopLoggingButton.IsEnabled = false;
			UpdateLoggingButtons();
		}

		private void UpdateLoggingButtons()
		{
			logButton.IsEnabled = !isLogging;
			stopLoggingButton.IsEnabled = isLogging;
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
						ConsoleLogger.OutputToConsole(logData);
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
			foreach (TabItem item in serialTabControl.Items)
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
