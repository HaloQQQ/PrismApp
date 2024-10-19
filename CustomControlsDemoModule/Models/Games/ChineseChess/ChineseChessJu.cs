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

        protected override bool TryPutToCore(IList<ChineseChessModel> datas, InnerChineseChessModel targetData)
        {
            int fromRow = this.Row, fromColumn = this.Column;
            int toRow = targetData.Row, toColumn = targetData.Column;

            var isSameRow = Row == toRow;
            var isSameColumn = Column == toColumn;

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
                    if (!datas[GetIndex(Row, currentColumn)].Data.IsEmpty)
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
                    if (!datas[GetIndex(currentRow, Column)].Data.IsEmpty)
                    {
                        return false;
                    }

                    currentRow = currentRow + step;
                }
            }

            return true;
        }
    }
}
