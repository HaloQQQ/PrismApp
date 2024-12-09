using CustomControlsDemoModule.Events;
using IceTea.Atom.BaseModels;
using IceTea.Desktop.Contracts.MouseHook;
using IceTea.Desktop.Extensions;
using IceTea.Desktop.Utils;
using IceTea.Wpf.Atom.Extensions;
using Prism.Events;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Media.Imaging;

namespace CustomControlsDemoModule.ViewModels
{
    internal class FetchBackColorViewModel : BaseNotifyModel, IDisposable, IDialogAware
    {
        internal IMouseHook _mouseHook;

        internal event EventHandler<CustomMouseEventArgs> MouseActionEvent
        {
            add
            {
                if (_mouseHook != null)
                {
                    _mouseHook.MouseActivity += value;
                }
            }

            remove
            {
                if (_mouseHook != null)
                {
                    _mouseHook.MouseActivity -= value;
                }
            }
        }

        public FetchBackColorViewModel(IEventAggregator eventAggregator)
        {
            _mouseHook = new GlobalMouseHook();

            _mouseHook.MouseActivity += _mouseHook_MouseActivity;

            _mouseHook.Start();

            Width = SystemParameters.WorkArea.Width;
            Height = SystemParameters.WorkArea.Height;

            Bitmap memoryImage = IceTea.Core.Extensions.ImageExtensions.CaptureScreen(new Rectangle(0, 0, (int)Width, (int)Height));

            this.BackgroundImage = memoryImage.GetImageSource();

            eventAggregator.GetEvent<ColorPickerEvent>().Subscribe(() => RequestClose?.Invoke(null));
        }

        private void _mouseHook_MouseActivity(object sender, CustomMouseEventArgs e)
        {
            if (e.OperationType == MouseOperationType.MOVE)
            {
                var point = new System.Drawing.Point(e.X, e.Y);

                var color = ColorExtensions.GetColorUnderCursor();

                this.X = point.X;
                this.Y = point.Y;

                this.Color = color.GetStringFromColor();

                this.R = color.R;
                this.G = color.G;
                this.B = color.B;
            }
        }

        #region Props
        public double Width { get; }
        public double Height { get; }

        private BitmapImage _backgroundImage;
        public BitmapImage BackgroundImage
        {
            get => _backgroundImage;
            private set => SetProperty(ref _backgroundImage, value);
        }

        private double _x;
        public double X
        {
            get => _x;
            private set => SetProperty(ref _x, value);
        }

        private double _y;
        public double Y
        {
            get => _y;
            private set => SetProperty(ref _y, value);
        }

        private string _color;
        public string Color
        {
            get => _color;
            private set => SetProperty(ref _color, value);
        }

        private byte _r;
        public byte R
        {
            get => _r;
            private set => SetProperty(ref _r, value);
        }

        private byte _g;
        public byte G
        {
            get => _g;
            private set => SetProperty(ref _g, value);
        }

        private byte _b;

        public byte B
        {
            get => _b;
            private set => SetProperty(ref _b, value);
        }
        #endregion

        public event Action<IDialogResult> RequestClose;

        public string Title => "标题";

        ~FetchBackColorViewModel()
        {
            this.Dispose();
        }

        public void Dispose()
        {
            _mouseHook.Dispose();
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            this.Dispose();
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }
    }
}
