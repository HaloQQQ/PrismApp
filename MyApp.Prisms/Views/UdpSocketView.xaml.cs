using IceTea.SocketStandard.Base;
using System;
using System.Net.Sockets;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Core.Contracts;
using System.ComponentModel;
using MyApp.Prisms.ViewModels.BaseViewModels;

namespace MyApp.Prisms.Views
{
    public partial class UdpSocketView : UserControl
    {
        private UdpSocketViewModel _udpSocketViewModel;

        public UdpSocketView()
        {
            InitializeComponent();

            this._udpSocketViewModel = this.DataContext as UdpSocketViewModel;

            _udpSocketViewModel.PropertyChanged += OnPropertyChanged;

            this.rhTxt.Clear();
        }

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(BaseSocketViewModel.Socket))
            {
                return;
            }

            if (!(this._udpSocketViewModel.Socket is IUdpSocket udpSocket))
            {
                return;
            }

            try
            {
                udpSocket.Started += ip => this.rhTxt.Info(this._udpSocketViewModel.IsLogging, udpSocket.Name, $"{ip}已启动");

                udpSocket.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = udpSocket.GetString(bytes).TrimWhiteSpace();

                    if (_udpSocketViewModel.IsHex && message.IsHexString())
                    {
                        message = message.GetStringFromHex();
                    }

                    this.rhTxt.Recv(from, to, this._udpSocketViewModel.IsLogging, udpSocket, message);
                };

                udpSocket.SentMessage += (from, to, bytes) =>
                {
                    this.rhTxt.Send(from, to, this._udpSocketViewModel.IsLogging, udpSocket, udpSocket.GetString(bytes));
                    this._udpSocketViewModel.SendMessage = string.Empty;
                };

                udpSocket.ExceptionOccurred += (remotePoint, exception) =>
                    this.rhTxt.Info(this._udpSocketViewModel.IsLogging, udpSocket.Name, $"{remotePoint}出现异常:{exception.Message}");
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._udpSocketViewModel.IsLogging, udpSocket.Name, ex.Message);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Command == CustomCommands.PostCommand)    // 回车发送
                {
                    var msg = this._udpSocketViewModel.SendMessage;

                    this._udpSocketViewModel.Socket.SendAsync(msg);

                    this._udpSocketViewModel.SendMessage = string.Empty;

                    e.Handled = true;
                }
                else if (e.Command == ApplicationCommands.Open)      // 打开日志文件
                {
                    if (e.Source == this.rhTxt || CustomConstants.LOG.EqualsIgnoreCase(e.Parameter?.ToString()))
                    {
                        Helper.Helper.OpenLog(this._udpSocketViewModel.Name);

                        e.Handled = true;
                    }
                }
            }
            catch (SocketException ex)
            {
                this.rhTxt.Info(this._udpSocketViewModel.IsLogging, this._udpSocketViewModel.Socket.Name, ex.Message);
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._udpSocketViewModel.IsLogging, this._udpSocketViewModel.Socket.Name, ex.Message);
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            e.CanExecute = true;

            if (e.Command == CustomCommands.PostCommand)
            {
                if (this._udpSocketViewModel != null)
                {
                    var forbid = string.IsNullOrEmpty(this._udpSocketViewModel.SendMessage)
                        || this._udpSocketViewModel.SendMessage.Trim().Length == 0
                        || !this._udpSocketViewModel.Socket.IsConnected;

                    e.CanExecute = !forbid;
                }
            }
            else if (e.Command == NavigationCommands.Refresh)
            {
                e.CanExecute = this.rhTxt.Document.Blocks.Count > 0 || (this._udpSocketViewModel != null && !string.IsNullOrEmpty(this._udpSocketViewModel.SendMessage?.Trim()));
            }
        }
    }
}