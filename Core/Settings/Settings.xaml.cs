using ActivityScheduler.Core.Appilcation;
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
        private SettingsManager _settingsManager;
        private ActivitySchedulerApp _app;
        public Settings(SettingsManager settingsManager, ActivitySchedulerApp app)
        {
            _settingsManager = settingsManager;
            _app = app;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void GetStateBtn_Click(object sender, RoutedEventArgs e)
        {
            StateLbl.Text = _app.GetServiceState();
        }

        private void InstallBtn_Click(object sender, RoutedEventArgs e)
        {
            _app.InstallService();
        }

        private void UninstallBtn_Click(object sender, RoutedEventArgs e)
        {
            _app.UninstallService();
        }

        private void RunBtn_Click(object sender, RoutedEventArgs e)
        {
            _app.StartService();
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            _app.StopService();
        }
    }
}
