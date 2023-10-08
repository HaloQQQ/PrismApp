using MusicPlayerModule.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace MusicPlayerModule.Models
{
    enum MusicClassifyType
    {
        Album,
        Singer,
        Dir
    }

    internal class MusicWithClassifyModel : BindableBase
    {
        public MusicClassifyType ClassifyType { get; }

        public MusicWithClassifyModel(string classifyKey, Collection<FavoriteMusicViewModel> displayByClassifyKeyFavorites, MusicClassifyType musicClassifyType)
        {
            ClassifyKey = classifyKey;
            DisplayByClassifyKeyFavorites = displayByClassifyKeyFavorites;

            ClassifyType = musicClassifyType;
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => this._isSelected;
            set
            {
                SetProperty<bool>(ref _isSelected, value);

                if (!value)
                {
                    SelectStatusChanged?.Invoke(value);
                }
            }
        }

        public static event Action<bool> SelectStatusChanged;

        public string ClassifyKey { get; private set; }

        public Collection<FavoriteMusicViewModel> DisplayByClassifyKeyFavorites { get; private set; }
    }
}
