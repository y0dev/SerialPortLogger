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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SerialLogAnalyzer.Views
{
	/// <summary>
	/// Interaction logic for AboutPage.xaml
	/// </summary>
	public partial class AboutPage : Window
	{
		public AboutPage()
		{
			InitializeComponent();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		} // End of CloseButton_Click()
	}
}
