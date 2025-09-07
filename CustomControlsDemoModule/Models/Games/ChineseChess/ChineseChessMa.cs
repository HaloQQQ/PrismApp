
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 马走日
    /// </summary>
    internal class ChineseChessMa : InnerChineseChessModel
    {
        public ChineseChessMa(bool isRed) : base(isRed, ChessType.馬)
        {
        }

        protected override bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
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

        public override bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn)
        {
            bool hasChoice = false;

            // 左上 --
            if (fromRow > 1 && fromColumn > 0)
            {
                var upLeft = datas[GetIndex(fromRow - 2, fromColumn - 1)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, upLeft.Row, upLeft.Column))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow > 0 && fromColumn > 1)
            {
                var upLeft = datas[GetIndex(fromRow - 1, fromColumn - 2)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, upLeft.Row, upLeft.Column))
                {
                    upLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 左下 +-
            if (fromRow < 8 && fromColumn > 0)
            {
                var downLeft = datas[GetIndex(fromRow + 2, fromColumn - 1)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, downLeft.Row, downLeft.Column))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow < 9 && fromColumn > 1)
            {
                var downLeft = datas[GetIndex(fromRow + 1, fromColumn - 2)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, downLeft.Row, downLeft.Column))
                {
                    downLeft.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右上 -+
            if (fromRow > 1 && fromColumn < 8)
            {
                var upRight = datas[GetIndex(fromRow - 2, fromColumn + 1)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, upRight.Row, upRight.Column))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow > 0 && fromColumn < 7)
            {
                var upRight = datas[GetIndex(fromRow - 1, fromColumn + 2)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, upRight.Row, upRight.Column))
                {
                    upRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            // 右下 ++
            if (fromRow < 8 && fromColumn < 8)
            {
                var downRight = datas[GetIndex(fromRow + 2, fromColumn + 1)];

                if (this.CheckPutTo(datas, fromRow, fromColumn, downRight.Row, downRight.Column))
                {
                    downRight.IsReadyToPut = true;

                    hasChoice = true;
                }
            }

            if (fromRow < 9 && fromColumn < 7)
            {
                var downRight = datas[GetIndex(fromRow + 1, fromColumn + 2)];

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
