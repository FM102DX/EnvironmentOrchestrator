using ActivityScheduler.Core;
using ActivityScheduler.Core.Appilcation;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.Models.Communication;
using ActivityScheduler.Data.Models.Settings;
using ActivityScheduler.Gui.EditWindow;
using ActivityScheduler.Shared.Pipes;
using ActivityScheduler.Shared.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace ActivityScheduler.Gui.MainWindow
{
    public class MainWindowViewModel : INotifyPropertyChanged
    {
        private Core.Settings.Settings settingsFrm;
        private ActivitySchedulerApp _app;
        private WorkerServiceManager _workerMgr;
        private BatchManager _batchManager;
        private Serilog.ILogger _logger;
        private SettingsManager _settingsManager;
        private List<Batch> _batchList;
        private List<Batch> _runningBatchList;
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

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentBatch"));

                UpdateSelectionMode();
            }
        }

        private void UpdateSelectionMode()
        {
            if (CurrentBatch == null) return;

            if (CurrentBatch.IsGroup)
            {
                SelectionModeVar = SelectionMode.Group;
            }
            else
            {
                if (IsBatchRunning(CurrentBatch.Number))
                {
                    SelectionModeVar = SelectionMode.RealBatchRunning;
                }
                else
                {
                    SelectionModeVar = SelectionMode.RealBatchStopped;
                }
            }

            if (SelectionModeChanged != null)
            {
                SelectionModeChanged(SelectionModeVar);
            }
        }

        public Batch? CurrentBatch { get; set; }
        private ActivityManager _activityManager;

        public SelectionMode SelectionModeVar { get; set; } = SelectionMode.None;
        private ClientCommunicationObjectT<WorkerToAppMessage> _pipeClient;
        private ServerCommunicationObjectT<AppToWorkerMessage> _pipeServer;
        private readonly Timer _timer;
        private List<string> _runningBatchesNumberList = new List<string>();
        public string InfoRunBatchText { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public event Action ListSourceChanged;
        public ObservableCollection<BatchListBoxViewModel> BatchListItemSource { get; set; }
        public ICommand CreateBatchCmd { get; private set; }
        public ICommand TestCmd { get; private set; }
        public ICommand CreateGroupCmd { get; private set; }
        public ICommand DeleteBatchOrGroupCmd { get; private set; }
        public ICommand OpenSettingsFrmCmd { get; private set; }
        public ICommand EditBatchCmd { get; private set; }
        public ICommand RunBatchCmd { get; private set; }
        public ICommand StopBatchCmd { get; private set; }
        public ICommand LoadBatchListCmd { get; private set; }

        public event SelectionModeChangedDelegate SelectionModeChanged;
        public delegate void SelectionModeChangedDelegate(SelectionMode selectionMode);

        public event RunningBatchesInfoUpdatedDelegate RunningBatchesInfoUpdated;
        public delegate void RunningBatchesInfoUpdatedDelegate(List<Batch> runningBatchList);


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

            _timer = new Timer(500) { AutoReset = true };
            _timer.Elapsed += ProcessMessages;

            BatchListItemSource = new ObservableCollection<BatchListBoxViewModel>();

            CreateBatchCmd = new ActionCommand(() =>
            {
                Batch btc = new Batch();
                btc.Number = "000000";
                btc.Name = "New.batch";
                var rez = _batchManager.AddNewBatch(btc).Result;
                MessageBox.Show($"Adding batch, rez success = {rez.Success}, message={rez.Message}");
                LoadBatchList();
            });

            CreateGroupCmd = new ActionCommand(() =>
            {
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

            OpenSettingsFrmCmd = new ActionCommand(() =>
            {
                settingsFrm = new Core.Settings.Settings(_settingsManager, _app, _workerMgr);
                settingsFrm.ShowDialog();
            });

            EditBatchCmd = new ActionCommand(() =>
            {
                if (SelectionModeVar != SelectionMode.None && CurrentBatch != null)
                {
                    EditBatch editBatch = new EditBatch(new EditWindowViewModel(_batchManager, _activityManager, CurrentBatch, _logger, this));
                    //EditBatch editBatch = new EditBatch(_batchManager, _activityManager, CurrentBatch, _logger);
                    editBatch.Show();
                }
            });

            RunBatchCmd = new ActionCommand(() =>
            {
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

            StopBatchCmd = new ActionCommand(() =>
            {
                if (SelectionModeVar != SelectionMode.None && CurrentBatch != null)
                {
                    _pipeServer.SendObject(new AppToWorkerMessage()
                    {
                        MessageType = "Command",
                        Command = "stopbatch",
                        StartTime = DateTime.Now,
                        TransactionId = CurrentBatch.Number
                    });
                }

            });

            LoadBatchListCmd = new ActionCommand(() =>
            {
                LoadBatchList();
            });

            TestCmd = new ActionCommand(() =>
            {
                ArrangeBatchListRunningStatus();
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
            var items = _runningBatchesNumberList.Where(x => x == number).ToList();
            return items.Count > 0;
        }

        public void LoadBatchList()
        {
            BatchListItemSource.Clear();
            _batchList = _batchManager.GetAll().Result.OrderBy(x => x.Number).ToList();
            _batchList.ForEach(x =>
            {
                // string src = IsBatchRunning(x.Number) ? _app.PlayIconFullPath : _app.NoneIconFullPath;
                string src = "";
                BatchListItemSource.Add(new BatchListBoxViewModel() { Id = x.Id, BatchNumber = x.Number, IsGroup = x.IsGroup, Text = x.Name, BatchObject = x, ImageSource = src });
            });

            ArrangeBatchListRunningStatus();

            //select first record
            if (BatchListItemSource.Count > 0)
            {
                SelectedItem = BatchListItemSource[0];
            }
        }

        private void ArrangeBatchListRunningStatus()
        {
            foreach (var x in BatchListItemSource)
            {
                string src = IsBatchRunning(x.BatchNumber) ? _app.PlayIconFullPath : _app.NoneIconFullPath;
                x.Text = $"_{GetRandomNumberString()}";
                x.ImageSource = src;
            }

            ListSourceChanged();

            UpdateSelectionMode();

            // if (SelectedItem != null) { SelectedItem = SelectedItem; }
        }

        private void AddBatchTbLine(string text)
        {
            InfoRunBatchText = text + Environment.NewLine + InfoRunBatchText;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("InfoRunBatchText"));
        }

        private void ProcessMessages(object? sender, ElapsedEventArgs e)
        {
            WorkerToAppMessage? m = _pipeClient.Take();

            if (m == null) { AddBatchTbLine("Got null"); return; }

            if (m.MessageType.ToLower() == "runningbatchesinfo")
            {
                _runningBatchesNumberList = m.RunningBatches.Batches;

                ArrangeBatchListRunningStatus();

                if (m.RunningBatches.Batches.Count == 0)
                {
                    AddBatchTbLine("Batches running: none");
                    _runningBatchList = new List<Batch>();
                }
                else
                {
                    AddBatchTbLine($"Batches running: {string.Join(",", _runningBatchesNumberList)}");
                    _runningBatchList = _batchList.Where(x => _runningBatchesNumberList.Contains(x.Number)).ToList();
                }
                RunningBatchesInfoUpdated(_runningBatchList);
            }

            if (m.MessageType.ToLower() == "CommandExecutionResult".ToLower())
            {
                if (!m.Result.Success)
                {
                    MessageBox.Show($"Unsuccessful operation: {m.Result.Message}");
                }
            }
            Task.Delay(100);
        }

        public enum SelectionMode
        {
            None = 1,
            Group = 2,
            RealBatchRunning = 3,
            RealBatchStopped = 4

        }
    }
}
