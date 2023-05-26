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
using System.Windows.Navigation;
using System.Windows.Shapes;
using ActivityScheduler.Core;
using Serilog;
using System.IO;
using ActivityScheduler.Core.Settings;
using ActivityScheduler.Core.Appilcation;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Data.DataAccess;
using ActivityScheduler.Shared.Service;
using System.Security.Policy;
using System.Windows.Forms;
using ActivityScheduler.Shared.Pipes;
using System.Timers;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using ActivityScheduler.Data.Models.Settings;
using ActivityScheduler.Data.Models.Communication;
using static System.Net.Mime.MediaTypeNames;
using System.Windows.Threading;
using Binding = System.Windows.Data.Binding;

namespace ActivityScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 



    public partial class MainWindow : System.Windows.Window
    {
        public MainWindowViewModel.SelectionMode SelectionModeVar { get; set; }

        private FormStateHolder formStateHolder = new FormStateHolder();

        private ActivityScheduler.MainWindowViewModel.SelectionMode _selectionMode;

        private MainWindowViewModel viewModel;
        private ActivityScheduler.MainWindowViewModel.SelectionMode SelectionMode
        { 
            get 
            { 
                return _selectionMode;
            }
            set 
            {
                _selectionMode = value;
            }
        }

        public MainWindow(MainWindowViewModel dataContext)
        {
            DataContext = dataContext;
            
            viewModel = (MainWindowViewModel)DataContext;

            viewModel.SelectionModeChanged += ViewModel_SelectionModeChanged;

            formStateHolder.CreateFormState(MainWindowViewModel.SelectionMode.RealBatch.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Visible;
                NumberTxt.Visibility    = Visibility.Visible;
                BatchName.Visibility    = Visibility.Visible;
                BatchNumber.Visibility  = Visibility.Visible;
                
                RunBatch.Visibility     = Visibility.Visible;
                DeleteBatch.Visibility  = Visibility.Visible;
                EditBatch.Visibility    = Visibility.Visible;

            }).Parent.CreateFormState(MainWindowViewModel.SelectionMode.Group.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Visible;
                NumberTxt.Visibility    = Visibility.Visible;
                BatchName.Visibility    = Visibility.Visible;
                BatchNumber.Visibility  = Visibility.Visible;
                
                RunBatch.Visibility     = Visibility.Hidden;
                DeleteBatch.Visibility  = Visibility.Visible;
                EditBatch.Visibility    = Visibility.Visible;

            }).Parent.CreateFormState(MainWindowViewModel.SelectionMode.None.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Hidden;
                NumberTxt.Visibility    = Visibility.Hidden;
                BatchName.Visibility    = Visibility.Hidden;
                BatchNumber.Visibility  = Visibility.Hidden;

                RunBatch.Visibility     = Visibility.Hidden;
                DeleteBatch.Visibility  = Visibility.Hidden;
                EditBatch.Visibility    = Visibility.Hidden;
            });

            InitializeComponent();
        }

        private void ViewModel_SelectionModeChanged(MainWindowViewModel.SelectionMode selectionMode)
        {
            formStateHolder.SetFormState(selectionMode.ToString());
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tag = "Closed";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((viewModel != null) && (viewModel.LoadBatchListCmd.CanExecute(null)))
                viewModel.LoadBatchListCmd.Execute(null);
            BatchList.Focus();
        }

        private void FormStateServiceField_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
