using Helper.Utils;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows;
using TcpSocket.Helper;
using TcpSocket.MsgEvents;
using TcpSocket.ViewModels;
using TcpSocket.Views;
using WpfStyleResources.Helper;
using WpfStyleResources.Interfaces;

namespace TcpSocket
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

            Helper.Helper.Log(Constants.Software_Log_Dir, $"退出成功!");
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

            if (config.IsTrue(Constants.IsMusicPlayer))
            {
                ViewModelLocationProvider.Register<MusicWindow, SoftwareViewModel>();

                return Container.Resolve<MusicWindow>();
            }

            if (config.IsTrue(Constants.IsVideoPlayer))
            {
                ViewModelLocationProvider.Register<VideoWindow, SoftwareViewModel>();

                return Container.Resolve<VideoWindow>();
            }                            

            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            containerRegistry.RegisterSingleton<IConfigManager, ConfigManager>();

            var config = this.Container.Resolve<IConfigManager>();

            if (config.IsTrue(Constants.ONLY_ONE_PROCESS))
            {
                string processName = Process.GetCurrentProcess().ProcessName;
                mutex = new Mutex(true, processName, out bool isNew);
                {
                    if (!isNew)
                    {
                        Helper.Helper.Log(Constants.Software_Log_Dir, "当前已有软件运行，启动失败!");

                        foreach (var process in Process.GetProcessesByName(processName))
                        {
                            if (process.Id != Process.GetCurrentProcess().Id)
                            {
                                CommonUtils.ShowWindowAsync(process.MainWindowHandle, 1);
                            }
                        }

                        MessageBox.Show("当前已有软件运行，启动失败!");

                        Environment.Exit(1);
                    }

                    Helper.Helper.Log(Constants.Software_Log_Dir, $"启动成功!");
                }
            }

            containerRegistry.RegisterScoped<UDPViewModel>();
            containerRegistry.RegisterScoped<TcpClientViewModel>();
            containerRegistry.RegisterScoped<TcpServerViewModel>();
            containerRegistry.RegisterScoped<AnotherTcpServerViewModel>();

            //containerRegistry.RegisterScoped<CommunicationViewModel>();

            containerRegistry.RegisterSingleton<ImageDisplayViewModel>();
            //containerRegistry.RegisterSingleton<UserViewModel>();
            containerRegistry.RegisterSingleton<SoftwareViewModel>();
            
            ViewModelLocationProvider.Register<MainWindow, SoftwareViewModel>();
            ViewModelLocationProvider.Register<WindowTitleBarView, SoftwareViewModel>();
            ViewModelLocationProvider.Register<ImageDisplayView, ImageDisplayViewModel>();
            ViewModelLocationProvider.Register<SwitchBackgroundView, ImageDisplayViewModel>();
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MusicPlayerModule.MusicPlayerModule>();
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            /// 
            /// 窗体回调函数，接收所有窗体消息的事件处理函数
            /// 
            /// 窗口句柄
            /// 消息
            /// 附加参数1
            /// 附加参数2
            /// 是否处理
            /// 返回句柄
            HotKeyHelper HotKeys = null;
            HotKeys = new HotKeyHelper(this.MainWindow.RegistHotKeyManager(mid =>
            {
                foreach (var item in HotKeys)
                {
                    if (item.Code == mid)
                    {
                        if (item.Name == "暂停")
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<MusicPlayerModule.MsgEvents.ToggeleCurrentMusicEvent>().Publish();
                        }
                        else if (item.Name == "上一个")
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<MusicPlayerModule.MsgEvents.PrevMusicEvent>().Publish();
                        }
                        else if (item.Name == "下一个")
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<MusicPlayerModule.MsgEvents.NextMusicEvent>().Publish();
                        }
                        else if (item.Name == "全屏")
                        {
                            this.Container.Resolve<IEventAggregator>().GetEvent<FullScreenEvent>().Publish();
                        }
                    }
                }
            }));

            var str = HotKeys.RegisterGlobalHotKey(new HotKeyModel[]{
                new HotKeyModel
                {
                    Name = "暂停",
                    IsSelectAlt = true,
                    SelectKey = Keys.S
                },
                new HotKeyModel
                {
                    Name = "上一个",
                    IsSelectAlt = true,
                    SelectKey = Keys.Left
                },
                new HotKeyModel
                {
                    Name = "下一个",
                    IsSelectAlt = true,
                    SelectKey = Keys.Right
                },
                new HotKeyModel
                {
                    Name = "全屏",
                    IsSelectCtrl = true,
                    SelectKey = Keys.F11
                }
            });

            if (str.Length > 0)
            {
                MessageBox.Show(str);
            }
        }
    }
}