using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MyIDE_WPF.ViewModels
{
    public class MyCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        private Action<object> execute;
        private Func<bool> canExecute;

        public MyCommand(Action<object> execute, Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return canExecute != null ? canExecute() : true;
        }

        public void Execute(object parameter)
        {
            if (this.execute != null)
            {
                this.execute(parameter);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}
