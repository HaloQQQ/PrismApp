
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessShi : InnerChineseChessModel
    {
        public ChineseChessShi(bool isRed, int row, int column) : base(isRed, ChessType.仕, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, InnerChineseChessModel targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            if (toColumn < 3 || toColumn > 5)
            {
                return false;
            }

            if ((bool)IsRed)
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

            if (Math.Abs(toRow - fromRow) == 1 && Math.Abs(toColumn - fromColumn) == 1)
            {
                return true;
            }

            return false;
        }
    }
}
