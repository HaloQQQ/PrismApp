using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Helper.Utils;
using SocketHelper.Base;
using SocketHelper.Udp;
using TcpSocket.Helper;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Function.Communication
{
    public partial class UsrCtrlUDP : UserControl
    {
        protected UdpSocketContext _socketContext;

        public UsrCtrlUDP(UdpSocketContext socketContext)
        {
            InitializeComponent();
            this._socketContext = socketContext;
            this.DataContext = socketContext;
        }

        protected ISocket _udpSocket = null!;

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
            if (this._socketContext.IsLogging)
            {
                Helper.Helper.Log(this._socketContext.Name, msg);
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
            this._socketContext.IsConnected = this._udpSocket.IsConnected;
        }

        private void CloseSocket()
        {
            this._udpSocket.Close();

            this.RefreshConnection();
        }

        public Func<string, string> ResolveMsg = null!;

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            try
            {
                if (this._udpSocket != null && this._udpSocket.IsConnected)
                {
                    this.CloseSocket();

                    return;
                }

                this._udpSocket = new NewUdpClient(this._socketContext.Encoding,
                    this._socketContext.IP, ushort.Parse(this._socketContext.Port),
                    this._socketContext.TargetIP, ushort.Parse(this._socketContext.TargetPort));

                AppUtils.Assert(this._udpSocket != null, "Socket连接未初始化..");

                this._udpSocket!.Started += ip => this.AppendMsg($"{ip}已启动");

                this._udpSocket.ReceivedMessage += (from, to, bytes) =>
                {
                    string message = this._udpSocket.GetString(bytes).Trim('\0').Trim();

                    if (this.ResolveMsg.GetInvocationList().Length > 0)
                    {
                        message = this.ResolveMsg(message);
                    }

                    message = GetMessage(from, to, message);

                    this.AppendMsg(message);

                    string[] arr = @from.ToString()!.Split(":");
                    this._socketContext.TargetIP = arr[0];
                    this._socketContext.TargetPort = arr[1];

                    ((this._udpSocket as NewUdpClient)!).ReConnect(from);
                };

                this._udpSocket.ExceptionOccurred += (remotePoint, exception) =>
                {
                    this.AppendMsg($"{remotePoint}出现异常:{exception.Message}");

                    this.RefreshConnection();
                };

                this._udpSocket.Start();

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
                        if (!string.IsNullOrEmpty(this._socketContext.SendMsg))
                        {
                            this._udpSocket.SendAsync(this._socketContext.SendMsg);

                            this._socketContext.SendMsg = string.Empty;
                        }
                        else
                        {
                            Helper.Helper.ShowBalloonTip("数据发送结果", "发送数据为空!");
                        }
                    }
                    else if (e.Source == this.rhTxt || Helper.Helper.Equals(e.Parameter?.ToString(), Constants.LOG))
                    {
                        this._socketContext.IsLogging = !this._socketContext.IsLogging;

                        Helper.Helper.ShowBalloonTip("日志记录功能", $"【{this._socketContext.Name}】日志功能" + (this._socketContext.IsLogging ? "开启" : "关闭"));
                    }
                }
                else if (e.Command == NavigationCommands.Refresh)
                {
                    this.rhTxt.Clear();
                }
                else if (e.Command == ApplicationCommands.Open)
                {
                    if (e.Source == this.rhTxt || Helper.Helper.Equals(e.Parameter?.ToString(), Constants.LOG))
                    {
                        Helper.Helper.OpenLog(this._socketContext.Name);
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
                    if (string.IsNullOrEmpty(this._socketContext.SendMsg)
                        || this._socketContext.SendMsg.Trim().Length == 0)
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