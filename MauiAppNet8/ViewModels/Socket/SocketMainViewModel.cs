using IceTea.Atom.BaseModels;
using MauiAppNet8.Views.Socket;
using Prism.Commands;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels.Socket
{
    class SocketMainViewModel : BaseNotifyModel
    {
        public SocketMainViewModel()
        {
            TcpServerDetailCommand = new DelegateCommand(async () => await Shell.Current.GoToAsync(nameof(TcpServerView),
                new Dictionary<string, object>()
                {
                    { "test", "Hello" }
                })
            );

            TcpClientDetailCommand = new DelegateCommand(async () => await Shell.Current.GoToAsync(nameof(TcpClientView)));

            UdpDetailCommand = new DelegateCommand(async () => await Shell.Current.GoToAsync(nameof(UdpSocketView)));
        }

        #region Commands
        public ICommand TcpServerDetailCommand { get; private set; }
        public ICommand TcpClientDetailCommand { get; private set; }
        public ICommand UdpDetailCommand { get; private set; }
        #endregion
    }
}
