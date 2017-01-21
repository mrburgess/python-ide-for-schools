using MyIDE_WPF.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIDE_WPF.ViewModels
{
    public class ProgramCodeViewModel : ViewModelBase
    {
        public event PropertyChangedEventHandler SyncRequested;

        private string code;

        public string Code
        {
            get
            {
                return this.code;
            }
            set
            {
                this.code = value;
                OnPropertyChanged(nameof(Code));
            }
        }

        private int _highlightedLineNumber = 0;

        public int HighlightedLineNumber
        {
            get
            {
                return _highlightedLineNumber;
            }
            set
            {
                _highlightedLineNumber = value;
                OnPropertyChanged(nameof(HighlightedLineNumber));
            }
        }

        public void SyncCodeToViewModel()
        {
            OnSyncRequested(nameof(Code));
        }

        protected void OnSyncRequested(string propertyName)
        {
            if (SyncRequested != null)
            {
                SyncRequested(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}
