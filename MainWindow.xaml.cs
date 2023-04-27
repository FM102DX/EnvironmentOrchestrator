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
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.DataAccess;

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
        private List<Batch> _batchList;
        private Batch _selectedItem;
        private ActivityManager _activityManager;
        public MainWindow(SettingsManager settingsManager, ActivitySchedulerApp app, WorkerServiceManager workerMgr, BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger)
        {
            _settingsManager = settingsManager;
            _app = app;
            _workerMgr = workerMgr;
            _logger = logger;
            _batchManager = batchManager;
            _activityManager = activityManager;
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
            NewBatch btc = new NewBatch(this, _batchManager, _logger);
            btc.ShowDialog();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBatchList();
        }

        public void LoadBatchList()
        {
            BatchList.Items.Clear();
            _batchList = _batchManager.GetAll().Result.OrderBy(x=>x.Number).ToList();


            _batchList.ForEach(x => {

                    if (x.IsGroup && (BatchList.Items.Count!=0)) { BatchList.Items.Add(new ListBoxItem() { Tag = "none", Content = $"" });}
                    var lstI = new ListBoxItem() { Tag = x, Content = $"{x.Number}--{x.Name}" };
                    if (x.IsGroup) { lstI.FontWeight = FontWeights.Bold; }
                    BatchList.Items.Add(lstI);
                });

        }

        private void BatchList_Selected(object sender, RoutedEventArgs e)
        {

        }

        private void BatchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = e.AddedItems.Cast<ListBoxItem>().ToList().FirstOrDefault();

            Batch btc = (Batch)item.Tag;
            _selectedItem = btc;
            BatchNumber.IsReadOnly = true;
            BatchName.IsReadOnly = true;
            IsGroup.IsHitTestVisible = false;
            BatchNumber.Text = "";
            BatchName.Text = "";
            IsGroup.IsChecked = false;
            if (btc.IsGroup)
            {
                BatchNumber.Text = btc.Number;
                BatchName.Text = btc.Name;
                IsGroup.IsChecked = btc.IsGroup;
            }

            //MessageBox.Show($"{btc.Number}");
        }

        private void EditBatch_Click(object sender, RoutedEventArgs e)
        {
            EditBatch editBatch = new EditBatch(this, _batchManager, _activityManager, _selectedItem, _logger);
            editBatch.Show();
        }
    }
}
