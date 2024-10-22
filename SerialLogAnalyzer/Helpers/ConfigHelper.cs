using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SerialLogAnalyzer.Helpers
{
	public static class ConfigHelper
	{
		private static readonly int MaxActivities = 5; // Keep only the last 5 activities

		public static void SaveConfigWithRecentActivities(MainViewModel viewModel, Activity newActivity)
		{
			if (viewModel.Config == null || viewModel.Config.RecentActivity == null)
			{
				return; // Early exit if there's no config or activities to save
			}

			// Make a copy to work with
			var recentActivities = viewModel.Config.RecentActivity.Activities.ToList();

			// List to hold activities for the specific computer
			List<Activity> computerActivities = new List<Activity>();

			// Find activities for the same computer and add the new activity
			foreach (var activity in recentActivities)
			{
				if (activity.ComputerName == newActivity.ComputerName)
				{
					computerActivities.Add(activity);
				}
			}

			// Add the new activity
			computerActivities.Add(newActivity);

			// If the count exceeds MaxActivities, remove the oldest activities
			while (computerActivities.Count > MaxActivities)
			{
				computerActivities.RemoveAt(0); // Remove the oldest activity
			}

			// Update the original list, removing old activities for this computer
			for (int i = recentActivities.Count - 1; i >= 0; i--)
			{
				if (recentActivities[i].ComputerName == newActivity.ComputerName)
				{
					recentActivities.RemoveAt(i);
				}
			}

			// Combine the updated activities for the specific computer
			recentActivities.AddRange(computerActivities);

			// Update the RecentActivities in the viewModel
			viewModel.Config.RecentActivity.Activities = recentActivities;

			// Save the configuration
			viewModel.SaveConfig();
		}
	}
}
