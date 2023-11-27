using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using SqlCreatorModule.Views;

namespace SqlCreatorModule
{
    public class SqlCreatorModule : IModule
    {
        private readonly IRegionManager _regionManager;

        public SqlCreatorModule(IRegionManager regionManager)
        {
            _regionManager = regionManager;
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
        }

        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            _regionManager.RegisterViewWithRegion<CreateModelView>(nameof(CreateModelView));
        }
    }
}
