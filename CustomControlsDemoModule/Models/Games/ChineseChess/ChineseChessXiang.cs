
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessXiang : InnerChineseChessModel
    {
        public ChineseChessXiang(bool isRed, int row, int column) : base(isRed, ChessType.相, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            if ((bool)IsRed)
            {
                if (toRow < 5)
                {
                    return false;
                }
            }
            else
            {
                if (toRow > 4)
                {
                    return false;
                }
            }

            if (Math.Abs(toRow - fromRow) == 2 && Math.Abs(toColumn - fromColumn) == 2)
            {
                var eyeRow = fromRow + (toRow - fromRow) / 2;
                var eyeColumn = fromColumn + (toColumn - fromColumn) / 2;

                if (datas[GetIndex(eyeRow, eyeColumn)].Data.IsEmpty)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
