using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.Services;

namespace SerialLogAnalyzer.Views
{
	public class ConfigFileInfo
	{
		public string FileName { get; set; }
		public string FilePath { get; set; }
		public DateTime LastModified { get; set; }
		public string DisplayName => Path.GetFileNameWithoutExtension(FileName);
		public string Theme { get; set; }
		public int ComputerCount { get; set; }
	}

	public partial class SettingsView : UserControl
	{
		public ObservableCollection<string> AvailableFonts { get; private set; }
		public ObservableCollection<int> FontSizes { get; set; }
		public ObservableCollection<ConfigFileInfo> AvailableConfigs { get; private set; }
		public bool IsDarkTheme { get; set; }
		public string SelectedFont { get; set; }
		public int SelectedFontSize { get; set; }
		public ConfigFileInfo SelectedConfig { get; set; }
		private Logger logger;
		MainViewModel mainViewModel;
		private ConfigurationService configService;

		// Private field to store the current theme state
		private bool? currentThemeState;

		public SettingsView(MainViewModel mainViewModel)
		{
			InitializeComponent();

			AvailableFonts = new ObservableCollection<string>
			{
				"Segoe UI",
				"Arial",
				"Tahoma",
				"Times New Roman",
				"Verdana",
				"Calibri"
			};

			FontSizes = new ObservableCollection<int> { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26 };
			AvailableConfigs = new ObservableCollection<ConfigFileInfo>();

			this.mainViewModel = mainViewModel;
			this.configService = new ConfigurationService();
			
			var theme = mainViewModel.Config?.Settings.Theme ?? "Light";
			IsDarkTheme = theme == "Dark" ? true : false;
			SelectedFont = mainViewModel.Config?.Settings?.Font ?? "Segoe UI";
			SelectedFontSize = mainViewModel.Config?.Settings?.FontSize ?? 12;

			logger = Logger.GetInstance("slate_app.log", false);

			// Load available configurations
			LoadAvailableConfigs();

			// Set the DataContext to itself for binding
			this.DataContext = this;

			// Initialize the toggle button state
			currentThemeState = IsDarkTheme;
			themeToggleButton.IsChecked = currentThemeState; // Set the toggle button based on current theme
		}

