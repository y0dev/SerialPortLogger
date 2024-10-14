using SerialLogAnalyzer.ViewModels;
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

namespace SerialLogAnalyzer
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		public MainWindow()
		{
			InitializeComponent();
			/*
			// Access the TabControl items
			foreach (var tabItem in MainTabControl.Items)
			{
				if (tabItem is MainViewTabItem mainViewTabItem)
				{
					string tabHeader = mainViewTabItem.TabHeader;
					MessageBox.Show($"Tab Header: {tabHeader}");
				}
			}
			*/
		}
	}
}
