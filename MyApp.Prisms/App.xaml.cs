using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using MyApp.Prisms.Helper;
using MyApp.Prisms.MsgEvents;
using MyApp.Prisms.ViewModels;
using MyApp.Prisms.Views;
using IceTea.Atom.Utils;
using IceTea.NetCore.Utils;
using IceTea.NetCore.Utils.AppHotKey;
using IceTea.Atom.Interfaces;
using IceTea.Atom.Utils.HotKey.GlobalHotKey;

namespace MyApp.Prisms
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : PrismApplication
    {
        private Mutex? mutex;

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Helper.Helper.Log(CustomConstants.Software_Log_Dir, $"退出成功!");
        }

        private IEnumerable<string> GetMessageList(Exception exception)
        {
            var list = new List<string>();
            list.Add(exception.Message);

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
            var config = this.Container.Resolve<IConfigManager>();

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
                mutex = new Mutex(true, processName, out bool isNew);
                if (!isNew)
                {
                    Helper.Helper.Log(CustomConstants.Software_Log_Dir, "当前已有软件运行，启动失败!");

                    foreach (var process in Process.GetProcessesByName(processName))
                    {
                        if (process.Id != Process.GetCurrentProcess().Id)
                        {
                            AppUtils.ShowWindowAsync(process.MainWindowHandle, 1);
                        }
                    }

                    MessageBox.Show("当前已有软件运行，启动失败!");

                    Environment.Exit(1);
                }
            }

            Helper.Helper.Log(CustomConstants.Software_Log_Dir, $"进程{processName}启动成功!");

            containerRegistry.RegisterSingleton<IAppHotKeyManager, AppHotKeyManager>();

            containerRegistry.RegisterSingleton<SettingsViewModel>();

            containerRegistry.RegisterScoped<UDPViewModel>();
            containerRegistry.RegisterScoped<TcpClientViewModel>();
            containerRegistry.RegisterScoped<TcpServerViewModel>();
            containerRegistry.RegisterScoped<AnotherTcpServerViewModel>();

            //containerRegistry.RegisterScoped<CommunicationViewModel>();

            containerRegistry.RegisterSingleton<ImageDisplayViewModel>();
            //containerRegistry.RegisterSingleton<UserViewModel>();
            containerRegistry.RegisterSingleton<SoftwareViewModel>();

            ViewModelLocationProvider.Register<WindowTitleBarView, SoftwareViewModel>();
            ViewModelLocationProvider.Register<ImageDisplayView, ImageDisplayViewModel>();
            ViewModelLocationProvider.Register<SwitchBackgroundView, ImageDisplayViewModel>();

            containerRegistry.Register<Settings>();
            this.Container.Resolve<IRegionManager>().RegisterViewWithRegion("SettingRegion", nameof(Settings));
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MusicPlayerModule.MusicPlayerModule>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            GlobalHotKeyManager hotKeyManager = null;
            hotKeyManager = new GlobalHotKeyManager(App.Current.MainWindow.RegistHotKeyManager(mid =>
            {
                foreach (var item in hotKeyManager)
                {
                    if (item.Code == mid)
                    {
                        if (item.Name == CustomConstants.GlobalHotKeysConst.Pause)
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<MusicPlayerModule.MsgEvents.ToggeleCurrentMusicEvent>().Publish();
                        }
                        else if (item.Name == CustomConstants.GlobalHotKeysConst.Prev)
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<MusicPlayerModule.MsgEvents.PrevMusicEvent>().Publish();
                        }
                        else if (item.Name == CustomConstants.GlobalHotKeysConst.Next)
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<MusicPlayerModule.MsgEvents.NextMusicEvent>().Publish();
                        }
                        else if (item.Name == CustomConstants.GlobalHotKeysConst.UpScreenBright)
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<UpdateScreenBrightEvent>().Publish(5);
                        }
                        else if (item.Name == CustomConstants.GlobalHotKeysConst.DownScreenBright)
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<UpdateScreenBrightEvent>().Publish(-5);
                        }
                    }
                }
            }));

            ContainerLocator.Current.RegisterSingleton<GlobalHotKeyManager>(() => hotKeyManager);

            var str = hotKeyManager.RegisterHotKeys(this.Container.Resolve<SettingsViewModel>().GlobalHotKeys);

            if (str.Length > 0)
            {
                this.Container.Resolve<IEventAggregator>().GetEvent<DialogMessageEvent>().Publish(new Models.DialogMessage(str, 2));
            }
        }
    }
}