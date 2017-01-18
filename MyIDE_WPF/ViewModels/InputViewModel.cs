using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIDE_WPF.ViewModels
{
    public class InputEventArgs : EventArgs
    {
        public string Prompt { get; set; }
        public string Answer { get; set; }

        public InputEventArgs(string prompt, string answer)
        {
            this.Prompt = prompt;
            this.Answer = answer;
        }
    }

    public class InputViewModel : ViewModelBase
    {
        public event EventHandler<InputEventArgs> Input;

        public MyCommand SubmitCommand { get; set; }

        public InputViewModel()
        {
            SubmitCommand = new MyCommand(Submit, CanSubmit);
        }

        private void Submit(object parameter)
        {
            OnInput(Prompt, Answer);
        }

        private bool CanSubmit()
        {
            return true;
        }

        private string prompt;

        public string Prompt
        {
            get
            {
                return this.prompt;
            }
            set
            {
                this.prompt = value;
                OnPropertyChanged(nameof(Prompt));
            }
        }

        private string answer;

        public string Answer
        {
            get
            {
                return this.answer;
            }
            set
            {
                this.answer = value;
                OnPropertyChanged(nameof(Answer));
            }
        }

        private void OnInput(string prompt, string answer)
        {
            if (Input != null)
            {
                Input(this, new InputEventArgs(prompt, answer));
            }
        }
    }
}
