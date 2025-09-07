using CustomControlsDemoModule.Models.Games.ChineseChess;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal interface IChineseChess
    {
        /// <summary>
        /// 尝试放置
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        bool TryPutTo(IList<ChineseChessModel> datas, int toRow, int toColumn, IList<IChessCommand> commandStack);

        /// <summary>
        /// 尝试选中棋子
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        bool TrySelect(IList<ChineseChessModel> datas);
    }
}
