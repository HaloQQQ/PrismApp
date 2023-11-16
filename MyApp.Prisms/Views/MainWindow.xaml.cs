using System.Windows;
using System.Windows.Controls;

namespace MyApp.Prisms.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (e.Source is TabControl tabControl)
            {
                var item = tabControl.SelectedItem as TabItem;

                this.Title = item.Header?.ToString();

                if (item.Header?.ToString() == "通讯工具")
                {
                    if (item.Content == null)
                    {
                        item.Content = new CommunicationView();
                    }
                }
                else if (item.Header?.ToString() == "进程服务")
                {
                    if (item.Content == null)
                    {
                        item.Content = new ProcessServiceView();
                    }
                }
            }
        }
    }
}