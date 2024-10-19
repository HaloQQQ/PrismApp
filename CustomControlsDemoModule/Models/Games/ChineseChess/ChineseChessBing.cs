
using System.Collections.Generic;

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

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, InnerChineseChessModel targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            int step = (bool)IsRed ? -1 : 1;

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
    }
}
