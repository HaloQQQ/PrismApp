using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 过河前只能前进，过河后可以左右前
    /// </summary>
    internal class ChineseChessBing : InnerChineseChessModel
    {
        public ChineseChessBing(bool isRed) : base(isRed, ChessType.兵)
        {
        }

        protected override bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            int blackRiverRow = 4, redRiverRow = 5;

            if ((bool)this.IsRed)
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

        public override bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn)
        {
            bool hasChoice = false;

            if (fromRow > 0)
            {
                var up = datas[GetIndex(fromRow - 1, fromColumn)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, up.Row, up.Column))
                {
                    up.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow < 9)
            {
                var down = datas[GetIndex(fromRow + 1, fromColumn)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, down.Row, down.Column))
                {
                    down.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromColumn > 0)
            {
                var left = datas[GetIndex(fromRow, fromColumn - 1)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, left.Row, left.Column))
                {
                    left.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromColumn < 8)
            {
                var right = datas[GetIndex(fromRow, fromColumn + 1)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, right.Row, right.Column))
                {
                    right.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            return hasChoice;
        }
    }
}
