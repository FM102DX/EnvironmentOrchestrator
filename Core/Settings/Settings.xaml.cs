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

namespace ActivityScheduler.Core.Settings
{
    public partial class Settings : Window
    {
        SettingsManager _settingsManager;
        public Settings(SettingsManager settingsManager)
        {
            _settingsManager = settingsManager;
            InitializeComponent();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            var settings = new SettingsData();

            settings.Setting01 = Setting1Tb.Text;

            if ((bool)Setting2Chbx.IsChecked) { settings.Setting02 = true; } else { settings.Setting02 = false; }
            
            _settingsManager.SaveSettings(settings);

            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var settings = _settingsManager.GetSettings();

            Setting1Tb.Text = settings.Setting01;
            
            Setting2Chbx.IsChecked = settings.Setting02;
        }
    }
}
