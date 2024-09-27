using System.Windows;
using System.Windows.Controls;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.DataTemplateSelectors
{
    public class ListImageDataTemplateSelector : DataTemplateSelector
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

    public class BlockImageDataTemplateSelector : DataTemplateSelector
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