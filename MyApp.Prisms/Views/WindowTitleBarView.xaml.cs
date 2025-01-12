using Prism.Events;
using Prism.Ioc;
using System.Windows.Controls;
using System.Windows.Input;
using MyApp.Prisms.MsgEvents;

namespace MyApp.Prisms.Views
{
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
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