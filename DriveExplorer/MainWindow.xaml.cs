using DriveExplorer.Models;
using DriveExplorer.ViewModels;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Linq;
using Cyc;
using Cyc.FluentDesign;

namespace DriveExplorer.Views {

	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : RevealWindow {

		#region Private Fields

		private readonly MainWindowVM vm;
		private bool selectLock;

		#endregion Private Fields

		#region Public Constructors

		public MainWindow()
		{
			InitializeComponent();
		}

		public MainWindow(MainWindowVM vm) : this()
		{
			this.vm = vm;
			DataContext = vm;
			Loaded += MainWindow_Loaded;
		}

		#endregion Public Constructors

		#region Private Methods

		private async void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			await vm.CurrentItemSelectedAsync(sender, e).ConfigureAwait(false);
		}

		private async void LoginGoogleDriveButton_Click(object sender, RoutedEventArgs e)
		{
			await vm.LoginGoogleDriveAsync().ConfigureAwait(false);
		}

		private async void LoginOneDriveButton_Click(object sender, RoutedEventArgs e)
		{
			await vm.LoginOneDriveAsync().ConfigureAwait(false);
		}

		private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
		{
			await vm.InitializeAsync().ConfigureAwait(false);
		}

		private void SpinnerBorder_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			vm.CancelCurrentTask();
		}

		private async void TreeViewItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is TreeViewItem treeViewItem) ||
				!(treeViewItem.DataContext is ItemVM itemVM)) {
				throw new InvalidOperationException();
			}
			if (e.RightButton == MouseButtonState.Pressed) {
				switch (itemVM.Item.DriveType) {
					case DriveTypes.OneDrive:
						await vm.LogoutOneDriveAsync(itemVM.Item).ConfigureAwait(false);
						break;
					case DriveTypes.GoogleDrive:
						await vm.LogoutGoogleDriveAsync(itemVM.Item).ConfigureAwait(false);
						break;
					default:
						break;
				}
			}
		}

		private async void TreeViewItem_Selected(object sender, RoutedEventArgs e)
		{
			if (vm.IsBusy) {
				return;
			}
			await vm.TreeItemSelectedAsync(sender, e).ConfigureAwait(false);
			(sender as TreeViewItem).BringIntoView();
		}

		#endregion Private Methods

		private void AddDriveButton_Click(object sender, RoutedEventArgs e)
		{
			var popup = new AddDrivePopupWindow(vm)
			{
				Owner = this
			};
			popup.ShowDialog();
		}

		private async void PathButton_Click(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListBoxItem listBoxItem) ||
				!(listBoxItem.DataContext is ItemVM itemVM)) {
				return;
			}
			await itemVM.SetIsSelectedAsync(true).ConfigureAwait(true);
		}

		private async void ListBoxItem_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!(sender is ListBoxItem listBoxItem) ||
				!(listBoxItem.DataContext is ItemVM itemVM)) {
				return;
			}
			if (e.LeftButton == MouseButtonState.Pressed) {
				await itemVM.SetIsSelectedAsync(true).ConfigureAwait(true);
			}
		}

		private async void ListBoxItem_Selected(object sender, RoutedEventArgs e)
		{
			if (!(sender is ListBoxItem listBoxItem) ||
				!(listBoxItem.DataContext is ItemVM itemVM)) {
				return;
			}
			await itemVM.SetIsSelectedAsync(true).ConfigureAwait(true);
		}
		/// <summary>
		/// somehow listbox selectedItem is not consistent with its datacontext...
		/// </summary>
		[Obsolete]
		private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (selectLock) {
				return;
			}
			var listbox = (sender as ListBox);
			var selected = listbox.Items.Cast<ItemVM>().FirstOrDefault(vm => vm.IsSelected);
			selectLock = true;
			listbox.SelectedItem = selected;
			selectLock = false;
		}
	}
}