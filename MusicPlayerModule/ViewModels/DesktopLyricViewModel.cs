using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.General.Extensions;
using MusicPlayerModule.Common;
using Prism.Commands;
using Prism.Mvvm;
using System.Windows.Input;
using System.Windows.Media;

namespace MusicPlayerModule.ViewModels
{
    internal class DesktopLyricViewModel : BindableBase
    {
        public DesktopLyricViewModel(IConfigManager config)
        {
            var currentLyricForeground = config.ReadConfigNode(CustomStatics.CurrentLyricForeground_ConfigKey);
            this.CurrentLyricForeground = this.DefaultLyricForegrounds.FirstOrDefault(f => f.Color.ToString() == currentLyricForeground) ??
                this.DefaultLyricForegrounds.First();

            var currentLyricFontSize = config.ReadConfigNode(CustomStatics.CurrentLyricFontSize_ConfigKey);
            double fontSize = 20;
            if (!currentLyricFontSize.IsNullOrBlank())
            {
                double.TryParse(currentLyricFontSize, out fontSize);
            }
            this.CurrentLyricFontSize = fontSize;


            this.IsDesktopLyricShow = config.IsTrue(CustomStatics.IsDesktopLyricShow_ConfigKey);
            this.IsVertical = config.IsTrue(CustomStatics.IsVertical_ConfigKey);
            this.IsSingleLine = config.IsTrue(CustomStatics.IsSingleLine_ConfigKey);

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.IsDesktopLyricShow, CustomStatics.IsDesktopLyricShow_ConfigKey);

                config.WriteConfigNode(this.IsVertical, CustomStatics.IsVertical_ConfigKey);

                config.WriteConfigNode(this.IsSingleLine, CustomStatics.IsSingleLine_ConfigKey);

                config.WriteConfigNode(this.CurrentLyricForeground.ToString(), CustomStatics.CurrentLyricForeground_ConfigKey);

                config.WriteConfigNode(this.CurrentLyricFontSize, CustomStatics.CurrentLyricFontSize_ConfigKey);
            };

            #region 桌面歌词渐变色起点
            foreach (var item in this.Colors)
            {
                item.PropertyChanged += (sender, e) =>
                {
                    RaisePropertyChanged(nameof(LinearGradientLyricStartColorBrush));
                };
            }

            var colorString = config.ReadConfigNode(CustomStatics.LinearGradientLyricColor_ConfigKey);
            if (!colorString.IsNullOrBlank())
            {
                var color = colorString.GetColorFromString();
                this.Colors[0].Value = color.R;
                this.Colors[1].Value = color.G;
                this.Colors[2].Value = color.B;
            }

            config.SetConfig += config =>
            {
                config.WriteConfigNode(LinearGradientLyricStartColorBrush.ToString(), CustomStatics.LinearGradientLyricColor_ConfigKey);
            };

            this.ResetColorCommand = new DelegateCommand(() =>
            {
                foreach (var item in this.Colors)
                {
                    item.Reset();
                }
            });
            #endregion
        }

        private bool _isDesktopLyricShow;

        public bool IsDesktopLyricShow
        {
            get => this._isDesktopLyricShow;
            set => SetProperty<bool>(ref _isDesktopLyricShow, value);
        }

        private bool _isVertical;

        public bool IsVertical
        {
            get => this._isVertical;
            set => SetProperty<bool>(ref _isVertical, value);
        }

        private bool _isSingleLine;

        public bool IsSingleLine
        {
            get => this._isSingleLine;
            set => SetProperty<bool>(ref _isSingleLine, value);
        }

        public IList<LinearGradientModel> Colors { get; } = new List<LinearGradientModel>
        {
            new LinearGradientModel("R:", 190),
            new LinearGradientModel("G:", 250),
            new LinearGradientModel("B:", 253)
        };

        public ICommand ResetColorCommand { get; }

        public Brush LinearGradientLyricStartColorBrush
        {
            get
            {
                byte red = this.Colors[0].Value;
                byte green = this.Colors[1].Value;
                byte blue = this.Colors[2].Value;

                return new SolidColorBrush(Color.FromRgb(red, green, blue));
            }

            set
            {
                if (value is SolidColorBrush solidColor)
                {
                    var color = solidColor.Color;

                    this.Colors[0].Value = color.R;
                    this.Colors[1].Value = color.G;
                    this.Colors[2].Value = color.B;

                    RaisePropertyChanged(nameof(LinearGradientLyricStartColorBrush));
                }
            }
        }

        public IEnumerable<SolidColorBrush> DefaultLyricForegrounds => new List<SolidColorBrush>()
        {
            "#FD6C6C".GetBrushFromString(),
            "#F2910D".GetBrushFromString(),
            "#FFAF00".GetBrushFromString(),
            "#C0DF4E".GetBrushFromString(),
            "#51DAC9".GetBrushFromString(),
            "#4DB0FF".GetBrushFromString(),
            "#A587F3".GetBrushFromString(),
            "#FF8DBB".GetBrushFromString(),
            "#8C8796".GetBrushFromString(),
            "#00FF7F".GetBrushFromString()
        };

        private SolidColorBrush _currentLyricForeground;

        public SolidColorBrush CurrentLyricForeground
        {
            get => this._currentLyricForeground;
            set => SetProperty<SolidColorBrush>(ref _currentLyricForeground, value);
        }

        private double _currentLyricFontSize;

        public double CurrentLyricFontSize
        {
            get => this._currentLyricFontSize;
            set => SetProperty<double>(ref _currentLyricFontSize, value);
        }
    }

    internal class LinearGradientModel : BindableBase
    {
        public LinearGradientModel(string name, byte defaultValue)
        {
            Name = name;
            Value = defaultValue;
            DefaultValue = defaultValue;
        }

        public string Name { get; }

        private byte _value;

        public byte Value
        {
            get => this._value;
            set => SetProperty<byte>(ref _value, value);
        }

        public byte DefaultValue { get; }

        public void Reset()
        {
            this.Value = this.DefaultValue;
        }
    }
}
