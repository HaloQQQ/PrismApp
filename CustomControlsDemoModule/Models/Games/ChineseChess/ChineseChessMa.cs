
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 马走日
    /// </summary>
    internal class ChineseChessMa : InnerChineseChessModel
    {
        public ChineseChessMa(bool isRed, int row, int column) : base(isRed, ChessType.馬, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            // 左上 - -
            if (fromColumn - 1 == toColumn && fromRow - 2 == toRow)
            {
                if (datas[GetIndex(fromRow - 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            if (fromColumn - 2 == toColumn && fromRow - 1 == toRow)
            {
                if (datas[GetIndex(fromRow, fromColumn - 1)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            // 左下 - +
            if (fromColumn - 1 == toColumn && fromRow + 2 == toRow)
            {
                if (datas[GetIndex(fromRow + 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            if (fromColumn - 2 == toColumn && fromRow + 1 == toRow)
            {
                if (datas[GetIndex(fromRow, fromColumn - 1)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            // 右上 + -
            if (fromColumn + 1 == toColumn && fromRow - 2 == toRow)
            {
                if (datas[GetIndex(fromRow - 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            if (fromColumn + 2 == toColumn && fromRow - 1 == toRow)
            {
                if (datas[GetIndex(fromRow, fromColumn + 1)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            // 右下 + +
            if (fromColumn + 1 == toColumn && fromRow + 2 == toRow)
            {
                if (datas[GetIndex(fromRow + 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            if (fromColumn + 2 == toColumn && fromRow + 1 == toRow)
            {
                if (datas[GetIndex(fromRow, fromColumn + 1)].Data.IsEmpty)
                {
                    return true;
                }

                return false;
            }

            return false;
        }

        protected override bool TryMoveCore(IList<ChineseChessModel> datas)
        {
            bool hasChoice = false;

            int fromRow = this.Row, fromColumn = this.Column;

            // 左上 --
            if (fromRow > 1 && fromColumn > 0)
            {
                var upLeft = datas[GetIndex(fromRow - 2, fromColumn - 1)];

                if (this.CheckPut(datas, upLeft.Data))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow > 0 && fromColumn > 1)
            {
                var upLeft = datas[GetIndex(fromRow - 1, fromColumn - 2)];

                if (this.CheckPut(datas, upLeft.Data))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 左下 +-
            if (fromRow < 8 && fromColumn > 0)
            {
                var downLeft = datas[GetIndex(fromRow + 2, fromColumn - 1)];

                if (this.CheckPut(datas, downLeft.Data))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow < 9 && fromColumn > 1)
            {
                var downLeft = datas[GetIndex(fromRow + 1, fromColumn - 2)];

                if (this.CheckPut(datas, downLeft.Data))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右上 -+
            if (fromRow > 1 && fromColumn < 8)
            {
                var upRight = datas[GetIndex(fromRow - 2, fromColumn + 1)];

                if (this.CheckPut(datas, upRight.Data))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow > 0 && fromColumn < 7)
            {
                var upRight = datas[GetIndex(fromRow - 1, fromColumn + 2)];

                if (this.CheckPut(datas, upRight.Data))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右下 ++
            if (fromRow < 8 && fromColumn < 8)
            {
                var downRight = datas[GetIndex(fromRow + 2, fromColumn + 1)];

                if (this.CheckPut(datas, downRight.Data))
                {
                    downRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow < 9 && fromColumn < 7)
            {
                var downRight = datas[GetIndex(fromRow + 1, fromColumn + 2)];

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
