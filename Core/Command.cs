using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ActivityScheduler.Core
{
    public class ActionCommand : ICommand
    {
        private Action _execute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public ActionCommand(Action execute)
        {
            this._execute = execute;
        }

        public bool CanExecute(object? parameter) => true;

        public void Execute(object? parameter)
        {
            this._execute();
        }
    }
}
