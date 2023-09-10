using MusicPlayerModule.Views;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace MusicPlayerModule
{
    public class MusicPlayerModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public MusicPlayerModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MusicPlayer>();
            containerRegistry.RegisterForNavigation<VideoPlayerView>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion("MusicPlayerRegion", nameof(MusicPlayer));
            _regionManager.RegisterViewWithRegion("VideoPlayerRegion", nameof(VideoPlayerView));

            _regionManager.RegisterViewWithRegion("Video1PlayerRegion", nameof(VideoPlayerView));
            _regionManager.RegisterViewWithRegion("Video2PlayerRegion", nameof(VideoPlayerView));
            _regionManager.RegisterViewWithRegion("Video3PlayerRegion", nameof(VideoPlayerView));
            _regionManager.RegisterViewWithRegion("Video4PlayerRegion", nameof(VideoPlayerView));
            _regionManager.RegisterViewWithRegion("Video5PlayerRegion", nameof(VideoPlayerView));
            _regionManager.RegisterViewWithRegion("Video6PlayerRegion", nameof(VideoPlayerView));
        }
    }
}