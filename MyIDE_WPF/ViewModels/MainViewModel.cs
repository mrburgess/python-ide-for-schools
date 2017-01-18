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

        private int[] _fontSizes = new int[] { 8, 10, 12, 14, 16, 18, 20, 24, 36, 48, 72 };

        private ProgramCodeViewModel _programCode = new ProgramCodeViewModel();

        public ProgramCodeViewModel ProgramCode
        {
            get
            {
                return this._programCode;
            }
            set
            {
                this._programCode = value;
                OnPropertyChanged(nameof(ProgramCode));
            }
        }

        private bool _isRunning = false;

        public bool IsRunning
        {
            get
            {
                return this._isRunning;
            }
            set
            {
                this._isRunning = value;
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        private ProgramInteractionViewModel _programInteraction = new ProgramInteractionViewModel();

        public ProgramInteractionViewModel ProgramInteraction
        {
            get
            {
                return this._programInteraction;
            }
            set
            {
                this._programInteraction = value;
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

            ProgramInteraction.Input += ProgramInteraction_Input;
        }

        private void ProgramInteraction_Input(object sender, InputEventArgs e)
        {
            // The user has sumbitted an answer to the interactive input prompt
            // So hide the input box
            ProgramInteraction.HideInputPrompt();

            // Copy the prompt and the user's answer into the output window
            if (e.Prompt != "")
                ProgramInteraction.OutputText(e.Prompt + " ");
            ProgramInteraction.OutputText(e.Answer);
            ProgramInteraction.OutputText(Environment.NewLine);

            // Submit the user's answer to the Python program, so it can continue
            runner.SubmitInput(e.Answer);
        }

        private void Runner_Input(object sender, RunnerInputEventArgs e)
        {
            // The Python program has requested input from the user
            App.Current.Dispatcher.Invoke(() =>
            {
                // Show the input box to the user, with the appropriate prompt
                // TO DO: Set the input focus to the text box?
                ProgramInteraction.ShowInputPrompt(e.Prompt);
            });
        }

        private int GetNextBiggestFontSize(int currentFontSize)
        {
            int next = _fontSizes.FirstOrDefault(s => s > currentFontSize);
            return next > 0 ? next : currentFontSize;
        }

        private int GetNextSmallestFontSize(int currentFontSize)
        {
            int next = _fontSizes.LastOrDefault(s => s < currentFontSize);
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
            return _isRunning;
        }

        private bool CanRun()
        {
            return !_isRunning;
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

            _programCode.SyncCodeToViewModel();
            runner.BeginRun(_programCode.Code);

            IsRunning = runner.IsRunning;
            _programCode.IsRunning = IsRunning;
            RunCommand.RaiseCanExecuteChanged();
            StopCommand.RaiseCanExecuteChanged();

            // Save the user's code for next time
            ProgramCode.SyncCodeToViewModel();
            Properties.Settings.Default.Code = ProgramCode.Code;
        }

        private void Stop()
        {
            ProgramInteraction.HideInputPrompt();
            runner.TerminateRun();
        }

        private void Runner_OutputReceived(object sender, RunnerOutputEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => _programInteraction.OutputText(e.Text));
        }

        private void Runner_ErrorReceived(object sender, RunnerErrorMessageEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => _programInteraction.OutputErrorMessage(e.ErrorMessage));
        }

        private void Runner_Terminated(object sender, EventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                IsRunning = runner.IsRunning;
                _programCode.IsRunning = IsRunning;
                RunCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            });
        }
    }
}
