using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TcpSocket.ViewModels;

namespace TcpSocket.Views
{
    public partial class ImageDisplayView : UserControl
    {
        private ImageDisplayViewModel _context;
        public ImageDisplayView()
        {
            InitializeComponent();

            this._context = this.DataContext as ImageDisplayViewModel;
        }

        private void VirtualUI_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (sender is Grid grid)
            {
                foreach (var item in this._context.ActualData)
                {
                    item.InList = !item.InList;
                }

                foreach (var item in grid.Children.OfType<ListBox>())
                {
                    item.ItemsSource = _context.ActualData;
                }
            }
        }
    }
}