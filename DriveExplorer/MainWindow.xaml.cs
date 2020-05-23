using DriveExplorer.IoC;
using System.Collections.Generic;
using System.Text;
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
            DataContext = vm;
        }

        private async void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
            await vm.TreeViewItem_SelectedAsync(sender, e);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            vm.ListBoxItem_SelectedAsync(sender, e);
        }

        private async void OneDriveButton_Click(object sender, RoutedEventArgs e) {
            await vm.LoginOneDriveAsync();
        }
    }
}
