using IceTea.Atom.Contracts;
using IceTea.Maui.Core.Contracts;
using MauiAppNet8.ViewModels;
using MauiAppNet8.ViewModels.Socket;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Prism.Mvvm;
using Prism.Events;

namespace MauiAppNet8
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("iconfont.ttf", "iconfont");
                })
                .ConfigureServices();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build()
                .ConfigureContainer();
        }

        private static MauiApp ConfigureContainer(this MauiApp app)
        {
            ViewModelLocationProvider.SetDefaultViewModelFactory((view, viewModelType) => app.Services.GetRequiredService(viewModelType));

            ViewModelLocationProvider.Register<MainPage, ClickCounter>();
            ViewModelLocationProvider.Register<PhonewordTranslator, PhoneNumTranslateViewModel>();

            ViewModelLocationProvider.Register<AppShell, CommonViewModel>();

            return app;
        }

        private static MauiAppBuilder ConfigureServices(this MauiAppBuilder builder)
        {
            builder.Services.TryAddSingleton<ClickCounter, ClickCounter>();
            builder.Services.TryAddSingleton<PhoneNumTranslateViewModel, PhoneNumTranslateViewModel>();
            builder.Services.TryAddTransient<SocketMainViewModel, SocketMainViewModel>();

            builder.Services.TryAddSingleton<TcpServerViewModel, TcpServerViewModel>();
            builder.Services.TryAddSingleton<TcpClientViewModel, TcpClientViewModel>();
            builder.Services.TryAddSingleton<UdpSocketViewModel, UdpSocketViewModel>();

            builder.Services.TryAddSingleton<OrderViewModel, OrderViewModel>();
            builder.Services.TryAddSingleton<ColorViewModel, ColorViewModel>();
            builder.Services.TryAddSingleton<CommonViewModel, CommonViewModel>();

            builder.Services.TryAddSingleton<NewsViewModel, NewsViewModel>();

            builder.Services.TryAddSingleton<_2048ViewModel>();

            builder.Services.TryAddSingleton<IConfigManager, MauiConfigManager>();
            builder.Services.TryAddSingleton<IEventAggregator, EventAggregator>();

            return builder;
        }
    }
}
