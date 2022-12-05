using System.Windows;
using TcpSocket.Models;

namespace TcpSocket.DataTemplateSelector
{
    public class BackgroundImageBlockDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public DataTemplate Empty { get; set; } = null!;
        public DataTemplate BlockData { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MyImage {Name: null})
            {
                return this.Empty;
            }

            return this.BlockData;
        }
    }
}