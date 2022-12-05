using SocketHelper.Base;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Helper.Utils;
using TcpSocket.Helper;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Function.Communication
{
    public abstract partial class UsrCtrlTcpSocket : UserControl
    {
        protected TcpSocketContext _tcpSocketContext;

        public UsrCtrlTcpSocket(TcpSocketContext tcpSocketContext)
        {
            InitializeComponent();
            this._tcpSocketContext = tcpSocketContext;
            this.DataContext = tcpSocketContext;
        }

        protected ISocket _tcpSocket = null!;

        protected string GetMessage(EndPoint from, EndPoint to, string coreMessage)
        {
            return
                $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsThreadPoolThread} {from}=>{to} " +
                coreMessage;
        }

        protected string GetMessage(string socketName, string coreMessage)
        {
            return
                $"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss")} {Thread.CurrentThread.ManagedThreadId} {Thread.CurrentThread.IsThreadPoolThread} {socketName} " +
                coreMessage;
        }

        protected void AppendMsg(string msg)
        {
            if (this._tcpSocketContext.IsLogging)
            {
                Helper.Helper.Log(this._tcpSocketContext.Name, msg);
            }

            this.Dispatcher.Invoke(() =>
                {
                    this.rhTxt.AppendText(msg + Environment.NewLine);
                    this.rhTxt.ScrollToEnd();

                    if (this.rhTxt.LineCount > 500)
                    {
                        this.rhTxt.Clear();
                    }
                }
            );
        }

        private void RefreshConnection()
        {
            this._tcpSocketContext.IsConnected = this._tcpSocket.IsConnected;
        }

        private void CloseSocket()
        {
            this._tcpSocket.Close();

            this._tcpSocketContext.ConnList.Clear();

            this.RefreshConnection();
        }

        /// <summary>
        /// 初始化Socket连接对象
        /// </summary>
        /// <param name="port"></param>
        protected abstract void InitTcpSocket(ushort port);

        public Func<string, string> ResolveMsg = null!;

        protected void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            try
            {
                if (this._tcpSocket != null && this._tcpSocket.IsConnected)
                {
                    this.CloseSocket();

                    return;
                }

                ushort port = ushort.Parse(this._tcpSocketContext.Port);

                this.InitTcpSocket(port);

                AppUtils.Assert(this._tcpSocket != null, "Socket连接未初始化..");

                this._tcpSocket!.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = this._tcpSocket.GetString(bytes).Trim('\0').Trim();

                    if (this.ResolveMsg.GetInvocationList().Length > 0)
                    {
                        message = this.ResolveMsg(message);
                    }

                    message = GetMessage(from, to, $"收到数据【{message}】!");

                    this.AppendMsg(message);
                };

                this._tcpSocket.ExceptionOccurred += (socketName, exception) =>
                {
                    this.AppendMsg(this.GetMessage(socketName, $" {exception.Message}"));

                    this.Dispatcher.Invoke(() =>
                        {
                            if (this._tcpSocketContext.ConnList.Contains(socketName))
                            {
                                this._tcpSocketContext.ConnList.Remove(socketName);
                            }
                        }
                    );

                    this.RefreshConnection();
                };

                this._tcpSocket.Start();

                this.RefreshConnection();
            }
            catch (Exception ex)
            {
                this.AppendMsg(ex.Message);
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
                        if (!string.IsNullOrEmpty(this._tcpSocketContext.SendMsg))
                        {
                            this._tcpSocket.SendAsync(this._tcpSocketContext.SendMsg);

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

                        Helper.Helper.ShowBalloonTip("日志记录功能", $"【{this._tcpSocketContext.Name}】日志功能" + (this._tcpSocketContext.IsLogging ? "开启" : "关闭"));
                    }
                }
                else if (e.Command == NavigationCommands.Refresh)
                {
                    this.rhTxt.Clear();
                }
                else if (e.Command == ApplicationCommands.Open)
                {
                    if (e.Source is TextBox || Helper.Helper.Equals(e.Parameter?.ToString(), Constants.LOG))
                    {
                        Helper.Helper.OpenLog(this._tcpSocketContext.Name);
                    }
                }
            }
            catch (SocketException ex)
            {
                this.RefreshConnection();

                this.AppendMsg(ex.Message);
            }
            catch (Exception ex)
            {
                this.AppendMsg(ex.Message);
            }
        }

        private void CommandBinding_OnCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.Handled = true;

            if (e.Command == ApplicationCommands.New)
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
                if (string.IsNullOrEmpty(this.rhTxt.Text))
                {
                    e.CanExecute = false;
                    return;
                }
            }

            e.CanExecute = true;
        }
    }
}