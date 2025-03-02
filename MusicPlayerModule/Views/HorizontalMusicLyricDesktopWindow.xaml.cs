using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using MusicPlayerModule.Contracts;
using MusicPlayerModule.Converters;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicPlayerModule.Views
{
    public partial class HorizontalMusicLyricDesktopWindow : Window
    {
        public HorizontalMusicLyricDesktopWindow(IConfigManager configManager)
        {
            InitializeComponent();

            var bindings = new MultiBinding();
            bindings.Bindings.Add(new Binding("DesktopLyric.IsDesktopLyricShow"));
            bindings.Bindings.Add(new Binding("DesktopLyric.IsVertical"));
            bindings.Converter = new HorizentalDesktopLyricMultiConverter();

            this.SetBinding(Window.VisibilityProperty, bindings);

            this.MaxWidth = SystemParameters.PrimaryScreenWidth;

            this._configManager = configManager;
            var pointStr = configManager.ReadConfigNode<string>(CustomStatics.Horizontal_DesktopLyric_WindowLeftTop_ConfigKey);
            if (!pointStr.IsNullOrBlank())
            {
                var arr = pointStr.Split(",");
                this.Left = double.Parse(arr[0]);
                this.Top = double.Parse(arr[1]);
            }
        }

        private IConfigManager _configManager;
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);

            _configManager.WriteConfigNode(",".Join(new double[] { this.Left, this.Top }), CustomStatics.Horizontal_DesktopLyric_WindowLeftTop_ConfigKey);
        }

        private void DesktopLyricPanel_Visible(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.DesktopLyricPanel.Visibility = Visibility.Visible;
        }

        private void DesktopLyricPanel_Hidden(object sender, MouseEventArgs e)
        {
            e.Handled = true;

            this.DesktopLyricPanel.Visibility = Visibility.Hidden;
        }

        private void DragWindow_MouseDonw(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);

            if (sizeInfo.WidthChanged)
            {
                this.Left = (SystemParameters.WorkArea.Width - sizeInfo.NewSize.Width) / 2;
            }
        }
    }
}
