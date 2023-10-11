using System.Windows;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.DataTemplateSelector
{
    public class BackgroundImageListDataTemplateSelector : System.Windows.Controls.DataTemplateSelector
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