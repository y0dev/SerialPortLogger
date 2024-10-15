using System;
using System.Runtime.InteropServices;

namespace SerialLogAnalyzer.Helpers
{
	public class ConsoleLogger
	{
		private static SerialPortReader _serialPortReader; // Changed to static for consistency
		private bool isConsoleClosed = false;

		// DllImport for handling console events
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate handler, bool add);

		private delegate bool ConsoleCtrlDelegate(CtrlTypes ctrlType);

		// Console control events
		private enum CtrlTypes
		{
			CTRL_C_EVENT = 0,
			CTRL_CLOSE_EVENT = 2,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT = 6
		}

		public ConsoleLogger(string consoleTitle, string portName, ConsoleColor foregroundColor, ConsoleColor backgroundColor)
		{
			AllocConsole();
			Console.Title = consoleTitle;
			Console.ForegroundColor = foregroundColor;
			Console.BackgroundColor = backgroundColor;
			//Console.Clear();

			// Add a handler to prevent the console from closing the entire application
			SetConsoleCtrlHandler(new ConsoleCtrlDelegate(ConsoleCtrlHandler), true);

			// Initialize the SerialPortReader for the specified port
			// _serialPortReader = new SerialPortReader(portName, 115200); // Adjust baud rate as needed
			// _serialPortReader.DataReceived += SerialPortReader_DataReceived;

			// Start reading from the serial port
			// _serialPortReader.StartReading();
		}

		// Public read-only property to expose the value outside the class
		public bool IsConsoleClosed
		{
			get { return isConsoleClosed; }
		}

		// Event handler for data received from the SerialPortReader
		private void SerialPortReader_DataReceived(object sender, DataReceivedEventArgs e)
		{
			// Output the received data to the console
			OutputToConsole(e.Data);
		}

		// Method to output data to the console
		public void OutputToConsole(string logData)
		{
			if (!isConsoleClosed)
			{
				Console.WriteLine(logData);
			}
		}

		// Method to stop the serial port reading
		public void StopReading()
		{
			/*
			if (_serialPortReader != null)
			{
				_serialPortReader.StopReading();
				_serialPortReader = null; // Clean up the instance
			}
			*/
		}

		// Allocates a new console
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool AllocConsole();

		// Console close event handler
		private bool ConsoleCtrlHandler(CtrlTypes ctrlType)
		{
			if (ctrlType == CtrlTypes.CTRL_CLOSE_EVENT)
			{
				// Just hide the console when it's closed, but don't exit the app
				isConsoleClosed = true;
				FreeConsole();
				return true; // Suppress the default behavior (which would terminate the app)
			}
			return false; // Let the default behavior happen for other control types
		}

		// Frees the console so the app continues running
		[DllImport("kernel32.dll", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeConsole();
	}
}
