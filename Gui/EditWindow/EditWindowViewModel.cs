﻿using ActivityScheduler.Core;
using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Gui.MainWindow;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;

namespace ActivityScheduler.Gui.EditWindow
{
    public class EditWindowViewModel : INotifyPropertyChanged
    {
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Activity> _repo;
        private BatchManager _batchManager;
        private ActivityManager _activityManager;
        public SelectionMode SelectionModeVar { get; set; } = SelectionMode.None;
        private Batch _currentBatch;
        public ICommand SaveActivityCmd { get; private set; }
        public ICommand SaveBatchCmd { get; private set; }

        public ICommand CreateActivityCmd { get; private set; }

        public ICommand DeleteActivityCmd { get; private set; }
        
        public ICommand RestoreSelectionOnGotFocusCmd { get; private set; }

        public ICommand SelectParentActivitiesCmd { get; private set; }

        private MainWindowViewModel _mainWindowViewModel;

        private bool IsModified
        {
            get
            {
                bool rez = !_activityManager.Similar(SelectedItemDisplayed, SelectedItem);
                return rez;
            }
        }

        public Batch CurrentBatch { 
            get
            {
                return _currentBatch;
            }
            set
            {
                _currentBatch = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentBatch"));
            }
        }

        private List<Activity> _activitiesList = new List<Activity>();

        public List<Activity> AcivityListItemSource 
        { 
            get
            {
                return (List<Activity>) _activitiesList;    
            }
            
            set
            {
                _activitiesList = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("AcivityListItemSource"));
            }
        }

        private Activity? _selectedItem;
        private Activity? _selectedItemDisplayed;

        //made to for record editing purposes
        public Activity? SelectedItemDisplayed 
        { 
            get
            {
                return _selectedItemDisplayed;
            }
            set
            {
                _selectedItemDisplayed=value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedItemDisplayed"));
            }
        }
        


        public Activity? SelectedItem 
        {
            get 
            { 
                return _selectedItem; 
            }
            set
            {
                if (value == null && AcivityListItemSource.Count != 0) return;
                
                _selectedItem = value;

                if (_selectedItem == null)
                {
                    SelectionModeVar = SelectionMode.None;
                    SelectionModeChanged(SelectionModeVar);
                    SelectedItemDisplayed = null;
                    return;
                }

                SelectedItemDisplayed = _selectedItem.Clone();

                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedItem"));

                UpdateSelectionMode();
            }
        }

        private TimeSpan _actStartTime { get; set; }

        private bool _saveActivityStopMarker = false;

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public event Action NeedToCloseForm;

        public event Action NeedToCancelGridChange;

        public event SelectionModeChangedDelegate SelectionModeChanged;

        public delegate void SelectionModeChangedDelegate(SelectionMode selectionMode);

        public string BufferIn { get; set; }

        public EditWindowViewModel (BatchManager batchManager, ActivityManager activityManager, Batch currentBatch, Serilog.ILogger logger, MainWindowViewModel mainWindowViewModel)
        {
            _logger = logger;
            
            _batchManager = batchManager;

            _mainWindowViewModel = mainWindowViewModel;

            CurrentBatch = _batchManager.Clone(currentBatch);
            
            _activityManager = activityManager;

            if(CurrentBatch.IsGroup) 
            {
                SelectionModeVar = SelectionMode.GroupMode;
            }
            else
            {
                SelectionModeVar = SelectionMode.ActivityModeNoSelection;
            }

            //_batchManager._checker.BindControlToCheck("UpdateNumber", CurrentBatch.Number).BindControlToCheck("UpdateName", CurrentBatch.Name);
            //_activityManager._checker.BindActionToCheck("UpdateTransactionId", () => { TransactionIdTb.Focus(); TransactionIdTb.SelectAll(); });

            SaveActivityCmd = new ActionCommand(() =>
            {
                if (_selectedItem != null)
                {
                    Guid id = _selectedItem.Id;
                    SaveActivity();
                    LoadActivities(RecordSelectionMode.SelectSpecifiedId, id);
                }
            });

            SaveBatchCmd = new ActionCommand(() =>
            {
                var btcAddRez = _batchManager.ModifyBatch(CurrentBatch).Result;

                if (!btcAddRez.Success)
                {
                    System.Windows.Forms.MessageBox.Show(btcAddRez.Message);
                    return;
                }
                _mainWindowViewModel.LoadBatchList();
                if (NeedToCloseForm !=null) NeedToCloseForm();
            });

            SelectParentActivitiesCmd = new ActionCommand(() =>
            {
                SetParentActivities act = new SetParentActivities(_activityManager, _currentBatch, SelectedItem, _logger, this);
                act.ShowDialog();
            });

            CreateActivityCmd = new ActionCommand(() =>
            {
               // add new activity to batch and re-read it into grid
               // get all activities for this batch

                var activities = _activityManager.GetAll(CurrentBatch.Id).Result.ToList();
                int newNumber = 100;

                if (activities.Count > 0)
                {
                    newNumber = activities.Max(x => x.ActivityId);

                    var x = Math.Round(Convert.ToDecimal(newNumber) / 100, 0) + 1;

                    newNumber = (int)(x * 100);
                }

                Activity newActivity = new Activity();
                newActivity.ActivityId = newNumber;
                newActivity.BatchId = CurrentBatch.Id;
                newActivity.TransactionId = "000000";
                newActivity.StartTime = TimeSpan.FromSeconds(0);
                newActivity.Name = $"NewActivity-{newNumber}";
                _activityManager.AddNewActivity(newActivity);

                LoadActivities(RecordSelectionMode.SelectSpecifiedId, newActivity.Id);

                //ShowFormSuccessMessage("Activity saved successfully");

            });

            DeleteActivityCmd= new ActionCommand(() =>
            {
                if (SelectedItem == null) return;
                var qRez = System.Windows.MessageBox.Show("Really delete?", "", MessageBoxButton.OKCancel);
                if (qRez == MessageBoxResult.Cancel) return;
                _activityManager.RemoveActivity(SelectedItem.Id);
                LoadActivities( RecordSelectionMode.SelectLastRecord);
            });

            LoadActivities(RecordSelectionMode.SelectFirstRecord);
        }
        public bool NeedToStopItemSelectionChange()
        {
            if (IsModified)
            {
                var mbxRez = MessageBox.Show("Save changes?", "Question", MessageBoxButton.YesNoCancel);

                if (mbxRez == MessageBoxResult.Yes)
                {
                    SaveActivity();

                    if (SelectedItemDisplayed!=null)
                    {
                        LoadActivities(RecordSelectionMode.SelectSpecifiedId, SelectedItemDisplayed.Id);
                    }
                    else
                    {
                        LoadActivities();
                    }
                    return true;
                }
                
                if (mbxRez == MessageBoxResult.No)
                {
                    if (SelectedItem!=null) SelectedItemDisplayed = SelectedItem.Clone(); else SelectedItemDisplayed = null;
                    return false;
                }

                if (mbxRez == MessageBoxResult.Cancel)
                {
                    return true;
                }

            }
            return false;
        }

