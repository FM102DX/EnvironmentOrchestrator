using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Service;
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

namespace ActivityScheduler.Core
{
    /// <summary>
    /// Interaction logic for NewBatch.xaml
    /// </summary>
    public partial class EditBatch : Window
    {
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Activity> _repo;
        private BatchManager _batchManager;
        private ActivityManager _activityManager;
        private MainWindow _mainWindow;
        private FormStateHolder formStateHolder = new FormStateHolder();
        private Batch _currentBatch;
        public EditBatch(MainWindow mainWindow, BatchManager batchManager,  ActivityManager activityManager,  Batch currentBatch, Serilog.ILogger logger)
        {
            _logger = logger;
            _batchManager = batchManager;
            _mainWindow = mainWindow;
            _currentBatch= currentBatch;
            _activityManager = activityManager;
            formStateHolder.CreateFormState("isgroup").AddAction(() => { 
                BatchName.Visibility = Visibility.Hidden;
                NumberTb.Visibility = Visibility.Hidden;
                IsHub.Visibility = Visibility.Hidden;
                TransactionIdTb.Visibility = Visibility.Hidden;
                Starttime.Visibility = Visibility.Hidden;
            }).Parent.CreateFormState("normal").AddAction(() => {
                BatchName.Visibility = Visibility.Visible;
                NumberTb.Visibility = Visibility.Visible;
                IsHub.Visibility = Visibility.Visible;
                TransactionIdTb.Visibility = Visibility.Visible;
                Starttime.Visibility = Visibility.Visible;
            });

            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void IsGroup_Loaded(object sender, RoutedEventArgs e)
        {
            IsGroup.IsChecked=false;
        }

        private void BatchSave_Click(object sender, RoutedEventArgs e)
        {
            CommonOperationResult chkRez;
            chkRez = _batchManager.CheckNumber(BatchNumber.Text).Result;
            if (!chkRez.Success)
            {
                MessageBox.Show(chkRez.Message);
                BatchNumber.Focus();
                return;
            }

            chkRez = _batchManager.CheckName(BatchName.Text).Result;
            if (!chkRez.Success)
            {
                MessageBox.Show(chkRez.Message);
                BatchName.Focus();
                return;
            }

            Batch batch = new Batch();
            batch.Number = BatchNumber.Text;
            batch.Name = BatchName.Text;
            batch.IsGroup = (bool)IsGroup.IsChecked;
            var btcAddRez = _batchManager.AddNewBatch(batch).Result;
            if (!btcAddRez.Success)
            {
                MessageBox.Show(btcAddRez.Message);
                return;
            }
            _mainWindow.LoadBatchList();
            Close();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //  ActivityGrid.Items.Add(new ListViewItem());
            BatchNumber.Text = _currentBatch.Number;
            BatchName.Text = _currentBatch.Name;
            LoadActivityList();
        }
        private void LoadActivityList()
        {
            ActivityGrid.Items.Clear();
            _activityManager.GetAll(_currentBatch.Id);
            var itemsAct = _activityManager.GetAll(_currentBatch.Id).Result.ToList().Select(x=>x.AsViewModel()).ToList();
            ActivityGrid.ItemsSource = itemsAct;
            ActivityGrid.Columns[0].Visibility=Visibility.Hidden;
        }

        private void CreateActivity_Click(object sender, RoutedEventArgs e)
        {
            //add new activity to batch and re-read it into grid 

            //get all activities for this batch

            var activities = _repo.GetAllAsync(x => x.Id == _currentBatch.Id).Result.ToList();

            int newNumber=100;

            if (activities.Count>0)
            {
                newNumber = activities.Max(x => x.ActivityId)+100;
            }

            Activity newActivity = new Activity();
            newActivity.ActivityId = newNumber;
            newActivity.BatchId = _currentBatch.Id;
            newActivity.TransactionId = "000000";
            newActivity.StartTime=TimeSpan.FromSeconds(0);
            newActivity.Name = $"NewActivity-{newNumber}";
            _activityManager.AddNewActivity(newActivity);
            LoadActivityList();

        }
    }
}
