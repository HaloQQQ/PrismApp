using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 只能水平和垂直移动
    /// </summary>
    internal class ChineseChessJu : InnerChineseChessModel
    {
        public ChineseChessJu(bool isRed, int row, int column) : base(isRed, ChessType.車, row, column)
        {
        }

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

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

            // 检查是否可以落子
            if (isSameRow)
            {
                int step = fromColumn < toColumn ? 1 : -1;
                var currentColumn = fromColumn + step;

                while (currentColumn != toColumn)
                {
                    if (!datas[GetIndex(fromRow, currentColumn)].Data.IsEmpty)
                    {
                        return false;
                    }

                    currentColumn = currentColumn + step;
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
                        return false;
                    }

                    currentRow = currentRow + step;
                }
            }

            return true;
        }

        protected override bool TryMoveCore(IList<ChineseChessModel> datas)
        {
            bool hasChoice = false;

            int fromRow = this.Row, fromColumn = this.Column;

            if (fromRow > 0)
            {
                var currentRow = fromRow - 1;
                var up = datas[GetIndex(currentRow, fromColumn)];

                while (this.CheckPut(datas, up.Data))
                {
                    up.IsReadyToPut = true;

                    hasChoice = true;

                    if (--currentRow < 0)
                    {
                        break;
                    }

                    up = datas[GetIndex(currentRow, fromColumn)];
                }
            }

            if (fromRow < 9)
            {
                var currentRow = fromRow + 1;
                var down = datas[GetIndex(currentRow, fromColumn)];

                while (this.CheckPut(datas, down.Data))
                {
                    down.IsReadyToPut = true;

                    hasChoice = true;

                    if (++currentRow > 9)
                    {
                        break;
                    }

                    down = datas[GetIndex(currentRow, fromColumn)];
                }
            }

            if (fromColumn > 0)
            {
                var currentColumn = fromColumn - 1;
                var left = datas[GetIndex(fromRow, currentColumn)];

                while (this.CheckPut(datas, left.Data))
                {
                    left.IsReadyToPut = true;

                    hasChoice = true;

                    if (--currentColumn < 0)
                    {
                        break;
                    }

                    left = datas[GetIndex(fromRow, currentColumn)];
                }
            }

            if (fromColumn < 8)
            {
                var currentColumn = fromColumn + 1;
                var right = datas[GetIndex(fromRow, currentColumn)];

                while (this.CheckPut(datas, right.Data))
                {
                    right.IsReadyToPut = true;

                    hasChoice = true;

                    if (++currentColumn > 8)
                    {
                        break;
                    }

                    right = datas[GetIndex(fromRow, currentColumn)];
                }
            }

            return hasChoice;
        }

    }
}
