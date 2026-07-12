using Prism.Events;
using Prism.Ioc;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.MsgEvents;

namespace MyApp.Prisms.Views
{
    public partial class WindowTitleBarView : UserControl
    {
        public WindowTitleBarView()
        {
            InitializeComponent();
        }

        #region 更换主题、背景
        private void SwitchBackSliderMoveOut_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();
            eventAggregator.GetEvent<BackgroundImageSelectorShowEvent>().Publish();
        }
        #endregion
    }
}