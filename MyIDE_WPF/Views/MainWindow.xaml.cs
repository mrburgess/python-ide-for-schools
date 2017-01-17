using MyIDE_WPF.Models;
using MyIDE_WPF.ViewModels;
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

namespace MyIDE_WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        MainViewModel model;

        public MainWindow()
        {
            InitializeComponent();

            model = new MainViewModel();
            this.DataContext = model;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Never start the application minimised - the user won't be able to find it!
            if (Properties.Settings.Default.WindowState == "Minimized")
            {
                Properties.Settings.Default.WindowState = "Normal";
            }

            // Restore the code to what the user was working on last time...
            model.ProgramCode.Code = Properties.Settings.Default.Code;
        }
    }
}
