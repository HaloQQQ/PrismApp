
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 隔山打牛
    /// </summary>
    internal class ChineseChessPao : InnerChineseChessModel
    {
        public ChineseChessPao(bool isRed, int row, int column) : base(isRed, ChessType.炮, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            // 移动
            var isSameRow = fromRow == toRow;
            var isSameColumn = fromColumn == toColumn;

            if (!isSameRow && !isSameColumn)
            {
                return false;
            }

            if (isSameRow && isSameColumn)
            {
                return false;
            }

            var mountainsCount = 0;

            // 检查是否可以落子
            if (isSameRow)
            {
                int step = fromColumn < toColumn ? 1 : -1;
                var currentColumn = fromColumn + step;

                while (currentColumn != toColumn)
                {
                    if (!datas[GetIndex(fromRow, currentColumn)].Data.IsEmpty)
                    {
                        // 移动
                        if (targetData.IsEmpty)
                        {
                            return false;
                        }

                        mountainsCount++;
                    }

                    currentColumn = currentColumn + step;
                }

                // 开炮
                if (!targetData.IsEmpty && mountainsCount != 1)
                {
                    return false;
                }
            }
            else
            {
                int step = fromRow < toRow ? 1 : -1;
                var currentRow = fromRow + step;

                while (currentRow != toRow)
                {
                    if (!datas[GetIndex(currentRow, fromColumn)].Data.IsEmpty)
                    {
                        // 移动
                        if (targetData.IsEmpty)
                        {
                            return false;
                        }

                        mountainsCount++;
                    }

                    currentRow = currentRow + step;
                }

                // 开炮
                if (!targetData.IsEmpty && mountainsCount != 1)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
