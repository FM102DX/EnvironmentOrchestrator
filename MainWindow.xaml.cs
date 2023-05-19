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

namespace ActivityScheduler
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {

        private FormStateHolder formStateHolder = new FormStateHolder();

        public MainWindow(MainWindowViewModel dataContext)
        {
            DataContext = dataContext;
            formStateHolder.CreateFormState("normal").AddAction(() => {
                NameTxt.Visibility = Visibility.Hidden;
                NumberTxt.Visibility = Visibility.Hidden;
                BatchName.Visibility = Visibility.Visible;
                BatchNumber.Visibility = Visibility.Visible;
            }).Parent.CreateFormState("isgroup").AddAction(() => {
                NameTxt.Visibility = Visibility.Visible;
                NumberTxt.Visibility = Visibility.Visible;
                BatchName.Visibility = Visibility.Visible;
                BatchNumber.Visibility = Visibility.Visible;
            }).Parent.CreateFormState("none").AddAction(() => {
                NameTxt.Visibility = Visibility.Hidden;
                NumberTxt.Visibility = Visibility.Hidden;
                BatchName.Visibility = Visibility.Hidden;
                BatchNumber.Visibility = Visibility.Hidden;
            });
            InitializeComponent();
        }
       
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Tag = "Closed";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
          //  LoadBatchList();
           // _timer.Start();
        }



        private void BatchList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //var item =  e.AddedItems.Cast<BatchListBoxViewModel>().ToList().FirstOrDefault();
            //if (item == null) { return; }
            //if (item.BatchObject == null) 
            //{
            //    _selectionMode= SelectionMode.None;
            //    formStateHolder.SetFormState("none");
            //    return;
            //}
            //Batch btc = (Batch)item.BatchObject;
            //_currentBatch = btc;
            //BatchNumber.IsReadOnly = true;
            //BatchName.IsReadOnly = true;
            //BatchNumber.Text = "";
            //BatchName.Text = "";
            //if (btc.IsGroup)
            //{
            //    _selectionMode = SelectionMode.Group;
            //    formStateHolder.SetFormState("isgroup");
            //    BatchNumber.Text = btc.Number;
            //    BatchName.Text = btc.Name;
            //}
            //else
            //{
            //    _selectionMode = SelectionMode.RealBatch;
            //    formStateHolder.SetFormState("normal");
            //    BatchNumber.Text = btc.Number;
            //    BatchName.Text = btc.Name;
            //}
        }

        private void EditBatch_Click(object sender, RoutedEventArgs e)
        {
            OpenEditBatchForm();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
          //  settingsFrm = new Core.Settings.Settings(_settingsManager, _app, _workerMgr);
           //+ settingsFrm.ShowDialog();
        }

        private void DeleteBatch_Click(object sender, RoutedEventArgs e)
        {
            //if (_selectionMode == SelectionMode.None) { return; }
            //if (_currentBatch == null) { return; }
            //var rez = _batchManager.RemoveBatch(_currentBatch.Id).Result;
            //if (!rez.Success)
            //{
            //    ShowRed($"{rez.Message}");
            //}
            //LoadBatchList();
        }


        private void ShowRed(string text)
        {
            // InfoTb.font
            //InfoTb.Text = text;
            //InfoTb.Foreground = Brushes.Red;
        }
        private void ShowGreen(string text)
        {
            // InfoTb.font
            //InfoTb.Text = text;
            //InfoTb.Foreground = Brushes.Green;
        }

        private void BatchList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenEditBatchForm();
        }

        private void OpenEditBatchForm()
        {
            //if (_selectionMode != SelectionMode.None)
            //{
            //    EditBatch editBatch = new EditBatch(this, _batchManager, _activityManager, _currentBatch, _logger);
            //    editBatch.Show();
            //}
        }

        private void NewGroup_Click(object sender, RoutedEventArgs e)
        {

        }



        private void RunBatch_Click(object sender, RoutedEventArgs e)
        {

            //if (_currentBatch == null) return;

            ////send message to service

            //_pipeServer.SendObject(new AppToWorkerMessage()
            //{
            //    MessageType="Command",
            //    Command = "startbatch",
            //    StartTime = DateTime.Now,  
            //    TransactionId = _currentBatch.Number
            //});
        }

    }
}
