using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIDE_WPF.ViewModels
{
    public class ProgramCodeViewModel : ViewModelBase
    {
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

        public void SyncCodeToViewModel()
        {
            OnSyncRequested(nameof(Code));
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
    }
}
