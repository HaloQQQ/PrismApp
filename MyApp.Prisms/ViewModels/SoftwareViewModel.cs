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
using MyApp.Prisms.Helper;
using MyApp.Prisms.MsgEvents;
using System.Reflection;
using IceTea.Atom.Utils;
using IceTea.Atom.Extensions;
using IceTea.Core.Utils.OS;
using IceTea.Core.Utils.QRCodes;
using System.Threading.Tasks;
using System.Collections.Generic;
using IceTea.Wpf.Core.Contracts.MyEvents;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils.Events;
using PrismAppBasicLib.MsgEvents;
using IceTea.Atom.Utils.HotKey.Contracts;
using IceTea.Wpf.Core.Utils.HotKey.App.Contracts;
using IceTea.General.Utils;

namespace MyApp.Prisms.ViewModels
{
    internal class SoftwareViewModel : BindableBase, IDisposable
    {
        public SoftwareViewModel(
                UserViewModel userContext,
                ImageDisplayViewModel imageDisplayViewModel,
                SettingsViewModel settings,
                IConfigManager config,
                IAppConfigFileHotKeyManager appConfigFileHotKeyManager,
                IEventAggregator eventAggregator
            )
        {
            this.UserContext = userContext;
            this.Settings = settings;
            this._imageDisplayViewModel = imageDisplayViewModel;
            this.AppConfigFileHotKeyManager = appConfigFileHotKeyManager;

            var bitmap = new QRCoderCreator().GenerateQRCodeImage(new QRModel("Hello3Q", Color.GreenYellow, Color.White, 20));

            this.BitmapImage = bitmap;

            this.SwitchThemeCommand = new DelegateCommand(() => this.RefreshTheme());

            eventAggregator.GetEvent<DialogMessageEvent>().Subscribe(item => this.DialogMessage = item);

            this.LoadConfig(config);
            this.InitHotkeys(config, appConfigFileHotKeyManager);

            this.SubscribeCustomCommandEvent();

            this.InitScreenBright(eventAggregator);

            this.InitBackgroundSwitch(eventAggregator);
        }

        public IAppConfigFileHotKeyManager AppConfigFileHotKeyManager { get; }

        public ICommand SwitchThemeCommand { get; private set; }

        private void LoadConfig(IConfigManager config)
        {
            this.OnlyOneProcess = config.IsTrue(CustomConstants.ONLY_ONE_PROCESS);
            this.AutoStart = config.IsTrue(CustomConstants.AUTO_START);
            this.BackgroundSwitch = config.IsTrue(CustomConstants.BACKGROUND_SWITCH);

            this.DefaultThemeURI = config.ReadConfigNode(CustomConstants.DefaultThemeURI);
            this.LoadDefaultTheme();

            this.SetBackgroundImage(config.ReadConfigNode(CustomConstants.BkgrdUri));
            this.IsMusicPlayer = config.IsTrue(CustomConstants.IsMusicPlayer);
            this.IsVideoPlayer = config.IsTrue(CustomConstants.IsVideoPlayer);

            config.SetConfig += config =>
            {
                config.WriteConfigNode<bool>(this.OnlyOneProcess, CustomConstants.ONLY_ONE_PROCESS);
                config.WriteConfigNode<bool>(this.AutoStart, CustomConstants.AUTO_START);
                config.WriteConfigNode<bool>(this.BackgroundSwitch, CustomConstants.BACKGROUND_SWITCH);

                config.WriteConfigNode(this.DefaultThemeURI, CustomConstants.DefaultThemeURI);
                config.WriteConfigNode(this.CurrentBkGrd, CustomConstants.BkgrdUri);

                config.WriteConfigNode(this.IsMusicPlayer, CustomConstants.IsMusicPlayer);
                config.WriteConfigNode(this.IsVideoPlayer, CustomConstants.IsVideoPlayer);

                AppUtils.AutoStartWithDirectory(this.AutoStart);
            };
        }

        private void SetBackgroundImage(string url)
        {
            if (!url.IsNullOrEmpty())
            {
                this.CurrentBkGrd = url;
            }

            if (!this.CurrentBkGrd.IsNullOrEmpty())
            {
                // TrySelectedImage
                foreach (var item in this._imageDisplayViewModel.ActualData)
                {
                    item.Selected = item.URI == this.CurrentBkGrd;
                }
            }
        }

        private void LoadDefaultTheme()
        {
            var defaultThemeUri = this.DefaultThemeURI;
            if (defaultThemeUri.IsNullOrEmpty())
            {
                this.RefreshTheme();
            }
            else
            {
                var currentUri = new Uri(defaultThemeUri, UriKind.RelativeOrAbsolute);

                if (CustomConstants.Dark.Source.ToString().EqualsIgnoreCase(currentUri.ToString()))
                {
                    _resourceDictionaries.Add(CustomConstants.Dark);
                }
                else
                {
                    _resourceDictionaries.Add(CustomConstants.Light);
                }
            }
        }

