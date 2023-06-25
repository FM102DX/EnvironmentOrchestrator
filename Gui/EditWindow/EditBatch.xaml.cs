using ActivityScheduler.Data.Models;
using ActivityScheduler.Gui.EditWindow;
using ActivityScheduler.Shared.Service;
using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using static ActivityScheduler.Core.SetParentActivities;
using static System.Net.Mime.MediaTypeNames;

namespace ActivityScheduler.Core
{
    public partial class EditBatch : Window
    {
        private FormStateHolder _formStateHolder = new FormStateHolder();
        private FormStateHolder _formStateHolder3 = new FormStateHolder(); //start point type selection
        private FormStateHolder _formStateHolder4 = new FormStateHolder(); //start point type selection

        private EditWindowViewModel _viewModel;

        public EditBatch(EditWindowViewModel dataContext)
        {
            DataContext = dataContext;

            _viewModel = (EditWindowViewModel)dataContext;

            _formStateHolder.CreateFormState(EditWindowViewModel.SelectionMode.GroupMode.ToString()).AddAction(() =>
            {
                BatchTbi.Visibility = Visibility.Visible;
                ActivitiesTbi.Visibility = Visibility.Hidden;
                CnvNameAndNumber.Visibility = Visibility.Visible;
                CnvStartTimePoint.Visibility = Visibility.Hidden;
                CnvIntervalDuration.Visibility = Visibility.Hidden;
                CnvDow.Visibility = Visibility.Hidden;
                ActivityEditCanvas.Visibility = Visibility.Hidden;
                ActivityGrid.Visibility = Visibility.Hidden;
                CreateActivityBtn.Visibility = Visibility.Hidden;

            }).Parent.CreateFormState(EditWindowViewModel.SelectionMode.ActivityModeNoSelection.ToString()).AddAction(() =>
            {
                BatchTbi.Visibility = Visibility.Visible;
                ActivitiesTbi.Visibility = Visibility.Visible;
                CnvNameAndNumber.Visibility = Visibility.Visible;
                CnvStartTimePoint.Visibility = Visibility.Visible;
                CnvIntervalDuration.Visibility = Visibility.Hidden;
                CnvDow.Visibility = Visibility.Hidden;
                ActivityGrid.Visibility = Visibility.Visible;
                ActivityEditCanvas.Visibility = Visibility.Hidden;
                CreateActivityBtn.Visibility = Visibility.Visible;

            }).Parent.CreateFormState(EditWindowViewModel.SelectionMode.ActivityModeRegularSelection.ToString()).AddAction(() =>
            {
                BatchTbi.Visibility = Visibility.Visible;
                ActivitiesTbi.Visibility = Visibility.Visible;
                CnvNameAndNumber.Visibility = Visibility.Visible;
                CnvStartTimePoint.Visibility = Visibility.Visible;
                CnvIntervalDuration.Visibility = Visibility.Hidden;
                CnvDow.Visibility = Visibility.Hidden;
                ActivityGrid.Visibility = Visibility.Visible;
                ActivityEditCanvas.Visibility = Visibility.Visible;
                CreateActivityBtn.Visibility = Visibility.Visible;

            });

            _formStateHolder3.CreateFormState(BatchStartPointTypeEnum.StartFromNow.ToString()).AddAction(() =>
            {
                StartDateTimeTb.Visibility = Visibility.Hidden;
                StartTimeTb.Visibility = Visibility.Hidden;

            }).Parent.CreateFormState(BatchStartPointTypeEnum.StartTodayFromSpecifiedTime.ToString()).AddAction(() =>
            {
                StartDateTimeTb.Visibility = Visibility.Hidden;
                StartTimeTb.Visibility = Visibility.Visible;
            }).Parent.CreateFormState(BatchStartPointTypeEnum.StartFromSpecifiedDateAndTime.ToString()).AddAction(() =>
            {
                StartDateTimeTb.Visibility = Visibility.Visible;
                StartTimeTb.Visibility = Visibility.Hidden;
            });

            _formStateHolder4.CreateFormState(BatchStartTypeEnum.Single.ToString()).AddAction(() =>
            {
                CnvIntervalDuration.Visibility= Visibility.Hidden;
                CnvDow.Visibility= Visibility.Hidden;
            }).Parent.CreateFormState(BatchStartTypeEnum.Periodic.ToString()).AddAction(() =>
            {
                CnvIntervalDuration.Visibility = Visibility.Visible;
                CnvDow.Visibility = Visibility.Hidden;

            }).Parent.CreateFormState(BatchStartTypeEnum.PeriodicDaily.ToString()).AddAction(() =>
            {
                CnvIntervalDuration.Visibility = Visibility.Visible;
                CnvDow.Visibility = Visibility.Visible;
            });

            _viewModel.SelectionModeChanged += _viewModel_SelectionModeChanged;
            _viewModel.SelectionModeChanged4 += _viewModel_SelectionModeChanged4;
            _viewModel.SelectionModeChanged3 += _viewModel_SelectionModeChanged3;

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            _viewModel.NeedToCloseForm += () => { Close(); };

            InitializeComponent();
            StartDateTimeTb.FormatProvider = CultureInfo.CurrentCulture;


        }

