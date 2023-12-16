using MusicPlayerModule.ViewModels;
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
            containerRegistry.RegisterSingleton<MusicPlayerViewModel>();
            Prism.Mvvm.ViewModelLocationProvider.Register<MusicLyricDesktopWindow, MusicPlayerViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion<MusicPlayer>("MusicPlayerRegion");
            _regionManager.RegisterViewWithRegion<VideoPlayerView>("VideoPlayerRegion");

            _regionManager.RegisterViewWithRegion<VideoPlayerView>("Video1PlayerRegion");
            _regionManager.RegisterViewWithRegion<VideoPlayerView>("Video2PlayerRegion");
            _regionManager.RegisterViewWithRegion<VideoPlayerView>("Video3PlayerRegion");
            _regionManager.RegisterViewWithRegion<VideoPlayerView>("Video4PlayerRegion");
            _regionManager.RegisterViewWithRegion<VideoPlayerView>("Video5PlayerRegion");
            _regionManager.RegisterViewWithRegion<VideoPlayerView>("Video6PlayerRegion");
        }
    }
}