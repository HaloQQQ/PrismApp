using System;
using System.Windows;
using System.Windows.Input;

namespace TcpSocket.Commands
{
    public class NotifyIconCommand : ICommand
    {
        public bool CanExecute(object? parameter)
        {
            return true;
        }

        public void Execute(object? parameter)
        {
            if (Helper.Helper.Equals(parameter, "Show"))
            {
                if (App.Current.MainWindow.Visibility == Visibility.Visible)
                {
                    App.Current.MainWindow.Hide();
                }
                else
                {
                    App.Current.MainWindow.Show();
                }
            }
            else if (Helper.Helper.Equals(parameter, "Hide"))
            {
                App.Current.MainWindow.Hide();
            }
            else if (Helper.Helper.Equals(parameter, "Exit"))
            {
                App.Current.MainWindow.Close();
            }
        }

        public event EventHandler? CanExecuteChanged;
    }

    public class NotifyIconDataContext
    {
        public ICommand NotifyCommand => new NotifyIconCommand();
    }
}