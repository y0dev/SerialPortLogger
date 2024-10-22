using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SerialLogAnalyzer.Helpers
{
	// Public class that represents the TFTP Server
	public class TftpServer
	{
		private const int TftpPort = 69;
		private UdpClient udpServer;
		private IPEndPoint localEP;
		private IPEndPoint remoteEP;
		private bool isRunning;
		private string baseDirectory;
		private Thread serverThread;

		private Logger _logger;
		public int FilesTransfered;

		public TftpServer(string ipAddress, string baseDirectory, Logger logger)
		{
			// Bind to the specific IP address
			localEP = new IPEndPoint(IPAddress.Parse(ipAddress), TftpPort);
			udpServer = new UdpClient(localEP);
			remoteEP = new IPEndPoint(IPAddress.Any, TftpPort);
			isRunning = false;
			this.baseDirectory = baseDirectory;
			this._logger = logger;
			FilesTransfered = 0;

		}

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
			byte[] fileData = File.ReadAllBytes(filePath);
			int block = 1;
			int bytesRead = 0;

			while (bytesRead < fileData.Length)
			{
				byte[] dataPacket = CreateDataPacket(block, fileData, bytesRead, Math.Min(512, fileData.Length - bytesRead));
				udpServer.Send(dataPacket, dataPacket.Length, remoteEP);
				bytesRead += 512;
				block++;
			}

			FilesTransfered += 1;
			_logger.Log($"File transfer complete: {filePath}", LogLevel.Info);
		}

		private void ReceiveFile(string filePath)
		{
			using (FileStream fs = new FileStream(filePath, FileMode.Create))
			{
				int block = 1;
				while (true)
				{
					byte[] ackPacket = CreateAckPacket(block);
					udpServer.Send(ackPacket, ackPacket.Length, remoteEP);

					byte[] receivedData = udpServer.Receive(ref remoteEP);
					int dataSize = receivedData.Length - 4;
					fs.Write(receivedData, 4, dataSize);

					if (dataSize < 512) break; // Last block of the file

					block++;
				}
			}
			FilesTransfered += 1;
			_logger.Log($"File successfully received: {filePath}", LogLevel.Info);
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
