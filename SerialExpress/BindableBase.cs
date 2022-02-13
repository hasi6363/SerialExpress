using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace SerialExpress
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void RaisePropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
    public class DelegateCommand : ICommand
    {
        System.Action<object?> execute;
        System.Func<bool> canExecute;

        public bool CanExecute(object? parameter)
        {
            return canExecute();
        }

        public event System.EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object? parameter)
        {
            execute(parameter);
        }

        public DelegateCommand(System.Action<object?> execute, System.Func<bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }
    }
}
