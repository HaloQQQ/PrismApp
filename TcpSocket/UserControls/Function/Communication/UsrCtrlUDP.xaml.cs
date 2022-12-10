using System;
using System.Net.Sockets;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Helper.Extensions;
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
            
            this.rhTxt.Clear();
        }

        protected ISocket _udpSocket = null!;

        public Func<string, string> ResolveMsg = null!;

        private void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            e.Handled = true;

            if (this._udpSocket != null && this._udpSocket.IsConnected)
            {
                this._udpSocket.Close();
                return;
            }

            try
            {
                this._udpSocket = new NewUdpClient(this._socketContext.Encoding,
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

                this._udpSocket.ConnectStatusChanged += connStatus => this._socketContext.IsConnected = connStatus;

                this._udpSocket.Start();
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._socketContext, ex.Message);
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
                        var msg = this._socketContext.SendMsg;
                        if (!string.IsNullOrEmpty(msg))
                        {
                            this._udpSocket.SendAsync(msg);

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
                    this.rhTxt.Document.Blocks.Clear();
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
                e.CanExecute = this.rhTxt.Document.Blocks.Count > 0;
                return;
            }

            e.CanExecute = true;
        }
    }
}