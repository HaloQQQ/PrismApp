using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Helper.AbstractModel;
using Helper.ThirdPartyUtils;
using Helper.Utils;
using TcpSocket.Helper;
using TcpSocket.Models.ProcessService;
using TcpSocket.UserControls.Function.Communication;

namespace TcpSocket.Models
{
    public class MediatorContext : BaseNotifyModel
    {
        public virtual MediatorContext Initialized()
        {
            ConfigurationHelper config = ConfigurationHelper.Instance;

            this.SoftwareContext = new SoftwareContext();
            this.ImagesContext = ImagesContext.CreateInstance();
            this.ProcessServiceContext = new ProcessServiceContext();
            
            this.SoftwareContext.DefaultThemeURI = config.GetDefaultThemeURI();

            Task.Run(() =>
            {
                this.SoftwareContext.OnlyOneProcess = config.IsOnlyOne();
                this.SoftwareContext.AutoStart = config.IsAutoStart();
                this.SoftwareContext.BackgroundSwitch = config.IsBackgroundSwitch();

                this.MachineServerContext.IsLogging = config.IsLogging(this.MachineServerContext.Name);
                this.ApplicationServerContext.IsLogging = config.IsLogging(this.ApplicationServerContext.Name);
                this.ApplicationClientContext.IsLogging = config.IsLogging(this.ApplicationClientContext.Name);
                this.UdpContext.IsLogging = config.IsLogging(this.UdpContext.Name);

                var bitmap = QRCodeUtil.GetColorfulQR("Hello 3Q", Color.GreenYellow, Color.White, 200);

                this.SoftwareContext.BitmapImage = bitmap;
            });

            App.Current.Exit += (sender, e) =>
            {
                var jsonObject = config.GetJObject();

                config.SetOnlyOne(jsonObject, this.SoftwareContext.OnlyOneProcess);
                config.SetAutoStart(jsonObject, this.SoftwareContext.AutoStart);
                config.SetBackgroundSwitch(jsonObject, this.SoftwareContext.BackgroundSwitch);

                config.SetLogging(jsonObject, this.MachineServerContext.Name, this.MachineServerContext.IsLogging);
                config.SetLogging(jsonObject, this.ApplicationServerContext.Name, this.ApplicationServerContext.IsLogging);
                config.SetLogging(jsonObject, this.ApplicationClientContext.Name, this.ApplicationClientContext.IsLogging);

                config.SetLogging(jsonObject, this.UdpContext.Name, this.UdpContext.IsLogging);

                config.SetDefaultThemeURI(jsonObject, this.SoftwareContext.DefaultThemeURI);

                config.SetImageURI(jsonObject, this.ImagesContext.ImageDir);

                config.WriteJsonToFile(jsonObject);


                AppUtils.SetAutoStart(this.SoftwareContext.AutoStart);
            };

            return this;
        }

        private bool isTcp;

        public bool IsTcp
        {
            get => this.isTcp;
            set
            {
                this.isTcp = value;
                CallModel();
            }
        }

        private bool isServer;

        public bool IsTcpServer
        {
            get => this.isServer;
            set
            {
                this.isServer = value;
                CallModel();
            }
        }

        private bool isConnected;

        public bool IsConnected
        {
            get => this.isConnected;
            set
            {
                this.isConnected = value;
                CallModel();
            }
        }

        private bool isHex;

        public bool IsHex
        {
            get => this.isHex;
            set
            {
                this.isHex = value;
                CallModel();
            }
        }

        public ushort MaxClientCount
        {
            set
            {
                this.ApplicationServerContext.MaxClientCount = value;
                this.MachineServerContext.MaxClientCount = value;
            }
        }

        /// <summary>
        /// 软件信息
        /// </summary>
        public SoftwareContext SoftwareContext { get; private set; }

        /// <summary>
        /// 背景切换列表
        /// </summary>
        public ImagesContext ImagesContext { get; private set; }

        /// <summary>
        /// 进程和服务列表
        /// </summary>
        public ProcessServiceContext ProcessServiceContext { get; private set; }

        #region UDP

        public UdpSocketContext UdpContext { get; set; } = null!;

        #endregion

        #region TCP

        public TcpSocketContext MachineServerContext { get; set; } = null!;
        public TcpSocketContext ApplicationServerContext { get; set; } = null!;
        public TcpSocketContext ApplicationClientContext { get; set; } = null!;

        public ISet<UsrCtrlTcpServer> Servers = new HashSet<UsrCtrlTcpServer>();

        public void AddServer(UsrCtrlTcpServer server)
        {
            this.Servers.Add(server);

            server.SetMediator(this);
        }

        /// <summary>
        /// 服务器之间为客户端转发消息
        /// </summary>
        /// <param name="server"></param>
        /// <param name="data"></param>
        public void TransmitFrom(UsrCtrlTcpServer server, byte[] data)
        {
            foreach (var item in this.Servers.Where(i => i != server))
            {
                item.TransmitMessage(data);
            }
        }

        #endregion
    }
}