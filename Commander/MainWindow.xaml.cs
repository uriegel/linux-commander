#if Windows
using System.Windows;

namespace Commander
{
	public partial class MainWindow : Window
	{
		public MainWindow() => InitializeComponent();
	}
}
// TODO: Windows single file with resources
#endif