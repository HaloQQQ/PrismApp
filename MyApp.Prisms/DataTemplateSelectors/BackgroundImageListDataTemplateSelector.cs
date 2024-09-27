using System.Windows;
using System.Windows.Controls;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.DataTemplateSelectors
{
    public class BackgroundImageListDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Empty { get; set; } = null!;
        public DataTemplate ListData { get; set; } = null!;

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            if (item is MyImage { Name: null })
            {
                return this.Empty;
            }

            return this.ListData;
        }
    }
}