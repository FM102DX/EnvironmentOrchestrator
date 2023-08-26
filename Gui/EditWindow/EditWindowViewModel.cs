﻿using ActivityScheduler.Core;
using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Gui.MainWindow;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Service;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit.Primitives;
using ReactiveUI;

namespace ActivityScheduler.Gui.EditWindow
{
    public class EditWindowViewModel : ReactiveObject
    {
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Activity> _repo;
        private BatchManager _batchManager;
        private ActivityManager _activityManager;
        public String StartDateTimeBindingVar { 
            get => CurrentBatch.StartDateTime.ToString(); 
            set 
            {
                DateTime _dt;
                if (DateTime.TryParse(value,CultureInfo.CurrentCulture, DateTimeStyles.None, out _dt ))
                {
                    CurrentBatch.StartDateTime = _dt;
                }
            } 
        }
        public SelectionMode SelectionModeVar { get; set; } = SelectionMode.None;

        public List<RunModeComboDataSourceItem> RunModeComboDataSource { get; set; }=new List<RunModeComboDataSourceItem>();

        public RunModeComboDataSourceItem? SelectedRunModeComboDataSourceItem
        {
            get
            {
                var stt= RunModeComboDataSource.Where(x => x.Value == CurrentBatch.RunMode).FirstOrDefault();
                return stt;
            }
            set
            {
                if (value != null)
                {
                    CurrentBatch.RunMode = value.Value;
                    SelectionModeChanged4(value.Value);
                }
            }
        }

        public List<BatchStartPointTypeComboItem> BatchStartPointTypeComboDataSource { get; set; } = new List<BatchStartPointTypeComboItem>();

        public BatchStartPointTypeComboItem? SelectedBatchStartPointTypeItem
        {
            get
            {
                var stt = BatchStartPointTypeComboDataSource.Where(x => x.Value == CurrentBatch.StartPointType).FirstOrDefault();
                return stt;
            }
            set
            {
                if (value != null)
                {
                    CurrentBatch.StartPointType = value.Value;
                    SelectionModeChanged3(value.Value);
                }
            }
        }

