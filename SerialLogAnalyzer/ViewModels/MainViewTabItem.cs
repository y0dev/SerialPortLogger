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
		private Button logButton;
		private Button stopLoggingButton;

		// Fields for ComboBox
		private ComboBox portComboBox = new ComboBox();
		private ComboBox baudRateComboBox = new ComboBox();

		private bool isAnalyzing = false;
		private List<string> selectedFiles = new List<string>();

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

			// Define rows for the grid
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the header label
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the ComboBoxes
			grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto }); // Row for the Buttons
			grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) }); // Row for the ListView

			// Define columns for ComboBoxes and Labels
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto }); // Column for Labels
			grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Column for ComboBoxes

			// Add a Label for the header
			var headerLabel = new Label
			{
				Content = $"Serial {TabHeader}",
				FontSize = 16,
				Margin = new Thickness(0, 0, 0, 10) // Margin for bottom spacing
			};
			Grid.SetRow(headerLabel, 0); // Place the label in the first row
			Grid.SetColumnSpan(headerLabel, 2); // Span across both columns
			grid.Children.Add(headerLabel);

			// Common button width and height
			double buttonWidth = 120; // Set desired width
			double buttonHeight = 30; // Set desired height

			// Add UI for the Logger tab
			if (this.TabHeader == "Logger")
			{
				Console.WriteLine("Logger tab content");

				// Add Label for Serial Port
				var portLabel = new Label {
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

				// Add Log Button to log serial output
				Button logButton = new Button {
					Content = "Log Serial Output",
					Width = buttonWidth,
					Height = buttonHeight,
					Margin = new Thickness(0, 10, 0, 0) // Margin for top spacing
				};
				logButton.Click += LogButton_Click; // Assuming you have a method to handle logging
				Grid.SetRow(logButton, 3);
				Grid.SetColumnSpan(logButton, 2); // Span across both columns
				grid.Children.Add(logButton);

				// Add Stop Logging Button
				Button stopLoggingButton = new Button
				{
					Content = "Stop Logging",
					Width = buttonWidth,
					Height = buttonHeight,
					IsEnabled = false, // Initially disabled
					Margin = new Thickness(5) // Margin for spacing
				};
				stopLoggingButton.Click += StopLoggingButton_Click; // Add a method to handle stopping logging
				Grid.SetRow(stopLoggingButton, 3);
				Grid.SetColumn(stopLoggingButton, 2); // Place next to Log button
				grid.Children.Add(stopLoggingButton);


				// Store the button references for later use
				this.logButton = logButton;
				this.stopLoggingButton = stopLoggingButton;
			}
			else if (this.TabHeader == "Analyzer")
			{
				Console.WriteLine("Analyzer tab content");

				// Add ListView for displaying selected files
				var listView = new ListView();
				listView.ItemsSource = selectedFiles;
				Grid.SetRow(listView, 3); // Place the ListView in the last row
				Grid.SetColumnSpan(listView, 2); // Span across both columns
				grid.Children.Add(listView);

				// Add Buttons (Browse, Analyze, Cancel)
				StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal };

				// Browse Button
				Button browseButton = new Button
				{
					Content = "Browse",
					Width = buttonWidth,
					Height = buttonHeight,
					Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
				};

				browseButton.Click += BrowseButton_Click;
				buttonPanel.Children.Add(browseButton);

				// Analyze Button
				Button analyzeButton = new Button {
					Content = "Analyze",
					IsEnabled = !isAnalyzing,
					Width = buttonWidth,
					Height = buttonHeight,
					Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
				};
				analyzeButton.Click += AnalyzeButton_Click;
				buttonPanel.Children.Add(analyzeButton);

				// Cancel Button (Initially Disabled)
				Button cancelButton = new Button {
					Content = "Cancel",
					IsEnabled = isAnalyzing,
					Width = buttonWidth,
					Height = buttonHeight,
					Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
				};
				cancelButton.Click += CancelButton_Click;
				buttonPanel.Children.Add(cancelButton);

				// Add Button Panel to grid
				Grid.SetRow(buttonPanel, 2); // Place the button panel in the third row
				Grid.SetColumnSpan(buttonPanel, 2); // Span across both columns
				grid.Children.Add(buttonPanel);

				// Store the button references for later use
				this.analyzeButton = analyzeButton;
				this.cancelButton = cancelButton;
			}

			// Set grid as the content of the TabItem
			this.Content = grid;
		} // End of UpdateUI()

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
			// Add logic here to log the serial output from the selected port
			Console.WriteLine("Log serial output clicked.");

			// Enable/Disable buttons accordingly
			logButton.IsEnabled = false; // Disable Log button
			stopLoggingButton.IsEnabled = true; // Enable Stop Logging button
		}

		private void StopLoggingButton_Click(object sender, RoutedEventArgs e)
		{
			// Add logic here to stop logging the serial output
			Console.WriteLine("Stop logging clicked.");

			// Enable/Disable buttons accordingly
			logButton.IsEnabled = true; // Enable Log button
			stopLoggingButton.IsEnabled = false; // Disable Stop Logging button
		}
	}
}
