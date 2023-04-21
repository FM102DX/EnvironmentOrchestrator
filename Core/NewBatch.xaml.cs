using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Shared;
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
using System.Windows.Shapes;

namespace ActivityScheduler.Core
{
    /// <summary>
    /// Interaction logic for NewBatch.xaml
    /// </summary>
    public partial class NewBatch : Window
    {
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Batch> _repo;
        private BatchManager _batchManager;
        private MainWindow _mainWindow;
        public NewBatch(MainWindow mainWindow, BatchManager batchManager, Serilog.ILogger logger)
        {
            _logger = logger;
            _batchManager = batchManager;
            _mainWindow = mainWindow;
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }



        private void BatchCreate_Click(object sender, RoutedEventArgs e)
        {

            CommonOperationResult chkRez;
            chkRez = _batchManager.CheckNumber(BatchNumber.Text).Result;
            if (!chkRez.Success)
            {
                MessageBox.Show(chkRez.Message);
                BatchNumber.Focus();
                return;
            }

            chkRez = _batchManager.CheckName(BatchName.Text).Result;
            if (!chkRez.Success)
            {
                MessageBox.Show(chkRez.Message);
                BatchName.Focus();
                return;
            }

            Batch batch = new Batch();
            batch.Number = BatchNumber.Text;
            batch.Name = BatchName.Text;
            batch.IsGroup = (bool)IsGroup.IsChecked;

            var btcAddRez= _batchManager.AddNewBatch(batch).Result;
            if(!btcAddRez.Success)
            {
                MessageBox.Show(btcAddRez.Message);
                return;
            }
            _mainWindow.LoadBatchList();
            Close();

            //MessageBox.Show("Btach successfully added");
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void IsGroup_Loaded(object sender, RoutedEventArgs e)
        {
            IsGroup.IsChecked=false;
        }
    }
}
