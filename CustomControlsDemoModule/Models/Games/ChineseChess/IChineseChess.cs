
using System.Collections.Generic;
using System.Windows.Media;

namespace CustomControlsDemoModule.Models
{
    internal interface IChineseChess
    {
        bool IsEmpty { get; }

        bool? IsRed { get; }

        ChessType? Type { get; }

        ImageSource BackSource { get; }

        int Row { get; }
        int Column { get; }

        /// <summary>
        /// 备份该点
        /// </summary>
        void Backup();

        /// <summary>
        /// 恢复该点数据
        /// </summary>
        void Restore();

        bool TryPutTo(IList<ChineseChessModel> datas, IChineseChess targetData);

        /// <summary>
        /// 清空数据并移动到
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns></returns>
        IChineseChess ResetAndMoveTo(IList<ChineseChessModel> datas, int row, int column);

        /// <summary>
        /// 以当前点为终点
        /// 起点和终点数据恢复
        /// </summary>
        /// <param name="datas"></param>
        /// <returns></returns>
        bool GoBack(IList<ChineseChessModel> datas);
    }
}
