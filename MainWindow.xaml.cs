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
using ActivityScheduler.Shared.Service;
using System.Security.Policy;
using System.Windows.Forms;
using ActivityScheduler.Shared.Pipes;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ActivityScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private ActivityScheduler.Core.Settings.Settings settingsFrm;
        private ActivitySchedulerApp _app;
        private WorkerServiceManager _workerMgr;
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private SettingsManager _settingsManager;
        private List<Batch> _batchList;
        private Batch _currentBatch;
        private ActivityManager _activityManager;
        private FormStateHolder formStateHolder = new FormStateHolder();
        private SelectionMode _selectionMode = SelectionMode.None;
        private ClientCommunicationObjectT<WorkerToAppMessage> _pipeClient;
        private ServerCommunicationObjectT<AppToWorkerMessage> _pipeServer;
        private readonly System.Timers.Timer _timer;
        public MainWindow(SettingsManager settingsManager, ActivitySchedulerApp app, WorkerServiceManager workerMgr, BatchManager batchManager, ActivityManager activityManager, Serilog.ILogger logger, ServerCommunicationObjectT<AppToWorkerMessage> pipeServer, ClientCommunicationObjectT<WorkerToAppMessage> pipeClient)
        {
            _settingsManager = settingsManager;
            _app = app;
            _workerMgr = workerMgr;
            _logger = logger;
            _batchManager = batchManager;
            _activityManager = activityManager;
            _pipeClient = pipeClient;
            _pipeServer = pipeServer;

            InitializeComponent();

            formStateHolder.CreateFormState("normal").AddAction(() => {
                NameTxt.Visibility = Visibility.Hidden;
                NumberTxt.Visibility = Visibility.Hidden;
                BatchName.Visibility = Visibility.Visible;
                BatchNumber.Visibility = Visibility.Visible;
            }).Parent.CreateFormState("isgroup").AddAction(() => {
                NameTxt.Visibility = Visibility.Visible;
                NumberTxt.Visibility = Visibility.Visible;
                BatchName.Visibility = Visibility.Visible;
                BatchNumber.Visibility = Visibility.Visible;
            }).Parent.CreateFormState("none").AddAction(() => {
                NameTxt.Visibility = Visibility.Hidden;
                NumberTxt.Visibility = Visibility.Hidden;
                BatchName.Visibility = Visibility.Hidden;
                BatchNumber.Visibility = Visibility.Hidden;
            });

            _timer = new System.Timers.Timer(500) { AutoReset = true };
            _timer.Elapsed += ProcessMessages;
        }
       
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tag = "Closed";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadBatchList();
            _timer.Start();
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
            //select first element
            BatchList.SelectedIndex=0;
        }

        private void BatchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item =  e.AddedItems.Cast<ListBoxItem>().ToList().FirstOrDefault();
            if (item == null) { return; }
            if (item.Tag== "none") 
            {
                _selectionMode= SelectionMode.None;
                formStateHolder.SetFormState("none");
                return; 
            }
            Batch btc = (Batch)item.Tag;
            _currentBatch = btc;
            BatchNumber.IsReadOnly = true;
            BatchName.IsReadOnly = true;
            BatchNumber.Text = "";
            BatchName.Text = "";
            if (btc.IsGroup)
            {
                _selectionMode = SelectionMode.Group;
                formStateHolder.SetFormState("isgroup");
                BatchNumber.Text = btc.Number;
                BatchName.Text = btc.Name;
            }
            else
            {
                _selectionMode = SelectionMode.RealBatch;
                formStateHolder.SetFormState("normal");
                BatchNumber.Text = btc.Number;
                BatchName.Text = btc.Name;
            }
        }

        private void EditBatch_Click(object sender, RoutedEventArgs e)
        {
            OpenEditBatchForm();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            settingsFrm = new Core.Settings.Settings(_settingsManager, _app, _workerMgr);
            settingsFrm.ShowDialog();
        }

        private void NewBatch_Click(object sender, RoutedEventArgs e)
        {
            Batch btc = new Batch();
            btc.Number = "000000";
            btc.Name = "New.batch";
            var rez = _batchManager.AddNewBatch(btc).Result;
            LoadBatchList();
        }

        private void DeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            if (_selectionMode == SelectionMode.None) { return; }
            if (_currentBatch == null) { return; }
            var rez = _batchManager.RemoveBatch(_currentBatch.Id).Result;
            if (!rez.Success)
            {
                ShowRed($"{rez.Message}");
            }
            LoadBatchList();
        }


        private void ShowRed(string text)
        {
            // InfoTb.font
            InfoTb.Text = text;
            InfoTb.Foreground = Brushes.Red;
        }
        private void ShowGreen(string text)
        {
            // InfoTb.font
            InfoTb.Text = text;
            InfoTb.Foreground = Brushes.Green;
        }

        private void BatchList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenEditBatchForm();
        }

        private void OpenEditBatchForm()
        {
            if (_selectionMode != SelectionMode.None)
            {
                EditBatch editBatch = new EditBatch(this, _batchManager, _activityManager, _currentBatch, _logger);
                editBatch.Show();
            }
        }

        private void NewGroup_Click(object sender, RoutedEventArgs e)
        {
            Batch btc = new Batch();
            btc.Number = "000000";
            btc.Name = "New.group";
            btc.IsGroup = true;
            var rez = _batchManager.AddNewBatch(btc).Result;
            LoadBatchList();
        }

        private enum SelectionMode
        {
            None=1,
            Group=2,
            RealBatch=3
        }

        private void RunBatch_Click(object sender, RoutedEventArgs e)
        {

            if (_currentBatch == null) return;

            //send message to service

            _pipeServer.SendObject(new AppToWorkerMessage()
            {
                Command = "startbatch",
                StartTime = DateTime.Now,  
                TransactionId = _currentBatch.Number
            });
        }


        private void AddBatchTbLine(string text)
        {
            InfoRunBatchTb.Dispatcher.Invoke(() =>
            {
                InfoRunBatchTb.Text += text;
                InfoRunBatchTb.Text += System.Environment.NewLine;
            });
        }

        private void ProcessMessages(object? sender, ElapsedEventArgs e)
        {
            WorkerToAppMessage? m = _pipeClient.Take();

            if (m == null) { AddBatchTbLine("Got null"); return; }

            AddBatchTbLine($"Command={m.Command} number = {_pipeClient.StackCount}");

            if (string.IsNullOrEmpty(m.Command)) return;

            if (m.Command.ToLower() == "ping")
            {
                AddBatchTbLine(m.Message);
            }
            Task.Delay(100);
        }
    }
}
