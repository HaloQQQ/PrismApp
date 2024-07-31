using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
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

            string currentFontFamily = config.ReadConfigNode(CustomStatics.CurrentLyricFontFamily_ConfigKey);

            if (currentFontFamily.IsNullOrBlank())
            {
                this.CurrentFontModel = this.Fonts.First();
            }
            else
            {
                this.CurrentFontModel = this.Fonts.FirstOrDefault(f => f.DisplayName.EqualsIgnoreCase(currentFontFamily));
            }

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.IsDesktopLyricShow, CustomStatics.IsDesktopLyricShow_ConfigKey);

                config.WriteConfigNode(this.IsVertical, CustomStatics.IsVertical_ConfigKey);

                config.WriteConfigNode(this.IsSingleLine, CustomStatics.IsSingleLine_ConfigKey);

                config.WriteConfigNode(this.CurrentLyricForeground.ColorBrush.ToString(), CustomStatics.CurrentLyricForeground_ConfigKey);

                config.WriteConfigNode(this.CurrentLyricFontSize, CustomStatics.CurrentLyricFontSize_ConfigKey);

                config.WriteConfigNode(this.CurrentFontModel.DisplayName, CustomStatics.CurrentLyricFontFamily_ConfigKey);
            };

            this.LinearGradientColorBrush = new ColorModel(config, CustomStatics.LinearGradientLyricColor_ConfigKey, 190, 250, 253);

            var currentLyricForeground = config.ReadConfigNode(CustomStatics.CurrentLyricForeground_ConfigKey);

            var colorBrush = currentLyricForeground.IsNullOrBlank()
                                ? this.DefaultLyricForegrounds.First().ColorBrush
                                : currentLyricForeground.GetBrushFromString();

            var color = ((SolidColorBrush)colorBrush).Color;
            this.LyricColorBrush = new ColorModel(config, CustomStatics.CurrentLyricForeground_ConfigKey, color.R, color.G, color.B);

            void SetCurrentLyric(Brush brush)
            {
                SelectableColorBrush.Default.ColorBrush = brush;
                this.CurrentLyricForeground = SelectableColorBrush.Default;
            }

            this.LyricColorBrush.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(ColorModel.ColorBrush))
                {
                    SetCurrentLyric(this.LyricColorBrush.ColorBrush);
                }
            };

            SetCurrentLyric(colorBrush);

            this.SelectLyricColorCommand = new DelegateCommand(() => SetCurrentLyric(this.LyricColorBrush.ColorBrush))
                                            .ObservesCanExecute(() => IsUnSelected)
                                            .ObservesProperty(() => this.LyricColorBrush.IsSelected);
        }

        public bool IsUnSelected => !this.LyricColorBrush.IsSelected;

        /// <summary>
        /// 桌面歌词前景渐变色
        /// </summary>
        public ColorModel LinearGradientColorBrush { get; }

        /// <summary>
        /// 桌面歌词非渐变色
        /// </summary>
        public ColorModel LyricColorBrush { get; }

        /// <summary>
        /// 设置自定义颜色为当前非渐变色
        /// </summary>
        public ICommand SelectLyricColorCommand { get; }

        #region 歌词颜色
        public IEnumerable<SelectableColorBrush> DefaultLyricForegrounds { get; } = new List<SelectableColorBrush>()
        {
            new SelectableColorBrush("#FD6C6C".GetBrushFromString()),
            new SelectableColorBrush("#F2910D".GetBrushFromString()),
            new SelectableColorBrush("#FFAF00".GetBrushFromString()),
            new SelectableColorBrush("#C0DF4E".GetBrushFromString()),
            new SelectableColorBrush("#51DAC9".GetBrushFromString()),
            new SelectableColorBrush("#4DB0FF".GetBrushFromString()),
            new SelectableColorBrush("#A587F3".GetBrushFromString()),
            new SelectableColorBrush("#FF8DBB".GetBrushFromString()),
            new SelectableColorBrush("#8C8796".GetBrushFromString()),
            new SelectableColorBrush("#00FF7F".GetBrushFromString())
        };

        private SelectableColorBrush _currentLyricForeground;

        public SelectableColorBrush CurrentLyricForeground
        {
            get => this._currentLyricForeground;
            set
            {
                if (value == null)
                {
                    return;
                }

                SetProperty<SelectableColorBrush>(ref _currentLyricForeground, value);
                {
                    var colorStr = _currentLyricForeground.ColorBrush.ToString();
                    this.LyricColorBrush.IsSelected = this.LyricColorBrush.ColorBrush.ToString() == colorStr;

                    this.DefaultLyricForegrounds.ForEach(item =>
                    {
                        item.Selected = item.ColorBrush.ToString() == colorStr;
                    });
                }
            }
        }
        #endregion

        #region 歌词字体
        public IEnumerable<FontModel> Fonts { get; } =
        [
          new FontModel("微软雅黑", "Microsoft YaHei"),
          new FontModel("宋体", "SimSun"),
          new FontModel("黑体", "SimHei"),
          new FontModel("微软正黑", "Microsoft JhengHei"),
          new FontModel("楷体", "KaiTi"),
          new FontModel("细明体", "MingLiU")
        ];

        private FontModel _fontModel;
        public FontModel CurrentFontModel
        {
            get => _fontModel;
            set { SetProperty(ref _fontModel, value); }
        }

        #endregion

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

        private double _currentLyricFontSize;

        public double CurrentLyricFontSize
        {
            get => this._currentLyricFontSize;
            set => SetProperty<double>(ref _currentLyricFontSize, value);
        }
    }

    internal class FontModel
    {
        public FontModel(string displayName, string familyName)
        {
            DisplayName = displayName.AssertNotNull(nameof(displayName));
            FamilyName = familyName.AssertNotNull(nameof(familyName));
            Family = new FontFamily(familyName);
        }

        public FontFamily Family { get; }
        public string DisplayName { get; }
        public string FamilyName { get; }
    }

    internal class SelectableColorBrush : BindableBase
    {
        public static SelectableColorBrush Default = new SelectableColorBrush(new SolidColorBrush(Color.FromRgb(0, 0, 0)));

        public SelectableColorBrush(Brush colorBrush)
        {
            ColorBrush = colorBrush;
        }

        private Brush _colorBrush;

        public Brush ColorBrush
        {
            get => this._colorBrush;
            set => SetProperty<Brush>(ref _colorBrush, value);
        }

        private bool _selected;

        public bool Selected
        {
            get => this._selected;
            set => SetProperty<bool>(ref _selected, value);
        }
    }

    internal class ColorModel : BindableBase
    {
        public ColorModel(IConfigManager config, string[] configKeys, byte r = 0, byte g = 0, byte b = 0)
        {
            this.Colors = new List<ThreePrimaryColorModel>
            {
                new ThreePrimaryColorModel("R:", r),
                new ThreePrimaryColorModel("G:", g),
                new ThreePrimaryColorModel("B:", b)
            };

            foreach (var item in this.Colors)
            {
                item.PropertyChanged += (sender, e) =>
                {
                    RaisePropertyChanged(nameof(ColorBrush));
                };
            }

            var colorString = config.ReadConfigNode(configKeys);
            if (!colorString.IsNullOrBlank())
            {
                var color = colorString.GetColorFromString();
                this.Colors[0].Value = color.R;
                this.Colors[1].Value = color.G;
                this.Colors[2].Value = color.B;
            }

            config.PreSetConfig += config =>
            {
                config.WriteConfigNode(ColorBrush.ToString(), configKeys);
            };

            this.ResetColorCommand = new DelegateCommand(() =>
            {
                foreach (var item in this.Colors)
                {
                    item.Reset();
                }
            });
        }

        public IList<ThreePrimaryColorModel> Colors { get; }

        public ICommand ResetColorCommand { get; }

        public Brush ColorBrush
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

                    RaisePropertyChanged(nameof(ColorBrush));
                }
            }
        }


        private bool _isSelected;

        public bool IsSelected
        {
            get => this._isSelected;
            set => SetProperty<bool>(ref _isSelected, value);
        }
    }

    internal class ThreePrimaryColorModel : BindableBase
    {
        public ThreePrimaryColorModel(string name, byte defaultValue)
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
