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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActivityScheduler.Core;
using Serilog;
using System.IO;
using ActivityScheduler.Core.Settings;

namespace ActivityScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ActivityScheduler.Core.Settings.Settings2 settingsFrm;

        SettingsManager _settingsManager;
        public MainWindow(SettingsManager settingsManager)
        {
            InitializeComponent();
            _settingsManager = settingsManager;
            InitializeComponent();
        }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingsFrm = new Core.Settings.Settings2(_settingsManager);
            settingsFrm.ShowDialog();
        }
    }
}
