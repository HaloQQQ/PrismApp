using System.Collections.ObjectModel;
using System.Windows;
using TcpSocket.Models;

namespace TcpSocket.Helper
{
    internal class Statics
    {
        internal static readonly ResourceDictionary Light = Helper.GetDict("../Resources/LightTheme.xaml");

        internal static readonly ResourceDictionary Dark = Helper.GetDict("../Resources/DarkTheme.xaml");

        internal static readonly MediatorContext DataContext = new MediatorContext()
        {
            IsTcp = true,
            IsTcpServer = true,
            MachineServerContext = new TcpSocketContext()
            {
                IP = "127.0.0.1",
                Port = "50000",
                ConnList = new ObservableCollection<string>(),
                MaxMessageLength = 256,
                Name = "设备服务器"
            },
            ApplicationServerContext = new TcpSocketContext()
            {
                IP = "127.0.0.1",
                Port = "50001",
                ConnList = new ObservableCollection<string>(),
                MaxMessageLength = 256,
                Name = "程序服务器"
            },
            ApplicationClientContext = new TcpSocketContext()
            {
                IP = "127.0.0.1",
                Port = "50001",
                ConnList = new ObservableCollection<string>(),
                MaxMessageLength = 256,
                Name = "用户客户端"
            },
            UdpContext = new UdpSocketContext()
            {
                IP = "127.0.0.1",
                Port = "50000",
                Name = "UDP通讯",
                TargetIP = "127.0.0.1",
                TargetPort = "50001",
            }
        }.Initialized();
    }
}