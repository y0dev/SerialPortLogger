using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.Services;
using System;
using System.ComponentModel;

namespace SerialLogAnalyzer.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private readonly ConfigurationService configService;
		private AppConfiguration config;
		private string selectedTheme; // Field to store the selected theme

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
			configService = new ConfigurationService(Properties.Resources.CONFIG_PATH);
			LoadConfig();
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

		// Implement INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
