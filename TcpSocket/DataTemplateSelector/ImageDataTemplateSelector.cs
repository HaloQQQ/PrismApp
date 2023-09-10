using System.Windows;
using TcpSocket.ViewModels;

namespace TcpSocket.DataTemplateSelector
{
    public class ImageDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public DataTemplate List { get; set; } = null!;
        public DataTemplate Block { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item.GetType() == typeof(MyImageBlock))
            {
                return this.Block;
            }

            return this.List;
        }
    }
}