﻿using ActivityScheduler.Data.Contracts;
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
        private ActivityManager _activityManager;
        private FormStateHolder _formStateHolder = new FormStateHolder();
        private Batch _currentBatch;
        private Activity _currentActivity;
        private List<Activity> _activitiesList = new List<Activity>();
        private EditBatch _parentFrm;
        private List<ParentActivitySelectionViewModel> _itemsAct =new List<ParentActivitySelectionViewModel>();

        public SetParentActivities(EditBatch parentFrm, ActivityManager activityManager, Batch currentBatch,  Activity currentActivity, Serilog.ILogger logger)
        {
            _logger = logger;
            _currentBatch = currentBatch;
            _parentFrm = parentFrm;
            _activityManager = activityManager;
            _currentActivity= currentActivity;
            InitializeComponent();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            _parentFrm.BufferIn = "-1";
            Close();
        }
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
        private void BatchSave_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            InfoTb.Text = $"{_currentActivity.ActivityId}--{_currentActivity.Name}";
            
            LoadActivityGrid();
        }
        private void LoadActivityGrid()
        {
            LoadActivities();

            _itemsAct = _activitiesList.Select(x=>x.AsViewModel()).ToList().Select(x=>new ParentActivitySelectionViewModel() { Id=x.Id, Selected=false, ActivityId=x.ActivityId, Text = x.Name }).ToList();

            var x = _currentActivity.GetParentActionIds().ToArray();
            
            foreach(ParentActivitySelectionViewModel y in _itemsAct)
            {
                if (x.Contains(y.ActivityId)) 
                { 
                    y.Selected = true;
                }
            }
           


            ActivityGrid.ItemsSource = _itemsAct;

            (new[] { 0 }).ToList().ForEach(x => { ActivityGrid.Columns[x].Visibility = Visibility.Hidden; }); //hide columns

            ActivityGrid.Columns[1].IsReadOnly = false;
            ActivityGrid.Columns[2].IsReadOnly = true;
            ActivityGrid.Columns[3].IsReadOnly = true;
        }

        public class ParentActivitySelectionViewModel
        {
            public Guid Id{ get; set; }
            public bool Selected { get; set;}
            public int ActivityId { get; set; }
            public string Text { get; set; }

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

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            //string x = string.Join(',',_itemsAct.Where(x=>x.Selected==true).ToList().Select(x=>x.ActivityId).ToList());

            //System.Windows.MessageBox.Show(x);

            _parentFrm.BufferIn = string.Join(',', _itemsAct.Where(x => x.Selected == true).ToList().Select(x => x.ActivityId).ToList());

            Close();

        }
    }
}