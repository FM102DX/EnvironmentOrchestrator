using ActivityScheduler.Data.Contracts;
using ActivityScheduler.Data.Managers;
using ActivityScheduler.Data.Models;
using ActivityScheduler.Gui.EditWindow;
using ActivityScheduler.Shared;
using ActivityScheduler.Shared.Service;
using Newtonsoft.Json.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using static ActivityScheduler.Core.SetParentActivities;
using static System.Windows.Forms.DataFormats;

namespace ActivityScheduler.Core
{
    
    public partial class SetParentActivities : Window
    {
        private Serilog.ILogger _logger;
        private EditWindowViewModel _editWindowViewModel;
        private ViewModel _viewModel;

        public SetParentActivities(ActivityManager activityManager, Batch currentBatch,  Activity currentActivity, Serilog.ILogger logger, EditWindowViewModel editWindowViewModel)
        {
            _logger = logger;
            
            _editWindowViewModel = editWindowViewModel;

            _viewModel = new ViewModel(activityManager, currentBatch, currentActivity);

            DataContext = _viewModel;

            InitializeComponent();
            
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
           // LoadActivities();
        }

        public class ParentActivitySelectionViewModel
        {
            public Guid Id{ get; set; }
            public bool Selected { get; set;}
            public int ActivityId { get; set; }
            public string Text { get; set; }

        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
                _editWindowViewModel.SetParentActivities(_viewModel.ParentActivitiesStr);
                Close();
        }

        public class ViewModel : INotifyPropertyChanged
        {
            public List<ParentActivitySelectionViewModel> ItemsAct { get; set; }
            private ActivityManager _activityManager;
            private Batch _currentBatch;
            private Activity _currentActivity;

            public event PropertyChangedEventHandler? PropertyChanged;

            public ViewModel(ActivityManager activityManager, Batch currentBatch, Activity currentActivity)
            {
                ItemsAct = new List<ParentActivitySelectionViewModel>();
                
                _currentBatch = currentBatch;

                _currentActivity = currentActivity;

                _activityManager = activityManager;

                var _activitiesList = _activityManager.GetAll(_currentBatch.Id).Result.ToList();

                ItemsAct = _activitiesList.Select(x => x.AsViewModel()).ToList().Select(x => new ParentActivitySelectionViewModel() { Id = x.Id, Selected = false, ActivityId = x.ActivityId, Text = x.Name }).ToList();

                var x = _currentActivity.GetParentActionIds().ToArray();

                foreach (ParentActivitySelectionViewModel y in ItemsAct)
                {
                    if (x.Contains(y.ActivityId))
                    {
                        y.Selected = true;
                    }
                }
            }

            public string ParentActivitiesStr
            {
                get
                {
                    return string.Join(',', ItemsAct.Where(x => x.Selected == true).ToList().Select(x => x.ActivityId).ToList());
                }
            }
        }
    }
}
