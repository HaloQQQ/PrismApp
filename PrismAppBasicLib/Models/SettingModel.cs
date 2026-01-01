using IceTea.Pure.BaseModels;
using System.Windows.Input;
using Prism.Commands;
using IceTea.Pure.Extensions;
using System;
using IceTea.Wpf.Atom.Utils;
using System.Windows;

namespace PrismAppBasicLib.Models
{
    public class SettingModel : NotifyBase
    {
        private Window _window = new Window();

        public SettingModel(string name, string value, Action action)
        {
            Name = name + ":";
            _value = value;

            this.OpenFolderCommand = new DelegateCommand(() =>
            {
                if (_window == null)
                {
                    return;
                }

                var folder = WpfAtomUtils.OpenFolderDialog(_window);

                if (!folder.IsNullOrBlank())
                {
                    this.Value = folder;
                }

                action?.Invoke();
            });
        }

        public string Name { get; }

        private string _value;
        public string Value
        {
            get => _value;
            set
            {
                if (!value.IsNullOrBlank())
                {
                    SetProperty(ref _value, value);
                }
            }
        }

        public ICommand OpenFolderCommand { get; private set; }

        protected override void DisposeCore()
        {
            base.DisposeCore();

            _window = null;

            OpenFolderCommand = null;
        }
    }
}
