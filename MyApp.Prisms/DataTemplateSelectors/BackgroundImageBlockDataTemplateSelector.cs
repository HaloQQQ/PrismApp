using System.Windows;
using System.Windows.Controls;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.DataTemplateSelectors
{
    public class BackgroundImageBlockDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Empty { get; set; } = null!;
        public DataTemplate BlockData { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MyImage image && image.Name is null)
            {
                return this.Empty;
            }

            return this.BlockData;
        }
    }
}