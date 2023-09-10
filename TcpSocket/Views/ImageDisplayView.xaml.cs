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
                if (grid.Children[0] is ListBox firstListBox
                    && grid.Children[1] is ListBox secondListBox
                )
                {
                    if (firstListBox.ItemsSource == _context.List)
                    {
                        firstListBox.ItemsSource = _context.Block;
                        secondListBox.ItemsSource = _context.List;
                    }
                    else
                    {
                        firstListBox.ItemsSource = _context.List;
                        secondListBox.ItemsSource = _context.Block;
                    }
                }
            }
        }
    }
}