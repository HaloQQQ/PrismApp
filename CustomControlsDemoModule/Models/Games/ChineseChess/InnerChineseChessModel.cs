using IceTea.Atom.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomControlsDemoModule.Models
{
    [DebuggerDisplay("IsRetd={IsRead}, Type={Type}")]
    internal class InnerChineseChessModel : IChineseChess
    {
        public InnerChineseChessModel(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }

        public InnerChineseChessModel(bool isRed, ChessType type, int row, int column) : this(row, column)
        {
            IsRed = isRed;
            Type = type;

            switch (type)
            {
                case ChessType.車:
                    if (isRed)
                    {
                        SetBackSource("红车.png");
                    }
                    else
                    {
                        SetBackSource("黑车.png");
                    }
                    break;
                case ChessType.馬:
                    if (isRed)
                    {
                        SetBackSource("红马.png");
                    }
                    else
                    {
                        SetBackSource("黑马.png");
                    }
                    break;
                case ChessType.相:
                    if (isRed)
                    {
                        SetBackSource("红相.png");
                    }
                    else
                    {
                        SetBackSource("黑象.png");
                    }
                    break;
                case ChessType.仕:
                    if (isRed)
                    {
                        SetBackSource("红仕.png");
                    }
                    else
                    {
                        SetBackSource("黑士.png");
                    }
                    break;
                case ChessType.帥:
                    if (isRed)
                    {
                        SetBackSource("红帅.png");
                    }
                    else
                    {
                        SetBackSource("黑将.png");
                    }
                    break;
                case ChessType.炮:
                    if (isRed)
                    {
                        SetBackSource("红炮.png");
                    }
                    else
                    {
                        SetBackSource("黑炮.png");
                    }
                    break;
                case ChessType.兵:
                    if (isRed)
                    {
                        SetBackSource("红兵.png");
                    }
                    else
                    {
                        SetBackSource("黑卒.png");
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }

            void SetBackSource(string picName)
            {
                BackSource = new BitmapImage(new Uri(Path.Combine("pack://application:,,,/CustomControlsDemoModule;component/Resources/pictures", picName), UriKind.RelativeOrAbsolute));
            }
        }

        public bool IsEmpty => Type == null;

        public int GetIndex(int row, int column) => row * 9 + column;

        public bool? IsRed { get; private set; }

        public ChessType? Type { get; private set; }

        public ImageSource BackSource { get; private set; }

        public int Row { get; private set; }
        public int Column { get; private set; }

        public InnerChineseChessModel Move(int row, int column)
        {
            this.Row = row;
            this.Column = column;

            return this;
        }

        public InnerChineseChessModel Reset(int row, int column)
        {
            this.IsRed = null;
            this.Type = null;

            this.BackSource = null;

            return this.Move(row, column);
        }

        #region TryPut
        protected virtual bool TryPutToCore(IList<ChineseChessModel> datas, InnerChineseChessModel targetData)
        {
            return false;
        }

        /// <summary>
        /// 空 => 空 false
        /// 空 => 棋 false
        /// 棋 => 空 可以移动?true:false
        /// 棋 => 棋 同色?false:true
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryPutTo(IList<ChineseChessModel> datas, InnerChineseChessModel targetData)
        {
            targetData.AssertNotNull(nameof(InnerChineseChessModel));

            if (this.IsEmpty || this.IsRed == targetData.IsRed || this == targetData)
            {
                return false;
            }

            if (this.TryPutToCore(datas, targetData))
            {
                int fromRow = this.Row, fromColumn = this.Column;
                int toRow = targetData.Row, toColumn = targetData.Column;

                // 交换空白棋子和车 OR  车 吃
                datas[GetIndex(toRow, toColumn)].Data = this.Move(toRow, toColumn);

                datas[GetIndex(fromRow, fromColumn)].Data = targetData.Reset(fromRow, fromColumn);

                return true;
            }

            return false;
        }
        #endregion
    }
}
