using System;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.Helper;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using MyApp.Prisms.ViewModels;
using System.ComponentModel;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.SocketStandard.Tcp.Contracts;
using IceTea.Wpf.Atom.Contracts;
using PrismAppBasicLib.Contracts;

namespace MyApp.Prisms.Views
{
#pragma warning disable CA1416 // 验证平台兼容性
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public partial class TcpServerView : UserControl
    {
        private TcpServerViewModel _tcpSocketViewModel;

        public TcpServerView()
        {
            InitializeComponent();

            this._tcpSocketViewModel = this.DataContext as TcpServerViewModel;

            _tcpSocketViewModel.PropertyChanged += OnPropertyChanged;

            this.rhTxt.Clear();
        }

        private ITcpServer _tcpServer;

        private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(SocketViewModelBase.Socket))
            {
                return;
            }

            _tcpServer = this._tcpSocketViewModel.Socket as ITcpServer;

            if (_tcpServer == null)
            {
                return;
            }

            try
            {
                _tcpServer.SocketCommunicateWithClientCreated += socket =>
                    rhTxt.Info(_tcpSocketViewModel.IsLogging, _tcpServer.Name, $"连接{socket.LocalEndPoint}=>{socket.RemoteEndPoint}建立!");

                _tcpServer.Started += socket =>
                    rhTxt.Info(_tcpSocketViewModel.IsLogging, _tcpServer.Name, $"开始监听{socket}..");

                _tcpServer.ReceivedMessage += (from, to, data) =>
                {
                    string message = _tcpServer.GetString(data).TrimWhiteSpace();

                    if (_tcpSocketViewModel.IsHex && message.IsHexString())
                    {
                        message = message.GetStringFromHex();
                    }

                    this.rhTxt.Recv(from, to, this._tcpSocketViewModel.IsLogging, _tcpServer, message);
                };

                _tcpServer.ReceivedMessage += (from, to, data) => _mediatorContext.TransmitFrom(this, data);

                _tcpServer.ExceptionOccurred += (socketName, exception) =>
                    this.rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpServer.Name, exception.Message);

                _tcpServer.SentMessage += (from, to, bytes) =>
                {
                    this.rhTxt.Send(from, to, this._tcpSocketViewModel.IsLogging, _tcpServer, _tcpServer.GetString(bytes));
                    this._tcpSocketViewModel.SendMessage = string.Empty;
                };
            }
            catch (Exception ex)
            {
                this.rhTxt.Info(this._tcpSocketViewModel.IsLogging, _tcpServer.Name, ex.Message);
            }
        }

        private CommunicationViewModel _mediatorContext;

        internal void SetMediator(CommunicationViewModel mediatorContext)
        {
            _mediatorContext = mediatorContext;
        }

        public void TransmitMessage(byte[] data)
        {
            try
            {
                AppUtils.Assert(_tcpServer != null, "Socket未初始化!");

                if (!_tcpServer.IsConnected)
                {
                    return;
                }

                AppUtils.Assert(_tcpServer.CurrentCount > 0,
                    $"【{_tcpServer.Name}】不存在客户端连接!请连接后重试!");

                var msg = _tcpServer.GetString(data);
                _tcpServer.SendAsync(msg);

                rhTxt.Info(_tcpSocketViewModel.IsLogging, _tcpServer.Name, $"转发数据：【{msg}】");
            }
            catch (Exception ex)
            {
                rhTxt.Info(_tcpSocketViewModel.IsLogging, this._tcpSocketViewModel.Name, ex.Message);
            }
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            try
            {
                if (e.Command == System.Windows.Documents.EditingCommands.EnterParagraphBreak) //回车发送
                {
                    var msg = this._tcpSocketViewModel.SendMessage;

                    this._tcpServer.SendAsync(msg);

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

            if (e.Command == CustomCommands.TextBoxEnterCommand) // 发送消息
            {
                if (this._tcpSocketViewModel != null)
                {
                    var forbid = this._tcpSocketViewModel.SendMessage.IsNullOr(msg => msg.IsNullOrBlank())
                                 || this._tcpSocketViewModel.ConnList.Count == 0
                                 || !this._tcpServer.IsConnected;
                    e.CanExecute = !forbid;
                }
            }
        }
    }
}