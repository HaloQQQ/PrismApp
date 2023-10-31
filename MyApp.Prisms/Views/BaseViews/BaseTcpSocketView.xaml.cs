using Prism.Mvvm;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.SocketStandard.Base;
using IceTea.SocketStandard.Tcp;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.Wpf.Core.Contracts;

namespace MyApp.Prisms.Views.BaseViews
{
    public abstract partial class BaseTcpSocketView : UserControl
    {
        protected BaseSocketViewModel _tcpSocketContext;

        protected BaseTcpSocketView()
        {
            InitializeComponent();

            ViewModelLocator.SetAutoWireViewModel(this, true);

            this._tcpSocketContext = this.DataContext as BaseSocketViewModel;

            this.rhTxt.Clear();
        }

        protected ISocket _tcpSocket = null!;

        protected string GetMessage(EndPoint from, EndPoint to, string coreMessage) =>
            $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsThreadPoolThread} {from}=>{to} {coreMessage}";

        protected virtual void CloseSocket()
        {
            this._tcpSocket?.Close();
        }

        /// <summary>
        /// 初始化Socket连接对象
        /// </summary>
        /// <param name="port"></param>
        protected abstract void InitTcpSocket(ushort port);

        public Func<string, string> ResolveMsg = null!;

        private void BtnConnect_Click(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            try
            {
                if (this._tcpSocketContext.Connecting)
                {
                    return;
                }

                if (this._tcpSocket != null && this._tcpSocket.IsConnected)
                {
                    this.CloseSocket();
                    this._tcpSocketContext.Connecting = false;
                    return;
                }

                this.CloseSocket();

                this._tcpSocketContext.Connecting = true;

                ushort port = ushort.Parse(this._tcpSocketContext.Port);

                this.InitTcpSocket(port);

                this._tcpSocket.AssertArgumentNotNull("Socket连接未初始化..");

                this._tcpSocket!.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = this._tcpSocket.GetString(bytes).TrimWhiteSpace();

                    if (this.ResolveMsg.GetInvocationList().Length > 0)
                    {
                        message = this.ResolveMsg(message);
                    }

                    //message = GetMessage(from, to, $"收到数据【{message}】!");

                    this.rhTxt.Recv(from, to, this._tcpSocketContext, message);
                };

                this._tcpSocket.SentMessage += (from, to, bytes) =>
                {
                    this.rhTxt.Send(from, to, this._tcpSocketContext, this._tcpSocket.GetString(bytes));
                };

                this._tcpSocket.ExceptionOccurred += (socketName, exception) =>
                {
                    this.rhTxt.Info(this._tcpSocketContext, exception.Message);
                };

                this._tcpSocket.Start();
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._tcpSocketContext, ex.Message);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Command == ApplicationCommands.Close)
                {
                    if (e.OriginalSource is Button item)
                    {
                        if (this._tcpSocket is NewTcpServer server)
                        {
                            server.DestoryClientHandler(item.DataContext.ToString());

                            e.Handled = true;
                        }
                    }
                }
                else if (e.Command == CustomCommands.PostCommand)
                {
                    var msg = this._tcpSocketContext.SendMsg;

                    this._tcpSocket.SendAsync(msg);

                    this._tcpSocketContext.SendMsg = string.Empty;

                    e.Handled = true;
                }
                else if (e.Command == ApplicationCommands.Open)
                {
                    if (e.Source == this.rhTxt || CustomConstants.LOG.EqualsIgnoreCase(e.Parameter?.ToString()))
                    {
                        Helper.Helper.OpenLog(this._tcpSocketContext.Name);
                    }

                    e.Handled = true;
                }
            }
            catch (SocketException ex)
            {
                this.rhTxt.Info(this._tcpSocketContext, ex.Message);
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._tcpSocketContext, ex.Message);
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            e.CanExecute = true;

            if (e.Command == ApplicationCommands.Close)
            {
                e.CanExecute = e.OriginalSource is Button;
            }
            else if (e.Command == CustomCommands.PostCommand) // 发送消息
            {
                if (this._tcpSocketContext != null)
                {
                    var forbid = string.IsNullOrEmpty(this._tcpSocketContext.SendMsg?.Trim())
                                 || this._tcpSocketContext.ConnList.Count == 0
                                 || !this._tcpSocketContext.IsConnected;
                    e.CanExecute = !forbid;
                }
            }
            else if (e.Command == NavigationCommands.Refresh)
            {
                e.CanExecute = this.rhTxt.Document.Blocks.Count > 0 || (this._tcpSocketContext != null && !string.IsNullOrEmpty(this._tcpSocketContext.SendMsg?.Trim()));
            }
        }
    }
}