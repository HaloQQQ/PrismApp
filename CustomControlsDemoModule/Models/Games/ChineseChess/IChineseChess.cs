
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal interface IChineseChess
    {
        bool TryPutTo(IList<ChineseChessModel> datas, InnerChineseChessModel targetData);
    }
}
