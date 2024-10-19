using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for SettingsWindow.xaml
	/// </summary>
	public partial class SettingsWindow : Window
	{

		public ObservableCollection<string> AvailableFonts { get; private set; }
		public ObservableCollection<int> FontSizes { get; set; }
		public int SelectedBaudRate { get; set; }

		public SettingsWindow()
		{
			InitializeComponent();

			AvailableFonts = new ObservableCollection<string>
			{
				"Arial",
				"Calibri",
				"Courier New",
				"Times New Roman",
				"Verdana"
			};

			// Initialize with common baud rates
			FontSizes = new ObservableCollection<int> { 8, 10, 12, 14, 16, 18, 20, 22, 24, 26 };
			//SelectedBaudRate = 115200; // Default baud rate

			// Set the DataContext to itself for binding
			this.DataContext = this;
		}

		// Logic to save the settings
		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			
			this.Close(); // Close the settings window
		} // End of SaveButton_Click()
		
		private void FontSizeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}

		private void FontsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
	}

}
