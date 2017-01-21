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

        public MyCommand RunCommand { get; private set; }
        public MyCommand StopCommand { get; private set; }
        public MyCommand StepCommand { get; private set; }
        public MyCommand IncreaseFontSizeCommand { get; private set; }
        public MyCommand DecreaseFontSizeCommand { get; private set; }

        public MainViewModel()
        {
            RunCommand = new MyCommand((parameter) => Run(), CanRun);
            StopCommand = new MyCommand((parameter) => Stop(), CanStop);
            StepCommand = new MyCommand((parameter) => Step(), CanStep);
            IncreaseFontSizeCommand = new MyCommand((parameter) => IncreaseFontSize(), CanIncreaseFontSize);
            DecreaseFontSizeCommand = new MyCommand((parameter) => DecreaseFontSize(), CanDecreaseFontSize);

            runner = new PythonRunner();
            runner.Output += Runner_OutputReceived;
            runner.Error += Runner_ErrorReceived;
            runner.ExecutionStateChanged += Runner_ExecutionStateChanged;
            ProgramInteraction.Input += ProgramInteraction_Input;
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
                        ProgramCode.HighlightedLineNumber = runner.LineNumber;
                        ProgramInteraction.HideInputPrompt();
                        break;

                    case ExecutionState.Running:
                        ProgramCode.HighlightedLineNumber = 0;
                        ProgramInteraction.HideInputPrompt();
                        break;

                    case ExecutionState.WaitingForInput:
                        ProgramCode.HighlightedLineNumber = runner.LineNumber;
                        ProgramInteraction.ShowInputPrompt(
                            !string.IsNullOrWhiteSpace(runner.Prompt) ? runner.Prompt : "Please enter your response:");
                        break;
                }

                RunCommand.RaiseCanExecuteChanged();
                StepCommand.RaiseCanExecuteChanged();
                StopCommand.RaiseCanExecuteChanged();
            });
        }

        private void Step()
        {
            Debug.Assert(runner.ExecutionState == ExecutionState.Paused);

            runner.StepNextLine();
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

        private bool CanRun()
        {
            switch (runner.ExecutionState)
            {
                case ExecutionState.Stopped:
                    return true;
                default:
                    return false;
            }
        }

        private bool CanStep()
        {
            switch (runner.ExecutionState)
            {
                case ExecutionState.Paused:
                    return true;
                default:
                    return false;
            }
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

            ProgramCode.SyncCodeToViewModel();
            runner.BeginRun(ProgramCode.Code);

            // Save the user's code for next time
            Properties.Settings.Default.Code = ProgramCode.Code;
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
