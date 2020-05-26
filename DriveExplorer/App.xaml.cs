using DriveExplorer.IoC;
using DriveExplorer.MicrosoftApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Application = System.Windows.Application;

namespace DriveExplorer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            var collection = new ServiceCollection();
            collection.AddSingleton<MainWindow>();
            collection.AddTransient<MainWindow>();
            var provider = collection.BuildServiceProvider();
            provider.GetService<MainWindow>();

            var ioc = IocContainer.Default;
            ConfigureServices(ioc);
            
        }

        private void ConfigureServices(IocContainer ioc) {
            ioc.Register(() => AuthProvider.Default);
            ioc.Register<GraphManager>();
            ioc.Register<MainWindowVM>();
            ioc.Register<OneDriveItem>();
            ioc.Register<MainWindow>();
        }
    }
}
