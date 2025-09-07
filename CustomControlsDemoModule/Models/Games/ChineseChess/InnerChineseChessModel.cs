using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using System.Collections.Generic;
using System.Diagnostics;

namespace CustomControlsDemoModule.Models
{
    [DebuggerDisplay("IsRed={IsRed}, Type={Type}")]
    internal class InnerChineseChessModel
    {
        public static InnerChineseChessModel Empty = new();

        private InnerChineseChessModel()
        {
        }

        public InnerChineseChessModel(bool isRed, ChessType type)
        {
            IsRed = isRed;
            Type = type;
        }

        public bool IsEmpty => Type == null;

        public bool? IsRed { get; }

        public ChessType? Type { get; }

        #region IChineseChess
        /// <summary>
        /// 空 => 空 false
        /// 空 => 棋 false
        /// 棋 => 空 可以移动 ? true : false
        /// 棋 => 棋 同色 ? false : true
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="fromRow"></param>
        /// <param name="fromColumn"></param>
        /// <param name="toRow"></param>
        /// <param name="toColumn"></param>
        /// <returns>是否可被放置到目标点</returns>
        public bool CheckPutTo(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
        {
            datas.AssertNotEmpty(nameof(IList<ChineseChessModel>));

            AppUtils.Assert(fromRow.IsInRange(0, 9), "超出行范围");
            AppUtils.Assert(toRow.IsInRange(0, 9), "超出行范围");
            AppUtils.Assert(fromColumn.IsInRange(0, 8), "超出列范围");
            AppUtils.Assert(toColumn.IsInRange(0, 8), "超出列范围");

            var targetData = datas[GetIndex(toRow, toColumn)].Data;

            if (this.IsEmpty || (!targetData.IsEmpty && this.IsRed == targetData.IsRed))
            {
                return false;
            }

            return this.CheckPutToCore(datas, fromRow, fromColumn, toRow, toColumn);
        }

        protected virtual bool CheckPutToCore(IList<ChineseChessModel> datas, int fromRow, int fromColumn, int toRow, int toColumn)
            => false;

        public virtual bool TryMarkMove(IList<ChineseChessModel> datas, int fromRow, int fromColumn) => false;
        #endregion

        protected int GetIndex(int row, int column) => row * 9 + column;
    }
}