        public bool MondayChk {get{return GetDowBoolValue(0);}set{SetDowValue(0, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("MondayChk")); } }
        public bool TuesdayChk { get { return GetDowBoolValue(1); } set { SetDowValue(1, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("TuesdayChk")); } }
        public bool WednesdayChk { get { return GetDowBoolValue(2); } set { SetDowValue(2, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("WednesdayChk")); } }
        public bool ThursdayChk { get { return GetDowBoolValue(3); } set { SetDowValue(3, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ThursdayChk")); } }
        public bool FridayChk { get { return GetDowBoolValue(4); } set { SetDowValue(4, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FridayChk")); } }
        public bool SatudayChk { get { return GetDowBoolValue(5); } set { SetDowValue(5, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SatudayChk")); } }
        public bool SundayChk { get { return GetDowBoolValue(6); } set { SetDowValue(6, value); PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SundayChk")); } }
        private bool GetDowBoolValue(int index)
        {
            SetDefaultDowValue();

            if (!string.IsNullOrEmpty(_currentBatch.ActiveDaysOfWeek))
            {
                if (_currentBatch.ActiveDaysOfWeek.Length == 7)
                {
                    return _currentBatch.ActiveDaysOfWeek[index] == '1';
                }
            }
            return false;
            
        }

        private void SetDowValue(int index, bool value)
        {
            SetDefaultDowValue();

            if (!string.IsNullOrEmpty(_currentBatch.ActiveDaysOfWeek))
            {
            
                List<string> lStr = new List<string>(); 

                for(int i = 0; i< _currentBatch.ActiveDaysOfWeek.Length;i++)
                {
                    if(i == index) 
                    {
                        string s = (value == true) ? "1" : "0";
                        lStr.Add(s);
                    }
                    else
                    {
                        lStr.Add(_currentBatch.ActiveDaysOfWeek[i].ToString());
                    }
                }
                _currentBatch.ActiveDaysOfWeek = string.Join("", lStr);
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentBatch"));
        }

        private void SetDefaultDowValue()
        {
            if (!string.IsNullOrEmpty(_currentBatch.ActiveDaysOfWeek))
            {
                if (_currentBatch.ActiveDaysOfWeek.Length != 7)
                {
                    _currentBatch.ActiveDaysOfWeek = "0000000";
                    return;
                }
            }
            else
            {
                _currentBatch.ActiveDaysOfWeek = "0000000";
            }
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("CurrentBatch"));
        }


        public ICommand FormLoadedCmd { get; private set; }
        public ICommand SaveActivityCmd { get; private set; }
        public ICommand SaveBatchCmd { get; private set; }

        public ICommand CreateActivityCmd { get; private set; }

        public ICommand DeleteActivityCmd { get; private set; }
        
        public ICommand CancelRecordEditCmd { get; private set; }

        public ICommand SelectParentActivitiesCmd { get; private set; }

        public ICommand Test01Cmd { get; private set; }

        public ICommand Test02Cmd { get; private set; }

        private MainWindowViewModel _mainWindowViewModel;

        private bool IsModified
        {
            get
            {
                bool rez = !_activityManager.Similar(SelectedItemDisplayed, SelectedItem);
                return rez;
            }
        }

        private Batch _currentBatch;
        public Batch CurrentBatch { 
            get=> _currentBatch;
            set => this.RaiseAndSetIfChanged(ref _currentBatch, value);
        }

        private List<Activity> _activitiesList = new List<Activity>();
        public List<Activity> AcivityListItemSource 
        { 
            get =>_activitiesList;
            set=> this.RaiseAndSetIfChanged(ref _activitiesList, value);
        }

        private Activity? _selectedItem;

        private Activity? _selectedItemDisplayed;

        //made to for record editing purposes
        public Activity? SelectedItemDisplayed 
        { 
            get => _selectedItemDisplayed;
            set=> this.RaiseAndSetIfChanged(ref _selectedItemDisplayed, value);
        }

        public Activity? SelectedItem 
        {
            get => _selectedItem;
            set
            {
                if (AcivityListItemSource.Count == 0)
                {
                    value = null;
                    SelectionModeVar = SelectionMode.ActivityModeNoSelection;
                    SelectedItemDisplayed = null;
                }
                else
                {
                    if (value == null) return;
                    SelectedItemDisplayed = _activityManager.Clone(value);
                    SelectionModeVar = SelectionMode.ActivityModeRegularSelection;
                }
                this.RaiseAndSetIfChanged(ref _selectedItem, value);
                UpdateSelectionMode();
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        
        public event Action NeedToCloseForm;

        public event Action NeedToCancelGridChange;

        //TODO need to use bindings here and get rid of event handling in view
        public event SelectionModeChangedDelegate   SelectionModeChanged;
        public event SelectionModeChangedDelegate3  SelectionModeChanged3;
        public event SelectionModeChangedDelegate4  SelectionModeChanged4;

        public delegate void SelectionModeChangedDelegate(SelectionMode selectionMode);
        public delegate void SelectionModeChangedDelegate3(BatchStartPointTypeEnum selectionMode);
        public delegate void SelectionModeChangedDelegate4(BatchStartTypeEnum selectionMode);

        public EditWindowViewModel (BatchManager batchManager, ActivityManager activityManager, Batch currentBatch, Serilog.ILogger logger, MainWindowViewModel mainWindowViewModel)
        {
            _logger = logger;
            
            _batchManager = batchManager;

            _mainWindowViewModel = mainWindowViewModel;

            CurrentBatch = _batchManager.Clone(currentBatch);
            
            _activityManager = activityManager;

            RunModeComboDataSource.Add(new RunModeComboDataSourceItem() { Id = ((byte)BatchStartTypeEnum.Single), Name= BatchStartTypeEnum.Single.ToString(),Value= BatchStartTypeEnum.Single });
            RunModeComboDataSource.Add(new RunModeComboDataSourceItem() { Id = ((byte)BatchStartTypeEnum.Periodic), Name = BatchStartTypeEnum.Periodic.ToString(), Value = BatchStartTypeEnum.Periodic });
            RunModeComboDataSource.Add(new RunModeComboDataSourceItem() { Id = ((byte)BatchStartTypeEnum.PeriodicDaily), Name = BatchStartTypeEnum.PeriodicDaily.ToString(), Value = BatchStartTypeEnum.PeriodicDaily });

            BatchStartPointTypeComboDataSource.Add(new BatchStartPointTypeComboItem() { Id = (byte)BatchStartPointTypeEnum.StartFromNow,                     Name= BatchStartPointTypeEnum.StartFromNow.ToString(),                      Value= BatchStartPointTypeEnum.StartFromNow});
            BatchStartPointTypeComboDataSource.Add(new BatchStartPointTypeComboItem() { Id = (byte)BatchStartPointTypeEnum.StartTodayFromSpecifiedTime,     Name = BatchStartPointTypeEnum.StartTodayFromSpecifiedTime.ToString(),      Value = BatchStartPointTypeEnum.StartTodayFromSpecifiedTime });
            BatchStartPointTypeComboDataSource.Add(new BatchStartPointTypeComboItem() { Id = (byte)BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime,   Name = BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime.ToString(),    Value = BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime });

            if (CurrentBatch.IsGroup) 
            {
                SelectionModeVar = SelectionMode.GroupMode;
            }
            else
            {
                SelectionModeVar = SelectionMode.ActivityModeNoSelection;
            }

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

            CancelRecordEditCmd = new ActionCommand(() =>
            {
                if (SelectedItem !=null)
                {
                    SelectedItemDisplayed = _activityManager.Clone(SelectedItem);
                }
            });

            SelectParentActivitiesCmd = new ActionCommand(() =>
            {
                SetParentActivities act = new SetParentActivities(_activityManager, _currentBatch, SelectedItem, _logger, this);
                act.ShowDialog();
            });

            FormLoadedCmd = new ActionCommand(() =>
            {
                if (_currentBatch!=null && SelectionModeChanged!=null)//is group or real batch
                {
                    if (_currentBatch.IsGroup)
                    {
                        SelectionModeVar = SelectionMode.GroupMode;
                    }
                    else
                    {
                        if (SelectedItem!=null)
                        {
                            SelectionModeVar = SelectionMode.ActivityModeRegularSelection;
                        }
                        else
                        {
                            SelectionModeVar = SelectionMode.ActivityModeNoSelection;
                        }
                    }
                    UpdateSelectionMode();
                }

                if (_currentBatch != null && SelectionModeChanged4 != null ) //run mode -- Single, Periodic, Periodic daily
                {
                    if (_currentBatch.IsGroup)
                    {
                        
                    }
                    else
                    {
                        SelectionModeChanged4(_currentBatch.RunMode);

                        if (_currentBatch.RunMode == BatchStartTypeEnum.Single)
                        {
                            
                        }
                        else
                        {
                            
                        }
                    }
                }
                if (_currentBatch != null && SelectionModeChanged3 != null) //start point type - FromNow, FromTime, FromDateTime
                {
                    SelectionModeChanged3(_currentBatch.StartPointType);
                }
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("StartDateTimeBindingVar"));

                UpdateSelectionMode();

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

            Test02Cmd = new ActionCommand(() =>
            {
                MessageBox.Show(StartDateTimeBindingVar.ToString());
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
                    if (SelectedItem!=null) SelectedItemDisplayed =_activityManager.Clone(SelectedItem); else SelectedItemDisplayed = null;
                    return false;
                }

                if (mbxRez == MessageBoxResult.Cancel)
                {
                    return true;
                }

            }
            return false;
        }

        public bool NeedToStopFormClosing()
        {
            if (IsModified)
            {
                var mbxRez = MessageBox.Show("Save changes?", "Question", MessageBoxButton.YesNoCancel);

                if (mbxRez == MessageBoxResult.Yes)
                {
                    if (SaveActivity())
                    {
                        //saved successfully
                        return false;
                    }
                    else
                    {
                        if (SelectedItemDisplayed != null)
                        {
                            LoadActivities(RecordSelectionMode.SelectSpecifiedId, SelectedItemDisplayed.Id);
                        }
                        else
                        {
                            LoadActivities();
                        }
                        return true;
                    }
                }

                if (mbxRez == MessageBoxResult.No)
                {
                    if (SelectedItem != null) SelectedItemDisplayed = _activityManager.Clone(SelectedItem); else SelectedItemDisplayed = null;
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
            if (SelectionModeChanged != null)
            {
                SelectionModeChanged(SelectionModeVar);
            }
        }

        public enum SelectionMode
        {
            None=0,
            GroupMode = 1,
            ActivityModeNoSelection = 2,
            ActivityModeRegularSelection = 3
        }
        public enum SelectionMode2
        {
            DowSeen = 0,
            DowUnseen = 1
        }

        public enum RecordSelectionMode
        {
            SelectNone = 0,
            SelectFirstRecord = 1,
            SelectLastRecord = 2,
            SelectSpecifiedId = 3,
            DontPerformSelection=4
        }

        public class RunModeComboDataSourceItem
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public BatchStartTypeEnum Value { get; set; }
        }

        public class BatchStartPointTypeComboItem
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public BatchStartPointTypeEnum Value { get; set; }
        }



    }


}
