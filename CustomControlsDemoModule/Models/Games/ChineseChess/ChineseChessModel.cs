using IceTea.Atom.BaseModels;
using IceTea.Atom.Utils;
using System;
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
    internal class ChineseChessModel : BaseNotifyModel
    {
        public ChineseChessModel(int row, int column)
        {
            Data = new InnerChineseChessModel(row, column);
        }

        public ChineseChessModel(bool isRed, ChessType type, int row, int column) : this(row, column)
        {
            switch (type)
            {
                case ChessType.車:
                    Data = new ChineseChessJu(isRed, row, column);
                    break;
                case ChessType.馬:
                    Data = new ChineseChessMa(isRed, row, column);
                    break;
                case ChessType.相:
                    Data = new ChineseChessXiang(isRed, row, column);
                    break;
                case ChessType.仕:
                    Data = new ChineseChessShi(isRed, row, column);
                    break;
                case ChessType.帥:
                    Data = new ChineseChessShuai(isRed, row, column);
                    break;
                case ChessType.炮:
                    Data = new ChineseChessPao(isRed, row, column);
                    break;
                case ChessType.兵:
                    Data = new ChineseChessBing(isRed, row, column);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private IChineseChess _data;
        public IChineseChess Data
        {
            get => _data;
            internal set => SetProperty(ref _data, value.AssertArgumentNotNull(nameof(Data)));
        }

        private bool _isReadyToPut;
        public bool IsReadyToPut
        {
            get => _isReadyToPut;
            set => SetProperty(ref _isReadyToPut, value);
        }
    }
}
