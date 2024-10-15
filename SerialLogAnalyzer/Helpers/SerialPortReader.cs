using System;
using System.IO;
using System.IO.Ports;
using System.Threading;

namespace SerialLogAnalyzer.Helpers
{
	public class DataReceivedEventArgs : EventArgs
	{
		private string _data;

		public string Data
		{
			get { return _data; }
		}

		public DataReceivedEventArgs(string data)
		{
			_data = data;
		}
	}

	public class SerialPortReader
	{
		private SerialPort _serialPort;
		private Thread _readingThread;
		private bool _isReading;

		public event EventHandler<DataReceivedEventArgs> DataReceived;

		public SerialPortReader(string portName, int baudRate)
		{
			_serialPort = new SerialPort(portName, baudRate);
			_serialPort.DataReceived += new SerialDataReceivedEventHandler(SerialPort_DataReceived);
			_isReading = false;
		}

		public void StartReading()
		{
			if (!_isReading)
			{
				_isReading = true;
				_serialPort.Open();
				_readingThread = new Thread(new ThreadStart(ReadingThread));
				_readingThread.IsBackground = true;
				_readingThread.Start();
			}
		}

		public void StopReading()
		{
			if (_isReading)
			{
				_isReading = false;
				_serialPort.Close();

				if (_readingThread != null && _readingThread.IsAlive)
				{
					_readingThread.Join();
				}
			}
		}

		private void ReadingThread()
		{
			while (_isReading)
			{
				try
				{
					string data = _serialPort.ReadLine();
					OnDataReceived(data);
				}
				catch (TimeoutException) { }
				catch (InvalidOperationException) { }
				catch (IOException) { }
			}
		}

		private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
			try
			{
				string data = _serialPort.ReadLine();
				OnDataReceived(data);
			}
			catch (TimeoutException) { }
			catch (InvalidOperationException) { }
			catch (IOException) { }
		}

		protected virtual void OnDataReceived(string data)
		{
			if (DataReceived != null)
			{
				DataReceived(this, new DataReceivedEventArgs(data));
			}
		}
	}
}
