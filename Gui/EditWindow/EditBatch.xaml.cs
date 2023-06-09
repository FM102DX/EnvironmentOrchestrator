using ActivityScheduler.Gui.EditWindow;
using ActivityScheduler.Shared.Service;
using System.Windows;
using System.Windows.Input;

namespace ActivityScheduler.Core
{
    public partial class EditBatch : Window
    {
        private FormStateHolder _formStateHolder = new FormStateHolder();
        
        private EditWindowViewModel _viewModel;

        public EditBatch(EditWindowViewModel dataContext)
        {
            DataContext = dataContext;

            _viewModel = (EditWindowViewModel)dataContext;

            _formStateHolder.CreateFormState(EditWindowViewModel.SelectionMode.GroupMode.ToString()).AddAction(() => {
                BatchNumberLabel.Visibility = Visibility.Visible;
                BatchNumberTb.Visibility = Visibility.Visible;
                BatchNameLabel.Visibility = Visibility.Visible;
                BatchNameTb.Visibility = Visibility.Visible;
                ActivityGrid.Visibility = Visibility.Hidden;
                ActivityEditCanvas.Visibility = Visibility.Hidden;
                CreateActivity.Visibility = Visibility.Hidden;
                DeleteActivityBtn.Visibility = Visibility.Hidden;
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
            });

            _viewModel.SelectionModeChanged += _viewModel_SelectionModeChanged;
            this.PreviewKeyDown += new KeyEventHandler(HandleEsc);
            _viewModel.NeedToCloseForm += () => { Close(); };

            InitializeComponent();
        }
        private void HandleEsc(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape) 
            {
                if(_viewModel.CancelRecordEditCmd.CanExecute(this))
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
    }
}
