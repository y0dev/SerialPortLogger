using System;
using System.Windows;
using System.Windows.Controls;
using SerialLogAnalyzer.Helpers;

namespace SerialLogAnalyzer.ViewModels
{
	public class SerialLoggerTabItem : TabItem
	{
		private TextBox logTextBox;
		private Button startLoggerButton;
		private Button stopLoggerButton;
		private SerialPortReader _serialPortReader;
		public bool isLogging { get; private set; }

		public string portName;

		public SerialLoggerTabItem(string portName)
		{
			this.portName = portName;
			this.isLogging = false;

			Console.WriteLine(portName);

			// Create the Close Button
			Button closeButton = new Button
			{
				Content = "X",
				Width = 20,
				Margin = new Thickness(5)
			};
			closeButton.Click += CloseButton_Click;

			// Set the tab header to the COM port name with close button
			this.Header = new StackPanel
			{
				Orientation = Orientation.Horizontal,
				Children = { new TextBlock { Text = portName }, closeButton }
			};

			InitializeComponents();
			InitializeSerialPortReader();
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

		private void InitializeSerialPortReader()
		{
			// Initialize the SerialPortReader with the specified port and baud rate
			_serialPortReader = new SerialPortReader(portName, 115200); // Adjust baud rate if necessary
			_serialPortReader.DataReceived += new EventHandler<DataReceivedEventArgs>(SerialPortReader_DataReceived);
		}

		private void SerialPortReader_DataReceived(object sender, DataReceivedEventArgs e)
		{
			// Append the received data to the logTextBox
			AppendLog(e.Data);
		}

		public void AppendLog(string data)
		{
			// Ensure the UI is updated on the UI thread
			if (logTextBox.Dispatcher.CheckAccess())
			{
				logTextBox.AppendText(data + Environment.NewLine);
				logTextBox.ScrollToEnd(); // Auto-scroll to the latest log
			}
			else
			{
				logTextBox.Dispatcher.Invoke(new Action(() => AppendLog(data)));
			}
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to close the tab
			CloseTab();
		}

		public void CloseTab()
		{
			// Stop logging and close the SerialPortReader if it's running
			StopLogging();

			// Logic to remove this tab from its parent
			var parentTabControl = GetParentTabControl();
			if (parentTabControl != null)
			{
				parentTabControl.Items.Remove(this);
			}
		}

		private TabControl GetParentTabControl()
		{
			// Get the parent TabControl
			return this.Parent as TabControl;
		}

		private void StartLoggerButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to start logging from the COM port
			logTextBox.AppendText($"Started logging from {portName}...\n");
			this.StartLogging();

			// Enable/Disable buttons accordingly
			startLoggerButton.IsEnabled = false; // Disable Start Logger button
			stopLoggerButton.IsEnabled = true; // Enable Stop Logger button

			// Start reading from the serial port
			_serialPortReader.StartReading();
		}

		private void StopLoggerButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to stop logging
			logTextBox.AppendText($"Stopped logging from {portName}.\n");
			this.StopLogging();

			// Enable/Disable buttons accordingly
			startLoggerButton.IsEnabled = true; // Enable Start Logger button
			stopLoggerButton.IsEnabled = false; // Disable Stop Logger button

			// Stop reading from the serial port
			_serialPortReader.StopReading();
		}

		public void StartLogging()
		{
			this.isLogging = true;
			// Your logging logic can be expanded here...
		}

		public void StopLogging()
		{
			this.isLogging = false;
			// Logic to stop logging...
		}
	}
}
