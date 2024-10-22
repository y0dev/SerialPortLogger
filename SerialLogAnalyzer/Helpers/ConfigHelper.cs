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

			// Ensure no more than 5 recent activities
			var activities = viewModel.Config.RecentActivity.Activities.ToList(); // Make a copy to work with

			// Add the new activity
			activities.Add(newActivity);

			// If the count exceeds MaxActivities, remove the oldest activities
			while (activities.Count > MaxActivities)
			{
				activities.RemoveAt(0); // Remove the oldest activity
			}

			// Update the RecentActivities in the viewModel
			viewModel.Config.RecentActivity.Activities = activities;

			// Save the configuration
			viewModel.SaveConfig();
		}
	}
}
