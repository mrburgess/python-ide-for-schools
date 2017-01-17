using MyIDE_WPF.Models;
using MyIDE_WPF.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MyIDE_WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const int MinimumFontSize = 8;
        private const int MaximumFontSize = 36;

        private int[] fontSizes = new int[] { 8, 10, 12, 14, 16, 18, 20, 24, 36, 48, 72 };

        private ProgramCodeViewModel programCode = new ProgramCodeViewModel();

        public ProgramCodeViewModel ProgramCode
        {
            get
            {
                return this.programCode;
            }
            set
            {
                this.programCode = value;
                OnPropertyChanged(nameof(ProgramCode));
            }
        }

        private bool isRunning = false;

        public bool IsRunning
        {
            get
            {
                return this.isRunning;
            }
            set
            {
                this.isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private ProgramInteractionViewModel programInteraction = new ProgramInteractionViewModel();

        public ProgramInteractionViewModel ProgramInteraction
        {
            get
            {
                return this.programInteraction;
            }
            set
            {
                this.programInteraction = value;
                OnPropertyChanged(nameof(ProgramInteraction));
            }
        }

        private PythonRunner runner;

        public MyCommand RunCommand { get; private set; }
        public MyCommand StopCommand { get; private set; }
        public MyCommand IncreaseFontSizeCommand { get; private set; }
        public MyCommand DecreaseFontSizeCommand { get; private set; }

        public MainViewModel()
        {
            RunCommand = new MyCommand((parameter) => Run(), CanRun);
            StopCommand = new MyCommand((parameter) => Stop(), CanStop);
            IncreaseFontSizeCommand = new MyCommand((parameter) => IncreaseFontSize(), CanIncreaseFontSize);
            DecreaseFontSizeCommand = new MyCommand((parameter) => DecreaseFontSize(), CanDecreaseFontSize);

            runner = new PythonRunner();
            runner.Output += Runner_OutputReceived;
            runner.Error += Runner_ErrorReceived;
            runner.Terminated += Runner_Terminated;
            runner.Input += Runner_Input;
        }

        private void Runner_Input(object sender, InputEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                programInteraction.OutputText(e.Prompt + " ");
                string input = "Andrew";
                runner.SubmitInput(input);
                programInteraction.OutputText(input + Environment.NewLine);
            });
        }

        private int GetNextBiggestFontSize(int currentFontSize)
        {
            int next = fontSizes.FirstOrDefault(s => s > currentFontSize);
            return next > 0 ? next : currentFontSize;
        }

        private int GetNextSmallestFontSize(int currentFontSize)
        {
            int next = fontSizes.LastOrDefault(s => s < currentFontSize);
            return next > 0 ? next : currentFontSize;
        }

        private bool CanIncreaseFontSize()
        {
            return GetNextBiggestFontSize(Settings.Default.CodeFontSize) != Settings.Default.CodeFontSize;
        }

        private void IncreaseFontSize()
        {
            if (CanIncreaseFontSize())
            {
                Settings.Default.CodeFontSize = GetNextBiggestFontSize(Settings.Default.CodeFontSize);
            }
        }

        private bool CanDecreaseFontSize()
        {
            return GetNextSmallestFontSize(Settings.Default.CodeFontSize) != Settings.Default.CodeFontSize;
        }

        private void DecreaseFontSize()
        {
            if (CanDecreaseFontSize())
            {
                Settings.Default.CodeFontSize = GetNextSmallestFontSize(Settings.Default.CodeFontSize);
            }
        }

        private bool CanStop()
        {
            return isRunning;
        }

        private bool CanRun()
        {
            return !isRunning;
        }

        private void Run()
        {
            runner.TerminateRun();

            if (ProgramInteraction == null)
            {
                ProgramInteraction = new ProgramInteractionViewModel();
            }
            else
            {
                ProgramInteraction.Reset();
            }

            programCode.SyncCodeToViewModel();
            runner.BeginRun(programCode.Code);

            IsRunning = runner.IsRunning;
            programCode.IsRunning = IsRunning;
            RunCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();

            // Save the user's code for next time
            ProgramCode.SyncCodeToViewModel();
            Properties.Settings.Default.Code = ProgramCode.Code;
        }

        private void Stop()
        {
            runner.TerminateRun();
        }

        private void Runner_OutputReceived(object sender, OutputEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => programInteraction.OutputText(e.Text));
        }

        private void Runner_ErrorReceived(object sender, ErrorMessageEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => programInteraction.OutputErrorMessage(e.ErrorMessage));
        }

        private void Runner_Terminated(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                IsRunning = runner.IsRunning;
                programCode.IsRunning = IsRunning;
                RunCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            });
        }
    }
}
