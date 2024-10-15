using System;
using System.Collections.Generic;
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
	/// Interaction logic for HelpPage.xaml
	/// </summary>
	public partial class HelpPage : Window
	{
		public HelpPage()
		{
			InitializeComponent();
			SetInstructionsText();
		}

		private void SetInstructionsText()
		{
			InstructionsTextBlock.Text =
				"1. Select a COM port from the dropdown menu.\n" +
				"2. Choose the baud rate for the connection.\n" +
				"3. Click 'Create Logger' to start logging data.\n" +
				"4. View the logged data in the console or in the logging tabs.\n" +
				"5. Use the 'Stop All Loggers' button to halt logging.";
		} // End of SetInstructionsText()

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close(); // Close the Help page
		}
	}
}
