using SerialLogAnalyzer.Helpers;
using SerialLogAnalyzer.Models;
using SerialLogAnalyzer.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for SerialAnalyzer.xaml
	/// </summary>
	public partial class SerialAnalyzerView : UserControl
	{

		public ObservableCollection<Item> AvailableProducts { get; private set; }
		public ObservableCollection<string> AvailableModes { get; set; }
		private Logger logger;

		private List<string> selectedFiles = new List<string>();
		private bool isAnalyzing = false;

		public SerialAnalyzerView()
		{
			InitializeComponent();

			// Access the MainViewModel instance which contains the config settings
			var viewModel = (MainViewModel)this.FindResource("MainViewModel");

			AvailableProducts = new ObservableCollection<Item>(viewModel.Config.Items);

			logger = Logger.GetInstance("slate_app.log", false);

			// Set the DataContext to itself for binding
			this.DataContext = this;
		}

		private void CancelButton_Click(object sender, RoutedEventArgs e)
		{
			if (isAnalyzing)
			{
				logger.Log("Cancelling analysis...", LogLevel.Info);
				isAnalyzing = false;
				analyzeButton.IsEnabled = true; // Re-enable analyze button
				cancelButton.IsEnabled = false; // Disable cancel button
			}
		}

		private void AnalyzeButton_Click(object sender, RoutedEventArgs e)
		{
			Item selectedProduct = productComboBox.SelectedItem as Item;
			Mode selectedMode = modeComboBox.SelectedItem as Mode;

			if (selectedFiles.Count > 0 && selectedProduct != null && selectedMode != null)
			{
				isAnalyzing = true;
				analyzeButton.IsEnabled = false; // Disable analyze button
				cancelButton.IsEnabled = true; // Enable cancel button

				// Get keywords from the selected mode
				var keywords = new List<string>();
				if (selectedMode.KeywordGroups != null)
				{
					foreach (var keywordGroup in selectedMode.KeywordGroups)
					{
						keywords.AddRange(keywordGroup.Keywords); // Add all keywords from each group
					}
				}

				// Start a new thread for file analysis
				Thread analysisThread = new Thread(() =>
				{
					foreach (var file in selectedFiles)
					{
						logger.Log($"Analyzing {file}...", LogLevel.Info);

						// Create the KeywordParser with the file and keywords
						KeywordParser keywordParser = new KeywordParser(file, keywords);
						var keywordData = keywordParser.ParseFile(); // Parse the file using the provided keywords

						// Here, you might want to handle or display the results from keywordData as needed
						// For example, you could log them, display them in a UI element, etc.

						Thread.Sleep(1000); // Simulate time taken for analyzing each file
					}

					// Analysis complete, update UI
					isAnalyzing = false;

					// Use Dispatcher to update UI controls
					Dispatcher.Invoke(new Action(delegate
					{
						analyzeButton.IsEnabled = true; // Enable analyze button
						cancelButton.IsEnabled = false; // Disable cancel button
						logger.Log("Analysis complete.", LogLevel.Info);
					}));
				});

				analysisThread.IsBackground = true; // Make the thread a background thread
				analysisThread.Start(); // Start the analysis thread
			}
			else
			{
				MessageBox.Show("Please select a product and mode before analyzing.");
				logger.Log("Product and Mode weren't selected", LogLevel.Warning);
			}
		} // End of AnalyzeButton_Click()


		private void ProductComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			// Get the selected product (Item)
			Item selectedProduct = productComboBox.SelectedItem as Item;

			// Call the new function to populate modes
			PopulateModesForSelectedProduct(selectedProduct);
			logger.Log("Populated Modes ComboBox", LogLevel.Debug);
		}

		private void BrowseButton_Click(object sender, RoutedEventArgs e)
		{
			// Open a file dialog to select files
			Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
			dlg.Multiselect = true; // Allow multiple file selection

			// Filter to only show CSV, TXT, and LOG files
			dlg.Filter = "CSV, TXT, LOG files (*.csv;*.txt;*.log)|*.csv;*.txt;*.log|All files (*.*)|*.*";


			if (dlg.ShowDialog() == true)
			{
				selectedFiles.AddRange(dlg.FileNames); // Add selected files to the list
				UpdateFileListView(); // Refresh the UI to show selected files
				logger.Log("Selected the following filenames:", LogLevel.Info);
				foreach(var filename in dlg.FileNames)
				{
					logger.Log($"\t{filename}", LogLevel.Info);
				}
			}
		} // End of BrowseButton_Click()

		// Method to update the ListView items
		private void UpdateFileListView()
		{
			filesListView.ItemsSource = null; // Clear current binding
			filesListView.ItemsSource = selectedFiles; // Rebind the updated list
		} // End of UpdateFileListView()

		private void PopulateModesForSelectedProduct(Item selectedProduct)
		{
			if (selectedProduct != null && selectedProduct.Modes != null)
			{
				// Set the ItemsSource to the actual Mode objects
				modeComboBox.ItemsSource = selectedProduct.Modes;

				// Use a combination of the DisplayMemberPath to show the formatted name
				modeComboBox.DisplayMemberPath = "FormattedName";

				// Optionally select the first mode by default if any modes are available
				if (modeComboBox.Items.Count > 0)
				{
					modeComboBox.SelectedIndex = 0;
				}
			}
			else
			{
				// Clear modeComboBox if no product or modes are available
				modeComboBox.ItemsSource = null;
			}
		} // End of UpdateFileListView()
	}
}
