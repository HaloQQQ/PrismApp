using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MyApp.Prisms.Helper;
using MyApp.Prisms.ViewModels;
using IceTea.Wpf.Core.Extensions;

namespace MyApp.Prisms.Views
{
    public partial class MainWindow : Window
    {
        private void Initialize()
        {
        }

        private readonly SoftwareViewModel _softwareContext;

        public MainWindow()
        {
            InitializeComponent();

            this._softwareContext = this.DataContext as SoftwareViewModel;

            this.Initialize();
        }

        private void UpdateScroll(System.Windows.Controls.TabControl tabControl, int index)
        {
            var child = tabControl.GetVisualChildObject<ScrollViewer>();

            // 如果找到了 ScrollViewer，将其滚动到选中项的位置
            if (child is ScrollViewer scrollViewer)
            {
                if (tabControl.Items.Count > 0)
                {
                    scrollViewer.ScrollToVerticalOffset(index * scrollViewer.ExtentHeight / tabControl.Items.Count);
                }
            }
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (e.Source is System.Windows.Controls.TabControl tabControl)
            {
                var item = tabControl.SelectedItem as TabItem;

                this._softwareContext.Title = item.Header.ToString();

                if (item.Header.ToString() == "通讯工具")
                {
                    if (item.Content == null)
                    {
                        item.Content = new CommunicationView();
                    }

                    UpdateScroll(tabControl, 0);
                }
                else if (item.Header.ToString() == "图片列表")
                {
                    if (item.Content == null)
                    {
                        item.Content = new ImageDisplayView();
                    }

                    UpdateScroll(tabControl, 1);
                }
                else if (item.Header.ToString() == "进程服务")
                {
                    if (item.Content == null)
                    {
                        item.Content = new ProcessServiceView();
                    }
                    UpdateScroll(tabControl, 2);
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //将装饰器添加到窗口的Content控件上
            var c = this.Content as UIElement;
            var layer = AdornerLayer.GetAdornerLayer(c);
            layer.Add(new WindowResizeAdorner(c));
        }
    }
}