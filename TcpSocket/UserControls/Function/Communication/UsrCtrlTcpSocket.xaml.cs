using SocketHelper.Base;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Input;
using Helper.Extensions;
using Helper.Utils;
using TcpSocket.Helper;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Function.Communication
{
    public abstract partial class UsrCtrlTcpSocket : UserControl
    {
        protected TcpSocketContext _tcpSocketContext;

        protected UsrCtrlTcpSocket(TcpSocketContext tcpSocketContext)
        {
            InitializeComponent();
            this._tcpSocketContext = tcpSocketContext;
            this.DataContext = tcpSocketContext;
            this.rhTxt.Clear();
        }

        protected ISocket _tcpSocket = null!;

        protected string GetMessage(EndPoint from, EndPoint to, string coreMessage) =>
            $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsThreadPoolThread} {from}=>{to} {coreMessage}";

        private void CloseSocket()
        {
            this._tcpSocket?.Close();

            this._tcpSocketContext.ConnList.Clear();
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

                AppUtils.Assert(this._tcpSocket != null, "Socket连接未初始化..");

                this._tcpSocket!.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = this._tcpSocket.GetString(bytes).TrimWhiteSpace();

                    if (this.ResolveMsg.GetInvocationList().Length > 0)
                    {
                        message = this.ResolveMsg(message);
                    }

                    message = GetMessage(from, to, $"收到数据【{message}】!");

                    this.rhTxt.Recv(from, to, this._tcpSocketContext, message);
                };

                this._tcpSocket.SentMessage += (from, to, bytes) =>
                {
                    this.rhTxt.Send(from, to, this._tcpSocketContext, this._tcpSocket.GetString(bytes));
                };

                this._tcpSocket.ExceptionOccurred += (socketName, exception) =>
                {
                    this.rhTxt.Info(this._tcpSocketContext, exception.Message);

                    Helper.Helper.Invoke(() =>
                        {
                            if (!this._tcpSocketContext.CanReConnect)
                            {
                                this._tcpSocketContext.Connecting = false;
                            }

                            if (this._tcpSocketContext.ConnList.Contains(socketName))
                            {
                                this._tcpSocketContext.ConnList.Remove(socketName);
                            }
                        }
                    );
                };

                this._tcpSocket.ConnectStatusChanged += status =>
                {
                    if ((this._tcpSocketContext.IsConnected = status) && !this._tcpSocketContext.CanReConnect)
                    {
                        this._tcpSocketContext.Connecting = false;
                    }
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
            e.Handled = true;

            try
            {
                if (e.Command == ApplicationCommands.New)
                {
                    if (Helper.Helper.Equals(e.Parameter?.ToString(), Constants.SEND_MSG))
                    {
                        var msg = this._tcpSocketContext.SendMsg;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            this._tcpSocket.SendAsync(msg);

                            this._tcpSocketContext.SendMsg = string.Empty;
                        }
                        else
                        {
                            Helper.Helper.ShowBalloonTip("数据发送结果", "发送数据为空!");
                        }
                    }
                    else if (e.Source is TextBox || Helper.Helper.Equals(e.Parameter?.ToString(), Constants.LOG))
                    {
                        this._tcpSocketContext.IsLogging = !this._tcpSocketContext.IsLogging;

                        Helper.Helper.ShowBalloonTip("日志记录功能",
                            $"【{this._tcpSocketContext.Name}】日志功能" + (this._tcpSocketContext.IsLogging ? "开启" : "关闭"));
                    }
                }
                else if (e.Command == NavigationCommands.Refresh)
                {
                    this.rhTxt.Document.Blocks.Clear();
                }
                else if (e.Command == ApplicationCommands.Open)
                {
                    if (e.Source == this.rhTxt || Constants.LOG.EqualsIgnoreCase(e.Parameter?.ToString()))
                    {
                        Helper.Helper.OpenLog(this._tcpSocketContext.Name);
                    }
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

            if (e.Command == ApplicationCommands.Open)
            {
                if ("ConnButton".EqualsIgnoreCase(e.Parameter.ToString()))
                {
                    e.CanExecute = !(this._tcpSocketContext.CanReConnect && this._tcpSocketContext.Connecting);
                    return;
                }
            }
            else if (e.Command == ApplicationCommands.New)
            {
                if (Helper.Helper.Equals(e.Parameter?.ToString(), Constants.SEND_MSG))
                {
                    if (string.IsNullOrEmpty(this._tcpSocketContext.SendMsg)
                        || this._tcpSocketContext.SendMsg.Trim().Length == 0
                        || this._tcpSocketContext.ConnList?.Count == 0)
                    {
                        e.CanExecute = false;
                        return;
                    }
                }
            }
            else if (e.Command == NavigationCommands.Refresh)
            {
                e.CanExecute = this.rhTxt.Document.Blocks.Count > 0;
                return;
            }

            e.CanExecute = true;
        }
    }
}