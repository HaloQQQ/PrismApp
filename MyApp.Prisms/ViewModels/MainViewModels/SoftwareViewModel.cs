using Prism.Commands;
using Prism.Events;
using System;
using System.Drawing;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using MyApp.Prisms.MsgEvents;
using IceTea.Pure.Utils;
using IceTea.Pure.Extensions;
using System.Threading.Tasks;
using IceTea.Pure.Contracts;
using IceTea.Wpf.Atom.Contracts.MyEvents;
using System.Windows.Media.Imaging;
using IceTea.Windows.Extensions;
using PrismAppBasicLib.Contracts;
using IceTea.Wpf.Core.Utils;
using IceTea.Core.Businesses.QRCode;
using IceTea.Wpf.Atom.Businesses.HotKey.App;
using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Businesses.Event;
using IceTea.Windows.Businesses.QRCodes;
using IceTea.Windows.Businesses.Bright;
using IceTea.Windows.Utils;
using MyApp.Prisms.Contracts;
using IceTea.Wpf.Atom.Businesses.HotKey.Global;
using IceTea.Wpf.Core.Extensions;
using IceTea.Wpf.Atom.ViewModels;
using IceTea.Wpf.Atom.Contracts;

#pragma warning disable CS8603 // 可能返回 null 引用。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
namespace MyApp.Prisms.ViewModels
{
    internal class SoftwareViewModel : IceTeaViewModelBase, IDialogMessage, IDisposable
    {
        public SoftwareViewModel(
                UserViewModel userContext,
                ImageDisplayViewModel imageDisplayViewModel,
                SettingsViewModel settings,
                IConfigManager config,
                IAppConfigFileHotKeyManager appCfgHotkeyManager,
                IEventAggregator eventAggregator,
                GlobalHotKeyHandlerBase gloablHotKeyHandler
            ) : base(appCfgHotkeyManager)
        {
            this.UserContext = userContext.AssertNotNull(nameof(UserContext));
            this.Settings = settings.AssertNotNull(nameof(SettingsViewModel));
            this._imageDisplayViewModel = imageDisplayViewModel.AssertNotNull(nameof(ImageDisplayViewModel));

            this._gloablHotKeyHandler = gloablHotKeyHandler.AssertNotNull(nameof(GlobalHotKeyHandlerBase));

            this.InitQRCodeImage();

            this.SwitchThemeCommand = new DelegateCommand(this.RefreshTheme);

            CommonUtil.SubscribeMessage(eventAggregator, item => this.DialogMessage = item);

            eventAggregator.GetEvent<SwitchThemeEvent>().Subscribe(isLightTheme =>
            {
                if (this.FollowSystemTheme)
                {
                    this.RefreshTheme();
                }
            });

            this.LoadConfig(config);

            this.SubscribeCustomCommandEvent();

            this.InitScreenBright(eventAggregator);

            this.InitBackgroundSwitch(eventAggregator);
        }


        public ICommand SwitchThemeCommand { get; }

        protected virtual void LoadConfig(IConfigManager config)
        {
            this.OnlyOneProcess = config.IsTrue(CustomConstants.ONLY_ONE_PROCESS.FillToArray());
            this.AutoStart = config.IsTrue(CustomConstants.AUTO_START.FillToArray());
            this.BackgroundSwitch = config.IsTrue(CustomConstants.BACKGROUND_SWITCH.FillToArray());

            this.FollowSystemTheme = config.IsTrue(CustomConstants.FollowSystemThemes);

            this.DefaultThemeURI = config.ReadConfigNode<string>(CustomConstants.DefaultThemeURIs);
            this.LoadDefaultTheme();

            this.SetBackgroundImage(config.ReadConfigNode<string>(CustomConstants.BkgrdUri.FillToArray()));
            this.IsMusicPlayer = config.IsTrue(CustomConstants.IsMusicPlayer.FillToArray());
            this.IsVideoPlayer = config.IsTrue(CustomConstants.IsVideoPlayer.FillToArray());

            config.SetConfig += config =>
            {
                config.WriteConfigNode<bool>(this.OnlyOneProcess, CustomConstants.ONLY_ONE_PROCESS.FillToArray());
                config.WriteConfigNode<bool>(this.AutoStart, CustomConstants.AUTO_START.FillToArray());
                config.WriteConfigNode<bool>(this.BackgroundSwitch, CustomConstants.BACKGROUND_SWITCH.FillToArray());

                config.WriteConfigNode(this.FollowSystemTheme, CustomConstants.FollowSystemThemes);
                config.WriteConfigNode(this.DefaultThemeURI, CustomConstants.DefaultThemeURIs);

                config.WriteConfigNode(this.CurrentBkGrd, CustomConstants.BkgrdUri.FillToArray());

                config.WriteConfigNode(this.IsMusicPlayer, CustomConstants.IsMusicPlayer.FillToArray());
                config.WriteConfigNode(this.IsVideoPlayer, CustomConstants.IsVideoPlayer.FillToArray());

                AppUtils.AutoStartWithShortcut(this.AutoStart);
                //_ = AppDesktopUtils.AutoStartWithRegistryKeyAsync(this.AutoStart);
            };
        }

        private void SetBackgroundImage(string url)
        {
            if (!url.IsNullOrBlank())
            {
                this._imageDisplayViewModel.SelectImage(this.CurrentBkGrd = url);
            }
        }

        private void LoadDefaultTheme()
        {
            if (this.FollowSystemTheme || this.DefaultThemeURI.IsNullOrBlank())
            {
                this.RefreshTheme();
            }
            else
            {
                var currentUri = new Uri(this.DefaultThemeURI, UriKind.RelativeOrAbsolute);

                if (CustomConstants.Dark.Source.ToString().EqualsIgnoreCase(currentUri.ToString()))
                {
                    Application.Current.Resources.MergedDictionaries.Add(CustomConstants.Dark);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(CustomConstants.Light);
                }
            }
        }

