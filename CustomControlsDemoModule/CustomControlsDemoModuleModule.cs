using CustomControlsDemoModule.Views;
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
            _regionManager.RegisterViewWithRegion<ControlsDemoView>("ControlsDemoRegion");

            _regionManager.RegisterViewWithRegion<_2048>("Game2048Region");
        }
    }
}