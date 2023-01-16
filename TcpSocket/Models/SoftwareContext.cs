using System;
using System.Drawing;
using System.Windows;
using Helper.AbstractModel;

namespace TcpSocket.Models
{
    public class SoftwareContext : BaseNotifyModel
    {
        public string Version { get; } = Application.ResourceAssembly.GetName().Version.ToString();
        public string DefaultThemeURI { get; set; } = null!;

        private string currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string CurrentTime
        {
            get => this.currentTime;
            set
            {
                this.currentTime = value;
                CallModel();
            }
        }

        private bool onlyOneProcess;

        public bool OnlyOneProcess
        {
            get => this.onlyOneProcess;
            set
            {
                this.onlyOneProcess = value;
                CallModel();
            }
        }

        private bool autoStart;

        public bool AutoStart
        {
            get => this.autoStart;
            set
            {
                this.autoStart = value;
                CallModel();
            }
        }

        private bool _backgroundSwitch = true;

        public bool BackgroundSwitch
        {
            get => this._backgroundSwitch;
            set
            {
                this._backgroundSwitch = value;
                CallModel();
            }
        }

        public string CurrentBkGrd { get; set; }

        /// <summary>
        /// QRCode
        /// </summary>
        public Bitmap BitmapImage { get; set; }

        public PopupResult PopupResult = new PopupResult();

        public UserContext UserContext { get; } = new UserContext();
    }
}