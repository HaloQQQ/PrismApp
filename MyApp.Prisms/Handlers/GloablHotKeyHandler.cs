using CustomControlsDemoModule.Events;
using IceTea.Pure.Businesses.HotKey.Global;
using IceTea.Pure.Utils;
using IceTea.Wpf.Atom.Businesses.HotKey.Global;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Music;
using MyApp.Prisms.Contracts;
using MyApp.Prisms.MsgEvents;
using Prism.Events;
using System.Windows;
using IceTea.Wpf.Atom.Extensions;

namespace MyApp.Prisms.Handlers
{
    internal class GloablHotKeyHandler : GlobalHotKeyHandlerBase
    {
        private IEventAggregator _ea;
        public GloablHotKeyHandler(IEventAggregator ea, IGlobalHotKeyManager manager, Window window)
            : base(manager, window)
        {
            _ea = ea.AssertArgumentNotNull(nameof(IEventAggregator));
        }

        protected override bool HandleCore(string hotKeyName)
        {
            switch (hotKeyName)
            {
                case CustomConstants.GlobalHotKeysConst.TogglePlay:
                    _ea.GetEvent<ToggeleCurrentMediaEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.Prev:
                    _ea.GetEvent<PrevMediaEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.Next:
                    _ea.GetEvent<NextMediaEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.FastForward:
                    _ea.GetEvent<FastForwardMediaEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.Rewind:
                    _ea.GetEvent<RewindMediaEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.IncreaseVolume:
                    _ea.GetEvent<IncreaseVolumeEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.DecreaseVolume:
                    _ea.GetEvent<DecreaseVolumeEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.UpScreenBright:
                    _ea.GetEvent<UpdateScreenBrightEvent>().Publish(5);
                    break;
                case CustomConstants.GlobalHotKeysConst.DownScreenBright:
                    _ea.GetEvent<UpdateScreenBrightEvent>().Publish(-5);
                    break;
                case CustomConstants.GlobalHotKeysConst.MusicLyricDesktop:
                    _ea.GetEvent<ToggleDesktopLyricEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.ColorPicker:
                    _ea.GetEvent<ColorPickerEvent>().Publish();
                    break;
                case CustomConstants.GlobalHotKeysConst.PCSleep:
                    AppUtils.SleepPC();
                    break;
                case CustomConstants.GlobalHotKeysConst.PCShutdown:
                    AppUtils.ShutdownPC(1); Application.Current?.MainWindow?.Close();
                    break;
                case CustomConstants.GlobalHotKeysConst.PCRestart:
                    AppUtils.RestartPC(1); Application.Current?.MainWindow?.Close();
                    break;
                case CustomConstants.GlobalHotKeysConst.ActiveWindow:
                    AppUtils.ShowWindowAsync(Application.Current.MainWindow.GetHandle());
                    break;
                default:
                    return false;
            }

            return true;
        }
    }
}
