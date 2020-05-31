using Cyc.Standard;

using DriveExplorer.Models;
using DriveExplorer.ViewModels;
using DriveExplorer.Views;

using Microsoft.Extensions.DependencyInjection;

using System.Windows;

namespace DriveExplorer {

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {

		#region Protected Methods

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			var services = new ServiceCollection();
			ConfigureServices(services);
			var serviceProvider = services.BuildServiceProvider();

			MainWindow = serviceProvider.GetService<MainWindow>();
			MainWindow.Show();
		}

		#endregion Protected Methods

		#region Private Methods

		private void ConfigureServices(ServiceCollection services)
		{
			services.AddSingleton<IDispatcher, ApplicationDispatcher>();
			services.AddSingleton<ILogger, MessageBoxLogger>();

			//services.AddSingleton<GoogleApiManager>();
			//services.AddSingleton<MicrosoftApiManager>();

			//services.AddSingleton<GoogleDriveManager>();
			//services.AddSingleton<OneDriveManager>();

			services.AddSingleton<LocalDriveManager>();

			services.AddSingleton<MainWindowVM>();
			services.AddSingleton<MainWindow>();
		}

		#endregion Private Methods
	}
}