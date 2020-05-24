using DriveExplorer.IoC;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace DriveExplorer {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {
        private readonly MainWindowVM vm;

        public MainWindow() {
            InitializeComponent();
            vm = IocContainer.Default.GetSingleton<MainWindowVM>();
            vm.GetLocalDrives();
            Task.Run(async () => await vm.GetOneDriveAsync().ConfigureAwait(false)).Wait();
            vm.StartPage();
            DataContext = vm;
        }

        private async void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
            await vm.TreeItem_SelectedAsync(sender, e).ConfigureAwait(false);
        }

        private async void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            await vm.CurrentItem_SelectedAsync(sender, e).ConfigureAwait(false);
        }

        private async void OneDriveButton_Click(object sender, RoutedEventArgs e) {
            await vm.GetOneDriveAsync().ConfigureAwait(false);
        }
    }
}
