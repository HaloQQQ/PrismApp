using System;
using System.Net;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using IceTea.Atom.Extensions;
using MyApp.Prisms.ViewModels;
using System.ComponentModel;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.SocketStandard.Tcp.Contracts;
using IceTea.Wpf.Atom.Contracts;

namespace MyApp.Prisms.Views
{
    public partial class TcpClientView : UserControl
    {
        private TcpClientViewModel _tcpSocketViewModel;

        public TcpClientView()
        {
            InitializeComponent();

            this._tcpSocketViewModel = this.DataContext as TcpClientViewModel;

            _tcpSocketViewModel.PropertyChanged += OnPropertyChanged;

            this.rhTxt.Clear();
        }

        private ITcpClient _tcpClient;

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BaseSocketViewModel.Socket))
            {
                return;
            }

            _tcpClient = this._tcpSocketViewModel.Socket as ITcpClient;

            if (_tcpClient == null)
            {
                return;
            }

            try
            {
                _tcpClient.Started += socket =>
                    rhTxt.Info(_tcpSocketViewModel.IsLogging, _tcpClient.Name, $"连接{socket}成功..");

                _tcpClient.StartFailed += socket =>
                    rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpClient.Name, $"连接{socket}失败..");

                _tcpClient.ReceivedMessage += (from, to, data) =>
                {
                    string message = _tcpClient.GetString(data).TrimWhiteSpace();

                    if (_tcpSocketViewModel.IsHex && message.IsHexString())
                    {
                        message = message.GetStringFromHex();
                    }

                    this.rhTxt.Recv(from, to, this._tcpSocketViewModel.IsLogging, _tcpClient, message);
                };

                _tcpClient.ExceptionOccurred += (socketName, exception) =>
                    this.rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpClient.Name, exception.Message);

                _tcpClient.SentMessage += (from, to, bytes) =>
                {
                    this.rhTxt.Send(from, to, this._tcpSocketViewModel.IsLogging, _tcpClient, _tcpClient.GetString(bytes));
                    this._tcpSocketViewModel.SendMessage = string.Empty;
                };
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpClient.Name, ex.Message);
            }
        }

        protected string GetMessage(EndPoint from, EndPoint to, string coreMessage) =>
            $"{DateTime.Now.FormatTime("yyyy/MM/dd HH:mm:ss")} {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsThreadPoolThread} {from}=>{to} {coreMessage}";

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Command == CustomCommands.PostCommand) //回车发送
                {
                    var msg = this._tcpSocketViewModel.SendMessage;

                    this._tcpClient.SendAsync(msg);

                    this._tcpSocketViewModel.SendMessage = string.Empty;

                    e.Handled = true;
                }
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpSocketViewModel.Name, ex.Message);
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            e.CanExecute = true;

            if (e.Command == CustomCommands.PostCommand) // 发送消息
            {
                if (this._tcpSocketViewModel != null)
                {
                    var forbid = this._tcpSocketViewModel.SendMessage.IsNullOr(msg => msg.IsNullOrBlank())
                                 || !this._tcpClient.IsConnected;
                    e.CanExecute = !forbid;
                }
            }
        }
    }
}