using SerialLogAnalyzer.ViewModels;
using System.Windows.Controls;

namespace SerialLogAnalyzer.Views
{
	public partial class HomeView : UserControl
	{
		public HomeView()
		{
			InitializeComponent();
			this.DataContext = new HomeViewModel();
		}
	}
}
