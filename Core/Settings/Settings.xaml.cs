using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
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

namespace ActivityScheduler.Core.Settings
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public Settings()
        {
            InitializeComponent();
            // Create a new binding
            // TheDate is a property of type DateTime on MyData class
/* 
            Binding bind01 = new Binding();
            bind01.Mode = BindingMode.TwoWay;
            bind01.Source = Property1_value;
            BindingOperations.SetBinding(Property2, TextBlock.TextProperty, bind01);

            */

        }

        private void Cbox_Checked(object sender, RoutedEventArgs e)
        {
            //this.txtLength.Text += ((CheckBox)sender).Content;
        }

        private void DropDownFinish_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                return;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void TxtSupplierName_TextChanged(object sender, TextChangedEventArgs e)
        {
        }




                


    }
}
