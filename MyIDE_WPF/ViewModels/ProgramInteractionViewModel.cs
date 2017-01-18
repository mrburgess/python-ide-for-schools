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
            InputViewModel.Prompt = prompt != "" ? prompt : "Please enter your response";
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

        private ObservableCollection<string> _lines = new ObservableCollection<string>();

        public ObservableCollection<string> Lines
        {
            get
            {
                return this._lines;
            }
            set
            {
                this._lines = value;
                OnPropertyChanged(nameof(Lines));
            }
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
            Lines.Clear();
            atStartOfLine = true;
            IsInputActive = false;
            InputViewModel = null;
        }

        private bool atStartOfLine = true;

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

        public void OutputText(string text)
        {
            bool textIsTerminatedWithNewLine = text.EndsWith(Environment.NewLine);

            if (textIsTerminatedWithNewLine)
            {
                text = text.TrimEnd(Environment.NewLine.ToCharArray());
            }

            if (atStartOfLine)
            {
                // At start of line, so just add a new line
                _lines.Add(text);
            }
            else
            {
                // Part way through a line, so append to the last one
                _lines[_lines.Count - 1] += text;
            }

            atStartOfLine = textIsTerminatedWithNewLine;
        }

        public void OutputErrorMessage(string message)
        {
            atStartOfLine = true;
            _lines.Add(message.Trim(Environment.NewLine.ToCharArray()));
        }
    }
}
