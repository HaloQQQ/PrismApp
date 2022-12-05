using System.Windows.Controls;
using System.Windows.Input;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Function
{
    public partial class VirtualUI : UserControl
    {
        public VirtualUI()
        {
            InitializeComponent();
        }

        private void VirtualUI_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (this.DataContext is ImagesContext context && sender is Grid grid)
            {
                if (grid.Children[0] is ListBox firstListBox
                    && grid.Children[1] is ListBox secondListBox
                )
                {
                    if (firstListBox.ItemsSource == context.List)
                    {
                        firstListBox.ItemsSource = context.Block;
                        secondListBox.ItemsSource = context.List;
                    }
                    else
                    {
                        firstListBox.ItemsSource = context.List;
                        secondListBox.ItemsSource = context.Block;
                    }
                }
            }
        }
    }
}