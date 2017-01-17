﻿using System;
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
    /// Interaction logic for ProgramInteractionView.xaml
    /// </summary>
    public partial class ProgramInteractionView : UserControl
    {
        public ProgramInteractionView()
        {
            InitializeComponent();
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange != 0)
            {
                ((ScrollViewer)sender).ScrollToBottom();
            }
        }
    }
}