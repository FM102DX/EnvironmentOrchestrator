using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ActivityScheduler;

namespace ActivityScheduler.Core
{
    /// <summary>
    /// Interaction logic for TrayContextMenu.xaml
    /// </summary>
    public partial class TrayContextMenu : Window
    {
        public App _app;
        
        public TrayContextMenu(App app)
        {
            _app = app;
            InitializeComponent();
        }

        private void Menu_Open(object sender, RoutedEventArgs e)
        {
            _app.ShowMainWindow();
        }
        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            _app.HideMainWindow();
        }
        private void Menu_Close(object sender, RoutedEventArgs e)
        {
            _app.Shutdown();
        }

    }
}