        #region 主题&背景
        private Collection<ResourceDictionary> _resourceDictionaries => Application.Current.Resources.MergedDictionaries;

        public string DefaultThemeURI { get; set; } = null!;
        private void RefreshTheme()
        {
            var dict = _resourceDictionaries.FirstOrDefault(item => item == CustomConstants.Light);

            if (dict == null)
            {
                _resourceDictionaries.Remove(CustomConstants.Dark);
                _resourceDictionaries.Add(CustomConstants.Light);

                dict = CustomConstants.Light;
            }
            else
            {
                _resourceDictionaries.Remove(CustomConstants.Light);
                _resourceDictionaries.Add(CustomConstants.Dark);

                dict = CustomConstants.Dark;
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
        public string Version { get; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        private double _cpuRate;

        public double CpuRate
        {
            get => this._cpuRate;
            set => SetProperty<double>(ref _cpuRate, value);
        }

        private double _ramRate;

        public double RamRate
        {
            get => this._ramRate;
            set => SetProperty<double>(ref _ramRate, value);
        }

        private string _title = string.Empty;
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

        #region 屏幕亮度
        private void InitScreenBright(IEventAggregator eventAggregator)
        {
            this._brightManager = new ScreenBrightManager();

            this.RefreshBrightness();

            eventAggregator.GetEvent<UpdateScreenBrightEvent>().Subscribe(step => this.CurrentBright += step);
        }

        internal void RefreshBrightness()
        {
            this.CurrentBright = this._brightManager.GetBrightness();
        }

        private ScreenBrightManager _brightManager;
        private bool _isLeastBright;

        public bool IsLeastBright
        {
            get => this._isLeastBright;
            set => SetProperty<bool>(ref _isLeastBright, value);
        }

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
                    this._brightManager.SetBrightness(newValue);
                }
            }
        }
        #endregion

        #region 窗口标题栏快捷键
        public Dictionary<string, IHotKey<Key, ModifierKeys>> WindowKeyBindingMap { get; private set; }

        private void InitHotkeys(IConfigManager config, IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
            var groupName = "窗口";
            appConfigFileHotKeyManager.TryAdd(groupName, PreDefinedHotKeys.ConfigWindowAppHotKeys);
            foreach (var item in PreDefinedHotKeys.WindowAppHotKeys)
            {
                appConfigFileHotKeyManager.TryRegisterItem(groupName, item);
            }

            this.WindowKeyBindingMap = appConfigFileHotKeyManager.First(g => g.GroupName == groupName).ToDictionary(hotKey => hotKey.Name);
        }
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

        private void InitBackgroundSwitch(IEventAggregator eventAggregator)
        {
            eventAggregator.GetEvent<BackgroundImageUpdateEvent>().Subscribe(this.SetBackgroundImage);

            this._timer = new DispatcherTimer();
            this._timer.Tick += (sender, e) =>
            {
                DateTime now = DateTime.Now;
                this.CurrentTime = now.FormatTime();
                this.Week = now.GetWeek();

                var seconds = now.TimeOfDay.Seconds;

                if (this.BackgroundSwitch)
                {
                    if (_imageDisplayViewModel.Data.Count > 0)
                    {
                        if (seconds == 0 || seconds == 30)
                        {
                            var totalCount = _imageDisplayViewModel.Data.Count;

                            this.SetBackgroundImage(_imageDisplayViewModel.Data[this.random.Next(0, totalCount)].URI);
                        }
                    }
                }

                if (this.DialogMessage != null && !this.DialogMessage.StopHide)
                {
                    this.DialogMessage.Decrease();
                }
            };
            this._timer.Interval = TimeSpan.FromMilliseconds(1000);
            this._timer.Start();

            Task.Run(async () =>
            {
                while (this._timer.IsNotNullAnd(timer => timer.IsEnabled))
                {
                    try
                    {
                        this.CpuRate = ComputerInfo.GetCpuUsedRate();
                        this.RamRate = ComputerInfo.GetMemoryUsedRate();

                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        Helper.Helper.Log("App异常日志", ex.Message);
                    }
                }
            });
        }

        private void SubscribeCustomCommandEvent()
        {
            CustomEventManager.Current.GetEvent<OpenSettingEvent>().Execute += () => this.Settings.IsEditingSetting = true;
            CustomEventManager.Current.GetEvent<HideTitleBarEvent>().Execute += () => this.IsTitleBarHidden = !this.IsTitleBarHidden;
            CustomEventManager.Current.GetEvent<LoginEvent>().Execute += () => this.IsLogin = !this.IsLogin;
        }

        public void Dispose()
        {
            this._timer.Stop();
            this._timer = null;
        }
    }
}