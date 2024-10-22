using SerialLogAnalyzer.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace SerialLogAnalyzer.ViewModels
{
	public class HomeViewModel : INotifyPropertyChanged
	{
		public ObservableCollection<RecentItem> RecentItems { get; set; }
		private MainViewModel viewModel;

		public HomeViewModel(MainViewModel viewModel)
		{
			this.viewModel = viewModel;
			InitializeRecentItems(); // Call a method to load recent items if applicable
		}

		private void InitializeRecentItems()
		{
			RecentItems = new ObservableCollection<RecentItem>();
			List<Activity> activities = viewModel.Config.RecentActivity.Activities;
			var sortedActivities = activities.OrderByDescending(a => a.ActivityDateTime).ToList();

			for (int i = 0; i < sortedActivities.Count; i++)
			{
				RecentItem item = new RecentItem
				{
					Title = sortedActivities[i].Type,
					Date = sortedActivities[i].ActivityDateTime.ToString("MMM dd, yyyy"),
					BackgroundColor = (i % 2 == 0) ? "#844EFF" : "#4E99FF",
				};

				if (!string.IsNullOrEmpty(sortedActivities[i].SerialPort))
				{
					item.Details = sortedActivities[i].SerialPort;
					RecentItems.Add(item);
					continue;
				}

				if (sortedActivities[i].FilesAnalyzed != null)
				{
					string details = sortedActivities[i].FilesAnalyzed > 1 ? "Files Analyzed" : "File Analyzed";
					item.Details = $"{sortedActivities[i].FilesAnalyzed} {details}";
					RecentItems.Add(item);
					continue;
				}

				if (sortedActivities[i].FilesTransferred != null)
				{
					string details = sortedActivities[i].FilesTransferred > 1 ? "Files Transferred" : "File Transferred";
					item.Details = $"{sortedActivities[i].FilesTransferred} {details} from {sortedActivities[i].IPAddress}";
					RecentItems.Add(item);
					continue;
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
		public string Date { get; set; }
		public string Details { get; set; } 
		public string BackgroundColor { get; set; }
	}
}
