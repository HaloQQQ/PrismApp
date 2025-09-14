using IceTea.Pure.BaseModels;
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models.Games.ChineseChess
{
    interface IChessCommand : IDisposable
    {
        public int FromRow { get; }
        public int FromColumn { get; }

        public int ToRow { get; }
        public int ToColumn { get; }

        IChessCommand Forward(IList<ChineseChessModel> datas);

        void Back(IList<ChineseChessModel> datas);
    }

    internal class ChinChessCommand : DisposableBase, IChessCommand
    {
        public ChinChessCommand(int index, bool isRed, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            Index = index;

            FromRow = fromRow;
            FromColumn = fromColumn;
            ToRow = toRow;
            ToColumn = toColumn;
            IsRed = isRed;
        }

        public int Index { get; }
        public bool IsRed { get; }

        public int FromRow { get; }
        public int FromColumn { get; }

        private InnerChineseChessModel _fromData;

        public int ToRow { get; }
        public int ToColumn { get; }

        private InnerChineseChessModel _toData;

        public IChessCommand Forward(IList<ChineseChessModel> datas)
        {
            this.CheckDispose();

            var from = datas[GetIndex(FromRow, FromColumn)];
            _fromData = from.Data;

            var to = datas[GetIndex(ToRow, ToColumn)];
            _toData = to.Data;

            to.Data = _fromData;
            from.Data = InnerChineseChessModel.Empty;

            return this;
        }

        public void Back(IList<ChineseChessModel> datas)
        {
            this.CheckDispose();

            var from = datas[GetIndex(ToRow, ToColumn)];
            from.Data = _toData;

            var to = datas[GetIndex(FromRow, FromColumn)];
            to.Data = _fromData;

            this.Dispose();
        }

        private int GetIndex(int row, int column) => row * 9 + column;

        protected override void DisposeCore()
        {
            this._fromData = null;
            this._toData = null;

            base.DisposeCore();
        }
    }
}
