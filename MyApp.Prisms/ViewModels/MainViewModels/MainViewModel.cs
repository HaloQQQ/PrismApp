using CustomControlsDemoModule.Views;
using CustomControlsDemoModule.Views.Controls;
using IceTea.Pure.Businesses.Config;
using IceTea.Wpf.Atom.Businesses.HotKey.App;
using IceTea.Wpf.Atom.Businesses.HotKey.Global;
using MusicPlayerModule.Views;
using MyApp.Prisms.Contracts;
using MyApp.Prisms.Views;
using Prism.Commands;
using Prism.Events;
using Prism.Regions;
using SqlCreatorModule.Views;
using System.Windows.Input;

namespace MyApp.Prisms.ViewModels
{
    internal class MainViewModel : SoftwareViewModel
    {
        public MainViewModel(
                UserViewModel userContext,
                ImageDisplayViewModel imageDisplayViewModel,
                SettingsViewModel settings,
                IConfigManager config,
                IAppConfigFileHotKeyManager appCfgHotkeyManager,
                IEventAggregator eventAggregator,
                IRegionManager regionManager,
                GlobalHotKeyHandlerBase gloablHotKeyHandler
            ) : base(userContext, imageDisplayViewModel, settings, config, appCfgHotkeyManager,
                eventAggregator, gloablHotKeyHandler)
        {
            this.NavigateToCommand = new DelegateCommand<string>(target =>
            {
                var uri = target;

                this.Title = uri;

                switch (uri)
                {
                    case "通讯工具":
                        uri = nameof(CommunicationView);
                        break;
                    case "进程服务":
                        uri = nameof(ProcessServiceView);
                        break;
                    case "邮件客户端":
                        uri = nameof(MailManager);
                        break;

                    case "数据表结构":
                        uri = nameof(CreateModelView);
                        break;
                    case "颜色转换":
                        uri = nameof(ColorView);
                        break;

                    case "控件样例":
                        uri = nameof(ControlsDemoView);
                        break;

                    case "音乐播放器":
                        uri = nameof(MusicPlayer);
                        break;

                    case "视频播放器":
                        uri = nameof(VideoPlayerView);
                        break;

                    case "小工具":
                        uri = nameof(ToolsView);
                        break;
                    default:
                        break;
                }

                regionManager.RequestNavigate(CustomConstants.RegionNames.MainContentRegion, uri, nr => { }, new NavigationParameters()
                    {
                        { "Key", "Value" }
                    });
            });
        }

        public ICommand NavigateToCommand { get; }
    }
}
