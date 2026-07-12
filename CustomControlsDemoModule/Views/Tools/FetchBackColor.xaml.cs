using CustomControlsDemoModule.ViewModels;
using IceTea.Desktop.Extensions;
using Prism.Services.Dialogs;
using System;
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

            Mouse.OverrideCursor = Cursors.Cross;

            this.Loaded += (s, e) =>
            {
                if (this.DataContext is FetchBackColorViewModel viewModel)
                {
                    viewModel.MouseHook.Activity += ViewModel_MouseActionEvent;

                    viewModel.MouseHook.StartAsync();
                }
            };
        }

        public IDialogResult Result { get; set; }

        protected override void OnClosed(EventArgs e)
        {
            if (this.DataContext is FetchBackColorViewModel viewModel)
            {
                viewModel.MouseHook.Activity -= ViewModel_MouseActionEvent;
            }

            Mouse.OverrideCursor = Cursors.Arrow;

            base.OnClosed(e);
        }

        private void ViewModel_MouseActionEvent(object sender, IceTea.Desktop.Contracts.MouseHook.CustomMouseEventArgs e)
        {
            if (e.OperationType == IceTea.Desktop.Contracts.MouseHook.MouseOperationType.MOVE)
            {
                this.SetPostion(new System.Drawing.Point(e.X, e.Y));
            }
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
