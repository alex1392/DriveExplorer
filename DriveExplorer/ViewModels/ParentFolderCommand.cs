using DriveExplorer.Models;
using System;
using System.Windows.Input;

namespace DriveExplorer.ViewModels {
	public class ParentFolderCommand : ICommand {
		private readonly NavigationManager<ItemVM> navigationManager;

		public event EventHandler CanExecuteChanged;
		public ParentFolderCommand(NavigationManager<ItemVM> navigationManager)
		{
			this.navigationManager = navigationManager;
			navigationManager.CurrentChanged += (_, _) => CanExecuteChanged?.Invoke(this, null);
		}
		public bool CanExecute(object parameter)
		{
			var vm = navigationManager.Current;
			return vm?.Parent != null;
		}

		public async void Execute(object parameter)
		{
			var vm = navigationManager.Current;
			await vm.Parent.SetIsSelectedAsync(true).ConfigureAwait(false);
		}
	}
}