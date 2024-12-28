using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using MyApp.Prisms.Helper;
using MyApp.Prisms.MsgEvents;
using MyApp.Prisms.ViewModels;
using MyApp.Prisms.Views;
using IceTea.Atom.Utils;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils.Setting;
using PrismAppBasicLib.MsgEvents;
using Prism.Regions;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.Views;
using IceTea.Atom.Utils.HotKey.Global;
using System.Linq;
using IceTea.Atom.Utils.HotKey.Global.Contracts;
using CustomControlsDemoModule;
using IceTea.Wpf.Atom.Utils;
using IceTea.Wpf.Atom.Utils.HotKey;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using IceTea.Atom.Extensions;
using MusicPlayerModule.MsgEvents.Music;
using PrismAppBasicLib.Models;
using CustomControlsDemoModule.Events;

namespace MyApp.Prisms
{
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8604 // 引用类型参数可能为 null。
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Helper.Helper.Log(CustomConstants.LogType.Software_Log_Dir, $"退出成功!");
        }

        private IEnumerable<string> GetMessageList(Exception exception)
        {
            var list = new List<string>
            {
                exception.Message
            };

            while ((exception = exception.InnerException) != null)
            {
                list.Add(exception.Message);

                if (!exception.StackTrace.IsNullOrBlank())
                {
                    list.Add(exception.StackTrace);
                }
            }

            return list;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var list = this.GetMessageList(e.ExceptionObject as Exception);

            var message = "Domain出现异常:\r\n" + string.Join("\r\n", list);
            Helper.Helper.Log(CustomConstants.LogType.DomainException_Log_Dir, message);
            MessageBox.Show(message);
        }

        private void Current_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var list = this.GetMessageList(e.Exception);

            var message = "App出现异常:\r\n" + string.Join("\r\n", list);
            Helper.Helper.Log(CustomConstants.LogType.Exception_Log_Dir, message);
            MessageBox.Show(message);
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            containerRegistry.RegisterSingleton<IConfigManager, WpfConfigManager>();

            var config = this.Container.Resolve<IConfigManager>();

            string processName = Process.GetCurrentProcess().ProcessName;

            if (config.IsTrue(CustomConstants.ONLY_ONE_PROCESS.FillToArray()))
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        AppUtils.ShowWindowAsync(process.MainWindowHandle);

                        Helper.Helper.Log(CustomConstants.LogType.Software_Log_Dir, "当前已有软件运行，启动失败!");

                        Environment.Exit(1);
                    }
                }
            }

            Helper.Helper.Log(CustomConstants.LogType.Software_Log_Dir, $"进程{processName}启动成功!");

            containerRegistry.RegisterSingleton<IAppConfigFileHotKeyManager, AppConfigFileHotKeyManager>();

            containerRegistry.RegisterSingleton<ISettingManager, SettingManager>();
            containerRegistry.RegisterSingleton<ISettingManager<SettingModel>, SettingManager<SettingModel>>();

            containerRegistry.RegisterSingleton<ImageDisplayViewModel>();
            containerRegistry.RegisterSingleton<SoftwareViewModel>();
            containerRegistry.RegisterSingleton<SettingsViewModel>();

            containerRegistry.RegisterScoped<UdpSocketViewModel>();
            containerRegistry.RegisterScoped<TcpClientViewModel>();
            containerRegistry.RegisterScoped<TcpServerViewModel>();
            containerRegistry.RegisterScoped<AnotherTcpServerViewModel>();

            ViewModelLocationProvider.Register<WindowTitleBarView, SoftwareViewModel>();
            ViewModelLocationProvider.Register<SwitchBackgroundView, ImageDisplayViewModel>();

            this.RegisterRegion();

            this.RegisterNavigation(containerRegistry);
        }

        private void RegisterNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CommunicationView>();

            containerRegistry.RegisterForNavigation<ProcessServiceView>();

            containerRegistry.RegisterForNavigation<MailManager>();
        }

        private void RegisterRegion()
        {
            var regionManager = this.Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("MainContentRegion", typeof(CommunicationView));

            regionManager.RegisterViewWithRegion("SettingRegion", typeof(SettingsView));

            regionManager.RegisterViewWithRegion("Smtp163MailRegion", typeof(Smtp163MailView));
            regionManager.RegisterViewWithRegion("SmtpQQMailRegion", typeof(SmtpQQMailView));

            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video1PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video2PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video3PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video4PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video5PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video6PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video7PlayerRegion");
            regionManager.RegisterViewWithRegion<VideoPlayerView>("Video8PlayerRegion");
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MusicPlayerModule.MusicPlayerModule>();

            moduleCatalog.AddModule<SqlCreatorModule.SqlCreatorModule>();

            moduleCatalog.AddModule<CustomControlsDemoModuleModule>();
        }

        protected override Window CreateShell()
        {
            var config = Container.Resolve<IConfigManager>();

            if (config.IsTrue(CustomConstants.IsMusicPlayer.FillToArray()))
            {
                ViewModelLocationProvider.Register<MusicWindow, SoftwareViewModel>();

                return Container.Resolve<MusicWindow>();
            }

            if (config.IsTrue(CustomConstants.IsVideoPlayer.FillToArray()))
            {
                ViewModelLocationProvider.Register<VideoWindow, SoftwareViewModel>();

                return Container.Resolve<VideoWindow>();
            }

            ViewModelLocationProvider.Register<MainWindow, SoftwareViewModel>();

            return Container.Resolve<MainWindow>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            this.RegisterGloablHotKey();
        }

        private void RegisterGloablHotKey()
        {
            IGlobalConfigFileHotKeyManager globalConfigFileHotKeyManager = null;

            globalConfigFileHotKeyManager = new GlobalConfigFileHotKeyManager(App.Current.MainWindow.RegisterHotKeyManager(mid =>
            {
#pragma warning disable CS8602 // 解引用可能出现空引用。
                foreach (var group in globalConfigFileHotKeyManager)
                {
                    foreach (var item in group)
                    {
                        if (item is GlobalHotKey hotKey && hotKey.Code == mid)
                        {
                            switch (item.Name)
                            {
                                case CustomConstants.GlobalHotKeysConst.Pause:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<ToggeleCurrentMediaEvent>().Publish();
                                    break;
                                case CustomConstants.GlobalHotKeysConst.Prev:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<PrevMediaEvent>().Publish();
                                    break;
                                case CustomConstants.GlobalHotKeysConst.Next:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<NextMediaEvent>().Publish();
                                    break;

                                case CustomConstants.GlobalHotKeysConst.FastForward:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<FastForwardMediaEvent>().Publish();
                                    break;
                                case CustomConstants.GlobalHotKeysConst.Rewind:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<RewindMediaEvent>().Publish();
                                    break;

                                case CustomConstants.GlobalHotKeysConst.IncreaseVolume:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<IncreaseVolumeEvent>().Publish();
                                    break;
                                case CustomConstants.GlobalHotKeysConst.DecreaseVolume:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<DecreaseVolumeEvent>().Publish();
                                    break;

                                case CustomConstants.GlobalHotKeysConst.UpScreenBright:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<UpdateScreenBrightEvent>().Publish(5);
                                    break;
                                case CustomConstants.GlobalHotKeysConst.DownScreenBright:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<UpdateScreenBrightEvent>().Publish(-5);
                                    break;
                                case CustomConstants.GlobalHotKeysConst.MusicLyricDesktop:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<ToggleDesktopLyricEvent>().Publish();
                                    break;

                                case CustomConstants.GlobalHotKeysConst.ColorPicker:
                                    this.Container.Resolve<IEventAggregator>().GetEvent<ColorPickerEvent>().Publish();
                                    break;
                            }
                        }
                    }
                }
#pragma warning restore CS8602 // 解引用可能出现空引用。
            }), this.Container.Resolve<IConfigManager>());

            ContainerLocator.Current.RegisterSingleton<IGlobalConfigFileHotKeyManager>(() => globalConfigFileHotKeyManager);

            this.Container.Resolve<SettingsViewModel>().GlobaConfigFilelHotKeyManager = globalConfigFileHotKeyManager;

            var systemGroupName = "系统";
            globalConfigFileHotKeyManager.TryAdd(systemGroupName, CustomConstants.ConfigGlobalHotkeys);

            foreach (var item in CustomConstants.GlobalHotKeys)
            {
                globalConfigFileHotKeyManager.TryRegisterItem(systemGroupName, item.Name, item.CustomKeys, item.CustomModifierKeys, item.IsUsable);
            }

            globalConfigFileHotKeyManager.TryRegisterItem(systemGroupName, CustomConstants.GlobalHotKeysConst.ColorPicker, CustomKeys.C, CustomModifierKeys.Alt | CustomModifierKeys.Shift, true);

            var failedKeys = globalConfigFileHotKeyManager.First(g => g.GroupName == systemGroupName).Where(i => i.HasChanged).Select(i => i.ToString());

            if (failedKeys.Any())
            {
                this.Container.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new DialogMessage($"{AppStatics.NewLineChars.Join(failedKeys)}{AppStatics.NewLineChars}注册失败", 4));
            }
        }
    }
}