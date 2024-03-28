using MauiAppNet8.Views.Socket;

namespace MauiAppNet8
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            //MainPage = new AppShell();

            Routing.RegisterRoute(nameof(TcpServerView), typeof(TcpServerView));
            Routing.RegisterRoute(nameof(TcpClientView), typeof(TcpClientView));
            Routing.RegisterRoute(nameof(UdpSocketView), typeof(UdpSocketView));

            Routing.RegisterRoute(nameof(About), typeof(About));
            Routing.RegisterRoute(nameof(Help), typeof(Help));
        }

        protected override Window CreateWindow(IActivationState activationState)
        {
            var window = new Window(new AppShell())
            {
                Height = 660
            };

            // Get display size
            var displayInfo = DeviceDisplay.Current.MainDisplayInfo;

            // Center the window
            window.X = (displayInfo.Width / displayInfo.Density - window.Width) / 2;
            window.Y = (displayInfo.Height / displayInfo.Density - window.Height) / 2;

            return window;
        }
    }
}
