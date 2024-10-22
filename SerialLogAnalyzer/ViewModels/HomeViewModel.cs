using SerialLogAnalyzer.Models;
using System;
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
			// Get the current computer name
			string currentComputerName = Environment.MachineName;

			// Create a list to hold the filtered activities
			List<Activity> filteredActivities = new List<Activity>();

			// Filter activities for the current computer
			for (int j = 0; j < activities.Count; j++)
			{
				if (activities[j].ComputerName == currentComputerName)
				{
					filteredActivities.Add(activities[j]);
				}
			}

			// Sort the filtered activities by ActivityDateTime in descending order
			filteredActivities.Sort((a, b) => b.ActivityDateTime.CompareTo(a.ActivityDateTime));

			// Now add the filtered and sorted activities to RecentItems
			for (int i = 0; i < filteredActivities.Count; i++)
			{
				RecentItem item = new RecentItem
				{
					Title = filteredActivities[i].Type,
					Date = filteredActivities[i].ActivityDateTime.ToString("MMM dd, yyyy"),
					BackgroundColor = (i % 2 == 0) ? "#844EFF" : "#4E99FF",
				};

				if (!string.IsNullOrEmpty(filteredActivities[i].SerialPort))
				{
					item.Details = filteredActivities[i].SerialPort;
					RecentItems.Add(item);
					continue;
				}

				if (filteredActivities[i].FilesAnalyzed != null)
				{
					string details = filteredActivities[i].FilesAnalyzed > 1 ? "Files Analyzed" : "File Analyzed";
					item.Details = $"{filteredActivities[i].FilesAnalyzed} {details}";
					RecentItems.Add(item);
					continue;
				}

				if (filteredActivities[i].FilesTransferred != null)
				{
					string details = filteredActivities[i].FilesTransferred > 1 ? "Files Transferred" : "File Transferred";
					item.Details = $"{filteredActivities[i].FilesTransferred} {details} from {filteredActivities[i].IPAddress}";
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
