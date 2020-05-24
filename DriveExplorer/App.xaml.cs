using DriveExplorer.IoC;
using DriveExplorer.MicrosoftApi;
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
            IocContainer.Default.Register(() => AuthProvider.Default);
            IocContainer.Default.Register<GraphManager>();
            IocContainer.Default.Register<MainWindowVM>();
            IocContainer.Default.Register<LocalItemFactory>();
            IocContainer.Default.Register<OneDriveItemFactory>();
            IocContainer.Default.Register<OneDriveItem>();



        }
    }
}
