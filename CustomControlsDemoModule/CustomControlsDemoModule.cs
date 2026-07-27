using CustomControlsDemoModule.ViewModels;
using CustomControlsDemoModule.Views;
using CustomControlsDemoModule.Views.Controls;
using CustomControlsDemoModule.Views.Controls.Buttons;
using CustomControlsDemoModule.Views.Controls.TextBoxes;
using Prism.Ioc;
using Prism.Modularity;
using IceTea.Windows.Businesses.GlobalEvent;
using IceTea.Windows.Contracts.GlobalEvent;

namespace CustomControlsDemoModule
{
    public class CustomControlsDemoModule : IModule
    {
        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            this.RegisterNavigation(containerRegistry);

            containerRegistry.RegisterDialog<_2048>();
            containerRegistry.RegisterSingleton<_2048ViewModel>();

            containerRegistry.RegisterDialog<FiveChessView>("五子棋");
            containerRegistry.RegisterSingleton<FiveChessViewModel>();

            containerRegistry.RegisterDialog<FetchBackColorView>();
            containerRegistry.RegisterDialogWindow<FetchBackColor>(nameof(FetchBackColor));

            //containerRegistry.RegisterSingleton<FetchBackColorViewModel>();

            containerRegistry.Register<IMouseHook, GlobalMouseHook>();
        }

        private void RegisterNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ColorView>();
            containerRegistry.RegisterForNavigation<ControlsDemoView>();

            containerRegistry.RegisterForNavigation<ToolsView>();


            containerRegistry.RegisterForNavigation<ButtonsView>();
            containerRegistry.RegisterForNavigation<ToggleButtonsView>();
            containerRegistry.RegisterForNavigation<IconFontView>();

            containerRegistry.RegisterForNavigation<PasswordBox>();
            containerRegistry.RegisterForNavigation<RichTextBox>();
            containerRegistry.RegisterForNavigation<TextBox>();

            containerRegistry.RegisterForNavigation<Panels>();
            
            containerRegistry.RegisterForNavigation<Selectors>();
            containerRegistry.RegisterForNavigation<TabControls>();

            containerRegistry.RegisterForNavigation<ItemsControls>();

            containerRegistry.RegisterForNavigation<VirtualizingPanels>();

            containerRegistry.RegisterForNavigation<Pickers>();
        }
    }
}