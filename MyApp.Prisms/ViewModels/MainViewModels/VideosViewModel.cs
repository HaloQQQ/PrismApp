using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Businesses.Setting;
using IceTea.Wpf.Atom.Businesses.HotKey.App;
using IceTea.Wpf.Atom.Businesses.HotKey.Global;
using MyApp.Prisms.Contracts;
using MyApp.Prisms.MsgEvents;
using Prism.Events;
using System;

#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
namespace MyApp.Prisms.ViewModels
{
    internal class VideosViewModel : SoftwareViewModel
    {
        public VideosViewModel(
                UserViewModel userContext,
                ImageDisplayViewModel imageDisplayViewModel,
                SettingsViewModel settings,
                IConfigManager config,
                ISettingManager settingManager,
                IAppConfigFileHotKeyManager appCfgHotkeyManager,
                IEventAggregator eventAggregator,
                GlobalHotKeyHandlerBase gloablHotKeyHandler
            ) : base(userContext, imageDisplayViewModel, settings, config, appCfgHotkeyManager,
                eventAggregator, gloablHotKeyHandler)
        {
            LoadVideosCount(settingManager);

            eventAggregator.GetEvent<VideosCountChangedEvent>()
                           .Subscribe(enumVideoCount =>
                            {
                                var tuple = ResolveGrid(enumVideoCount);

                                Row = tuple.row;
                                Column = tuple.col;
                            });
        }

        private int _row = 2;
        public int Row
        {
            get => _row;
            set => SetProperty(ref _row, value);
        }

        private int _column = 4;
        public int Column
        {
            get => _column;
            set => SetProperty(ref _column, value);
        }

        private void LoadVideosCount(ISettingManager settingManager)
        {
            int count = 8;

            if (settingManager.TryGetValue(CustomConstants.SettingKeys.VideosCount, out string videosCountStr))
            {
                count = int.Parse(videosCountStr);
            }

            EnumVideosCount videosCount = EnumVideosCount.Eight;
            if (Enum.IsDefined(typeof(EnumVideosCount), count))
            {
                videosCount = (EnumVideosCount)count;
            }
            else
            {
                throw new IndexOutOfRangeException();
            }

            var tuple = ResolveGrid(videosCount);

            Row = tuple.row;
            Column = tuple.col;
        }

        private (int row, int col) ResolveGrid(EnumVideosCount count) => count switch
        {
            EnumVideosCount.One => (1, 1),
            EnumVideosCount.Two => (1, 2),
            EnumVideosCount.Three => (1, 3),
            EnumVideosCount.Four => (2, 2),
            EnumVideosCount.Six => (2, 3),
            EnumVideosCount.Eight => (2, 4),
            EnumVideosCount.Ten => (2, 5),
            EnumVideosCount.Twelve => (3, 4),
            EnumVideosCount.Fifteen => (3, 5),
            _ => (2, 4),
        };
    }
}
