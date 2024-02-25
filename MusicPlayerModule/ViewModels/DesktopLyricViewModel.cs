using IceTea.Atom.Contracts;
using Prism.Mvvm;

namespace MusicPlayerModule.ViewModels
{
    internal class DesktopLyricViewModel : BindableBase
    {
        public DesktopLyricViewModel(IConfigManager config)
        {
            var isDesktopLyricShow_ConfigKey = new[] { "Music", "IsDesktopLyricShow" };
            var isVertical_ConfigKey = new[] { "Music", "IsVertical" };
            var isSingleLine_ConfigKey = new[] { "Music", "IsSingleLine" };

            this.IsDesktopLyricShow = config.IsTrue(isDesktopLyricShow_ConfigKey);
            this.IsVertical = config.IsTrue(isVertical_ConfigKey);
            this.IsSingleLine = config.IsTrue(isSingleLine_ConfigKey);

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.IsDesktopLyricShow, isDesktopLyricShow_ConfigKey);

                config.WriteConfigNode(this.IsVertical, isVertical_ConfigKey);

                config.WriteConfigNode(this.IsSingleLine, isSingleLine_ConfigKey);
            };
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
    }
}
