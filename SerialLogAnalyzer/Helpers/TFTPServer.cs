using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Controls;

namespace SerialLogAnalyzer.Helpers
{
	// Public class that represents the TFTP Server
	public class TftpServer
	{
		private UdpClient udpServer;
		private IPEndPoint localEP;
		private IPEndPoint remoteEP;
		private bool isRunning;
		private string baseDirectory;
		private Thread serverThread;
		private int serverPort;

		private ListView tftpServerListView;

		private Logger _logger;
		public int FilesTransfered;

		public TftpServer(string ipAddress, int port, string baseDirectory, Logger logger, ListView listView)
		{
			// Bind to the specific IP address and port
			this.serverPort = port;
			localEP = new IPEndPoint(IPAddress.Parse(ipAddress), port);
			udpServer = new UdpClient(localEP);
			remoteEP = new IPEndPoint(IPAddress.Any, 0); // Will be set when we receive a request
			isRunning = false;
			this.baseDirectory = baseDirectory;
			this._logger = logger;
			this.tftpServerListView = listView;
			FilesTransfered = 0;
		}

		// Add a new log entry to the ListView
		public void AddLogEntry(string message)
		{
			tftpServerListView.Items.Add($"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}: {message}"); // Add the log entry to the ListView
		} // End of AddLogEntry()

		// Method to start the TFTP server
		public void Start()
		{
			if (!isRunning)
			{
				isRunning = true;
				serverThread = new Thread(ServerLoop);
				serverThread.Start();
				_logger.Log($"Starting TFTP Server on {localEP.Address}:{localEP.Port}...", LogLevel.Debug);
			}
		}

		// Server loop that listens for requests
		private void ServerLoop()
		{
			while (isRunning)
			{
				try
				{

					_logger.Log("Waiting for incoming TFTP requests...", LogLevel.Debug);
					byte[] request = udpServer.Receive(ref remoteEP);

					if (!isRunning) break; // Exit if stop has been called

					// Determine if it's a Read (RRQ) or Write (WRQ) request
					if (request[1] == 1)
					{
						HandleReadRequest(request);
					}
					else if (request[1] == 2)
					{
						HandleWriteRequest(request);
					}
				}
				catch (SocketException ex)
				{
					if (isRunning) // Only log if the server is still running
						_logger.Log($"Socket exception: {ex.Message}", LogLevel.Error);
				}
				catch (Exception ex)
				{
					_logger.Log($"Error: {ex.Message}", LogLevel.Error);
				}
			}
		}

		// Method to stop the TFTP server
		public void Stop()
		{
			if (isRunning)
			{
				isRunning = false;
				udpServer.Close(); // Safely close the UDP listener
				_logger.Log("Stopping TFTP Server...", LogLevel.Info);
				serverThread.Join(); // Wait for the server loop to finish
				_logger.Log("TFTP Server stopped.", LogLevel.Info);
			}
		}

		private void HandleReadRequest(byte[] request)
		{
			string fileName = ParseFileNameFromRequest(request);
			_logger.Log($"RRQ received for file: {fileName}", LogLevel.Info);

			string filePath = Path.Combine(baseDirectory, fileName);

			if (File.Exists(filePath))
			{
				_logger.Log($"Sending file: {filePath}", LogLevel.Info);
				SendFile(filePath);
			}
			else
			{
				_logger.Log($"File not found: {filePath}", LogLevel.Info);
				SendError("File not found.");
			}
		}

		private void HandleWriteRequest(byte[] request)
		{
			string fileName = ParseFileNameFromRequest(request);
			_logger.Log($"WRQ received for file: {fileName}", LogLevel.Info);

			string filePath = Path.Combine(baseDirectory, fileName);
			string directoryPath = Path.GetDirectoryName(filePath);

			// Create directory if it doesn't exist
			if (!Directory.Exists(directoryPath))
			{
				Directory.CreateDirectory(directoryPath);
				_logger.Log($"Directory created: {directoryPath}", LogLevel.Info);
			}

			ReceiveFile(filePath);
		}

		private string ParseFileNameFromRequest(byte[] request)
		{
			int endIndex = Array.IndexOf(request, (byte)0, 2);
			return Encoding.ASCII.GetString(request, 2, endIndex - 2);
		}

		private void SendFile(string filePath)
		{
			try
			{
				byte[] fileData = File.ReadAllBytes(filePath);
				int block = 1;
				int bytesRead = 0;
				int maxRetries = 5;
				int timeoutMs = 5000;

				while (bytesRead < fileData.Length)
				{
					int dataSize = Math.Min(512, fileData.Length - bytesRead);
					byte[] dataPacket = CreateDataPacket(block, fileData, bytesRead, dataSize);
					
					bool ackReceived = false;
					int retryCount = 0;

					while (!ackReceived && retryCount < maxRetries)
					{
						// Send data packet
						udpServer.Send(dataPacket, dataPacket.Length, remoteEP);
						_logger.Log($"Sent data block {block}, size: {dataSize}", LogLevel.Debug);

						// Wait for ACK with timeout
						try
						{
							udpServer.Client.ReceiveTimeout = timeoutMs;
							byte[] ackResponse = udpServer.Receive(ref remoteEP);
							
							if (ackResponse.Length >= 4 && ackResponse[0] == 0 && ackResponse[1] == 4)
							{
								int ackBlock = (ackResponse[2] << 8) | ackResponse[3];
								if (ackBlock == block)
								{
									ackReceived = true;
									_logger.Log($"Received ACK for block {block}", LogLevel.Debug);
								}
								else
								{
									_logger.Log($"Received ACK for wrong block {ackBlock}, expected {block}", LogLevel.Warning);
								}
							}
						}
						catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
						{
							retryCount++;
							_logger.Log($"Timeout waiting for ACK block {block}, retry {retryCount}/{maxRetries}", LogLevel.Warning);
						}
					}

					if (!ackReceived)
					{
						_logger.Log($"Failed to receive ACK for block {block} after {maxRetries} retries", LogLevel.Error);
						SendError("Transfer failed - no ACK received");
						return;
					}

					bytesRead += dataSize;
					block++;
				}

				FilesTransfered += 1;
				_logger.Log($"File transfer complete: {filePath}", LogLevel.Info);
			}
			catch (Exception ex)
			{
				_logger.Log($"Error sending file {filePath}: {ex.Message}", LogLevel.Error);
				SendError("Internal server error");
			}
		}

