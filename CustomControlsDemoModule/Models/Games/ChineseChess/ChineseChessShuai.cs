
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessShuai : InnerChineseChessModel
    {
        public ChineseChessShuai(bool isRed, int row, int column) : base(isRed, ChessType.帥, row, column)
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

            return Math.Abs(toRow - fromRow) + Math.Abs(toColumn - fromColumn) == 1;
        }

        protected override bool TryMoveCore(IList<ChineseChessModel> datas)
        {
            bool hasChoice = false;

            int fromRow = this.Row, fromColumn = this.Column;

            // 上
            if (fromRow > 0)
            {
                var up = datas[GetIndex(fromRow - 1, fromColumn)];
                if (this.CheckPut(datas, up.Data))
                {
                    up.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 下
            if (fromRow < 9)
            {
                var down = datas[GetIndex(fromRow + 1, fromColumn)];
                if (this.CheckPut(datas, down.Data))
                {
                    down.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 左
            if (fromColumn > 3)
            {
                var left = datas[GetIndex(fromRow, fromColumn - 1)];
                if (this.CheckPut(datas, left.Data))
                {
                    left.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右
            if (fromColumn < 5)
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
