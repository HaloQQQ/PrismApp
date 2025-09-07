
using System;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ChineseChessShuai : InnerChineseChessModel
    {
        public ChineseChessShuai(bool isRed) : base(isRed, ChessType.帥)
        {
        }

        protected override bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            if (toColumn < 3 || toColumn > 5)
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

            return Math.Abs(toRow - fromRow) + Math.Abs(toColumn - fromColumn) == 1;
        }

        public override bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn)
        {
            bool hasChoice = false;

            // 上
            if (fromRow > 0)
            {
                var up = datas[GetIndex(fromRow - 1, fromColumn)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, up.Row, up.Column))
                {
                    up.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 下
            if (fromRow < 9)
            {
                var down = datas[GetIndex(fromRow + 1, fromColumn)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, down.Row, down.Column))
                {
                    down.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 左
            if (fromColumn > 3)
            {
                var left = datas[GetIndex(fromRow, fromColumn - 1)];
                if (this.CheckPutTo(datas, fromRow, fromColumn, left.Row, left.Column))
                {
                    left.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右
            if (fromColumn < 5)
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
