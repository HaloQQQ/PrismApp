using Prism.Events;
using System.Windows.Controls;
using System.Windows.Input;
using Prism.Ioc;
using MusicPlayerModule.MsgEvents.Music;

namespace MusicPlayerModule.Views
{
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602 // 解引用可能出现空引用。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    public partial class MusicPlayer : UserControl
    {
        public MusicPlayer()
        {
            InitializeComponent();
        }

        private void UserControl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var handled = true;

            if (e.Command == ApplicationCommands.Find)
            {
                switch (e.Parameter?.ToString())
                {
                    case "Favorite":
                        ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<FocusFavoriteKeyWordTextBoxEvent>().Publish();
                        break;
                    default:
                        throw new NotImplementedException();
                }

            }
            else if (e.Command == NavigationCommands.GoToPage)
            {
                ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<LyricPageSlideEvent>().Publish();
            }
            else if (e.Command == ApplicationCommands.Open)
            {
                ContainerLocator.Current.Resolve<IEventAggregator>().GetEvent<PlayListPanelOpenEvent>().Publish();
            }
            else
            {
                handled = false;
            }

            e.Handled = handled;
        }
    }
}