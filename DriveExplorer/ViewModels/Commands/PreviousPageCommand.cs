using DriveExplorer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DriveExplorer.ViewModels {
	public class PreviousPageCommand : ICommand {
		private readonly NavigationManager<ItemVM> navigationManager;

		public event EventHandler CanExecuteChanged;

		public PreviousPageCommand(NavigationManager<ItemVM> navigationManager)
		{
			this.navigationManager = navigationManager;
			navigationManager.CanGoPreviousChanged += (_, _) => CanExecuteChanged?.Invoke(this, null);
		}

		public bool CanExecute(object parameter)
		{
			return navigationManager.CanGoPrevious;
		}

		public async void Execute(object parameter)
		{
			await navigationManager.GoPreviousAsync(async vm => 
				await vm.SetIsSelectedAsync(true).ConfigureAwait(false)).ConfigureAwait(false);
		}
	}
}
