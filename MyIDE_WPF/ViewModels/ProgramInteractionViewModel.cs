using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyIDE_WPF.ViewModels
{
    public class ProgramInteractionViewModel : ViewModelBase
    {
        private ObservableCollection<string> lines = new ObservableCollection<string>();

        public ObservableCollection<string> Lines
        {
            get
            {
                return this.lines;
            }
            set
            {
                this.lines = value;
                OnPropertyChanged(nameof(Lines));
            }
        }

        public void Reset()
        {
            lines.Clear();
            atStartOfLine = true;
        }

        private bool atStartOfLine = true;

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
                lines.Add(text);
            }
            else
            {
                // Part way through a line, so append to the last one
                lines[lines.Count - 1] += text;
            }

            atStartOfLine = textIsTerminatedWithNewLine;
        }

        public void OutputErrorMessage(string message)
        {
            atStartOfLine = true;
            lines.Add(message.Trim(Environment.NewLine.ToCharArray()));
        }
    }
}
