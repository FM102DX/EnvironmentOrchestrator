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
using ActivityScheduler.Core.Appilcation;
using ActivityScheduler.Data.Managers;

namespace ActivityScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ActivityScheduler.Core.Settings.Settings settingsFrm;
        private ActivitySchedulerApp _app;
        private WorkerServiceManager _workerMgr;
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private SettingsManager _settingsManager;
        public MainWindow(SettingsManager settingsManager, ActivitySchedulerApp app, WorkerServiceManager workerMgr, BatchManager batchManager, Serilog.ILogger logger)
        {
            _settingsManager = settingsManager;
            _app = app;
            _workerMgr = workerMgr;
            _logger = logger;
            _batchManager = batchManager;
            InitializeComponent();
        }
       
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            settingsFrm = new Core.Settings.Settings(_settingsManager, _app, _workerMgr);
            settingsFrm.ShowDialog();
        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tag = "Closed";
        }

        private void NewBatch_Click(object sender, RoutedEventArgs e)
        {
            NewBatch btc = new NewBatch(_batchManager, _logger);
            btc.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var lst = _batchManager.GetAll().Result;
            BatchList.Items.Add(lst);
            
        }
    }
}
