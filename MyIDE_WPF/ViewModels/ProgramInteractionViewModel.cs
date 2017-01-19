using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyIDE_WPF.ViewModels
{
    public class ProgramInteractionViewModel : ViewModelBase
    {
        public event EventHandler<string> TextAppeded;

        public event EventHandler TextCleared;

        public event EventHandler<InputEventArgs> Input;

        private void OnInput(string prompt, string answer)
        {
            if (Input != null)
            {
                Input(this, new InputEventArgs(prompt, answer));
            }
        }

        public void ShowInputPrompt(string prompt)
        {
            InputViewModel = new InputViewModel();
            InputViewModel.Prompt = prompt;
            InputViewModel.Input += InputViewModel_Input;
            IsInputActive = true;
        }

        public void HideInputPrompt()
        {
            IsInputActive = false;
        }

        private void InputViewModel_Input(object sender, InputEventArgs e)
        {
            OnInput(e.Prompt, e.Answer);
        }

        private InputViewModel _inputViewModel;

        public InputViewModel InputViewModel
        {
            get
            {
                return this._inputViewModel;
            }
            set
            {
                this._inputViewModel = value;
                OnPropertyChanged(nameof(InputViewModel));
            }
        }

        public void Reset()
        {
            OnTextCleared();
            IsInputActive = false;
            InputViewModel = null;
        }

        private void OnTextCleared()
        {
            if (TextCleared != null)
            {
                TextCleared(this, EventArgs.Empty);
            }
        }

        private bool _isInputActive = false;

        public bool IsInputActive
        {
            get
            {
                return this._isInputActive;
            }
            set
            {
                this._isInputActive = value;
                OnPropertyChanged(nameof(IsInputActive));
            }
        }

        string mostRecentTextOutput = null;

        public void OutputText(string text)
        {
            OnTextAppended(text);
            mostRecentTextOutput = text;
        }

        private void OnTextAppended(string text)
        {
            if (TextAppeded != null)
            {
                TextAppeded(this, text);
            }
        }

        public void OutputErrorMessage(string message)
        {
            // Make sure the error message starts on a new line
            if (!mostRecentTextOutput?.EndsWith(Environment.NewLine) ?? false)
            {
                OutputText(Environment.NewLine);
            }

            // Show the error message
            OutputText(message);
        }
    }
}
