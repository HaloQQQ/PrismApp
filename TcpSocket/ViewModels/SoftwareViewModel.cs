using Helper.ThirdPartyUtils;
using Helper.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TcpSocket.Helper;
using TcpSocket.Models;
using TcpSocket.MsgEvents;
using WpfStyleResources.Helper;
using WpfStyleResources.Helper.MyEvents;

namespace TcpSocket.ViewModels
{
    internal class SoftwareViewModel : BindableBase, IDisposable
    {
        public SoftwareViewModel(
            UserViewModel userContext,
            ImageDisplayViewModel imageDisplayViewModel,
            SettingsViewModel settings,
            IConfigManager config,
            IEventAggregator eventAggregator)
        {
            this.UserContext = userContext;
            this.Settings = settings;
            this._imageDisplayViewModel = imageDisplayViewModel;

            var bitmap = QRCodeUtil.GetColorfulQR("Hello 3Q", Color.GreenYellow, Color.White, 200);

            this.BitmapImage = bitmap;

            this.SwitchThemeCommand = new DelegateCommand(() => this.RefreshTheme());

            eventAggregator.GetEvent<BackgroundImageUpdateEvent>().Subscribe(uri => this.SetBackgroundImage(uri));

            eventAggregator.GetEvent<DialogMessageEvent>().Subscribe(item => this.DialogMessage = item);

            this.LoadConfig(config);

            this.SubscribeCustomCommandEvent();

            this.InitBackgroundSwitchTimer();
        }

        public ICommand SwitchThemeCommand { get; private set; }

        private void LoadConfig(IConfigManager config)
        {
            this.OnlyOneProcess = config.IsTrue(Constants.ONLY_ONE_PROCESS);
            this.AutoStart = config.IsTrue(Constants.AUTO_START);
            this.BackgroundSwitch = config.IsTrue(Constants.BACKGROUND_SWITCH);

            this.DefaultThemeURI = config.ReadConfigNode(Constants.DefaultThemeURI);
            this.LoadDefaultTheme();

            this.SetBackgroundImage(config.ReadConfigNode(Constants.BkgrdUri));
            this.IsMusicPlayer = config.IsTrue(Constants.IsMusicPlayer);
            this.IsVideoPlayer = config.IsTrue(Constants.IsVideoPlayer);

            config.SetConfig += config =>
            {
                config.WriteConfigNode<bool>(this.OnlyOneProcess, Constants.ONLY_ONE_PROCESS);
                config.WriteConfigNode<bool>(this.AutoStart, Constants.AUTO_START);
                config.WriteConfigNode<bool>(this.BackgroundSwitch, Constants.BACKGROUND_SWITCH);

                config.WriteConfigNode(this.DefaultThemeURI, Constants.DefaultThemeURI);
                config.WriteConfigNode(this.CurrentBkGrd, Constants.BkgrdUri);

                config.WriteConfigNode(this.IsMusicPlayer, Constants.IsMusicPlayer);
                config.WriteConfigNode(this.IsVideoPlayer, Constants.IsVideoPlayer);

                AppUtils.AutoStartWithDirectory(this.AutoStart);
            };
        }

        private void SetBackgroundImage(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                this.CurrentBkGrd = url;
            }

            if (!string.IsNullOrEmpty(this.CurrentBkGrd))
            {
                this.TrySelectedImage();
            }
        }

        internal void TrySelectedImage()
        {
            foreach (var item in this._imageDisplayViewModel.ActualData)
            {
                item.Selected = item.URI == this.CurrentBkGrd;
            }
        }

        private void LoadDefaultTheme()
        {
            var defaultThemeUri = this.DefaultThemeURI;
            if (string.IsNullOrEmpty(defaultThemeUri))
            {
                this.RefreshTheme();
            }
            else
            {
                var currentUri = new Uri(defaultThemeUri, UriKind.RelativeOrAbsolute);

                if (Helper.Helper.Equals(Constants.Dark.Source.ToString(), currentUri.ToString()))
                {
                    _resourceDictionaries.Add(Constants.Dark);
                }
                else
                {
                    _resourceDictionaries.Add(Constants.Light);
                }
            }
        }

        #region 主题&背景
        private Collection<ResourceDictionary> _resourceDictionaries => Application.Current.Resources.MergedDictionaries;

