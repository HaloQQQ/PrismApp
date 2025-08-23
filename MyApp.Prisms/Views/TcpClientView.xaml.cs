using System;
using System.Net;
using System.Threading;
using System.Windows.Controls;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using System.ComponentModel;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.SocketStandard.Tcp.Contracts;
using PrismAppBasicLib.Contracts;
using IceTea.Pure.Extensions;

namespace MyApp.Prisms.Views
{
#pragma warning disable CA1416 // 验证平台兼容性
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable IDE0044 // 添加只读修饰符
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
            if (e.PropertyName != nameof(SocketViewModelBase.Socket))
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

                _tcpClient.Closed += () => this.rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpClient.Name, "连接已关闭..");

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
    }
}