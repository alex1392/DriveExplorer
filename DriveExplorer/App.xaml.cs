using Cyc.GoogleApi;
using Cyc.MicrosoftApi;
using Cyc.Standard;
using DriveExplorer.Models;
using DriveExplorer.ViewModels;
using DriveExplorer.Views;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Windows;

namespace DriveExplorer {

	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application {
		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);

			var services = new ServiceCollection();
			ConfigureServices(services);
			var serviceProvider = services.BuildServiceProvider();

			MainWindow = serviceProvider.GetService<MainWindow>();
			MainWindow.Show();
		}

		private void ConfigureServices(ServiceCollection services) {
			services.AddSingleton<ILogger, MessageBoxLogger>();
			
			services.AddSingleton<GoogleApiManager>();
			services.AddSingleton<GoogleDriveItem.Factory>();
			services.AddSingleton<GoogleDriveManager>();
			
			services.AddSingleton<MicrosoftApiManager>();
			services.AddSingleton<OneDriveItem.Factory>();
			services.AddSingleton<OneDriveManager>();
			
			services.AddSingleton<LocalItem.Factory>();
			services.AddSingleton<LocalDriveManager>();
			
			services.AddSingleton<MainWindowVM>();
			services.AddSingleton<MainWindow>();
		}
	}
}
