using ActivityScheduler.Gui.EditWindow;
using ActivityScheduler.Shared.Service;
using System;
using System.Windows;
using System.Windows.Input;
using static ActivityScheduler.Core.SetParentActivities;

namespace ActivityScheduler.Core
{
    public partial class EditBatch : Window
    {
        private FormStateHolder _formStateHolder = new FormStateHolder();
        private FormStateHolder _formStateHolder2 = new FormStateHolder();

        private EditWindowViewModel _viewModel;

        public EditBatch(EditWindowViewModel dataContext)
        {
            DataContext = dataContext;

            _viewModel = (EditWindowViewModel)dataContext;

            _formStateHolder.CreateFormState(EditWindowViewModel.SelectionMode.GroupMode.ToString()).AddAction(() =>
            {
                BatchNumberLabel.Visibility = Visibility.Visible;
                BatchNumberTb.Visibility = Visibility.Visible;
                BatchNameLabel.Visibility = Visibility.Visible;
                BatchNameTb.Visibility = Visibility.Visible;
                ActivityGrid.Visibility = Visibility.Hidden;
                ActivityEditCanvas.Visibility = Visibility.Hidden;
                CreateActivity.Visibility = Visibility.Hidden;
                DeleteActivityBtn.Visibility = Visibility.Hidden;
                CnvRealBatch.Visibility = Visibility.Hidden;
                CnvRealBatchDow.Visibility = Visibility.Hidden;
            }).Parent.CreateFormState(EditWindowViewModel.SelectionMode.ActivityModeNoSelection.ToString()).AddAction(() =>
            {
                BatchNumberLabel.Visibility = Visibility.Visible;
                BatchNumberTb.Visibility = Visibility.Visible;
                BatchNameLabel.Visibility = Visibility.Visible;
                BatchNameTb.Visibility = Visibility.Visible;
                ActivityGrid.Visibility = Visibility.Visible;
                ActivityEditCanvas.Visibility = Visibility.Visible;
                CreateActivity.Visibility = Visibility.Visible;
                DeleteActivityBtn.Visibility = Visibility.Visible;
                CnvRealBatch.Visibility = Visibility.Visible;
                CnvRealBatchDow.Visibility = Visibility.Visible;
            }).Parent.CreateFormState(EditWindowViewModel.SelectionMode.ActivityModeRegularSelection.ToString()).AddAction(() =>
            {
                BatchNumberLabel.Visibility = Visibility.Visible;
                BatchNumberTb.Visibility = Visibility.Visible;
                BatchNameLabel.Visibility = Visibility.Visible;
                BatchNameTb.Visibility = Visibility.Visible;
                ActivityGrid.Visibility = Visibility.Visible;
                ActivityEditCanvas.Visibility = Visibility.Visible;
                CreateActivity.Visibility = Visibility.Visible;
                DeleteActivityBtn.Visibility = Visibility.Visible;
                CnvRealBatch.Visibility = Visibility.Visible;
                CnvRealBatchDow.Visibility = Visibility.Visible;
            });

            _formStateHolder2.CreateFormState(EditWindowViewModel.SelectionMode2.DowSeen.ToString()).AddAction(() =>
            {
                CnvRealBatchDow.Visibility = Visibility.Visible;

            }).Parent.CreateFormState(EditWindowViewModel.SelectionMode2.DowUnseen.ToString()).AddAction(() =>
            {
                CnvRealBatchDow.Visibility = Visibility.Hidden;
            });

            _viewModel.SelectionModeChanged += _viewModel_SelectionModeChanged;
            _viewModel.SelectionModeChanged2 += _viewModel_SelectionModeChanged2;

            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            _viewModel.NeedToCloseForm += () => { Close(); };

            InitializeComponent();
        }

        private void _viewModel_SelectionModeChanged2(EditWindowViewModel.SelectionMode2 selectionMode)
        {
            _formStateHolder2.SetFormState(selectionMode.ToString());
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
    }
}
