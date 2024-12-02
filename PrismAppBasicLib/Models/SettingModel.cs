using IceTea.Atom.BaseModels;
using System.Windows.Input;
using Prism.Commands;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Extensions;
using System;

namespace PrismAppBasicLib.Models
{
    public class SettingModel : BaseNotifyModel
    {
        public SettingModel(string name, string value, Action action)
        {
            Name = name + ":";
            _value = value;

            this.OpenFolderCommand = new DelegateCommand(() =>
            {
                var folder = CommonCoreUtils.OpenFolderDialog(this.Value);

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

        public ICommand OpenFolderCommand { get; }
    }
}