        private void _viewModel_SelectionModeChanged3(BatchStartPointTypeEnum selectionMode)
        {
            _formStateHolder3.SetFormState(selectionMode.ToString());
        }

        private void _viewModel_SelectionModeChanged4(BatchStartTypeEnum selectionMode)
        {
            _formStateHolder4.SetFormState(selectionMode.ToString());
        }

        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_viewModel.CancelRecordEditCmd.CanExecute(this))
                    _viewModel.CancelRecordEditCmd.Execute(this);
            }
        }

        private void _viewModel_SelectionModeChanged(EditWindowViewModel.SelectionMode selectionMode)
        {
            _formStateHolder.SetFormState(selectionMode.ToString());
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ActivityGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_viewModel.NeedToStopItemSelectionChange())
            {
                e.Handled = true;
            }
        }

        private void EditBatchFrm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (_viewModel.NeedToStopFormClosing())
            {
                e.Cancel = true;
            }
        }

        private void SelectFolder_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SelectFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".ps1";
            dlg.Filter = "PowershellScript (.ps1)|*.ps1";

            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                SelectScriptPathTb.Text = filename;
            }
        }

        private void ParentActivitiesReset_Click(object sender, RoutedEventArgs e)
        {
            ParentActivitiesTb.Text = string.Empty;
        }

        private void SelectFileReset_Click(object sender, RoutedEventArgs e)
        {
            SelectScriptPathTb.Text = string.Empty;
        }

        private void SelectScriptrFileForBatchReset_Click(object sender, RoutedEventArgs e)
        {
            SelectScriptPathForBatchTb.Text = string.Empty; 
        }

        private void SelectScriptFileForBatch_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".ps1";
            dlg.Filter = "PowershellScript (.ps1)|*.ps1";

            //dlg.Filter = "JPEG Files (*.jpeg)|*.jpeg|PNG Files (*.png)|*.png|JPG Files (*.jpg)|*.jpg|GIF Files (*.gif)|*.gif";

            // Display OpenFileDialog by calling ShowDialog method

            Nullable<bool> result = dlg.ShowDialog();

            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                SelectScriptPathForBatchTb.Text = filename;
            }
        }

        private void SaveBatch_Click(object sender, RoutedEventArgs e)
        {

        }

        private void SetDaysOfWeek_Click(object sender, RoutedEventArgs e)
        {
            //

        }

        private void EditBatchFrm_Loaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.FormLoadedCmd.CanExecute(this))
            {
                _viewModel.FormLoadedCmd.Execute(this);
            }
            


        }

        private void EmptyStartDateBtn_Click(object sender, RoutedEventArgs e)
        {
            //StartDateTb.Text = string.Empty;
        }

        private void EmptyStartTimeBtn_Click(object sender, RoutedEventArgs e)
        {
            StartTimeTb.Text = string.Empty;
        }

        private void test_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.Test01Cmd.CanExecute(this))
            {
                _viewModel.Test01Cmd.Execute(this);
            }
            
            if (_viewModel.Test02Cmd.CanExecute(this))
            {
                _viewModel.Test02Cmd.Execute(this);
            }

        }

        private void SelectScriptFileForBatch_Click_1(object sender, RoutedEventArgs e)
        {

        }
    }
}
