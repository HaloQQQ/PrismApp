using System.Windows;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.DataTemplateSelector
{
    public class ListImageDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public DataTemplate List { get; set; } = null!;
        public DataTemplate Block { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MyImage image && !image.InList)
            {
                return this.Block;
            }

            return this.List;
        }
    }

    public class BlockImageDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
    {
        public DataTemplate List { get; set; } = null!;
        public DataTemplate Block { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MyImage image && image.InList)
            {
                return this.Block;
            }

            return this.List;
        }
    }
}