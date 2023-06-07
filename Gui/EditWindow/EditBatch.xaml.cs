using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Gui.EditWindow;
using ActivityScheduler.Gui.MainWindow;
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
            _viewModel.NeedToCloseForm += () => { Close(); };

            InitializeComponent();
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
           
            // System.Windows.Forms.MessageBox.Show("this is PreviewMouseDown");
            if (_viewModel.NeedToStopItemSelectionChange())
            {
                e.Handled = true;
            }
        }
    }
}
