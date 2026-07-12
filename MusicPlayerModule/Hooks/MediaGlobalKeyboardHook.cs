using IceTea.Desktop.Businesses.GlobalEvent;
using IceTea.Desktop.Contracts.KeyboardHook;
using IceTea.Pure.Utils;
using MusicPlayerModule.MsgEvents;
using MusicPlayerModule.MsgEvents.Media;
using Prism.Events;

#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
namespace MusicPlayerModule.Hooks
{
    public class MediaGlobalKeyboardHook : GlobalKeyboardHook
    {
        private IEventAggregator _eventAggregator;

        public MediaGlobalKeyboardHook(IEventAggregator eventAggregator) : base(true)
        {
            _eventAggregator = eventAggregator.AssertArgumentNotNull(nameof(IEventAggregator));
        }

        protected override void OnActivity(CustomKeyboardEventArgs args)
        {
            base.OnActivity(args);

            var key = args.MediaCode;
            switch (key)
            {
                case MediaCode.MediaPlayPause:
                    _eventAggregator.GetEvent<ToggeleCurrentMediaEvent>().Publish();
                    break;
                case MediaCode.MediaNext:
                    _eventAggregator.GetEvent<NextMediaEvent>().Publish();
                    break;
                case MediaCode.MediaPrevious:
                    _eventAggregator.GetEvent<PrevMediaEvent>().Publish();
                    break;
                case MediaCode.MediaStop:
                    _eventAggregator.GetEvent<StopMediaEvent>().Publish();
                    break;
            }
        }

        protected override void DisposeCore()
        {
            base.DisposeCore();

            _eventAggregator = null;
        }
    }
}