		private void LoadAvailableConfigs()
		{
			try
			{
				AvailableConfigs.Clear();
				
				// Try multiple possible paths
				string[] possiblePaths = {
					Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "Scripts"),
					Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Scripts"),
					Path.Combine(Directory.GetCurrentDirectory(), "Scripts"),
					Path.Combine(Directory.GetCurrentDirectory(), "..", "Scripts"),
					@"F:\Programming\WPF\SerialLogAnalyzer\Scripts" // Absolute path as fallback
				};
				
				string scriptsPath = null;
				foreach (var path in possiblePaths)
				{
					if (Directory.Exists(path))
					{
						scriptsPath = path;
						break;
					}
				}
				
				logger.Log($"Base Directory: {AppDomain.CurrentDomain.BaseDirectory}", LogLevel.Info);
				logger.Log($"Current Directory: {Directory.GetCurrentDirectory()}", LogLevel.Info);
				logger.Log($"Using Scripts path: {scriptsPath}", LogLevel.Info);
				
				if (scriptsPath != null && Directory.Exists(scriptsPath))
				{
					var configFiles = Directory.GetFiles(scriptsPath, "*.xml")
						.Where(f => !f.Contains("_old") && !f.Contains("Archive"))
						.ToList();

					logger.Log($"Found {configFiles.Count} config files in {scriptsPath}", LogLevel.Info);
					
					// List all found files for debugging
					foreach (var file in configFiles)
					{
						logger.Log($"Found file: {Path.GetFileName(file)}", LogLevel.Info);
					}

					foreach (var filePath in configFiles)
					{
						try
						{
							var configInfo = new ConfigFileInfo
							{
								FileName = Path.GetFileName(filePath),
								FilePath = filePath,
								LastModified = File.GetLastWriteTime(filePath)
							};

							// Try to load config to get additional info
							try
							{
								var tempService = new ConfigurationService(filePath);
								var config = tempService.LoadConfiguration();
								configInfo.Theme = config?.Settings?.Theme ?? "Unknown";
								configInfo.ComputerCount = config?.ComputerConfigs?.Count ?? 0;
								
								// Test if the config loaded successfully
								if (config != null)
								{
									logger.Log($"Successfully loaded config: {configInfo.FileName} - Theme: {configInfo.Theme}, Computers: {configInfo.ComputerCount}", LogLevel.Info);
								}
							}
							catch (Exception ex)
							{
								configInfo.Theme = "Unknown";
								configInfo.ComputerCount = 0;
								logger.Log($"Failed to load config {configInfo.FileName}: {ex.Message}", LogLevel.Warning);
							}

							AvailableConfigs.Add(configInfo);
							logger.Log($"Added config to AvailableConfigs: {configInfo.FileName}", LogLevel.Info);
						}
						catch (Exception ex)
						{
							logger.Log($"Error loading config file {filePath}: {ex.Message}", LogLevel.Warning);
						}
					}
					
					logger.Log($"Total configs added to AvailableConfigs: {AvailableConfigs.Count}", LogLevel.Info);
				}
				else
				{
					logger.Log($"Scripts directory not found in any of the attempted paths", LogLevel.Warning);
					foreach (var path in possiblePaths)
					{
						logger.Log($"Tried path: {path} - Exists: {Directory.Exists(path)}", LogLevel.Warning);
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log($"Error loading available configs: {ex.Message}", LogLevel.Error);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				bool hasChanges = false;
				string originalTheme = mainViewModel.Config?.Settings?.Theme ?? "Light";

				// Check font changes
				if (fontsComboBox.SelectedItem != null &&
					!string.IsNullOrEmpty(fontsComboBox.SelectedItem.ToString()) &&
					mainViewModel.Config?.Settings?.Font != fontsComboBox.SelectedItem.ToString())
				{
					string selectedFont = fontsComboBox.SelectedItem.ToString();
					logger.Log($"Changing font from {mainViewModel.Config?.Settings?.Font} to {selectedFont}.", LogLevel.Info);
					if (mainViewModel.Config?.Settings != null)
					{
						mainViewModel.Config.Settings.Font = selectedFont;
						hasChanges = true;
					}
				}

				// Check font size changes
				if (fontSizeComboBox.SelectedItem != null &&
					int.TryParse(fontSizeComboBox.SelectedItem.ToString(), out int selectedFontSize) &&
					mainViewModel.Config?.Settings?.FontSize != selectedFontSize)
				{
					logger.Log($"Changing font size from {mainViewModel.Config?.Settings?.FontSize} to {selectedFontSize}.", LogLevel.Info);
					if (mainViewModel.Config?.Settings != null)
					{
						mainViewModel.Config.Settings.FontSize = selectedFontSize;
						hasChanges = true;
					}
				}

				// Check theme changes - compare current toggle state with original theme
				string currentToggleTheme = themeToggleButton.IsChecked == true ? "Dark" : "Light";
				if (currentToggleTheme != originalTheme)
				{
					logger.Log($"Theme change detected: {originalTheme} -> {currentToggleTheme}", LogLevel.Info);
					if (mainViewModel.Config?.Settings != null)
					{
						mainViewModel.Config.Settings.Theme = currentToggleTheme;
						mainViewModel.SelectedTheme = currentToggleTheme;
						hasChanges = true;
					}
				}

				if (hasChanges)
				{
					// Ensure we have a valid config before saving
					if (mainViewModel.Config == null)
					{
						mainViewModel.Config = new AppConfiguration();
					}
					
					// Ensure Settings object exists
					if (mainViewModel.Config.Settings == null)
					{
						mainViewModel.Config.Settings = new Settings();
					}

					mainViewModel.SaveConfig();
					logger.Log("Configuration saved successfully", LogLevel.Info);
					MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
				}
				else
				{
					logger.Log("No changes detected to save", LogLevel.Info);
					MessageBox.Show("No changes to save.", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
				}
			}
			catch (Exception ex)
			{
				logger.Log($"Error saving settings: {ex.Message}", LogLevel.Error);
				MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void ToggleButton_Click(object sender, RoutedEventArgs e)
		{
			ToggleButton toggleButton = (ToggleButton)sender;
			string theme = toggleButton.IsChecked == true ? "Dark" : "Light";
			
			// Apply the theme immediately for visual feedback
			ChangeTheme(theme);
			
			// Update the MainViewModel's theme for persistence
			if (mainViewModel.Config?.Settings != null)
			{
				mainViewModel.Config.Settings.Theme = theme;
				mainViewModel.SelectedTheme = theme;
			}
			
			logger.Log($"Theme toggle clicked - Applied {theme} theme", LogLevel.Info);
		}

		private void ChangeTheme(string themeName)
		{
			try
			{
				logger.Log($"Changing theme to {themeName}.", LogLevel.Info);

				// Use the MainViewModel's theme application method
				mainViewModel.ApplyTheme(themeName);
				
				logger.Log($"Theme changed to {themeName}", LogLevel.Info);
			}
			catch (Exception ex)
			{
				logger.Log($"Error changing theme: {ex.Message}", LogLevel.Error);
			}
		}

		private void NewConfigButton_Click(object sender, RoutedEventArgs e)
		{
			NewConfigWindow newConfigWindow = new NewConfigWindow();
			newConfigWindow.ShowDialog();
			LoadAvailableConfigs(); // Refresh the config list
		}

		private void LoadConfigButton_Click(object sender, RoutedEventArgs e)
		{
			if (SelectedConfig != null)
			{
				try
				{
					var result = MessageBox.Show($"Are you sure you want to load configuration '{SelectedConfig.DisplayName}'? This will replace the current configuration.", 
						"Confirm Load", MessageBoxButton.YesNo, MessageBoxImage.Question);
					
					if (result == MessageBoxResult.Yes)
					{
						var tempService = new ConfigurationService(SelectedConfig.FilePath);
						var loadedConfig = tempService.LoadConfiguration();
						if (loadedConfig != null)
						{
							mainViewModel.Config = loadedConfig;
							mainViewModel.SaveConfig();
							
							// Update UI to reflect loaded config
							IsDarkTheme = loadedConfig.Settings?.Theme == "Dark";
							SelectedFont = loadedConfig.Settings?.Font ?? "Segoe UI";
							SelectedFontSize = loadedConfig.Settings?.FontSize ?? 12;
							
							// Apply theme immediately
							if (loadedConfig.Settings?.Theme != null)
							{
								mainViewModel.SelectedTheme = loadedConfig.Settings.Theme;
								mainViewModel.ApplyTheme(loadedConfig.Settings.Theme);
							}
							
							MessageBox.Show($"Configuration '{SelectedConfig.DisplayName}' loaded successfully!", "Success", 
								MessageBoxButton.OK, MessageBoxImage.Information);
						}
					}
				}
				catch (Exception ex)
				{
					logger.Log($"Error loading config: {ex.Message}", LogLevel.Error);
					MessageBox.Show($"Error loading configuration: {ex.Message}", "Error", 
						MessageBoxButton.OK, MessageBoxImage.Error);
				}
			}
			else
			{
				MessageBox.Show("Please select a configuration to load.", "No Selection", 
					MessageBoxButton.OK, MessageBoxImage.Warning);
			}
		}

		private void DeleteConfigButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Debug selection state
				LogSelectionState();
				
				// Check if SelectedConfig is null or if no item is selected
				if (SelectedConfig == null || string.IsNullOrEmpty(SelectedConfig.FilePath))
				{
					logger.Log("Delete button clicked but no configuration selected", LogLevel.Warning);
					MessageBox.Show("Please select a configuration to delete.", "No Selection", 
						MessageBoxButton.OK, MessageBoxImage.Warning);
					return;
				}

				// Additional safety check - verify the file exists
				if (!File.Exists(SelectedConfig.FilePath))
				{
					MessageBox.Show($"Configuration file not found: {SelectedConfig.FilePath}", "File Not Found", 
						MessageBoxButton.OK, MessageBoxImage.Error);
					LoadAvailableConfigs(); // Refresh the list in case file was already deleted
					return;
				}

				// Store the display name before deletion for the success message
				string configDisplayName = SelectedConfig.DisplayName ?? "Unknown Configuration";
				
				var result = MessageBox.Show($"Are you sure you want to delete configuration '{configDisplayName}'?\n\nFile: {SelectedConfig.FilePath}\n\nThis action cannot be undone.", 
					"Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
				
				if (result == MessageBoxResult.Yes)
				{
					// Double-check file still exists before deletion
					if (File.Exists(SelectedConfig.FilePath))
					{
						File.Delete(SelectedConfig.FilePath);
						logger.Log($"Successfully deleted configuration: {configDisplayName} from {SelectedConfig.FilePath}", LogLevel.Info);
						
						// Clear the selection
						SelectedConfig = null;
						
						// Refresh the list
						LoadAvailableConfigs();
						
						MessageBox.Show($"Configuration '{configDisplayName}' deleted successfully!", "Success", 
							MessageBoxButton.OK, MessageBoxImage.Information);
					}
					else
					{
						MessageBox.Show($"Configuration file was not found and could not be deleted.", "File Not Found", 
							MessageBoxButton.OK, MessageBoxImage.Warning);
						LoadAvailableConfigs(); // Refresh the list
					}
				}
			}
			catch (Exception ex)
			{
				logger.Log($"Error deleting config: {ex.Message}", LogLevel.Error);
				MessageBox.Show($"Error deleting configuration: {ex.Message}", "Error", 
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		private void RefreshConfigsButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				logger.Log("Refreshing configuration list...", LogLevel.Info);
				LoadAvailableConfigs();
				MessageBox.Show($"Configuration list refreshed! Found {AvailableConfigs.Count} configurations.", "Refresh Complete", 
					MessageBoxButton.OK, MessageBoxImage.Information);
			}
			catch (Exception ex)
			{
				logger.Log($"Error refreshing configs: {ex.Message}", LogLevel.Error);
				MessageBox.Show($"Error refreshing configurations: {ex.Message}", "Error", 
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		// Helper method to debug selection state
		private void LogSelectionState()
		{
			logger.Log($"Selection State - SelectedConfig: {(SelectedConfig != null ? SelectedConfig.FileName : "NULL")}", LogLevel.Info);
			logger.Log($"Selection State - ListView SelectedItem: {(configListView.SelectedItem != null ? ((ConfigFileInfo)configListView.SelectedItem).FileName : "NULL")}", LogLevel.Info);
			logger.Log($"Selection State - AvailableConfigs Count: {AvailableConfigs.Count}", LogLevel.Info);
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			AboutPage aboutPage = new AboutPage();
			aboutPage.ShowDialog();
		}

		private void HelpButton_Click(object sender, RoutedEventArgs e)
		{
			HelpPage helpPage = new HelpPage();
			helpPage.ShowDialog();
		}
	}
}