		private void ReceiveFile(string filePath)
		{
			try
			{
				using (FileStream fs = new FileStream(filePath, FileMode.Create))
				{
					int block = 0; // Start with block 0 for WRQ ACK
					int maxRetries = 5;
					int timeoutMs = 5000;

					// Send initial ACK for WRQ
					byte[] initialAck = CreateAckPacket(block);
					udpServer.Send(initialAck, initialAck.Length, remoteEP);
					_logger.Log($"Sent initial ACK for WRQ", LogLevel.Debug);

					while (true)
					{
						bool dataReceived = false;
						int retryCount = 0;

						while (!dataReceived && retryCount < maxRetries)
						{
							try
							{
								udpServer.Client.ReceiveTimeout = timeoutMs;
								byte[] receivedData = udpServer.Receive(ref remoteEP);
								
								if (receivedData.Length >= 4 && receivedData[0] == 0 && receivedData[1] == 3)
								{
									int receivedBlock = (receivedData[2] << 8) | receivedData[3];
									
									if (receivedBlock == block + 1)
									{
										int dataSize = receivedData.Length - 4;
										fs.Write(receivedData, 4, dataSize);
										
										// Send ACK for this block
										byte[] ackPacket = CreateAckPacket(receivedBlock);
										udpServer.Send(ackPacket, ackPacket.Length, remoteEP);
										
										_logger.Log($"Received and ACKed block {receivedBlock}, size: {dataSize}", LogLevel.Debug);
										
										if (dataSize < 512)
										{
											// Last block of the file
											FilesTransfered += 1;
											_logger.Log($"File successfully received: {filePath}", LogLevel.Info);
											return;
										}
										
										block = receivedBlock;
										dataReceived = true;
									}
									else
									{
										_logger.Log($"Received wrong block {receivedBlock}, expected {block + 1}", LogLevel.Warning);
										// Send ACK for the block we actually received (duplicate)
										byte[] ackPacket = CreateAckPacket(receivedBlock);
										udpServer.Send(ackPacket, ackPacket.Length, remoteEP);
									}
								}
							}
							catch (SocketException ex) when (ex.SocketErrorCode == SocketError.TimedOut)
							{
								retryCount++;
								_logger.Log($"Timeout waiting for data block {block + 1}, retry {retryCount}/{maxRetries}", LogLevel.Warning);
							}
						}

						if (!dataReceived)
						{
							_logger.Log($"Failed to receive data block {block + 1} after {maxRetries} retries", LogLevel.Error);
							SendError("Transfer failed - no data received");
							return;
						}
					}
				}
			}
			catch (Exception ex)
			{
				_logger.Log($"Error receiving file {filePath}: {ex.Message}", LogLevel.Error);
				SendError("Internal server error");
			}
		}

		private byte[] CreateDataPacket(int block, byte[] fileData, int offset, int length)
		{
			byte[] dataPacket = new byte[length + 4];
			dataPacket[0] = 0;  // Opcode for data (3)
			dataPacket[1] = 3;
			dataPacket[2] = (byte)(block >> 8);  // Block number (high byte)
			dataPacket[3] = (byte)(block & 0xFF);  // Block number (low byte)
			Array.Copy(fileData, offset, dataPacket, 4, length);
			return dataPacket;
		}

		private byte[] CreateAckPacket(int block)
		{
			byte[] ackPacket = new byte[4];
			ackPacket[0] = 0;  // Opcode for ACK (4)
			ackPacket[1] = 4;
			ackPacket[2] = (byte)(block >> 8);  // Block number (high byte)
			ackPacket[3] = (byte)(block & 0xFF);  // Block number (low byte)
			return ackPacket;
		}

		private void SendError(string errorMessage)
		{
			byte[] errorPacket = new byte[4 + errorMessage.Length + 1];
			errorPacket[0] = 0;  // Opcode for Error (5)
			errorPacket[1] = 5;
			errorPacket[2] = 0;  // Error code (file not found)
			errorPacket[3] = 1;
			Array.Copy(Encoding.ASCII.GetBytes(errorMessage), 0, errorPacket, 4, errorMessage.Length);
			errorPacket[4 + errorMessage.Length] = 0;  // Null terminator
			udpServer.Send(errorPacket, errorPacket.Length, remoteEP);
		}
	}
}
