using System;
using System.Runtime.InteropServices;

namespace SerialLogAnalyzer.Helpers
{
	public static class ConsoleLogger
	{
		private static SerialPortReader _serialPortReader; // Changed to static for consistency
														   // Import kernel32.dll to allocate console
		[DllImport("kernel32.dll")]
		private static extern bool AllocConsole();

		// Method to create a console for a specific port
		public static void CreateConsoleForPort(string portName, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
		{
			AllocConsole();
			Console.Title = $"Console Output for {portName}";
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
			//Console.Clear();

			// Initialize the SerialPortReader for the specified port
			_serialPortReader = new SerialPortReader(portName, 115200); // Adjust baud rate as needed
			_serialPortReader.DataReceived += SerialPortReader_DataReceived;

			// Start reading from the serial port
			_serialPortReader.StartReading();
		}

		// Event handler for data received from the SerialPortReader
		private static void SerialPortReader_DataReceived(object sender, DataReceivedEventArgs e)
		{
			// Output the received data to the console
			OutputToConsole(e.Data);
		}

		// Method to output data to the console
		public static void OutputToConsole(string logData)
		{
			Console.WriteLine(logData);
		}

		// Method to stop the serial port reading
		public static void StopReading()
		{
			if (_serialPortReader != null)
			{
				_serialPortReader.StopReading();
				_serialPortReader = null; // Clean up the instance
			}
		}
	}
}
