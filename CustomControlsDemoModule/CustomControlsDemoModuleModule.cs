using CustomControlsDemoModule.ViewModels;
using CustomControlsDemoModule.Views;
using CustomControlsDemoModule.Views.Controls;
using CustomControlsDemoModule.Views.Controls.TextBoxes;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

namespace CustomControlsDemoModule
{
    public class CustomControlsDemoModuleModule : IModule
    {
        private IRegionManager _regionManager;

        public CustomControlsDemoModuleModule(IRegionManager regionManager)
        {
             this._regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            //_regionManager.RegisterViewWithRegion<ControlsDemoView>("ControlsDemoRegion");

            //_regionManager.RegisterViewWithRegion<ColorView>("ColorRegion");

            this.RegisterNavigation(containerRegistry);

            containerRegistry.RegisterDialog<_2048>();
            containerRegistry.RegisterSingleton<_2048ViewModel>();

            containerRegistry.RegisterDialog<FiveChessView>("五子棋");
            containerRegistry.RegisterSingleton<FiveChessViewModel>();
            
            containerRegistry.RegisterDialog<ChineseChessView>("象棋");
            containerRegistry.RegisterSingleton<ChineseChessViewModel>();

            containerRegistry.RegisterDialog<FetchBackColorView>();
            containerRegistry.RegisterDialogWindow<FetchBackColor>(nameof(FetchBackColor));

            containerRegistry.RegisterSingleton<FetchBackColorViewModel>();
        }

        private void RegisterNavigation(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterForNavigation<ColorView>();
            containerRegistry.RegisterForNavigation<ControlsDemoView>();


            containerRegistry.RegisterForNavigation<ButtonsView>();
            containerRegistry.RegisterForNavigation<ToggleButtonsView>();

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