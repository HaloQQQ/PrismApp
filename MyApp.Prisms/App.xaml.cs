using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Businesses.HotKey.Global;
using IceTea.Pure.Businesses.Setting;
using IceTea.Pure.Contracts;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using IceTea.Wpf.Atom.Businesses.Config;
using IceTea.Wpf.Atom.Businesses.GlobalKeyEvent;
using IceTea.Wpf.Atom.Businesses.HotKey.App;
using IceTea.Wpf.Atom.Businesses.HotKey.Global;
using IceTea.Wpf.Atom.Extensions;
using IceTea.Wpf.Atom.Utils;
using MusicPlayerModule.Hooks;
using MusicPlayerModule.Views;
using MyApp.Prisms.Contracts;
using MyApp.Prisms.Handlers;
using MyApp.Prisms.ViewModels;
using MyApp.Prisms.Views;
using MyApp.Prisms.Views.MainViews;
using Prism.DryIoc;
using Prism.Events;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Mvvm;
using Prism.Regions;
using PrismAppBasicLib.Contracts;
using PrismAppBasicLib.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8602 // 解引用可能出现空引用。
namespace MyApp.Prisms
{
    public partial class App : PrismApplication
    {
        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            CommonUtil.Log(CustomConstants.LogType.Software_Log_Dir, $"退出成功!");
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<IKeyHook, MediaGlobalKeyHook>();

            containerRegistry.RegisterSingleton<IConfigManager, WpfYamlConfigManager>();

            var config = this.Container.Resolve<IConfigManager>();

            string processName = Process.GetCurrentProcess().ProcessName;

            if (config.IsTrue(CustomConstants.ONLY_ONE_PROCESS.FillToArray()))
            {
                if (AppUtils.HasProcessExistsAlready(() => CommonUtil.Log(CustomConstants.LogType.Software_Log_Dir, "当前已有软件运行，启动失败!")))
                {
                    return;
                }
            }

            WpfAtomUtils.AddGlobalExceptionHandler(ex => CommonUtil.Log(CustomConstants.LogType.Exception_Log_Dir, ex.Message));

            CommonUtil.Log(CustomConstants.LogType.Software_Log_Dir, $"进程{processName}启动成功!");

            containerRegistry.RegisterSingleton<IAppConfigFileHotKeyManager, AppConfigFileHotKeyManager>();

            containerRegistry.RegisterSingleton<ISettingManager, SettingManager>();
            containerRegistry.RegisterSingleton<ISettingManager<SettingModel>, SettingManager<SettingModel>>();

            containerRegistry.RegisterSingleton<IGlobalConfigFileHotKeyManager>(() =>
                new GlobalConfigFileHotKeyManager(
                    () => Application.Current.MainWindow.GetHandle(),
                    this.Container.Resolve<IConfigManager>()
                ));

            containerRegistry.RegisterSingleton<GlobalHotKeyHandlerBase>(() => new GloablHotKeyHandler(
                    this.Container.Resolve<IEventAggregator>(),
                    this.Container.Resolve<IGlobalConfigFileHotKeyManager>(),
                    Application.Current.MainWindow
                ));

            containerRegistry.RegisterSingleton<ImageDisplayViewModel>();

            containerRegistry.RegisterSingleton<SettingsViewModel>();

            containerRegistry.RegisterScoped<UdpSocketViewModel>();
            containerRegistry.RegisterScoped<TcpClientViewModel>();
            containerRegistry.RegisterScoped<TcpServerViewModel>();
            containerRegistry.RegisterScoped<AnotherTcpServerViewModel>();

            ViewModelLocationProvider.Register<WindowTitleView, SoftwareViewModel>();
            ViewModelLocationProvider.Register<SwitchBackgroundView, ImageDisplayViewModel>();

            this.RegisterRegion(config);

            this.RegisterNavigation(containerRegistry);
        }

        private void RegisterNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<CommunicationView>();

            containerRegistry.RegisterForNavigation<ProcessServiceView>();

            containerRegistry.RegisterForNavigation<MailManager>();
        }

        private void RegisterRegion(IConfigManager config)
        {
            var regionManager = this.Container.Resolve<IRegionManager>();
            regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.MainContentRegion, typeof(CommunicationView));

            regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.SettingRegion, typeof(SettingsView));

            regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.Smtp163MailRegion, typeof(Smtp163MailView));
            regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.SmtpQQMailRegion, typeof(SmtpQQMailView));

            if (config.IsTrue(CustomConstants.IsMusicPlayer.FillToArray()))
            {
                regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.MainWindowRegion, typeof(MusicPlayer));
            }
            else if (config.IsTrue(CustomConstants.IsVideoPlayer.FillToArray()))
            {
                regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.MainWindowRegion, typeof(VideosView));
            }
            else
            {
                regionManager.RegisterViewWithRegion(CustomConstants.RegionNames.MainWindowRegion, typeof(MultiPageView));
            }
        }

        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            moduleCatalog.AddModule<MusicPlayerModule.MusicPlayerModule>();

            moduleCatalog.AddModule<SqlCreatorModule.SqlCreatorModule>();

            moduleCatalog.AddModule<CustomControlsDemoModule.CustomControlsDemoModule>();
        }

        protected override Window CreateShell()
        {
            var config = Container.Resolve<IConfigManager>();

            if (config.IsTrue(CustomConstants.IsMusicPlayer.FillToArray()))
            {
                ((IContainerExtension)Container).RegisterSingleton<SoftwareViewModel, SoftwareViewModel>();
            }
            else if (config.IsTrue(CustomConstants.IsVideoPlayer.FillToArray()))
            {
                ((IContainerExtension)Container).RegisterSingleton<SoftwareViewModel, VideosViewModel>();
            }
            else
            {
                ((IContainerExtension)Container).RegisterSingleton<SoftwareViewModel, MainViewModel>();
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
            IGlobalConfigFileHotKeyManager globaConfigFilelHotKeyManager = Container.Resolve<IGlobalConfigFileHotKeyManager>();

            var systemGroupName = "系统";
            globaConfigFilelHotKeyManager.TryRegister(systemGroupName, CustomConstants.ConfigGlobalHotkeys);

            var globalGroup = globaConfigFilelHotKeyManager[systemGroupName];
            foreach (var item in CustomConstants.GlobalHotKeys)
            {
                globalGroup.TryRegister(item.Name, item.CustomKeys, item.CustomModifierKeys, item.IsUsable);
            }

            var failedKeys = globalGroup.Values.Where(i => i.HasChanged).Select(i => i.ToString());

            if (failedKeys.Any())
            {
                MessageBox.Show($"{AppStatics.NewLineChars.Join(failedKeys)}{AppStatics.NewLineChars}注册失败", $"警告 - {systemGroupName}", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}