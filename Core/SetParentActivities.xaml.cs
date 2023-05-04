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
    public partial class SetParentActivities : Window
    {
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Activity> _repo;
        private BatchManager _batchManager;
        private ActivityManager _activityManager;
        private MainWindow _mainWindow;
        private FormStateHolder _formStateHolder = new FormStateHolder();
        private Batch _currentBatch;
        private Activity _currentActivity;
        private List<Activity> _activitiesList = new List<Activity>();
        private TimeSpan _actStartTime { get; set; }
        private bool _saveActivityStopMarker=false;

        public SetParentActivities(MainWindow mainWindow, BatchManager batchManager,  ActivityManager activityManager,  Batch currentBatch, Serilog.ILogger logger)
        {
            _logger = logger;
            _batchManager = batchManager;
            _mainWindow = mainWindow;
            _currentBatch= _batchManager.Clone(currentBatch);
            _activityManager = activityManager;
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

            var btcAddRez = _batchManager.ModifyBatch(_currentBatch).Result;
            
            if (!btcAddRez.Success)
            {
                System.Windows.Forms.MessageBox.Show(btcAddRez.Message);
                return;
            }
            
            _mainWindow.LoadBatchList();

            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_currentBatch.IsGroup) { _formStateHolder.SetFormState("isgroup"); } else { _formStateHolder.SetFormState("normal"); }
            LoadActivityGrid();
        }
        private void LoadActivityGrid()
        {
       
            LoadActivities();

            var itemsAct = _activitiesList.Select(x=>x.AsViewModel()).ToList();
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

        private void ActivityIdTb_TextChanged(object sender, TextChangedEventArgs e)
        {
            string s = Regex.Replace(((TextBox)sender).Text, @"[^\d.]", "");
            if (s.Length > 3) { s = s.Substring(0,3); }
            ((TextBox)sender).Text = s;
        }

        private void ReloadGrid_Click(object sender, RoutedEventArgs e)
        {
            LoadActivityGrid();
        }
    }
}
