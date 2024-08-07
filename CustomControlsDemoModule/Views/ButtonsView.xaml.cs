using System;
using System.Windows.Controls;

namespace CustomControlsDemoModule.Views
{
    /// <summary>
    /// ButtonsView.xaml 的交互逻辑
    /// </summary>
    public partial class ButtonsView : UserControl
    {
        public ButtonsView()
        {
            InitializeComponent();
        }

        private void UpdateProgressBar_ClickHandler(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ProgressBar.Value = DateTime.Now.Second;
            this.ProgressBar1.Value = DateTime.Now.Second * 6;
        }
    }
}
