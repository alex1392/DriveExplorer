using DriveExplorer.MicrosoftApi;
using DriveExplorer.ViewModels;
using DriveExplorer.Views;

using Microsoft.Extensions.DependencyInjection;

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
			services.AddSingleton<MicrosoftManager>();
			services.AddSingleton<MainWindowVM>();
			services.AddSingleton<MainWindow>();
			services.AddSingleton<ILogger, MessageBoxLogger>();
		}
	}
}
