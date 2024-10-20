using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SerialLogAnalyzer.Helpers
{
	/// <summary>
	/// Enumeration for log levels.
	/// </summary>
	public enum LogLevel
	{
		Info,    // Informational messages
		Warning, // Warning messages that may indicate a potential problem
		Error,   // Error messages that indicate something went wrong
		Debug    // Debug messages, useful for troubleshooting
	}

	/// <summary>
	/// Logger class for logging messages to specified log files in a hidden folder.
	/// Handles log file archiving if the log file already exists.
	/// </summary>
	public class Logger
	{
		private static readonly Dictionary<string, Logger> _loggers = new Dictionary<string, Logger>(); // Dictionary to hold loggers by file name
		private readonly string _logFilePath; // Path to the log file
		private const string LogFolderName = ".logs"; // Hidden folder name

		/// <summary>
		/// Private constructor to prevent instantiation from outside.
		/// Initializes the log file path and optionally archives existing log files.
		/// </summary>
		/// <param name="logFileName">The name of the log file.</param>
		/// <param name="archiveIfExists">Determines if the existing log file should be archived. Defaults to true.</param>
		private Logger(string logFileName, bool archiveIfExists = true)
		{
			string appDirectory = AppDomain.CurrentDomain.BaseDirectory; // Get the application base directory
			string logDirectory = Path.Combine(appDirectory, LogFolderName); // Path to the hidden log directory

			// Ensure the log directory exists and is hidden
			CreateHiddenDirectory(logDirectory);

			// Set the full log file path
			_logFilePath = Path.Combine(logDirectory, logFileName);

			// Archive or overwrite the existing log file based on the archiveIfExists parameter
			if (File.Exists(_logFilePath) && archiveIfExists)
			{
				ArchiveLogFile();
			}
		}

		/// <summary>
		/// Gets the Logger instance associated with the specified log file.
		/// </summary>
		/// <param name="logFileName">The name of the log file.</param>
		/// <param name="archiveIfExists">Indicates whether to archive the existing log file if it exists.</param>
		/// <returns>The Logger instance for the specified log file.</returns>
		public static Logger GetInstance(string logFileName, bool archiveIfExists = true)
		{
			if (!_loggers.ContainsKey(logFileName))
			{
				// Create a new logger instance if it doesn't exist
				_loggers[logFileName] = new Logger(logFileName, archiveIfExists);
			}

			return _loggers[logFileName];
		}

		/// <summary>
		/// Logs a message to the log file with a specified log level.
		/// </summary>
		/// <param name="message">The message to log.</param>
		/// <param name="logLevel">The log level of the message.</param>
		public void Log(string message, LogLevel logLevel)
		{
			// Format the log entry with the current timestamp and log level
			string logEntry = $"{DateTime.Now} [{logLevel}]: {message}";

			// Append the log entry to the log file
			File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
		}

		/// <summary>
		/// Clears the log file by overwriting it with an empty string.
		/// </summary>
		public void ClearLog()
		{
			File.WriteAllText(_logFilePath, string.Empty); // Clear log file content
		}

		/// <summary>
		/// Archives the existing log file by renaming it with a timestamp.
		/// </summary>
		private void ArchiveLogFile()
		{
			string directory = Path.GetDirectoryName(_logFilePath);
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(_logFilePath);
			string fileExtension = Path.GetExtension(_logFilePath);

			// Create a new file name with a timestamp to archive the existing log
			string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
			string archivedLogFileName = $"{fileNameWithoutExtension}_{timestamp}{fileExtension}";
			string archivedLogFilePath = Path.Combine(directory, archivedLogFileName);

			// Rename (archive) the existing log file
			File.Move(_logFilePath, archivedLogFilePath);
		}

		/// <summary>
		/// Creates a hidden directory if it does not already exist.
		/// </summary>
		/// <param name="directoryPath">The path to the directory.</param>
		private void CreateHiddenDirectory(string directoryPath)
		{
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);

				// Set the directory as hidden on Windows
				File.SetAttributes(directoryPath, FileAttributes.Hidden);
			}
		}
	}
}
