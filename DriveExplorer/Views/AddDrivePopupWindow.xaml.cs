using DriveExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DriveExplorer.Views {
	/// <summary>
	/// Interaction logic for AddDrivePopupWindow.xaml
	/// </summary>
	public partial class AddDrivePopupWindow : Window {
		private MainWindowVM vm;

		public AddDrivePopupWindow()
		{
			InitializeComponent();
		}

		public AddDrivePopupWindow(MainWindowVM vm) : this()
		{
			this.vm = vm;
		}

		private async void OneDriveButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
			await vm.LoginOneDriveAsync().ConfigureAwait(true);
		}

		private async void GoogleDriveButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
			await vm.LoginGoogleDriveAsync().ConfigureAwait(true);
		}
	}
}
