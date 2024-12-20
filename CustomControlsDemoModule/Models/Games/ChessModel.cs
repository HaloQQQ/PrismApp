using IceTea.Atom.BaseModels;
using IceTea.Atom.Contracts;
using Prism.Commands;
using System;
using System.Diagnostics;
using System.Windows.Input;

namespace CustomControlsDemoModule.Models
{
    [DebuggerDisplay("Row={Row}, Column={Column}, IsWhite={IsWhite}")]
    internal class ChessModel : BaseNotifyModel, IReset
    {
        public ChessModel(int row, int column)
        {
            this.Row = row;
            this.Column = column;

            this.PressCommand = new DelegateCommand<bool?>(isWhiteTurn =>
            {
                IsWhite = isWhiteTurn;

                SwitchEvent?.Invoke(this);
            }, _ => IsWhite is null).ObservesProperty(() => IsWhite);
        }

        public int Row { get; }
        public int Column { get; }

        private bool? _isWhite;
        public bool? IsWhite
        {
            get => _isWhite;
            private set => SetProperty(ref _isWhite, value);
        }

        private bool _isSucceed;
        public bool IsSucceed
        {
            get => _isSucceed;
            private set => SetProperty(ref _isSucceed, value);
        }

        public ICommand PressCommand { get; }

        internal static Action<ChessModel> SwitchEvent;

        public void Success()
        {
            this.IsSucceed = true;
        }

        public bool Reset()
        {
            IsWhite = null;

            IsSucceed = false;

            return true;
        }
    }
}
