using IceTea.Pure.BaseModels;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using ImTools;
using System.Collections.Generic;
using System.Diagnostics;

namespace CustomControlsDemoModule.Models
{
    internal enum ChessType
    {
        車,
        馬,
        相,
        仕,
        帥,
        炮,
        兵
    }

    [DebuggerDisplay("IsRed={Data.IsRed}, Type={Data.Type}")]
    internal class ChineseChessModel : NotifyBase, IChineseChess
    {
        public int Row { get; }
        public int Column { get; }

        public ChineseChessModel(int row, int column)
        {
            this.InitData(row, column);

            this.Row = row;
            this.Column = column;
        }

        private InnerChineseChessModel _data;
        public InnerChineseChessModel Data
        {
            get => _data;
            set => SetProperty(ref _data, value.AssertArgumentNotNull(nameof(Data)));
        }

        private bool _isReadyToPut;
        public bool IsReadyToPut
        {
            get => _isReadyToPut;
            set => SetProperty(ref _isReadyToPut, value);
        }

        #region IChineseChess
        public bool TryPutTo(IList<ChineseChessModel> datas, int toRow, int toColumn, IList<IChessCommand> commandStack)
        {
            if (this.Data.CheckPutTo(datas, this.Row, this.Column, toRow, toColumn))
            {
                datas.ForEach(c => c.IsReadyToPut = false);

                int fromRow = this.Row, fromColumn = this.Column;

                commandStack.Insert(0, new ChinChessCommand(commandStack.Count + 1, (bool)this.Data.IsRed, fromRow, fromColumn, toRow, toColumn).Forward(datas));

                return true;
            }

            return false;
        }

        public bool TrySelect(IList<ChineseChessModel> datas)
        {
            if (this.IsReadyToPut)
            {
                return false;
            }

            datas.ForEach(c => c.IsReadyToPut = false);

            return this.Data.TryMarkMove(datas, this.Row, this.Column);
        }
        #endregion

        protected override void DisposeCore()
        {
            _data = null;

            base.DisposeCore();
        }

        private void InitData(int row, int column)
        {
            AppUtils.Assert(row >= 0 && row < 10, "超出行范围");
            AppUtils.Assert(column >= 0 && column < 9, "超出列范围");

            bool isRed = row > 4;

            if (row == 0 || row == 9)
            {
                if (column == 0 || column == 8)
                {
                    _data = new ChineseChessJu(isRed);
                    return;
                }
                else if (column == 1 || column == 7)
                {
                    _data = new ChineseChessMa(isRed);
                    return;
                }
                else if (column == 2 || column == 6)
                {
                    _data = new ChineseChessXiang(isRed);
                    return;
                }
                else if (column == 3 || column == 5)
                {
                    _data = new ChineseChessShi(isRed);
                    return;
                }
                else if (column == 4)
                {
                    _data = new ChineseChessShuai(isRed);
                    return;
                }
            }
            else if (row == 3 || row == 6)
            {
                if (column == 0 || column == 2 || column == 4 || column == 6 || column == 8)
                {
                    _data = new ChineseChessBing(isRed);
                    return;
                }
            }
            else if (row == 2 || row == 7)
            {
                if (column == 1 || column == 7)
                {
                    _data = new ChineseChessPao(isRed);
                    return;
                }
            }

            _data = InnerChineseChessModel.Empty;
        }
    }
}
