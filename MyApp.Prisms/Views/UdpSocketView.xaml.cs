using System;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Atom.Contracts;
using System.ComponentModel;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.SocketStandard.Udp.Contracts;
using PrismAppBasicLib.Contracts;

namespace MyApp.Prisms.Views
{
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
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
                if (e.Command == CustomCommands.TextBoxEnterCommand)    // 回车发送
                {
                    var msg = this._udpSocketViewModel.SendMessage;

                    this._udpSocketViewModel.Socket.SendAsync(msg);

                    this._udpSocketViewModel.SendMessage = string.Empty;

                    e.Handled = true;
                }
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

            if (e.Command == CustomCommands.TextBoxEnterCommand)
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
                e.CanExecute = this.rhTxt.Document.Blocks.Count > 0 || (this._udpSocketViewModel.IsNotNullAnd(model => !string.IsNullOrEmpty(model.SendMessage?.Trim())));
            }
        }
    }
}