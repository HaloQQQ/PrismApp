using Helper.ThirdPartyUtils;
using Helper.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using TcpSocket.Helper;
using TcpSocket.MsgEvents;

namespace TcpSocket.ViewModels
{
    public class SoftwareViewModel : BindableBase, IDisposable
    {
        public SoftwareViewModel(
            UserViewModel userContext,
            ImageDisplayViewModel imageDisplayViewModel,
            WpfStyleResources.Interfaces.IConfigManager config,
            IEventAggregator eventAggregator)
        {
            this.UserContext = userContext;
            this._imageDisplayViewModel = imageDisplayViewModel;
            this.OnlyOneProcess = config.IsTrue(Constants.ONLY_ONE_PROCESS);
            this.AutoStart = config.IsTrue(Constants.AUTO_START);
            this.BackgroundSwitch = config.IsTrue(Constants.BACKGROUND_SWITCH);

            this.DefaultThemeURI = config.ReadConfigNode(Constants.DefaultThemeURI);
            this.LoadDefaultTheme();

            this.CurrentBkGrd = config.ReadConfigNode(Constants.BkgrdUri);
            this.IsMusicPlayer = config.IsTrue(Constants.IsMusicPlayer);
            this.IsVideoPlayer = config.IsTrue(Constants.IsVideoPlayer);

            var bitmap = QRCodeUtil.GetColorfulQR("Hello 3Q", Color.GreenYellow, Color.White, 200);

            this.BitmapImage = bitmap;

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

            this.SwitchThemeCommand = new DelegateCommand(() => this.RefreshTheme());

            eventAggregator.GetEvent<BackgroundImageUpdateEvent>().Subscribe(uri => this.CurrentBkGrd = uri);

            eventAggregator.GetEvent<FullScreenEvent>().Subscribe(() => this.IsTitleBarHidden = !this.IsTitleBarHidden);

            this.InitBackgroundSwitchTimer();
        }

        public ICommand SwitchThemeCommand { get; private set; }

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

        private Collection<ResourceDictionary> _resourceDictionaries => Application.Current.Resources.MergedDictionaries;

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

        public string Version { get; } = Application.ResourceAssembly.GetName().Version.ToString();
        public string DefaultThemeURI { get; set; } = null!;

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


        private bool _isTitleBarHidden;

        public bool IsTitleBarHidden
        {
            get => _isTitleBarHidden;
            set => SetProperty<bool>(ref _isTitleBarHidden, value);
        }

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

        private string _currentBkGrd;
        public string CurrentBkGrd
        {
            get => this._currentBkGrd;
            set => SetProperty<string>(ref _currentBkGrd, value);
        }

        /// <summary>
        /// QRCode
        /// </summary>
        public Bitmap BitmapImage { get; set; }

        public UserViewModel UserContext { get; }


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
                    if (_imageDisplayViewModel.Block.Count > 0)
                    {
                        if (now.TimeOfDay.Seconds == 0 || now.TimeOfDay.Seconds == 30)
                        {
                            var totalCount = _imageDisplayViewModel.Block.Count;

                            this.CurrentBkGrd = _imageDisplayViewModel.Block[this.random.Next(0, totalCount)].URI;
                        }
                    }
                }
            };
            this._timer.Interval = TimeSpan.FromMilliseconds(1000);
            this._timer.Start();
        }

        public void Dispose()
        {
            this._timer.Stop();
            this._timer = null;
        }
    }
}