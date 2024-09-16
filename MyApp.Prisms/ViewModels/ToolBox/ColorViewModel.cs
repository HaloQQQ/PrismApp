using IceTea.Atom.BaseModels;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.Wpf.Atom.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace MyApp.Prisms.ViewModels.ToolBox
{
    internal class ColorViewModel : BaseNotifyModel
    {
        private string _colorCode;

        public ColorViewModel()
        {
            this.ColorCode = "#5542b983";
        }

        private string _contrastColorCode;

        public string ContrastColorCode
        {
            get => this._contrastColorCode;
            private set
            {
                if (SetProperty<string>(ref _contrastColorCode, value))
                {
                    RaisePropertyChanged(nameof(ContrastColor));
                }
            }
        }

        public Brush ContrastColor => new SolidColorBrush(Color.FromArgb((byte)(255 - this._aInt), (byte)(255 - this._rInt), (byte)(255 - this._gInt), (byte)(255 - this._bInt)));

        public string ColorCode
        {
            get => this._colorCode;
            set
            {
                this._colorCode = value;
                if (value.IsNullOrBlank())
                {
                    return;
                }

                var code = value.EnsureStartsWith("#");

                if (code.Length != 7 && code.Length != 9)
                {
                    return;
                }

                var data = code.Substring(1);

                if (!data.IsHexString())
                {
                    return;
                }

                if (code.Length == 7)
                {
                    data = "FF" + data;
                }

                this.AInt = byte.Parse(data.Substring(0, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                this.RInt = byte.Parse(data.Substring(2, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                this.GInt = byte.Parse(data.Substring(4, 2), System.Globalization.NumberStyles.HexNumber).ToString();
                this.BInt = byte.Parse(data.Substring(6, 2), System.Globalization.NumberStyles.HexNumber).ToString();

                SetProperty<string>(ref _colorCode, code);
                RaisePropertyChanged(nameof(Background));

                string result = string.Empty;
                for (int i = 1; i < code.Length; i += 2)
                {
                    result += (255 - Convert.ToByte(code.Substring(i, 2), 16)).ToString("X2");
                }

                this.ContrastColorCode = result.EnsureStartsWith("#");
            }
        }

        private short GetValue(string value)
        {
            if (!short.TryParse(value, out short v))
            {
                return 0;
            }

            v = Math.Min(v, byte.MaxValue);
            v = Math.Max(v, byte.MinValue);

            return v;
        }

        private string ArgbHex
            => "#" + this._aInt.ToString("X2") + this._rInt.ToString("X2") + this._gInt.ToString("X2") + this._bInt.ToString("X2");

        private byte _aInt;

        public string AInt
        {
            get => this._aInt.ToString();
            set
            {
                var v = this.GetValue(value);

                SetProperty<byte>(ref _aInt, (byte)v);

                this._colorCode = this.ArgbHex;
                RaisePropertyChanged(nameof(ColorCode));
                RaisePropertyChanged(nameof(Background));
            }
        }

        private byte _rInt;

        public string RInt
        {
            get => this._rInt.ToString();
            set
            {
                var v = this.GetValue(value);

                SetProperty<byte>(ref _rInt, (byte)v);

                this._colorCode = this.ArgbHex;
                RaisePropertyChanged(nameof(ColorCode));
                RaisePropertyChanged(nameof(Background));
            }
        }

        private byte _gInt;

        public string GInt
        {
            get => this._gInt.ToString();
            set
            {
                var v = this.GetValue(value);

                SetProperty<byte>(ref _gInt, (byte)v);

                this._colorCode = this.ArgbHex;
                RaisePropertyChanged(nameof(ColorCode));
                RaisePropertyChanged(nameof(Background));
            }
        }

        private byte _bInt;

        public string BInt
        {
            get => this._bInt.ToString();
            set
            {
                var v = this.GetValue(value);

                SetProperty<byte>(ref _bInt, (byte)v);

                this._colorCode = this.ArgbHex;
                RaisePropertyChanged(nameof(ColorCode));
                RaisePropertyChanged(nameof(Background));
            }
        }

        public Brush Background => this.ColorCode.GetBrushFromString();

        public IEnumerable<ColorModel> Colors
        {
            get
            {
                var list = new List<string>
                {
                    "#FFB6C1",
                    "#FFC0CB",
                    "#DC143C" ,
                    "#FFF0F5" ,
                    "#DB7093" ,
                    "#FF69B4" ,
                    "#FF1493" ,
                    "#C71585" ,
                    "#DA70D6" ,
                    "#D8BFD8" ,
                    "#DDA0DD" ,
                    "#EE82EE" ,
                    "#FF00FF" ,
                    "#FF00FF" ,
                    "#8B008B" ,
                    "#800080" ,
                    "#BA55D3" ,
                    "#9932CC" ,
                    "#4B0082" ,
                    "#8A2BE2" ,
                    "#9370DB" ,
                    "#7B68EE" ,
                    "#6A5ACD" ,
                    "#483D8B" ,
                    "#E6E6FA" ,
                    "#F8F8FF" ,
                    "#0000FF" ,
                    "#0000CD" ,
                    "#191970" ,
                    "#00008B" ,
                    "#000080" ,
                    "#4169E1" ,
                    "#6495ED" ,
                    "#B0C4DE" ,
                    "#778899" ,
                    "#708090" ,
                    "#1E90FF" ,
                    "#F0F8FF" ,
                    "#4682B4" ,
                    "#87CEFA" ,
                    "#87CEEB" ,
                    "#00BFFF" ,
                    "#ADD8E6" ,
                    "#B0E0E6" ,
                    "#5F9EA0" ,
                    "#F0FFFF" ,
                    "#E1FFFF" ,
                    "#AFEEEE" ,
                    "#00FFFF" ,
                    "#D4F2E7" ,
                    "#00CED1" ,
                    "#2F4F4F" ,
                    "#008B8B" ,
                    "#008080" ,
                    "#48D1CC" ,
                    "#20B2AA" ,
                    "#40E0D0" ,
                    "#7FFFAA" ,
                    "#00FA9A" ,
                    "#00FF7F" ,
                    "#F5FFFA" ,
                    "#3CB371" ,
                    "#2E8B57" ,
                    "#F0FFF0" ,
                    "#90EE90" ,
                    "#98FB98" ,
                    "#8FBC8F" ,
                    "#32CD32" ,
                    "#00FF00" ,
                    "#228B22" ,
                    "#008000" ,
                    "#006400" ,
                    "#7FFF00" ,
                    "#7CFC00" ,
                    "#ADFF2F" ,
                    "#556B2F" ,
                    "#F5F5DC" ,
                    "#FAFAD2" ,
                    "#FFFFF0" ,
                    "#FFFFE0" ,
                    "#FFFF00" ,
                    "#808000" ,
                    "#BDB76B" ,
                    "#FFFACD" ,
                    "#EEE8AA" ,
                    "#F0E68C" ,
                    "#FFD700" ,
                    "#FFF8DC" ,
                    "#DAA520" ,
                    "#FFFAF0" ,
                    "#FDF5E6" ,
                    "#F5DEB3" ,
                    "#FFE4B5" ,
                    "#FFA500" ,
                    "#FFEFD5" ,
                    "#FFEBCD" ,
                    "#FFDEAD" ,
                    "#FAEBD7" ,
                    "#D2B48C" ,
                    "#DEB887" ,
                    "#FFE4C4" ,
                    "#FF8C00" ,
                    "#FAF0E6" ,
                    "#CD853F" ,
                    "#FFDAB9" ,
                    "#F4A460" ,
                    "#D2691E" ,
                    "#8B4513" ,
                    "#FFF5EE" ,
                    "#A0522D" ,
                    "#FFA07A" ,
                    "#FF7F50" ,
                    "#FF4500" ,
                    "#E9967A" ,
                    "#FF6347" ,
                    "#FFE4E1" ,
                    "#FA8072" ,
                    "#FFFAFA" ,
                    "#F08080" ,
                    "#BC8F8F" ,
                    "#CD5C5C" ,
                    "#FF0000" ,
                    "#A52A2A" ,
                    "#B22222" ,
                    "#8B0000" ,
                    "#800000" ,
                    "#FFFFFF" ,
                    "#F5F5F5" ,
                    "#DCDCDC" ,
                    "#D3D3D3" ,
                    "#C0C0C0" ,
                    "#A9A9A9" ,
                    "#808080" ,
                    "#696969" ,
                    "#000000"
            };

                return list.Select(str => new ColorModel(str));
            }
        }
    }

    internal class ColorModel
    {
        public ColorModel(string colorCode)
        {
            AppUtils.Assert(colorCode.IsNotNullAnd(c => c.StartsWith("#") && c.Substring(1).IsHexString() && c.Length == 7), "数据不符合要求");
            this.ColorCode = colorCode;

            var data = colorCode.Substring(1);
            Color color = colorCode.GetColorFromString();

            byte[] arr = data.GetBytesFromHex();

            this.Background = colorCode.GetBrushFromString();

            this.Foreground = new SolidColorBrush(Color.FromRgb((byte)(255 - arr[0]), (byte)(255 - arr[1]), (byte)(255 - arr[2])));
        }

        public string ColorCode { get; }

        public Brush Foreground { get; }
        public Brush Background { get; }
    }
}
