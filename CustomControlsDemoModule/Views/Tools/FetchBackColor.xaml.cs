using CustomControlsDemoModule.ViewModels;
using IceTea.Desktop.Extensions;
using Prism.Services.Dialogs;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace CustomControlsDemoModule.Views
{
    /// <summary>
    /// FetchBackColorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class FetchBackColor : Window, IDialogWindow
    {
        public FetchBackColor()
        {
            InitializeComponent();

            ColorExtensions.GetCursorPos(out System.Drawing.Point point);

            this.SetPostion(point);

            if (this.DataContext is FetchBackColorViewModel viewModel)
            {
                viewModel.MouseHook.MouseActivity += ViewModel_MouseActionEvent;

                viewModel.MouseHook.StartAsync();
            }

            Mouse.OverrideCursor = Cursors.Cross;

            Application.Current.MainWindow.Closing += MainWindow_Closing;
        }

        public IDialogResult Result { get; set; }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            Mouse.OverrideCursor = Cursors.Arrow;

            Application.Current.MainWindow.Closing -= MainWindow_Closing;
        }

        private void ViewModel_MouseActionEvent(object sender, IceTea.Desktop.Contracts.MouseHook.CustomMouseEventArgs e)
        {
            if (e.OperationType == IceTea.Desktop.Contracts.MouseHook.MouseOperationType.MOVE)
            {
                this.SetPostion(new System.Drawing.Point(e.X, e.Y));
            }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            this.Owner = null;

        }

        private void SetPostion(System.Drawing.Point point)
        {
            double hOffset = point.X + 6;
            double vOffset = point.Y + 10;

            var screenWidth = SystemParameters.WorkArea.Width;
            var screenHeight = SystemParameters.WorkArea.Height;

            hOffset = Math.Min(screenWidth - ActualWidth, hOffset);
            vOffset = Math.Min(screenHeight - ActualHeight, vOffset);

            this.Left = hOffset;
            this.Top = vOffset;
        }
    }
}