        public string DefaultThemeURI { get; set; } = null!;
        private void RefreshTheme()
        {
            var dict = _resourceDictionaries.FirstOrDefault(item => item == Constants.Light);

            if (dict == null)
            {
                _resourceDictionaries.Remove(Constants.Dark);
                _resourceDictionaries.Add(Constants.Light);

                dict = Constants.Light;
            }
            else
            {
                _resourceDictionaries.Remove(Constants.Light);
                _resourceDictionaries.Add(Constants.Dark);

                dict = Constants.Dark;
            }

            this.DefaultThemeURI = dict.Source.ToString();
        }

        private string _currentBkGrd;
        public string CurrentBkGrd
        {
            get => this._currentBkGrd;
            set => SetProperty<string>(ref _currentBkGrd, value);
        }
        #endregion

        #region 周边信息
        public string Version { get; } = Application.ResourceAssembly.GetName().Version.ToString();

        private string _title;
        public string Title { get => this._title; set => SetProperty<string>(ref this._title, value); }

        private string _currentTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

        public string CurrentTime
        {
            get => this._currentTime;
            set => SetProperty<string>(ref this._currentTime, value);
        }

        private string _week;

        public string Week
        {
            get => this._week;
            set => SetProperty<string>(ref this._week, value);
        }

        /// <summary>
        /// QRCode
        /// </summary>
        public Bitmap BitmapImage { get; set; }

        public UserViewModel UserContext { get; }
        #endregion

        private DialogMessage _dialogMessage;

        public DialogMessage DialogMessage
        {
            get => this._dialogMessage;
            set => SetProperty<DialogMessage>(ref _dialogMessage, value);
        }

        public SettingsViewModel Settings { get; }

        #region 辅助功能
        private bool _onlyOneProcess;

        public bool OnlyOneProcess
        {
            get => this._onlyOneProcess;
            set => SetProperty<bool>(ref _onlyOneProcess, value);
        }

        private bool _autoStart;

        public bool AutoStart
        {
            get => this._autoStart;
            set => SetProperty<bool>(ref _autoStart, value);
        }

        private bool _backgroundSwitch;

        public bool BackgroundSwitch
        {
            get => this._backgroundSwitch;
            set => SetProperty<bool>(ref _backgroundSwitch, value);
        }

        private bool _isMusicPlayer;

        public bool IsMusicPlayer
        {
            get => _isMusicPlayer;
            set => SetProperty<bool>(ref _isMusicPlayer, value);
        }

        private bool _isVideoPlayer;

        public bool IsVideoPlayer
        {
            get => this._isVideoPlayer;
            set => SetProperty<bool>(ref _isVideoPlayer, value);
        }

        private bool _isTitleBarHidden;

        public bool IsTitleBarHidden
        {
            get => _isTitleBarHidden;
            set => SetProperty<bool>(ref _isTitleBarHidden, value);
        }

        private bool _isLogin;

        public bool IsLogin
        {
            get => this._isLogin;
            set => SetProperty<bool>(ref _isLogin, value);
        }
        #endregion

        private Random random = new Random();
        private DispatcherTimer _timer = null;
        private readonly ImageDisplayViewModel _imageDisplayViewModel;

        private void InitBackgroundSwitchTimer()
        {
            this._timer = new DispatcherTimer();
            this._timer.Tick += (sender, e) =>
            {
                DateTime now = DateTime.Now;
                this.CurrentTime = now.FormatTime();
                this.Week = now.GetWeek();

                if (this.BackgroundSwitch)
                {
                    if (_imageDisplayViewModel.Data.Count > 0)
                    {
                        if (now.TimeOfDay.Seconds == 0 || now.TimeOfDay.Seconds == 30)
                        {
                            var totalCount = _imageDisplayViewModel.Data.Count;

                            this.SetBackgroundImage(_imageDisplayViewModel.Data[this.random.Next(0, totalCount)].URI);
                        }
                    }
                }

                if (this.DialogMessage != null && !this.DialogMessage.StopHide)
                {
                    if (--this.DialogMessage.Seconds <= 0)
                    {
                        this.DialogMessage.IsDisplayingDialogMessage = false;
                    }
                }
            };
            this._timer.Interval = TimeSpan.FromMilliseconds(1000);
            this._timer.Start();
        }

        private void SubscribeCustomCommandEvent()
        {
            MyEventManager.Default.GetEvent<OpenSettingEvent>().Execute += () => this.Settings.IsEditingSetting = true;
            MyEventManager.Default.GetEvent<HideTitleBarEvent>().Execute += () => this.IsTitleBarHidden = !this.IsTitleBarHidden;
            MyEventManager.Default.GetEvent<LoginEvent>().Execute += () => this.IsLogin = !this.IsLogin;
        }

        public void Dispose()
        {
            this._timer.Stop();
            this._timer = null;
        }
    }
}