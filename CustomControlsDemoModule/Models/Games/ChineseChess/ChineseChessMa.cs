
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

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, InnerChineseChessModel targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            // 左上 - -
            if (fromColumn - 1 == targetData.Column && fromRow - 2 == targetData.Row)
            {
                if (datas[GetIndex(fromRow - 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (fromColumn - 2 == targetData.Column && fromRow - 1 == targetData.Row)
            {
                if (datas[GetIndex(fromRow, fromColumn - 1)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // 左下 - +
            if (fromColumn - 1 == targetData.Column && fromRow + 2 == targetData.Row)
            {
                if (datas[GetIndex(fromRow + 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (fromColumn - 2 == targetData.Column && fromRow + 1 == targetData.Row)
            {
                if (datas[GetIndex(fromRow, fromColumn - 1)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // 右上 + -
            if (fromColumn + 1 == targetData.Column && fromRow - 2 == targetData.Row)
            {
                if (datas[GetIndex(fromRow - 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (fromColumn + 2 == targetData.Column && fromRow - 1 == targetData.Row)
            {
                if (datas[GetIndex(fromRow, fromColumn + 1)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            // 右下 + +
            if (fromColumn + 1 == targetData.Column && fromRow + 2 == targetData.Row)
            {
                if (datas[GetIndex(fromRow + 1, fromColumn)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (fromColumn + 2 == targetData.Column && fromRow + 1 == targetData.Row)
            {
                if (datas[GetIndex(fromRow, fromColumn + 1)].Data.IsEmpty)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            return false;
        }
    }
}
