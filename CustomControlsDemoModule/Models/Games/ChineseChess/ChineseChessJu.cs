using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    /// <summary>
    /// 只能水平和垂直移动
    /// </summary>
    internal class ChineseChessJu : InnerChineseChessModel
    {
        public ChineseChessJu(bool isRed) : base(isRed, ChessType.車)
        {
        }

        protected override bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
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

        public override bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn)
        {
            bool hasChoice = false;

            if (fromRow > 0)
            {
                var currentRow = fromRow - 1;
                var up = datas[GetIndex(currentRow, fromColumn)];

                while (this.CheckPutTo(datas, fromRow, fromColumn, up.Row, up.Column))
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

                while (this.CheckPutTo(datas, fromRow, fromColumn, down.Row, down.Column))
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

                while (this.CheckPutTo(datas, fromRow, fromColumn, left.Row, left.Column))
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

                while (this.CheckPutTo(datas, fromRow, fromColumn, right.Row, right.Column))
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
