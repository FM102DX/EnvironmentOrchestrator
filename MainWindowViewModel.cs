using ActivityScheduler.Core;
using ActivityScheduler.Core.Appilcation;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Data.Models.Settings;
using ActivityScheduler.Shared.Pipes;
using ActivityScheduler.Shared.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ActivityScheduler
{
    public class MainWindowViewModel : INotifyPropertyChanged, INotifyCollectionChanged
    {
        private ActivityScheduler.Core.Settings.Settings settingsFrm;
        private ActivitySchedulerApp _app;
        private WorkerServiceManager _workerMgr;
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private SettingsManager _settingsManager;
        private List<Batch> _batchList;
        private BatchListBoxViewModel _selectedItem;
        public BatchListBoxViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                _selectedItem = value;

                if (_selectedItem == null)
                {
                    SelectionModeVar = SelectionMode.None;
                    CurrentBatch = null;
                    return;
                }

                CurrentBatch = _batchList.FirstOrDefault(x => x.Id == _selectedItem.Id);

                if (CurrentBatch == null) return;

                if (CurrentBatch.IsGroup)
                {
                    SelectionModeVar = SelectionMode.Group;
                }
                else
                {
                    SelectionModeVar = SelectionMode.RealBatch;
                }
                //PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectionModeVar"));
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentBatch"));

                if (SelectionModeChanged != null)
                {
                    SelectionModeChanged(SelectionModeVar);
                }
            }
        }
        public Batch? CurrentBatch { get; set; }
        private ActivityManager _activityManager;

        public  SelectionMode SelectionModeVar { get; set; }= SelectionMode.None;
        private ClientCommunicationObjectT<WorkerToAppMessage> _pipeClient;
        private ServerCommunicationObjectT<AppToWorkerMessage> _pipeServer;
        private readonly System.Timers.Timer _timer;
        private List<string> _runningBatches = new List<string>();
        
        public string MakitaTextBox2=> MakitaTextBox;
        public string MakitaTextBox { get; set; } = "Initial text";

        public string InfoRunBatchText { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        public event NotifyCollectionChangedEventHandler? CollectionChanged;

        public ObservableCollection<BatchListBoxViewModel> BatchListItemSource { get; set; }

        public ICommand CreateBatchCmd { get; private set; }
        public ICommand TestCmd { get; private set; }
        public ICommand TestCmd2 { get; private set; }
        public ICommand TestCmd3 { get; private set; }
        public ICommand CreateGroupCmd { get; private set; }
        public ICommand DeleteBatchOrGroupCmd { get; private set; }
        public ICommand OpenSettingsFrmCmd { get; private set; }
        
        public ICommand EditBatchCmd { get; private set; }
        public ICommand RunBatchCmd { get; private set; }
        public ICommand StopBatchCmd { get; private set; }
        public ICommand LoadBatchListCmd { get; private set; }

        public event SelectionModeChangedDelegate SelectionModeChanged;
        
        public delegate void SelectionModeChangedDelegate(SelectionMode selectionMode);

        public MainWindowViewModel(
                                        SettingsManager settingsManager, 
                                        ActivitySchedulerApp app, 
                                        WorkerServiceManager workerMgr, 
                                        BatchManager batchManager, 
                                        ActivityManager activityManager, 
                                        Serilog.ILogger logger, 
                                        ServerCommunicationObjectT<AppToWorkerMessage> pipeServer, 
                                        ClientCommunicationObjectT<WorkerToAppMessage> pipeClient
            ) 
        {

            _settingsManager = settingsManager;
            _app = app;
            _workerMgr = workerMgr;
            _logger = logger;
            _batchManager = batchManager;
            _activityManager = activityManager;
            _pipeClient = pipeClient;
            _pipeServer = pipeServer;

            _timer = new System.Timers.Timer(500) { AutoReset = true };
            _timer.Elapsed += ProcessMessages;

            BatchListItemSource = new ObservableCollection<BatchListBoxViewModel>();

            CreateBatchCmd = new ActionCommand(() => {
                Batch btc = new Batch();
                btc.Number = "000000";
                btc.Name = "New.batch";
                var rez = _batchManager.AddNewBatch(btc).Result;
                MessageBox.Show($"Adding batch, rez success = {rez.Success}, message={rez.Message}");
                LoadBatchList();
            });

            CreateGroupCmd = new ActionCommand(() => {
                Batch btc = new Batch();
                btc.Number = "000000";
                btc.Name = "New.group";
                btc.IsGroup = true;
                var rez = _batchManager.AddNewBatch(btc).Result;
                LoadBatchList();
            });

            DeleteBatchOrGroupCmd = new ActionCommand(() =>
            {
                if (SelectionModeVar == SelectionMode.None) { return; }
                if (CurrentBatch == null) { return; }
                var rez = _batchManager.RemoveBatch(CurrentBatch.Id).Result;
                if (!rez.Success)
                {
                    MessageBox.Show(rez.AsShrotString());
                }
            });

            OpenSettingsFrmCmd = new ActionCommand(() => {
                settingsFrm = new Core.Settings.Settings(_settingsManager, _app, _workerMgr);
                settingsFrm.ShowDialog();
            });

            EditBatchCmd = new ActionCommand(() => {
                if (SelectionModeVar != SelectionMode.None && CurrentBatch!=null)
                {
                    EditBatch editBatch = new EditBatch(_batchManager, _activityManager, CurrentBatch, _logger);
                    editBatch.Show();
                }
            });

            RunBatchCmd = new ActionCommand(() => {
                if (SelectionModeVar != SelectionMode.None && CurrentBatch != null)
                {
                    _pipeServer.SendObject(new AppToWorkerMessage()
                    {
                        MessageType = "Command",
                        Command = "startbatch",
                        StartTime = DateTime.Now,
                        TransactionId = CurrentBatch.Number
                    });
                }
            });

            LoadBatchListCmd = new ActionCommand(() => {
                LoadBatchList();
            });

            _timer.Start();
            // LoadBatchList();
        }

        private string GetRandomNumberString()
        {
            var random = new Random();
            return random.Next(100, 200).ToString();
        }

        private bool IsBatchRunning(string number)
        {
            var items = _runningBatches.Where(x => x == number).ToList();
            return items.Count > 0;
        }

        public void LoadBatchList()
        {
            BatchListItemSource.Clear();
            _batchList = _batchManager.GetAll().Result.OrderBy(x => x.Number).ToList();
            _batchList.ForEach(x => {
                string src = IsBatchRunning(x.Number) ? _app.PlayIconFullPath : _app.NoneIconFullPath;
                BatchListItemSource.Add(new BatchListBoxViewModel() { Id = x.Id, BatchNumber = x.Number, IsGroup = x.IsGroup, Text = x.Name, BatchObject = x, ImageSource = src });
            });

            //select first record
            if(BatchListItemSource.Count>0)
            {
                SelectedItem = BatchListItemSource[0];
            }
        }

        private void ArrangeBatchListRunningStatus()
        {
            foreach(var x in BatchListItemSource) 
            {
                string src = IsBatchRunning(x.BatchNumber) ? _app.PlayIconFullPath : _app.NoneIconFullPath;
                x.ImageSource = src;
            }
        }

        private void AddBatchTbLine(string text)
        {
            InfoRunBatchText = text+ System.Environment.NewLine+InfoRunBatchText;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InfoRunBatchText"));
        }

        private void ProcessMessages(object? sender, ElapsedEventArgs e)
        {
            WorkerToAppMessage? m = _pipeClient.Take();

            if (m == null) { AddBatchTbLine("Got null"); return; }

            if (m.MessageType.ToLower() == "runningbatchesinfo")
            {
                if (m.RunningBatches.Batches.Count == 0)
                {
                    AddBatchTbLine("Batches running: none");
                }
                else
                {
                    _runningBatches = new List<string>();

                    m.RunningBatches.Batches.ForEach(x => _runningBatches.Add(x));

                    AddBatchTbLine($"Batches running: {string.Join(",", _runningBatches)}");

                    ArrangeBatchListRunningStatus();

                    //Tabs.Dispatcher.Invoke(() =>
                    //{
                    //    foreach (var x in m.RunningBatches.Batches)
                    //    {
                    //        //check if tab with this name exists

                    //        var tab1 = Tabs.Items[0];

                    //        var lst1 = Tabs.Items.OfType<TabItem>().ToList();

                    //        var lst2 = lst1.Where(y => y.Header.ToString() == x).ToList();

                    //        if (lst2.Count == 0)
                    //        {
                    //            //tab not opened, need to open
                    //            var tbi = new TabItem();
                    //            tbi.Header = x;
                    //            //tbi.Name = x.ToString();

                    //            StackPanel stackPanel = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
                    //            stackPanel.Children.Add(new System.Windows.Controls.TextBox { Height = 60, Width = 60, TextWrapping = TextWrapping.Wrap, Text = "Some text" });

                    //            var listBox = new System.Windows.Controls.ListBox();
                    //            listBox.Items.Add(new ListBoxItem { Content = "Text" });
                    //            listBox.Items.Add(new ListBoxItem { Content = "Text to" });
                    //            listBox.Items.Add(new ListBoxItem { Content = "And text" });

                    //            StackPanel stackPanelAsContent = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical, HorizontalAlignment = 0 };
                    //            stackPanelAsContent.Children.Add(stackPanel);
                    //            stackPanelAsContent.Children.Add(listBox);
                    //            tbi.Content = stackPanelAsContent;
                    //            Tabs.Items.Add(tbi);
                    //        }
                    //    }
                    //});
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

        public enum SelectionMode
        {
            None = 1,
            Group = 2,
            RealBatch = 3
        }
    }
}