        private bool SaveActivity()
        {
            if (SelectedItemDisplayed == null) { return false; }
            var rez = _activityManager.ModifyActivity(SelectedItemDisplayed).Result;
            if (!rez.Success)
            {
                ShowFormErrorMessage(rez.Message);
                return false;
            }
            ShowFormSuccessMessage("Activity saved successfully");
            return true;
        }
        private void ShowFormErrorMessage(string text)
        {
            MessageBox.Show(text,"Operation error", MessageBoxButton.OK , MessageBoxImage.Error);
        }

        public void SetParentActivities(string activities)
        {
           SelectedItemDisplayed.ParentActivities=activities;
           PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedItemDisplayed"));
        }

        private void ShowFormSuccessMessage(string text)
        {
            MessageBox.Show(text, "Operation successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        private void ShowFormInfoMessage(string text)
        {
            MessageBox.Show(text, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
        }


        private void LoadActivities(RecordSelectionMode recordSelectionMode= RecordSelectionMode.SelectNone, Guid? selectedItemId = null)
        {
            AcivityListItemSource = _activityManager.GetAll(CurrentBatch.Id).Result.ToList();
            
            if(AcivityListItemSource.Count==0)
            {
                recordSelectionMode = RecordSelectionMode.SelectNone;
            }
            
            switch (recordSelectionMode)
            {
                case RecordSelectionMode.SelectNone:
                    SelectedItem = null;
                    break;

                case RecordSelectionMode.SelectFirstRecord:
                    if (AcivityListItemSource.Count != 0)
                    {
                        SelectedItem = AcivityListItemSource[0];
                    }
                    break;

                case RecordSelectionMode.SelectLastRecord:
                    if (AcivityListItemSource.Count != 0)
                    {
                        SelectedItem = AcivityListItemSource[AcivityListItemSource.Count-1];
                    }
                    break;
                
                case RecordSelectionMode.SelectSpecifiedId:
                    if (AcivityListItemSource.Count != 0 && selectedItemId !=null)
                    {
                        SelectedItem = AcivityListItemSource.Where(x=>x.Id== selectedItemId).ToList().FirstOrDefault();
                    }
                    break;

                case RecordSelectionMode.DontPerformSelection:
                    break;
            }
        }

        private void UpdateSelectionMode()
        {
            //if (CurrentBatch == null) return;

            //if (CurrentBatch.IsGroup)
            //{
            //    SelectionModeVar = SelectionMode.Group;
            //}
            //else
            //{
            //    if (IsBatchRunning(CurrentBatch.Number))
            //    {
            //        SelectionModeVar = SelectionMode.RealBatchRunning;
            //    }
            //    else
            //    {
            //        SelectionModeVar = SelectionMode.RealBatchStopped;
            //    }
            //}

            //if (SelectionModeChanged != null)
            //{
            //    SelectionModeChanged(SelectionModeVar);
            //}
        }

        public enum SelectionMode
        {
            None=0,
            GroupMode = 1,
            ActivityModeNoSelection = 2,
            ActivityModeRegularSelection = 3
        }
        public enum RecordSelectionMode
        {
            SelectNone = 0,
            SelectFirstRecord = 1,
            SelectLastRecord = 2,
            SelectSpecifiedId = 3,
            DontPerformSelection=4
        }


    }


}