
using DriveExplorer.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DriveExplorer.Views {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		private readonly MainWindowVM vm;

		public MainWindow() {
			InitializeComponent();
		}

		public MainWindow(MainWindowVM vm) : this() {
			this.vm = vm;
			DataContext = vm;
			Loaded += MainWindow_Loaded;
		}

		private async void MainWindow_Loaded(object sender, RoutedEventArgs e) {
			vm.GetLocalDrives();
			await vm.AutoLoginOneDriveAsync().ConfigureAwait(false);
			await vm.AutoLoginGoogleDriveAsync().ConfigureAwait(false);
		}

		private async void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
			await vm.TreeItem_SelectedAsync(sender, e).ConfigureAwait(false);
		}

		private async void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			await vm.CurrentItem_SelectedAsync(sender, e).ConfigureAwait(false);
		}

		private async void LoginGoogleDriveButton_Click(object sender, RoutedEventArgs e) {
			await vm.LoginGoogleDriveAsync().ConfigureAwait(false);
		}

		private async void LoginOneDriveButton_Click(object sender, RoutedEventArgs e) {
			await vm.LoginOneDriveAsync().ConfigureAwait(false);
		}

		private async void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e) {
			if (!(sender is TreeViewItem treeViewItem) ||
				!(treeViewItem.DataContext is ItemVM itemVM)) {
				throw new InvalidOperationException();
			}
			if (e.RightButton == MouseButtonState.Pressed) {
				switch (itemVM.Item.Type) {
					case Models.ItemTypes.OneDrive:
						await vm.LogoutOneDriveAsync(itemVM).ConfigureAwait(false);
						break;
					case Models.ItemTypes.GoogleDrive:
						vm.LogoutGoogleDrive(itemVM);
						break;
				}
			}
		}
	}
}
