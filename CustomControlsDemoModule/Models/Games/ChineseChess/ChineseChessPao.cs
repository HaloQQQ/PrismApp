
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

                if (currentRow > 0)
                {
                    currentRow--;

                    while (datas[GetIndex(currentRow, fromColumn)].Data.IsEmpty)
                    {
                        if (currentRow <= 0)
                        {
                            break;
                        }

                        currentRow--;
                    }

                    up = datas[GetIndex(currentRow, fromColumn)];
                    if (!up.Data.IsEmpty)
                    {
                        if (up.Data.IsRed != this.IsRed)
                        {
                            up.IsReadyToPut = true;

                            hasChoice = true;
                        }
                    }
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

                if (currentRow < 9)
                {
                    currentRow++;

                    while (datas[GetIndex(currentRow, fromColumn)].Data.IsEmpty)
                    {
                        if (currentRow >= 9)
                        {
                            break;
                        }

                        currentRow++;
                    }

                    down = datas[GetIndex(currentRow, fromColumn)];
                    if (!down.Data.IsEmpty)
                    {
                        if (down.Data.IsRed != this.IsRed)
                        {
                            down.IsReadyToPut = true;

                            hasChoice = true;
                        }
                    }
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

                if (currentColumn > 0)
                {
                    currentColumn--;

                    while (datas[GetIndex(fromRow, currentColumn)].Data.IsEmpty)
                    {
                        if (currentColumn <= 0)
                        {
                            break;
                        }

                        currentColumn--;
                    }

                    left = datas[GetIndex(fromRow, currentColumn)];
                    if (!left.Data.IsEmpty)
                    {
                        if (left.Data.IsRed != this.IsRed)
                        {
                            left.IsReadyToPut = true;

                            hasChoice = true;
                        }
                    }
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

                if (currentColumn < 8)
                {
                    currentColumn++;

                    while (datas[GetIndex(fromRow, currentColumn)].Data.IsEmpty)
                    {
                        if (currentColumn >= 8)
                        {
                            break;
                        }

                        currentColumn++;
                    }

                    right = datas[GetIndex(fromRow, currentColumn)];
                    if (!right.Data.IsEmpty)
                    {
                        if (right.Data.IsRed != this.IsRed)
                        {
                            right.IsReadyToPut = true;

                            hasChoice = true;
                        }
                    }
                }
            }

            return hasChoice;
        }
    }
}
