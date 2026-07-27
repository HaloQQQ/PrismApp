using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Businesses.Setting;
using IceTea.Pure.Extensions;
using MusicPlayerModule.Contracts;
using MusicPlayerModule.Views;
using MyApp.Prisms.Contracts;
using MyApp.Prisms.MsgEvents;
using Prism.Events;
using Prism.Ioc;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
namespace MyApp.Prisms.Views.MainViews
{
    /// <summary>
    /// VideosView.xaml 的交互逻辑
    /// </summary>
    public partial class VideosView : UserControl
    {
        private int _currentVideosCount;

        public VideosView()
        {
            InitializeComponent();

            Application.Current.MainWindow.Loaded += VideoWindow_Loaded;
            this.Unloaded += VideosView_Unloaded;
        }

        private void VideosView_Unloaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<VideosCountChangedEvent>().Unsubscribe(OnVideosCountChanged);
        }

        private void VideoWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var eventAggregator = ContainerLocator.Container.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<VideosCountChangedEvent>().Subscribe(OnVideosCountChanged);

            // 优先从 ISettingManager 读取（SettingsViewModel/VideosViewModel 启动时已写入默认值）；
            // 首次启动时内存字典为空，回退到 DataContext（VideosViewModel）的 Row×Column。
            var settingManager = ContainerLocator.Container.Resolve<ISettingManager>();
            if (settingManager.TryGetValue(CustomConstants.SettingKeys.VideosCount, out string videosCountStr)
                && int.TryParse(videosCountStr, out int count))
            {
                RefreshViews((EnumVideosCount)count);
            }
        }

        /// <summary>
        /// 响应用户动态调整视频格数量（VideosViewModel发布此事件）
        /// </summary>
        private void OnVideosCountChanged(EnumVideosCount videosCount)
        {
            RefreshViews(videosCount);
        }

        /// <summary>
        /// 增量刷新视频区域的视图。
        /// 扩容时保留现有视图只追加新窗格；缩容时移除多余视图。
        /// 历史记录（上次关闭时播放的视频）用于填充新窗格；
        /// 历史比需求少 → 用有效数据补齐，历史比需求多 → 只取需求的数量。
        /// </summary>
        private void RefreshViews(EnumVideosCount videosCount)
        {
            int targetCount = (int)videosCount;
            if (targetCount == _currentVideosCount) return;

            var configManager = ContainerLocator.Container.Resolve<IConfigManager>();
            var regionManager = ContainerLocator.Container.Resolve<IRegionManager>();
            var settingManager = ContainerLocator.Container.Resolve<ISettingManager>();
            IRegion region = regionManager.Regions[CustomConstants.RegionNames.VideosRegion];

            int currentCount = region.Views.Count();
            if (targetCount > currentCount)
            {
                // 扩容：保留现有视图，只追加不足的窗格
                IList<string> historyList = configManager.ReadConfigNode<List<string>>(CustomStatics.HistoryList_ConfigKey);
                var validHistory = historyList?.Where(h => h.IsFileExists()).ToList()
                                   ?? new List<string>();

                bool isVideosAutoLoad = configManager.ReadConfigNode<bool>(CustomConstants.ConfigNodes.IsVideosAutoLoad);

                for (int i = currentCount; i < targetCount; i++)
                {
                    var parameters = new NavigationParameters();
                    if (isVideosAutoLoad && validHistory.Count > 0)
                    {
                        // 历史有数据：按索引取，超出则用第一个补齐
                        var url = i < validHistory.Count ? validHistory[i] : validHistory[0];
                        parameters.Add("originUrl", url);
                    }
                    regionManager.RequestNavigate(CustomConstants.RegionNames.VideosRegion, nameof(VideoPlayerView), parameters);
                }
            }
            else if (targetCount < currentCount)
            {
                // 缩容：移除多余视图，主动调用 Cleanup 注销 SetConfig
                foreach (var view in region.Views.Cast<object>().Skip(targetCount).ToList())
                {
                    region.Remove(view);
                    if (view is VideoPlayerView vpv)
                    {
                        vpv.Cleanup();
                    }
                }
            }

            // 同步更新 VideosCount，确保 VideoPlayerViewModel 在 SetConfig 时读取到正确的 count
            settingManager.AddOrUpdate(CustomConstants.SettingKeys.VideosCount, targetCount.ToString());
            _currentVideosCount = targetCount;
        }
    }
}
