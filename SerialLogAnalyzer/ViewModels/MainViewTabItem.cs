using System;
using System.Collections.Generic;
using System.IO.Ports;
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


		// Common button width and height
		private double buttonWidth = 120; // Set desired width
		private double buttonHeight = 30; // Set desired height
		private bool isAnalyzing = false;
		private List<string> selectedFiles = new List<string>();
		private bool isLogging;

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
				ItemsSource = SerialPort.GetPortNames(),
				Margin = new Thickness(10, 0, 0, 10) // Margin for left and bottom spacing
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
				Margin = new Thickness(10, 0, 0, 10) // Margin for left and bottom spacing
			};
			Grid.SetRow(baudRateComboBox, 2);
			Grid.SetColumn(baudRateComboBox, 1);
			grid.Children.Add(baudRateComboBox);

			// Create a StackPanel for buttons and align to the right
			StackPanel buttonPanel = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				HorizontalAlignment = HorizontalAlignment.Right,
				VerticalAlignment = VerticalAlignment.Bottom,
				Margin = new Thickness(0, 10, 0, 0) // Margin for top spacing
			};

			// Add Start Logging Button
			Button logButton = new Button
			{
				Content = "Start Logging",
				Width = buttonWidth,
				Height = buttonHeight,
				Margin = new Thickness(0, 0, 5, 0) // Right margin for spacing between buttons
			};
			logButton.Click += LogButton_Click; // Assuming you have a method to handle logging
			buttonPanel.Children.Add(logButton);

			// Add Stop Logging Button
			Button stopLoggingButton = new Button
			{
				Content = "Stop Logging",
				Width = buttonWidth,
				Height = buttonHeight,
				IsEnabled = false, // Initially disabled
				Margin = new Thickness(0, 0, 0, 0) // No margin
			};
			stopLoggingButton.Click += StopLoggingButton_Click; // Assuming you have a method to handle logging
			buttonPanel.Children.Add(stopLoggingButton);

			// Add the button panel to the grid
			Grid.SetRow(buttonPanel, 4); // Place in the third row
			Grid.SetColumnSpan(buttonPanel, 2); // Span across both columns
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
				foreach (var file in selectedFiles)
				{
					Console.WriteLine($"Analyzing {file}...");
				}

				/*
				// Simulate analysis completion after some time
				Task T = Task.Factory.StartNew(() =>
				{
					isAnalyzing = false;
					Dispatcher.Invoke(() =>
					{
						analyzeButton.IsEnabled = true; // Enable analyze button
						cancelButton.IsEnabled = false; // Disable cancel button
						Console.WriteLine("Analysis complete.");
					});
				});
				*/

			}
		}

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

		private void LogButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to start logging from the selected COM port
			string selectedPort = portComboBox.SelectedItem as string;

			// Add logic here to log the serial output from the selected port
			Console.WriteLine("Log serial output clicked.");
			if (!string.IsNullOrEmpty(selectedPort))
			{
				// Create a new logger tab for the selected port
				SerialLoggerTabItem loggerTab = new SerialLoggerTabItem(selectedPort);
				// Add the logger tab to the main tab control (assume it's named "mainTabControl")
				// mainTabControl.Items.Add(loggerTab);

				// Optionally: remove the port from the ComboBox
				portComboBox.Items.Remove(selectedPort);
				isLogging = true;
			}
			else
			{
				MessageBox.Show("Please select a COM port.");
			}

		}

		private void StopLoggingButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to stop logging (this could be implemented in the SerialLoggerTabItem)
			isLogging = false;
			UpdateLoggingButtons();
		}

		private void UpdateLoggingButtons()
		{
			logButton.IsEnabled = !isLogging;
			stopLoggingButton.IsEnabled = isLogging;
		}

	}
}
