using SerialLogAnalyzer.Commands;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.Services;
using SerialLogAnalyzer.Views;
using System;
using System.ComponentModel;

namespace SerialLogAnalyzer.ViewModels
{
	public class MainViewModel : INotifyPropertyChanged
	{
		private readonly ConfigurationService configService;
		private AppConfiguration config;
		private string selectedTheme; // Field to store the selected theme
		private object _currentView;

		public RelayCommand ChangeViewCommand { get; private set; }

		public object SelectedView
		{
			get { return _currentView; }
			set
			{
				_currentView = value;
				OnPropertyChanged("View");
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

			SelectedView = new HomeView();
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
			Console.WriteLine($"View Selected: {viewName}");
			switch (viewName)
			{
				case "Home":
					SelectedView = new HomeView(); // Or however you're creating your view
					break;
				case "Serial Logger":
					SelectedView = new SerialLoggerView();
					break;
				case "Serial Analyzer":
					SelectedView = new SerialAnalyzerView();
					break;
				case "TFTP Server":
					SelectedView = new TFTPServerView();
					break;
				case "Settings":
					SelectedView = new SettingsWindow();
					break;
					// Add other cases as needed
			}
		}


		// Implement INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged(string propertyName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
