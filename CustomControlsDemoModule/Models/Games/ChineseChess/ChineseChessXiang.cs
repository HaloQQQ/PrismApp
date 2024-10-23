
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

        protected override bool TryMoveCore(IList<ChineseChessModel> datas)
        {
            bool hasChoice = false;

            int fromRow = this.Row, fromColumn = this.Column;

            // 左上 --
            if (fromRow > 0 && fromColumn > 0)
            {
                var upLeft = datas[GetIndex(fromRow - 2, fromColumn - 2)];
                if (this.CheckPut(datas, upLeft.Data))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右上 -+
            if (fromRow > 0 && fromColumn < 8)
            {
                var upRight = datas[GetIndex(fromRow - 2, fromColumn + 2)];
                if (this.CheckPut(datas, upRight.Data))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 左下 +-
            if (fromRow < 9 && fromColumn > 0)
            {
                var downLeft = datas[GetIndex(fromRow + 2, fromColumn - 2)];
                if (this.CheckPut(datas, downLeft.Data))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右下 ++
            if (fromRow < 9 && fromColumn < 8)
            {
                var downRight = datas[GetIndex(fromRow + 2, fromColumn + 2)];
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
