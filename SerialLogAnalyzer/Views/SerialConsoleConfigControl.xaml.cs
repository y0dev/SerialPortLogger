using System.Windows;
using System.Windows.Controls;

namespace SerialLogAnalyzer
{
	public partial class SerialConsoleConfigControl : UserControl
	{
		public SerialConsoleConfigControl()
		{
			InitializeComponent();
		}

		private void CreateSerialConsoleConfigButton_Click(object sender, RoutedEventArgs e)
		{
			// Logic to create a new Serial Console Config
			string configName = ConfigNameTextBox.Text;
			string title = TitleTextBox.Text;
			string colorScheme = ColorSchemeTextBox.Text;
			if (ushort.TryParse(FontSizeTextBox.Text, out ushort fontSize))
			{
				// Here you would normally add the created config to a list or save it somewhere
				MessageBox.Show($"Serial Console Config '{configName}' created with Title '{title}' and Color Scheme '{colorScheme}'.");
			}
			else
			{
				MessageBox.Show("Please enter a valid Font Size.");
			}
		}
	}
}
