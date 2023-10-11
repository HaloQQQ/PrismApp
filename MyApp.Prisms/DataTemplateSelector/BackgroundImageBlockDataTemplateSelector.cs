using System.Windows;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.DataTemplateSelector
{
    public class BackgroundImageBlockDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
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