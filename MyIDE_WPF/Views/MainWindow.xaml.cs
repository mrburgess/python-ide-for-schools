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
        public MainWindow()
        {
            InitializeComponent();

            MainViewModel vm = new MainViewModel();

            vm.ProgramCode.Code = "print(\"Hello\")";

            this.DataContext = vm;
        }

        private void Grid_Loaded(object sender, RoutedEventArgs e)
        {
            // Never start the application minimised - the user won't be able to find it!
            if (Properties.Settings.Default.WindowState == "Minimized")
            {
                Properties.Settings.Default.WindowState = "Normal";
            }
        }
    }
}
