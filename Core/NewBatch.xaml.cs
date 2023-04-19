using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Models;
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
        private App _app;
        private Serilog.ILogger _logger;
        private IAsyncRepositoryT<Batch> _repo;
        public NewBatch(App app, Serilog.ILogger logger)
        {
            _app = app;
            _logger = logger;
            InitializeComponent();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BatchCreate_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
