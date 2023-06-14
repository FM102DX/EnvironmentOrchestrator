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
using ActivityScheduler.Gui.MainWindow;

namespace ActivityScheduler
{

    public partial class MainWindow : System.Windows.Window
    {
        public MainWindowViewModel.SelectionMode SelectionModeVar { get; set; }

        private FormStateHolder _formStateHolder = new FormStateHolder();

        private MainWindowViewModel.SelectionMode _selectionMode;

        private MainWindowViewModel _viewModel;
        private MainWindowViewModel.SelectionMode SelectionMode
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
            
            _viewModel = (MainWindowViewModel)DataContext;

            _viewModel.SelectionModeChanged += ViewModel_SelectionModeChanged;

            _viewModel.ListSourceChanged += ViewModel_ListSourceChanged;

            _viewModel.RunningBatchesInfoUpdated += ViewModel_RunningBatchesInfoUpdated;

            _formStateHolder.CreateFormState(MainWindowViewModel.SelectionMode.RealBatchRunning.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Visible;
                NumberTxt.Visibility    = Visibility.Visible;
                BatchName.Visibility    = Visibility.Visible;
                BatchNumber.Visibility  = Visibility.Visible;
                RunBatch.Visibility     = Visibility.Hidden;
                StopBatch.Visibility    = Visibility.Visible;
                DeleteBatch.Visibility  = Visibility.Visible;
                EditBatch.Visibility    = Visibility.Visible;

            }).Parent.CreateFormState(MainWindowViewModel.SelectionMode.RealBatchStopped.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Visible;
                NumberTxt.Visibility    = Visibility.Visible;
                BatchName.Visibility    = Visibility.Visible;
                BatchNumber.Visibility  = Visibility.Visible;
                RunBatch.Visibility     = Visibility.Visible;
                StopBatch.Visibility    = Visibility.Hidden;
                DeleteBatch.Visibility  = Visibility.Visible;
                EditBatch.Visibility    = Visibility.Visible;

            }).Parent.CreateFormState(MainWindowViewModel.SelectionMode.Group.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Visible;
                NumberTxt.Visibility    = Visibility.Visible;
                BatchName.Visibility    = Visibility.Visible;
                BatchNumber.Visibility  = Visibility.Visible;
                
                RunBatch.Visibility     = Visibility.Hidden;
                StopBatch.Visibility    = Visibility.Hidden;

                DeleteBatch.Visibility  = Visibility.Visible;
                EditBatch.Visibility    = Visibility.Visible;

            }).Parent.CreateFormState(MainWindowViewModel.SelectionMode.None.ToString()).AddAction(() => {
                NameTxt.Visibility      = Visibility.Hidden;
                NumberTxt.Visibility    = Visibility.Hidden;
                BatchName.Visibility    = Visibility.Hidden;
                BatchNumber.Visibility  = Visibility.Hidden;

                RunBatch.Visibility     = Visibility.Hidden;
                StopBatch.Visibility    = Visibility.Hidden;

                DeleteBatch.Visibility  = Visibility.Hidden;
                EditBatch.Visibility    = Visibility.Hidden;
            });

            InitializeComponent();
        }

        private void ViewModel_RunningBatchesInfoUpdated(List<Batch> runningBatchList)
        {
            //here syncronize tabs with batches

            Tabs.Dispatcher.Invoke(() =>
            {
                foreach (var x in runningBatchList)
                {
                    //open tabs wich are running but not opened

                    var lst2 = Tabs.Items.OfType<TabItem>().ToList().Where(y => y.Header == x.Name).ToList();

                    if (lst2.Count == 0)
                    {
                        //tab not opened, need to open
                        var tbi = new TabItem();
                        tbi.Header = x.Name;
                        //tbi.Name = x.Name;

                        StackPanel stackPanel = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Horizontal, VerticalAlignment = VerticalAlignment.Top };
                        stackPanel.Children.Add(new System.Windows.Controls.TextBox { Height = 60, Width = 60, TextWrapping = TextWrapping.Wrap, Text = "Some text" });

                        var listBox = new System.Windows.Controls.ListBox();
                        listBox.Items.Add(new ListBoxItem { Content = "Text" });
                        listBox.Items.Add(new ListBoxItem { Content = "Text to" });
                        listBox.Items.Add(new ListBoxItem { Content = "And text" });

                        StackPanel stackPanelAsContent = new StackPanel() { Orientation = System.Windows.Controls.Orientation.Vertical, HorizontalAlignment = 0 };
                        stackPanelAsContent.Children.Add(stackPanel);
                        stackPanelAsContent.Children.Add(listBox);
                        tbi.Content = stackPanelAsContent;
                        Tabs.Items.Add(tbi);
                    }
                }
            });

            Tabs.Dispatcher.Invoke(() => {

                var lst3 = Tabs.Items.OfType<TabItem>().ToList();

                lst3.ForEach(x =>
                {
                    bool notContainingInRunningBatches = runningBatchList.Select(y => y.Name).ToList().Contains(x.Header);
                    bool isMainTab = x.Name.ToLower() == "main";
                    bool isInfoTab = x.Name.ToLower() == "info";
                    if (!notContainingInRunningBatches && !isMainTab && !isInfoTab)
                    {
                           Tabs.Items.Remove(x);
                    }
                });
            });
        }

        private void ViewModel_ListSourceChanged()
        {
            BatchList.Dispatcher.Invoke(()=> { BatchList.Items.Refresh(); });
        }

        private void ViewModel_SelectionModeChanged(MainWindowViewModel.SelectionMode selectionMode)
        {
            this.Dispatcher.Invoke(() => {
                _formStateHolder.SetFormState(selectionMode.ToString());
            });
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tag = "Closed";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if ((_viewModel != null) && _viewModel.LoadBatchListCmd.CanExecute(null))
                _viewModel.LoadBatchListCmd.Execute(null);
            BatchList.Focus();
        }

        private void FormStateServiceField_TextChanged(object sender, TextChangedEventArgs e)
        {
            
        }
    }
}
