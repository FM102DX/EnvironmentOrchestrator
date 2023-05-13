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
using ActivityScheduler.Data.Models.Settings;
using ActivityScheduler.Data.Models.Communication;
using static System.Net.Mime.MediaTypeNames;

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
        private List<string> _runningBatches = new List<string>();
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
                MessageType="Command",
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
                InfoRunBatchTb.CaretIndex = InfoRunBatchTb.Text.Length;
            });
        }

        private void 

        private void ProcessMessages(object? sender, ElapsedEventArgs e)
        {
            WorkerToAppMessage? m = _pipeClient.Take();

            if (m == null) { AddBatchTbLine("Got null"); return; }

            if (m.MessageType.ToLower() == "runningbatchesinfo")
            {
                if (m.RunningBatches.Batches.Count==0)
                {
                    AddBatchTbLine("Batches running: none");
                }
                else
                {
                    _runningBatches = new List<string>();

                    m.RunningBatches.Batches.ForEach(x=> _runningBatches.Add(x));

                    AddBatchTbLine($"Batches running: {string.Join(",",m.RunningBatches.Batches)}");

                    Tabs.Dispatcher.Invoke(() =>
                    {
                        foreach (var x in m.RunningBatches.Batches)
                        {
                            //check if tab with this name exists

                            var tab1 = Tabs.Items[0];

                            var lst1 = Tabs.Items.OfType<TabItem>().ToList();

                            var lst2 = lst1.Where(y => y.Header.ToString() == x).ToList();

                            if (lst2.Count == 0)
                            {
                                //tab not opened, need to open
                                var tbi = new TabItem();
                                tbi.Header = x;
                                //tbi.Name = x.ToString();

                                StackPanel stackPanel = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
                                stackPanel.Children.Add(new System.Windows.Controls.TextBox { Height = 60, Width = 60, TextWrapping = TextWrapping.Wrap, Text = "Some text" });

                                var listBox = new System.Windows.Controls.ListBox();
                                listBox.Items.Add(new ListBoxItem { Content = "Text" });
                                listBox.Items.Add(new ListBoxItem { Content = "Text to" });
                                listBox.Items.Add(new ListBoxItem { Content = "And text" });

                                StackPanel stackPanelAsContent = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical, HorizontalAlignment = 0 };
                                stackPanelAsContent.Children.Add(stackPanel);
                                stackPanelAsContent.Children.Add(listBox);
                                tbi.Content = stackPanelAsContent;


                                Tabs.Items.Add(tbi);
                            }
                        }
                    });
                }
            }

            if (m.MessageType.ToLower() == "CommandExecutionResult".ToLower())
            {
                if (!m.Result.Success)
                {
                    System.Windows.MessageBox.Show($"Unsuccessful operation: {m.Result.Message}");
                }
            }

            Task.Delay(100);
        }
    }
}
