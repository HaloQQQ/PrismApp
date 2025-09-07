using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessXiang : InnerChineseChessModel
    {
        public ChineseChessXiang(bool isRed) : base(isRed, ChessType.相)
        {
        }

        protected override bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            if ((bool)this.IsRed)
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

        public override bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn)
        {
            bool hasChoice = false;

            // 左上 --
            if (fromRow > 0 && fromColumn > 0)
            {
                var upLeft = datas[GetIndex(fromRow - 2, fromColumn - 2)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, upLeft.Row, upLeft.Column))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右上 -+
            if (fromRow > 0 && fromColumn < 8)
            {
                var upRight = datas[GetIndex(fromRow - 2, fromColumn + 2)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, upRight.Row, upRight.Column))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 左下 +-
            if (fromRow < 9 && fromColumn > 0)
            {
                var downLeft = datas[GetIndex(fromRow + 2, fromColumn - 2)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, downLeft.Row, downLeft.Column))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右下 ++
            if (fromRow < 9 && fromColumn < 8)
            {
                var downRight = datas[GetIndex(fromRow + 2, fromColumn + 2)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, downRight.Row, downRight.Column))
                {
                    downRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            return hasChoice;
        }
    }
}
