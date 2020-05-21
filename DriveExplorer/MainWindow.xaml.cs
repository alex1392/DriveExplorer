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
        private MainWindowVM vm;

        public MainWindow() {
            InitializeComponent();
            vm = DataContext as MainWindowVM;
        }

        private void TreeViewItem_Selected(object sender, RoutedEventArgs e) {
            vm.TreeViewItem_Selected(sender, e);
        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) {
            vm.ListBoxItem_Selected(sender, e);
        }
    }
}
