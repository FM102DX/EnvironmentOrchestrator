using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Service;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;
using static System.Windows.Forms.DataFormats;

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
        private FormStateHolder _formStateHolder = new FormStateHolder();
        private Batch _currentBatch;
        private Activity _currentActivity;
        private List<Activity> _activitiesList = new List<Activity>();
        private TimeSpan _actStartTime { get; set; }
        private bool _saveActivityStopMarker=false;
        public string BufferIn { get; set; }

        public EditBatch(BatchManager batchManager,  ActivityManager activityManager,  Batch currentBatch, Serilog.ILogger logger)
        {
            _logger = logger;
            _batchManager = batchManager;
            _currentBatch= _batchManager.Clone(currentBatch);
            _activityManager = activityManager;

            _formStateHolder.CreateFormState("isgroup").AddAction(() => {
                BatchNumberLabel.Visibility = Visibility.Visible;
                BatchNumberTb.Visibility = Visibility.Visible;
                BatchNameLabel.Visibility = Visibility.Visible;
                BatchNameTb.Visibility = Visibility.Visible;
                ActivityGrid.Visibility = Visibility.Hidden;
                ActivityEditCanvas.Visibility = Visibility.Hidden;
                CreateActivity.Visibility = Visibility.Hidden;
                DeleteActivityBtn.Visibility = Visibility.Hidden;

            }).Parent.CreateFormState("normal").AddAction(() =>
            {
                BatchNumberLabel.Visibility = Visibility.Visible;
                BatchNumberTb.Visibility = Visibility.Visible;
                BatchNameLabel.Visibility = Visibility.Visible;
                BatchNameTb.Visibility = Visibility.Visible;
                ActivityGrid.Visibility = Visibility.Visible;
                ActivityEditCanvas.Visibility = Visibility.Visible;
                CreateActivity.Visibility = Visibility.Visible;
                DeleteActivityBtn.Visibility = Visibility.Visible;

            });

            _batchManager._checker.BindControlToCheck("UpdateNumber", BatchNumberTb)
                                  .BindControlToCheck("UpdateName",   BatchNameTb);
            _activityManager._checker.BindActionToCheck("UpdateTransactionId", () => { TransactionIdTb.Focus(); TransactionIdTb.SelectAll();});
            InitializeComponent();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void BatchSave_Click(object sender, RoutedEventArgs e)
        {
            CommonOperationResult chkRez;

            _currentBatch.Number = BatchNumberTb.Text;
            
            _currentBatch.Name = BatchNameTb.Text;

            var btcAddRez = _batchManager.ModifyBatch(_currentBatch).Result;
            
            if (!btcAddRez.Success)
            {
                System.Windows.Forms.MessageBox.Show(btcAddRez.Message);
                return;
            }
            
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentBatch.IsGroup) { _formStateHolder.SetFormState("isgroup"); } else { _formStateHolder.SetFormState("normal"); }
            BatchNumberTb.Text = _currentBatch.Number;
            BatchNameTb.Text = _currentBatch.Name;
            LoadActivityGrid();
            SetFirstGridElementAsActive();
        }
        private void LoadActivityGrid()
        {
         
            LoadActivities();

            var itemsAct = _activitiesList.Select(x=>x.AsViewModel()).ToList();
            
            ActivityGrid.ItemsSource = itemsAct;
            
            ActivityGrid.Columns[0].Visibility=Visibility.Hidden;
        }

        private void SetFirstGridElementAsActive()
        {
            if (ActivityGrid.Items.Count > 0)
            {
                ActivityGrid.SelectedItem = ActivityGrid.Items[0];
                ActivityGrid.ScrollIntoView(ActivityGrid.Items[0]);
            }
        }

        private void LoadActivities()
        {
            _activitiesList = _activityManager.GetAll(_currentBatch.Id).Result.ToList();
        }

        private void CreateActivity_Click(object sender, RoutedEventArgs e)
        {
            //add new activity to batch and re-read it into grid 
            //get all activities for this batch

            var activities = _activityManager.GetAll(_currentBatch.Id).Result.ToList();
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
            LoadActivityGrid();

        }

        private void ActivityGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0) {return;}

            var item = (ActivityGridViewModel)e.AddedItems[0];

            _currentActivity = _activitiesList.Where(x => x.Id == item.Id).FirstOrDefault();

            SetActivityToFields(_currentActivity);
        }

        private void SetActivityToFields(Activity activity)
        {
            ActivityIdTb.Text = activity.ActivityId.ToString();
            ActivityNameTb.Text = activity.Name;
            IsHub.IsChecked = activity.IsHub;
            TransactionIdTb.Text=activity.TransactionId.ToString();
            IsDomestic.IsChecked= activity.IsDomestic;
            StartTimeTb.Text=activity.StartTime.ToString();
            AlwaysSuccess.IsChecked = activity.AlwaysSuccess;
            ParentActivitiesTb.Text = activity.ParentActivities;
        }

        private Activity GetActivityFromFields()
        {
            Activity activity = new Activity();

            activity.Id = _currentActivity.Id;

            activity.BatchId = _currentBatch.Id;

            activity.ActivityId = Convert.ToInt32(ActivityIdTb.Text);

            activity.Name = ActivityNameTb.Text;

            activity.IsHub = (bool)IsHub.IsChecked;

            activity.TransactionId = TransactionIdTb.Text;

            activity.IsDomestic = (bool)IsDomestic.IsChecked;

            //TimeSpan ts = (TimeSpan)"00:00:00";

            TimeSpan.TryParseExact(StartTimeTb.Text, "hh\\:mm\\:ss", CultureInfo.CurrentCulture, out TimeSpan ts);

            activity.StartTime = ts;

            activity.AlwaysSuccess= (bool)AlwaysSuccess.IsChecked;

            activity.ParentActivities = ParentActivitiesTb.Text;

            return activity;
        }

        private void SaveActivityBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_saveActivityStopMarker) return;
            SaveCurrentActivity();
        }

        private void SaveCurrentActivity()
        {
            MsgLabel.Text = "";
            Activity activity = GetActivityFromFields();
            var rez = _activityManager.ModifyActivity(activity).Result;
            if (!rez.Success)
            {
                //focus on field
                if (rez.StoredAction != null) rez.StoredAction.Invoke();

                //Task.Run(() => ShowFormErrorMessage(rez.Message));
                MsgLabel.Text = rez.Message;
                return;
            }
            LoadActivityGrid();
        }
        private void ShowFormErrorMessage(string text)
        {
            MsgLabel.Text = text;
            Thread.Sleep(3000);
            MsgLabel.Text ="";
        }

        private void ActivityIdTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
            if (s.Length > 3) { s = s.Substring(0,3); }
            ((TextBox)sender).Text = s;
        }

        private void StartTimeTb_LostFocus(object sender, RoutedEventArgs e)
        {
            _saveActivityStopMarker = false;
            var rezCtrl = (MaskedTextBox)sender;
            var rezTxt = (rezCtrl).Text;
            bool convertRez=TimeSpan.TryParseExact(rezTxt, "hh\\:mm\\:ss", CultureInfo.CurrentCulture, out TimeSpan ts);
            if (!convertRez)
            {
                StartTimeTb.Text = "";
                //StartTimeTb.Focus();
                _saveActivityStopMarker=true;
                MsgLabel.Text="Please enter a valid timespan, for example 02:15:30";
            }
        }

        private void ReloadGrid_Click(object sender, RoutedEventArgs e)
        {
            LoadActivityGrid();
        }

        private void SetParentActivitiesBtn_Click(object sender, RoutedEventArgs e)
        {
            SetParentActivities frm = new SetParentActivities(this, _activityManager, _currentBatch, _currentActivity, _logger);
            frm.ShowDialog();
            ParentActivitiesTb.Text = BufferIn;
            if (BufferIn !="-1")
            {
                SaveCurrentActivity();
            }
        }

        private void DeleteActivityBtn_Click(object sender, RoutedEventArgs e)
        {
            if (_currentActivity == null) return;
            var qRez = System.Windows.MessageBox.Show("Really delete?","",MessageBoxButton.OKCancel);
            if (qRez == MessageBoxResult.Cancel) return;
            _activityManager.RemoveActivity(_currentActivity.Id);
            LoadActivityGrid();
        }
    }
}
