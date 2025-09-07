
using IceTea.Pure.Extensions;
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessShi : InnerChineseChessModel
    {
        public ChineseChessShi(bool isRed) : base(isRed, ChessType.仕)
        {
        }

        protected override bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            if (!toColumn.IsInRange(3, 5))
            {
                return false;
            }

            if ((bool)this.IsRed)
            {
                if (toRow < 7)
                {
                    return false;
                }
            }
            else
            {
                if (toRow > 2)
                {
                    return false;
                }
            }

            return Math.Abs(toRow - fromRow) == 1 && Math.Abs(toColumn - fromColumn) == 1;
        }

        public override bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn)
        {
            bool hasChoice = false;

            int toRow = fromRow - 1, toColumn = fromColumn - 1;
            if(TryMark(datas, toRow, toColumn))
            {
                hasChoice = true;
            }

            toRow = fromRow - 1;
            toColumn = fromColumn + 1;
            if (TryMark(datas, toRow, toColumn))
            {
                hasChoice = true;
            }

            toRow = fromRow + 1;
            toColumn = fromColumn - 1;
            if (TryMark(datas, toRow, toColumn))
            {
                hasChoice = true;
            }

            toRow = fromRow + 1;
            toColumn = fromColumn + 1;
            if (TryMark(datas, toRow, toColumn))
            {
                hasChoice = true;
            }

            return hasChoice;

            bool TryMark(IList<ChineseChessModel> datas, int toRow, int toColumn)
            {
                bool isRowValid = toRow.IsInRange(0, 2) || toRow.IsInRange(7, 9);
                bool isColumnValid = toColumn.IsInRange(3, 5);

                if (isRowValid && isColumnValid)
                {
                    var upLeft = datas[GetIndex(toRow, toColumn)];

                    if (this.CheckPutTo(datas, fromRow, fromColumn, upLeft.Row, upLeft.Column))
                    {
                        upLeft.IsReadyToPut = true;

                        return true;
                    }
                }

                return false;
            }
        }
    }
}
