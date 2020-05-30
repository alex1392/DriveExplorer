
using DriveExplorer.Models;
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
			await vm.InitializeAsync().ConfigureAwait(false);
		}

		private async void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
			await vm.TreeItemSelectedAsync(sender, e).ConfigureAwait(false);
		}

		private async void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
			await vm.CurrentItemSelectedAsync(sender, e).ConfigureAwait(false);
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
					case ItemTypes.OneDrive:
						await vm.LogoutOneDriveAsync(itemVM.Item).ConfigureAwait(false);
						break;
					case ItemTypes.GoogleDrive:
						vm.LogoutGoogleDriveAsync(itemVM.Item);
						break;
				}
			}
		}
	}
}
