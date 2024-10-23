using SerialLogAnalyzer.Commands;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.Services;
using SerialLogAnalyzer.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;

namespace SerialLogAnalyzer.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private readonly ConfigurationService configService;
		private AppConfiguration config;
		private string selectedTheme; // Field to store the selected theme
		private object _currentView;
		private string previousView = null; 

		public RelayCommand ChangeViewCommand { get; private set; }

		public object SelectedView
		{
			get { return _currentView; }
			set
			{
				_currentView = value;
				OnPropertyChanged("SelectedView");
			}
		}

		public AppConfiguration Config
		{
			get => config;
			set
			{
				config = value;
				OnPropertyChanged("Config"); // Pass the property name as a string
			}
		}

		public string SelectedTheme
		{
			get => selectedTheme;
			set
			{
				if (selectedTheme != value)
				{
					selectedTheme = value;
					OnPropertyChanged("SelectedTheme");
				}
			}
		}

		public MainViewModel()
		{
			ChangeViewCommand = new RelayCommand(ChangeView);

			configService = new ConfigurationService(Properties.Resources.CONFIG_PATH);
			LoadConfig();

			// Apply the theme when the ViewModel is instantiated
			ApplyInitialTheme();

			SelectedView = new HomeView(this);
		}

		private void ApplyInitialTheme()
		{
			// Check the saved theme in config or other state management (dark or light)
			bool isDarkTheme = SelectedTheme == "Dark";

			// Save the existing dictionaries excluding the theme dictionary
			var existingDictionaries = new List<ResourceDictionary>();

			foreach (var dictionary in Application.Current.Resources.MergedDictionaries)
			{
				// Check if the dictionary is a theme dictionary by its source or other criteria
				var source = dictionary.Source?.ToString();
				if (source != null && (source.Contains("DarkTheme.xaml") || source.Contains("LightTheme.xaml")))
				{
					continue; // Skip the current theme dictionary
				}

				// Add non-theme dictionaries to the list
				existingDictionaries.Add(dictionary);
			}

			// Clear all the dictionaries from the merged dictionary collection
			Application.Current.Resources.MergedDictionaries.Clear();

			// Add the non-theme dictionaries back
			foreach (var dictionary in existingDictionaries)
			{
				Application.Current.Resources.MergedDictionaries.Add(dictionary);
			}

			// Add the new theme dictionary based on the selected theme
			var themeUri = isDarkTheme ? "Themes/DarkTheme.xaml" : "Themes/LightTheme.xaml";
			Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary() { Source = new Uri(themeUri, UriKind.Relative) });
		}

		private void LoadConfig()
		{
			Config = configService.LoadConfiguration();

			SelectedTheme = Config?.Settings.Theme ?? "Light"; // Load theme from config
		}

		public void SaveConfig()
		{
			configService.SaveConfiguration(Config);
		}

		private void ChangeView(object viewName)
		{
			// Check if the previous view was "Settings" before switching views
			if (previousView == "Settings")
			{
				// Reload config, assuming it may have been updated
				LoadConfig();
			}

			switch (viewName)
			{
				case "Home":
					SelectedView = new HomeView(this); // Or however you're creating your view
					break;
				case "Serial Logger":
					SelectedView = new SerialLoggerView(this);
					break;
				case "Serial Analyzer":
					SelectedView = new SerialAnalyzerView(this);
					break;
				case "TFTP Server":
					SelectedView = new TFTPServerView(this);
					break;
				case "Settings":
					SelectedView = new SettingsView(this);
					break;
					// Add other cases as needed
			}

			// Save the current view as the previous view for future reference
			previousView = viewName.ToString();

			// Apply the theme after the view is changed
			ApplyInitialTheme();
		}


		// Implement INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
