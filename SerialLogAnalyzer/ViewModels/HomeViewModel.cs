using System.Collections.ObjectModel;
using System.ComponentModel;

namespace SerialLogAnalyzer.ViewModels
{
	public class HomeViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<RecentItem> RecentItems { get; set; }
		private string title = "Home";

		public string Title
		{
			get => title;
			set
			{
				title = value;
				OnPropertyChanged("Title");
			}
		}

		public HomeViewModel()
		{
			// Sample initialization logic
			InitializeRecentItems(); // Call a method to load recent items if applicable
			Title = "Home"; // Set the default title for the view
		}

		private void InitializeRecentItems()
		{
			RecentItems = new ObservableCollection<RecentItem>();
			for (int i = 0; i < 10; i++)
			{
				// Only add if there are less than 5 items
				if (RecentItems.Count < 5)
				{
					RecentItem item;

					if (i == 0) // First item: Serial Logger
					{
						item = new RecentItem
						{
							Title = "Serial Logger",
							SerialPort = "COM3", // Example serial port
							BackgroundColor = "#844EFF"
						};
					}
					else if (i == 1) // Second item: Serial Analyzer
					{
						item = new RecentItem
						{
							Title = "Serial Analysis",
							BackgroundColor = "#4E99FF"
						};
					}
					else if (i == 2) // Third item: TFTP Server
					{
						item = new RecentItem
						{
							Title = "TFTP Server",
							BackgroundColor = "#844EFF"
						};
					}
					else
					{
						// Just generic items for demonstration
						item = new RecentItem
						{
							Title = $"Item {i + 1}",
							BackgroundColor = (i % 2 == 0) ? "#4E99FF" : "#844EFF"
						};
					}

					RecentItems.Add(item);
				}
			}
		}


		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}

	public class RecentItem
	{
		public string Title { get; set; }
		public string SerialPort { get; set; } // Additional property for Serial Logger
		public string BackgroundColor { get; set; }
	}
}
