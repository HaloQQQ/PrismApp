using System;
using System.Windows.Input;
using TcpSocket.Models;

namespace TcpSocket.Commands
{
    public class SelectAllCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            var selectAll = parameter as ICanSelect;

            selectAll?.Select();
        }

        public event EventHandler? CanExecuteChanged;
    }
}