
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessShi : InnerChineseChessModel
    {
        public ChineseChessShi(bool isRed, int row, int column) : base(isRed, ChessType.仕, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
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

            return Math.Abs(toRow - fromRow) == 1 && Math.Abs(toColumn - fromColumn) == 1;
        }

        protected override bool TryMoveCore(IList<ChineseChessModel> datas)
        {
            bool hasChoice = false;

            int fromRow = this.Row, fromColumn = this.Column;

            // 上
            if(fromRow > 0)
            {
                var upLeft = datas[GetIndex(fromRow - 1, fromColumn - 1)];
                if (this.CheckPut(datas, upLeft.Data))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }

                var upRight = datas[GetIndex(fromRow - 1, fromColumn + 1)];
                if (this.CheckPut(datas, upRight.Data))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 下
            if (fromRow < 9)
            {
                var downLeft = datas[GetIndex(fromRow + 1, fromColumn - 1)];
                if (this.CheckPut(datas, downLeft.Data))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }

                var downRight = datas[GetIndex(fromRow + 1, fromColumn + 1)];
                if (this.CheckPut(datas, downRight.Data))
                {
                    downRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            return hasChoice;
        }
    }
}