        #region 主题&背景
        public string DefaultThemeURI { get; private set; }
        private void RefreshTheme()
        {
            var dict = WpfCoreUtils.RefreshTheme(this.FollowSystemTheme);

            this.DefaultThemeURI = dict.Source.ToString();
        }

        private string _currentBkGrd;
        public string CurrentBkGrd
        {
            get => this._currentBkGrd;
            private set => SetProperty<string>(ref _currentBkGrd, value);
        }
        #endregion

        #region 周边信息
        public string Version => AppStatics.AssemblyVersion?.ToString();

        private decimal _cpuRate;

        public decimal CpuRate
        {
            get => this._cpuRate;
            private set => SetProperty<decimal>(ref _cpuRate, value);
        }

        private decimal _ramRate;

        public decimal RamRate
        {
            get => this._ramRate;
            private set => SetProperty<decimal>(ref _ramRate, value);
        }

        private string _currentTime = DateTime.Now.FormatTime();

        public string CurrentTime
        {
            get => this._currentTime;
            private set => SetProperty<string>(ref this._currentTime, value);
        }

        private string _week;

        public string Week
        {
            get => this._week;
            private set => SetProperty<string>(ref this._week, value);
        }

        /// <summary>
        /// QRCode
        /// </summary>
        public BitmapImage ImageSource { get; private set; }
        private void InitQRCodeImage()
        {
            var data = new QRCoderCreator().GenerateQRCode(new QRModel("Hello3Q", (uint)Color.GreenYellow.ToArgb(), (uint)Color.White.ToArgb(), 20));

            this.ImageSource = ImageExtensions.ToBitmap(data).GetImageSource();
        }

        public UserViewModel UserContext { get; }
        #endregion

        #region 屏幕亮度
        private void InitScreenBright(IEventAggregator eventAggregator)
        {
            try
            {
                this._brightManager = new ScreenBrightManager();
                this.RefreshBrightness();
                eventAggregator.GetEvent<UpdateScreenBrightEvent>().Subscribe(step => this.CurrentBright += step);
            }
            catch
            {
                this._brightManager = null;
            }
        }

        internal void RefreshBrightness()
        {
            if (this._brightManager == null)
                return;
            try
            {
                this.CurrentBright = this._brightManager.GetBrightness();
            }
            catch { }
        }

        private ScreenBrightManager _brightManager;

        private double _currentBright;
        /// <summary>
        /// 当前屏幕亮度
        /// </summary>
        public double CurrentBright
        {
            get => this._currentBright;
            set
            {
                var newValue = Convert.ToInt32(value);
                if (newValue < 0)
                {
                    newValue = 0;
                }

                if (newValue > 100)
                {
                    newValue = 100;
                }

                if (newValue != Convert.ToInt32(_currentBright) && SetProperty<double>(ref _currentBright, value))
                {
                    this._brightManager?.SetBrightness(newValue);
                }
            }
        }
        #endregion

        public SettingsViewModel Settings { get; }

        #region 辅助功能
        private bool _followSystemTheme;
        public bool FollowSystemTheme
        {
            get => _followSystemTheme;
            set => SetProperty<bool>(ref _followSystemTheme, value);
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
        #endregion

        private DispatcherTimer _timer = null;
        private readonly ImageDisplayViewModel _imageDisplayViewModel;
        private GlobalHotKeyHandlerBase _gloablHotKeyHandler;

        private void InitBackgroundSwitch(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<BackgroundImageUpdateEvent>().Subscribe(this.SetBackgroundImage);

            this._timer = new DispatcherTimer();
            this._timer.Tick += (sender, e) =>
            {
                DateTime now = DateTime.Now;
                this.CurrentTime = now.FormatTime();
                this.Week = now.GetWeek();

                var seconds = now.Second;

                if (this.BackgroundSwitch)
                {
                    if (_imageDisplayViewModel.ImagesCount > 0)
                    {
                        if (seconds == 0 || seconds == 30)
                        {
                            this.SetBackgroundImage(_imageDisplayViewModel.GetRandomImage());
                        }
                    }
                }

                if (this.DialogMessage.IsNotNullAnd(m => m.IsEnable))
                {
                    this.DialogMessage.Decrease();
                }
            };
            this._timer.Interval = TimeSpan.FromSeconds(1);
            this._timer.Start();

            Task.Run(async () =>
            {
                while (this._timer.IsNotNullAnd(timer => timer.IsEnabled))
                {
                    try
                    {
                        this.CpuRate = AppDesktopUtils.GetCpuUsedRate();
                        this.RamRate = AppDesktopUtils.GetMemoryUsedRate();

                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        CommonUtil.Log(CustomConstants.LogType.Exception_Log_Dir, ex.Message);
                    }
                }
            });
        }

        private void SubscribeCustomCommandEvent()
        {
            CustomEventManager.Current.GetEvent<OpenSettingEvent>().Execute += () =>
            {
                bool isEditingSetting = this.Settings.IsEditingSetting;

                this.Settings.IsEditingSetting = !isEditingSetting;
            };
            CustomEventManager.Current.GetEvent<HideTitleBarEvent>().Execute += () => this.IsTitleBarHidden = !this.IsTitleBarHidden;
            CustomEventManager.Current.GetEvent<LoginEvent>().Execute += () => this.IsLogin = !this.IsLogin;
        }

        protected override void DisposeCore()
        {
            this._timer?.Stop();
            this._timer = null;

            _gloablHotKeyHandler.Dispose();
            _gloablHotKeyHandler = null;

            base.DisposeCore();
        }
    }
}