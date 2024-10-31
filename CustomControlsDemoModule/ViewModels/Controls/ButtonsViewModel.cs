using Prism.Regions;
using System.Diagnostics;

namespace CustomControlsDemoModule.ViewModels.Controls
{
    internal class ButtonsViewModel : INavigationAware
    {
        public bool IsNavigationTarget(NavigationContext navigationContext)
        {
            return true;
        }

        public void OnNavigatedFrom(NavigationContext navigationContext)
        {
            Debug.WriteLine("再来啊老弟");
        }

        public void OnNavigatedTo(NavigationContext navigationContext)
        {
            Debug.WriteLine("来了老弟");
        }
    }
}
