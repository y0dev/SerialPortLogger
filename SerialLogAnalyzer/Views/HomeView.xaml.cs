using SerialLogAnalyzer.ViewModels;
using System.Windows.Controls;

namespace SerialLogAnalyzer.Views
{
	public partial class HomeView : UserControl
	{
		public HomeView(MainViewModel viewModel)
		{
			InitializeComponent();
			this.DataContext = new HomeViewModel(viewModel);
		}
	}
}
