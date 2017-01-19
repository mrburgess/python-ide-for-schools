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
using System.Windows.Threading;

namespace MyIDE_WPF.Views
{
    /// <summary>
    /// Interaction logic for ProgramInteractionView.xaml
    /// </summary>
    public partial class ProgramInteractionView : UserControl
    {
        public ProgramInteractionView()
        {
            InitializeComponent();

            DataContextChanged += ProgramInteractionView_DataContextChanged;

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(0.1);
            timer.IsEnabled = true;
            timer.Tick += Timer_Tick;
        }

        private StringBuilder sb = new StringBuilder();

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (sb.Length > 0)
            {
                Console.AppendText(sb.ToString());
                sb.Clear();
            }

            if (scrollAttemptsRemaining > 0)
            {
                Console.ScrollToEnd();
                scrollAttemptsRemaining--;
            }
        }

        private void ProgramInteractionView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var dataContext = this.DataContext as ProgramInteractionViewModel;
            if (dataContext != null)
            {
                dataContext.TextAppeded += DataContext_TextAppeded;
                dataContext.TextCleared += DataContext_TextCleared;
            }
        }

        private void DataContext_TextCleared(object sender, EventArgs e)
        {
            Console.Clear();
        }

        int scrollAttemptsRemaining = 0;

        private void DataContext_TextAppeded(object sender, string e)
        {
            sb.Append(e);
            scrollAttemptsRemaining = 5;
        }
    }
}
