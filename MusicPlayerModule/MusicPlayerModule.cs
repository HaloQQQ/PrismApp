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
            containerRegistry.RegisterForNavigation<MusicPlayer>();
            containerRegistry.RegisterForNavigation<VideoPlayerView>();

            containerRegistry.RegisterSingleton<MusicPlayerViewModel>();
            Prism.Mvvm.ViewModelLocationProvider.Register<HorizontalMusicLyricDesktopWindow, MusicPlayerViewModel>();
            Prism.Mvvm.ViewModelLocationProvider.Register<VerticalMusicLyricDesktopWindow, MusicPlayerViewModel>();
            Prism.Mvvm.ViewModelLocationProvider.Register<MusicFooterView, MusicPlayerViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            _regionManager.RegisterViewWithRegion<MusicPlayer>("MusicPlayerRegion");
        }
    }
}