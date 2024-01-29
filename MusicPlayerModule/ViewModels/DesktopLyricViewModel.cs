using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using Prism.Mvvm;
using System.Windows;
using System.Windows.Threading;

namespace MusicPlayerModule.ViewModels
{
    internal class DesktopLyricViewModel : BindableBase
    {
        public DesktopLyricViewModel(IConfigManager config)
        {
            var isDesktopLyricShow_ConfigKey = new[] { "Music", "IsDesktopLyricShow" };
            var isVertical_ConfigKey = new[] { "Music", "IsVertical" };

            this.IsDesktopLyricShow = config.IsTrue(isDesktopLyricShow_ConfigKey);
            this.IsVertical = config.IsTrue(isVertical_ConfigKey);

            config.SetConfig += config =>
            {
                config.WriteConfigNode(this.IsDesktopLyricShow, isDesktopLyricShow_ConfigKey);

                config.WriteConfigNode(this.IsVertical, isVertical_ConfigKey);
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
    }
}
