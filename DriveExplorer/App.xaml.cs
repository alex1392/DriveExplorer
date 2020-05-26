using DriveExplorer.MicrosoftApi;
using Microsoft.Extensions.DependencyInjection;
using System;
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
            services.AddSingleton<IServiceProvider>(sp => sp);

            services.AddSingleton(_ => new AuthProvider(AuthProvider.Authority.Common));
            services.AddSingleton<GraphManager>();
            services.AddSingleton<MainWindowVM>();
            services.AddSingleton<MainWindow>();


            services.AddSingleton<LocalItemFactory>();
            services.AddTransient<LocalItem>();
            services.AddSingleton<OneDriveItemFactory>();
            services.AddTransient<OneDriveItem>();
        }
    }
}
