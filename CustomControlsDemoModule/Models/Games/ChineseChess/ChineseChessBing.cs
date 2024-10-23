
using DryIoc;
using System.Collections.Generic;
using System.Data.Common;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 过河前只能前进，过河后可以左右前
    /// </summary>
    internal class ChineseChessBing : InnerChineseChessModel
    {
        public ChineseChessBing(bool isRed, int row, int column) : base(isRed, ChessType.兵, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            int blackRiverRow = 4, redRiverRow = 5;

            if ((bool)IsRed)
            {
                if (fromRow - 1 == toRow && fromColumn == toColumn)
                {
                    return true;
                }

                if (fromRow < redRiverRow)
                {
                    if ((fromRow == toRow && fromColumn - 1 == toColumn)
                        || (fromRow == toRow && fromColumn + 1 == toColumn)
                        )
                    {
                        return true;
                    }
                }
            }
            else
            {
                if (fromRow + 1 == toRow && fromColumn == toColumn)
                {
                    return true;
                }

                if (fromRow > blackRiverRow)
                {
                    if ((fromRow == toRow && fromColumn - 1 == toColumn)
                        || (fromRow == toRow && fromColumn + 1 == toColumn)
                        )
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        protected override bool TryMoveCore(IList<ChineseChessModel> datas)
        {
            bool hasChoice = false;

            int fromRow = this.Row, fromColumn = this.Column;

            if (fromRow > 0)
            {
                var up = datas[GetIndex(fromRow - 1, fromColumn)];

                if (this.CheckPut(datas, up.Data))
                {
                    up.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow < 9)
            {
                var down = datas[GetIndex(fromRow + 1, fromColumn)];

                if (this.CheckPut(datas, down.Data))
                {
                    down.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromColumn > 0)
            {
                var left = datas[GetIndex(fromRow, fromColumn - 1)];

                if (this.CheckPut(datas, left.Data))
                {
                    left.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromColumn < 8)
            {
                var right = datas[GetIndex(fromRow, fromColumn + 1)];

                if (this.CheckPut(datas, right.Data))
                {
                    right.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            return hasChoice;
        }
    }
}
