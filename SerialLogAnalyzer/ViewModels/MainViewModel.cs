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

		public AppConfiguration Config
		{
			get => config;
			set
			{
				config = value;
				OnPropertyChanged("Config"); // Pass the property name as a string
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
