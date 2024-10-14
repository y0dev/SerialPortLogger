using System.Windows;
using System.Windows.Controls;
using System.IO.Ports;

namespace SerialLogAnalyzer.ViewModels
{
	public class SerialLoggerTabItem : TabItem
	{
		private TextBox logTextBox;
		private Button startLoggerButton;
		private Button stopLoggerButton;
		private string portName;
		public bool isLogging { get; private set; }

		public SerialLoggerTabItem(string portName)
		{
			this.portName = portName;
			this.Header = portName; // Set the tab header to the COM port name
			this.isLogging = false;
			InitializeComponents();
		}

		private void InitializeComponents()
		{
			// Create a StackPanel to hold the TextBox and buttons
			StackPanel stackPanel = new StackPanel();

			// Create a large TextBox for logging
			logTextBox = new TextBox
			{
				AcceptsReturn = true,
				VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
				Height = 200,
				Margin = new Thickness(0, 0, 0, 10) // Margin for bottom spacing
			};
			stackPanel.Children.Add(logTextBox);

			// Create Start Logger Button
			startLoggerButton = new Button
			{
				Content = "Start Logger",
				Width = 120,
				Height = 30,
				Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
			};
			startLoggerButton.Click += StartLoggerButton_Click; // Event handler for starting logging
			stackPanel.Children.Add(startLoggerButton);

			// Create Stop Logger Button
			stopLoggerButton = new Button
			{
				Content = "Stop Logger",
				Width = 120,
				Height = 30,
				IsEnabled = false, // Initially disabled
				Margin = new Thickness(0, 0, 10, 0) // Margin for right spacing
			};
			stopLoggerButton.Click += StopLoggerButton_Click; // Event handler for stopping logging
			stackPanel.Children.Add(stopLoggerButton);

			// Set the StackPanel as the content of the TabItem
			this.Content = stackPanel;
		}

		private void StartLoggerButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to start logging from the COM port
			logTextBox.AppendText($"Started logging from {portName}...\n");
			this.StartLogging();

			// Enable/Disable buttons accordingly
			startLoggerButton.IsEnabled = false; // Disable Start Logger button
			stopLoggerButton.IsEnabled = true; // Enable Stop Logger button

			// Here you can add the logic to actually start the logging from the specified COM port
			// For example, initializing a SerialPort instance and reading data
		}

		private void StopLoggerButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to stop logging
			logTextBox.AppendText($"Stopped logging from {portName}.\n");
			this.StopLogging();
			// Enable/Disable buttons accordingly
			startLoggerButton.IsEnabled = true; // Enable Start Logger button
			stopLoggerButton.IsEnabled = false; // Disable Stop Logger button

			// Here you can add the logic to stop logging and close the SerialPort if applicable
		}

		public void StartLogging()
		{

			this.isLogging = true;
			// Your logging logic here...
		}

		public void StopLogging()
		{
			this.isLogging = false;
			// Logic to stop logging...
		}
	}
}
