using System;
using System.Diagnostics;

namespace SerialLogAnalyzer.Helpers
{
	public class ConsoleLogger
	{
		private static SerialPortReader _serialPortReader; // Changed to static for consistency
		private bool isConsoleClosed = false;
		private Process externalConsoleProcess; // Track the process of the external console
		private string externalAppPath = Properties.Resources.SCRIPTS_DIR_PATH;

		public ConsoleLogger(string consoleTitle, string portName, int baudRate, string schemeName, string baseDirectory, string logFileName)
		{
			// Launch the external application (e.g., cmd.exe or custom logger)
			StartExternalConsole(consoleTitle, portName, baudRate, schemeName, baseDirectory, logFileName);
		}

		// Method to launch an external console (e.g., cmd.exe) for each logger
		private void StartExternalConsole(string consoleTitle, string portName, int baudRate, string schemeName, string baseDirectory, string logFileName)
		{
			try
			{
				externalConsoleProcess = new Process
				{
					StartInfo = new ProcessStartInfo
					{
						FileName = $"{externalAppPath}\\COM_Port_Logger.exe", // Path to cmd.exe or any external console application
						Arguments = $"{baseDirectory} {logFileName} {portName} {baudRate} {schemeName} \"{consoleTitle}\"",
						UseShellExecute = true,			// Ensures the external app launches with a GUI
						CreateNoWindow = false,			// Optional: set to false to ensure the window is visible
					}
				};
				externalConsoleProcess.Start();
			}
			catch (Exception ex)
			{
				Console.WriteLine($"Failed to launch external console: {ex.Message}");
			}
		}

		// Public read-only property to expose the value outside the class
		public bool IsConsoleClosed
		{
			get { return isConsoleClosed; }
		}

		// Event handler for data received from the SerialPortReader
		private void SerialPortReader_DataReceived(object sender, DataReceivedEventArgs e)
		{
			// Output the received data to the external application or log it
			OutputToConsole(e.Data);
		}

		// Method to output data to the console (can be adapted to send data to the external process)
		public void OutputToConsole(string logData)
		{
			if (!isConsoleClosed)
			{
				// You could pass this data to the external console using methods like piping or files
				Console.WriteLine(logData); // For now, simply log it to the original process console
			}
		}

		// Method to stop the serial port reading
		public void StopReading()
		{
			// You can stop the external process if needed
			if (externalConsoleProcess != null && !externalConsoleProcess.HasExited)
			{
				externalConsoleProcess.Kill();
			}
		}
	}
}
