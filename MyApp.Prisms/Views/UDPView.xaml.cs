using IceTea.SocketStandard.Base;
using IceTea.SocketStandard.Udp;
using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Core.Contracts;

namespace MyApp.Prisms.Views
{
    public partial class UDPView : UserControl
    {
        protected UDPViewModel _socketContext;

        public UDPView()
        {
            InitializeComponent();

            this._socketContext = this.DataContext as UDPViewModel;

            this.rhTxt.Clear();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.ResolveMsg += str =>
            {
                if (this._socketContext.IsHex)
                {
                    if (str.IsHexString())
                    {
                        return str.GetStringFromHex();
                    }

                    return str.GetHexFromString();
                }

                return str;
            };
        }

        protected ISocket _udpSocket = null!;

        public Func<string, string> ResolveMsg = null!;

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this._udpSocket.IsNotNullAnd(udp => udp.IsConnected))
            {
                this._udpSocket.Close();
                return;
            }

            try
            {
                this._udpSocket = new NewUdpSocket(this._socketContext.Encoding, false,
                    this._socketContext.IP, ushort.Parse(this._socketContext.Port),
                    this._socketContext.TargetIP, ushort.Parse(this._socketContext.TargetPort));

                this._udpSocket!.Started += ip => this.rhTxt.Info(this._socketContext, $"{ip}已启动");

                this._udpSocket.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = this._udpSocket.GetString(bytes).TrimWhiteSpace();

                    if (this.ResolveMsg.GetInvocationList().Length > 0)
                    {
                        message = this.ResolveMsg(message);
                    }

                    this.rhTxt.Recv(from, to, this._socketContext, message);

                    string[] arr = from.ToString()!.Split(":");
                    this._socketContext.TargetIP = arr[0];
                    this._socketContext.TargetPort = arr[1];
                };

                this._udpSocket.SentMessage += (from, to, bytes) =>
                {
                    this.rhTxt.Send(from, to, this._socketContext, this._udpSocket.GetString(bytes));
                };

                this._udpSocket.ExceptionOccurred += (remotePoint, exception) =>
                {
                    this.rhTxt.Info(this._socketContext, $"{remotePoint}出现异常:{exception.Message}");
                };

                this.Handler(this._socketContext.UnreachableDisconnect);

                this._socketContext.UnreachableDisconnectChanged -= Handler;
                this._socketContext.UnreachableDisconnectChanged += Handler;

                this._udpSocket.ConnectStatusChanged += connStatus => this._socketContext.IsConnected = connStatus;

                this._udpSocket.Start();
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._socketContext, ex.Message);
            }
        }

        private void Handler(bool value)
        {
            if (this._udpSocket is IUdpSocket udp)
            {
                udp.UnreachableDisconnect = value;
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Command == CustomCommands.PostCommand)
                {
                    var msg = this._socketContext.SendMsg;

                    this._udpSocket.SendAsync(msg);

                    this._socketContext.SendMsg = string.Empty;

                    e.Handled = true;
                }
                else if (e.Command == MediaCommands.Record)
                {
                    this._socketContext.IsLogging = !this._socketContext.IsLogging;

                    e.Handled = true;
                }
                else if (e.Command == ApplicationCommands.Open)
                {
                    if (e.Source == this.rhTxt || CustomConstants.LOG.EqualsIgnoreCase(e.Parameter?.ToString()))
                    {
                        Helper.Helper.OpenLog(this._socketContext.Name);

                        e.Handled = true;
                    }
                }
            }
            catch (SocketException ex)
            {
                this.rhTxt.Info(this._socketContext, ex.Message);
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._socketContext, ex.Message);
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            e.CanExecute = true;

            if (e.Command == CustomCommands.PostCommand)
            {
                if (this._socketContext != null)
                {
                    var forbid = string.IsNullOrEmpty(this._socketContext.SendMsg)
                        || this._socketContext.SendMsg.Trim().Length == 0
                        || !this._socketContext.IsConnected;

                    e.CanExecute = !forbid;
                }
            }
            else if (e.Command == NavigationCommands.Refresh)
            {
                e.CanExecute = this.rhTxt.Document.Blocks.Count > 0 || (this._socketContext != null && !string.IsNullOrEmpty(this._socketContext.SendMsg?.Trim()));
            }
        }
    }
}