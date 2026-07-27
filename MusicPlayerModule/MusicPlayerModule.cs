using MusicPlayerModule.Views;
using Prism.Ioc;
using Prism.Modularity;

namespace MusicPlayerModule
{
    public class MusicPlayerModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<MusicPlayer>();
            containerRegistry.RegisterForNavigation<VideoPlayerView>();

            //containerRegistry.RegisterSingleton<MusicPlayerViewModel>();

            //Prism.Mvvm.ViewModelLocationProvider.Register<HorizontalMusicLyricDesktopWindow, MusicPlayerViewModel>();
            //Prism.Mvvm.ViewModelLocationProvider.Register<VerticalMusicLyricDesktopWindow, MusicPlayerViewModel>();
            //Prism.Mvvm.ViewModelLocationProvider.Register<MusicFooterView, MusicPlayerViewModel>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }
    }
}