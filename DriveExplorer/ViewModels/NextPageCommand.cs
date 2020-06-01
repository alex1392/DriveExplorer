using DriveExplorer.ViewModels;
using System;
using System.Windows.Input;

namespace DriveExplorer.Models {
	public class NextPageCommand : ICommand {
		private readonly NavigationManager<ItemVM> navigationManager;

		public event EventHandler CanExecuteChanged;

		public NextPageCommand(NavigationManager<ItemVM> navigationManager)
		{
			this.navigationManager = navigationManager;
			navigationManager.CanGoNextChanged += (_, _) => CanExecuteChanged?.Invoke(this, null);
		}

		public bool CanExecute(object parameter)
		{
			return navigationManager.CanGoNext;
		}

		public async void Execute(object parameter)
		{
			var vm = navigationManager.GoNext();
			navigationManager.AddLock = true;
			await vm.SetIsSelectedAsync(true).ConfigureAwait(false);
			navigationManager.AddLock = false;
		}
	}
}
