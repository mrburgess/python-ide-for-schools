using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyIDE_WPF.ViewModels
{
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public event PropertyChangedEventHandler SyncRequested;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
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
