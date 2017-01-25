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

        public MyCommand GoCommand { get; private set; }
        public MyCommand StopCommand { get; private set; }
        public MyCommand IncreaseFontSizeCommand { get; private set; }
        public MyCommand DecreaseFontSizeCommand { get; private set; }

        public MainViewModel()
        {
            GoCommand = new MyCommand(p => Go(), CanGo);
            StopCommand = new MyCommand(p => Stop(), CanStop);
            IncreaseFontSizeCommand = new MyCommand(p => IncreaseFontSize(), CanIncreaseFontSize);
            DecreaseFontSizeCommand = new MyCommand(p => DecreaseFontSize(), CanDecreaseFontSize);

            runner = new PythonRunner();
            runner.Output += Runner_OutputReceived;
            runner.Error += Runner_ErrorReceived;
            runner.ExecutionStateChanged += Runner_ExecutionStateChanged;
            ProgramInteraction.Input += ProgramInteraction_Input;
        }

        public ExecutionState ExecutionState
        {
            get
            {
                return runner.ExecutionState;
            }
        }

        private bool _singleStepMode;

        public bool SingleStepMode
        {
            get
            {
                return _singleStepMode;
            }
            set
            {
                _singleStepMode = value;
                OnPropertyChanged(nameof(SingleStepMode));
            }
        }

        private void Runner_ExecutionStateChanged(object sender, ExecutionStateChangedEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() =>
            {
                switch (e.ExecutionState)
                {
                    case ExecutionState.Stopped:
                        ProgramCode.HighlightedLineNumber = 0;
                        ProgramInteraction.HideInputPrompt();
                        break;

                    case ExecutionState.Paused:
                        if (SingleStepMode)
                        {
                            ProgramCode.HighlightedLineNumber = runner.LineNumber;
                            ProgramInteraction.HideInputPrompt();
                        }
                        else
                        {
                            // Want to run at full speed
                            runner.ExecuteNextLine();
                        }
                        break;

                    case ExecutionState.Running:
                        if (SingleStepMode)
                        {
                            ProgramCode.HighlightedLineNumber = 0;
                            ProgramInteraction.HideInputPrompt();
                        }
                        break;

                    case ExecutionState.WaitingForInput:
                        ProgramInteraction.ShowInputPrompt(!string.IsNullOrWhiteSpace(runner.Prompt) ? runner.Prompt : "Please enter your response:");
                        if (SingleStepMode)
                        {
                            ProgramCode.HighlightedLineNumber = runner.LineNumber;
                        }
                        break;
                }

                OnPropertyChanged(nameof(ExecutionState));
                GoCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            });
        }

        private void ProgramInteraction_Input(object sender, InputEventArgs e)
        {
            // The user has sumbitted an answer to the interactive input prompt
            // So hide the input box
            ProgramInteraction.HideInputPrompt();

            // Show the original prompt (if any)
            if (!string.IsNullOrWhiteSpace(runner.Prompt))
            {
                ProgramInteraction.OutputText(runner.Prompt.Trim() + " ");
            }

            // Show the user's response
            ProgramInteraction.OutputText(e.Answer + Environment.NewLine);

            // Submit the user's answer to the Python program, so it can continue
            runner.SubmitInput(e.Answer);
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
            switch (runner.ExecutionState)
            {
                case ExecutionState.Running:
                case ExecutionState.Paused:
                case ExecutionState.WaitingForInput:
                    return true;
                default:
                    return false;
            }
        }

        private bool CanGo()
        {
            switch (runner.ExecutionState)
            {
                case ExecutionState.Paused:
                case ExecutionState.Stopped:
                    return true;
                default:
                    return false;
            }
        }

        private void Go()
        {
            if (runner.ExecutionState == ExecutionState.Stopped)
            {
                // Need to start the program...
                ProgramInteraction.Reset();
                ProgramCode.SyncCodeToViewModel();
                runner.BeginRun(ProgramCode.Code);
                Properties.Settings.Default.Code = ProgramCode.Code; // Save for next time
            }
            else if (runner.ExecutionState == ExecutionState.Paused)
            {
                runner.ExecuteNextLine();
            }
        }

        private void Stop()
        {
            runner.TerminateRun();
        }

        private void Runner_OutputReceived(object sender, RunnerOutputEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => ProgramInteraction.OutputText(e.Text));
        }

        private void Runner_ErrorReceived(object sender, RunnerErrorMessageEventArgs e)
        {
            App.Current.Dispatcher.Invoke(() => ProgramInteraction.OutputErrorMessage(e.ErrorMessage));
        }
    }
}
