using MusicPlayerModule.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;

namespace MusicPlayerModule.Models
{
    internal class MusicWithClassifyModel : BindableBase
    {
        public MusicWithClassifyModel(string classifyKey, Collection<FavoriteMusicViewModel> displayByAlbumFavorites)
        {
            ClassifyKey = classifyKey;
            DisplayByClassifyKeyFavorites = displayByAlbumFavorites;
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
