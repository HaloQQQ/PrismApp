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
using IceTea.Atom.Utils.HotKey.GlobalHotKey;
using IceTea.General.Utils;
using IceTea.General.Utils.AppHotKey;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils.Setting;
using MyApp.Prisms.Views.ToolBox;
using PrismAppBasicLib.MsgEvents;
using IceTea.Atom.GeneralModels;
using Prism.Regions;
using MusicPlayerModule.MsgEvents;

namespace MyApp.Prisms
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Helper.Helper.Log(CustomConstants.Software_Log_Dir, $"退出成功!");
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
            }

            return list;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var list = this.GetMessageList(e.ExceptionObject as Exception);

            var message = "Domain出现异常:\r\n" + string.Join("\r\n", list);
            Helper.Helper.Log("Domain异常日志", message);
            MessageBox.Show(message);
        }

        private void Current_DispatcherUnhandledException(object sender,
            System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var list = this.GetMessageList(e.Exception);

            var message = "App出现异常:\r\n" + string.Join("\r\n", list);
            Helper.Helper.Log("App异常日志", message);
            MessageBox.Show(message);
        }

        protected override Window CreateShell()
        {
            var config = Container.Resolve<IConfigManager>();

            if (config.IsTrue(CustomConstants.IsMusicPlayer))
            {
                ViewModelLocationProvider.Register<MusicWindow, SoftwareViewModel>();

                return Container.Resolve<MusicWindow>();
            }

            if (config.IsTrue(CustomConstants.IsVideoPlayer))
            {
                ViewModelLocationProvider.Register<VideoWindow, SoftwareViewModel>();

                return Container.Resolve<VideoWindow>();
            }

            ViewModelLocationProvider.Register<MainWindow, SoftwareViewModel>();

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            containerRegistry.RegisterSingleton<IConfigManager, WpfConfigManager>();

            var config = this.Container.Resolve<IConfigManager>();

            string processName = Process.GetCurrentProcess().ProcessName;

            if (config.IsTrue(CustomConstants.ONLY_ONE_PROCESS))
            {
                foreach (var process in Process.GetProcessesByName(processName))
                {
                    if (process.Id != Process.GetCurrentProcess().Id)
                    {
                        AppUtils.ShowWindowAsync(process.MainWindowHandle);

                        Helper.Helper.Log(CustomConstants.Software_Log_Dir, "当前已有软件运行，启动失败!");

                        Environment.Exit(1);
                    }
                }
            }

            Helper.Helper.Log(CustomConstants.Software_Log_Dir, $"进程{processName}启动成功!");

            containerRegistry.RegisterSingleton<IAppHotKeyManager, AppHotKeyManager>();
            containerRegistry.RegisterSingleton<ISettingManager, SettingManager>();

            containerRegistry.RegisterSingleton<ImageDisplayViewModel>();
            containerRegistry.RegisterSingleton<SoftwareViewModel>();
            containerRegistry.RegisterSingleton<SettingsViewModel>();

            containerRegistry.RegisterScoped<UdpSocketViewModel>();
            containerRegistry.RegisterScoped<TcpClientViewModel>();
            containerRegistry.RegisterScoped<TcpServerViewModel>();
            containerRegistry.RegisterScoped<AnotherTcpServerViewModel>();

            ViewModelLocationProvider.Register<WindowTitleBarView, SoftwareViewModel>();
            ViewModelLocationProvider.Register<SwitchBackgroundView, ImageDisplayViewModel>();

            containerRegistry.Register<Settings>();

            this.RegisterRegion();
        }

        private void RegisterRegion()
        {
            var regionManager = this.Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion("SettingRegion", nameof(Settings));

            regionManager.RegisterViewWithRegion("Smtp163MailRegion", () => new Smtp163MailView());
            regionManager.RegisterViewWithRegion("SmtpQQMailRegion", () => new SmtpQQMailView());

            regionManager.RegisterViewWithRegion("ColorRegion", () => new ColorView());
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MusicPlayerModule.MusicPlayerModule>();

            moduleCatalog.AddModule<SqlCreatorModule.SqlCreatorModule>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            GlobalHotKeyManager hotKeyManager = null;
            hotKeyManager = new GlobalHotKeyManager(
                    App.Current.MainWindow.RegisterHotKeyManager(mid =>
                        {
                            foreach (var item in hotKeyManager)
                            {
                                if (item.Code == mid)
                                {
                                    switch (item.Name)
                                    {
                                        case CustomConstants.GlobalHotKeysConst.Pause:
                                            this.Container.Resolve<IEventAggregator>().GetEvent<ToggeleCurrentMusicEvent>().Publish();
                                            break;
                                        case CustomConstants.GlobalHotKeysConst.Prev:
                                            this.Container.Resolve<IEventAggregator>().GetEvent<PrevMusicEvent>().Publish();
                                            break;
                                        case CustomConstants.GlobalHotKeysConst.Next:
                                            this.Container.Resolve<IEventAggregator>().GetEvent<NextMusicEvent>().Publish();
                                            break;

                                        case CustomConstants.GlobalHotKeysConst.Ahead:
                                            this.Container.Resolve<IEventAggregator>().GetEvent<AheadEvent>().Publish();
                                            break;
                                        case CustomConstants.GlobalHotKeysConst.Delay:
                                            this.Container.Resolve<IEventAggregator>().GetEvent<DelayEvent>().Publish();
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
                                            this.Container.Resolve<IEventAggregator>().GetEvent<ToggleLyricDesktopEvent>().Publish();
                                            break;
                                    }
                                }
                            }
                        }
                    )
                );

            ContainerLocator.Current.RegisterSingleton<GlobalHotKeyManager>(() => hotKeyManager);

            var str = hotKeyManager.RegisterHotKeys(this.Container.Resolve<SettingsViewModel>().GlobalHotKeys);

            if (str.Length > 0)
            {
                this.Container.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new DialogMessage(str, 2));
            }
        }
    }
}