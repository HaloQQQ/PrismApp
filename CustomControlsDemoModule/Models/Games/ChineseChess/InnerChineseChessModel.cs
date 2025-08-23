using IceTea.Pure.BaseModels;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace CustomControlsDemoModule.Models
{
    [DebuggerDisplay("IsRed={IsRed}, Type={Type}")]
    internal class InnerChineseChessModel : NotifyBase, IChineseChess
    {
        public InnerChineseChessModel(int row, int column)
        {
            this.Row = row;
            this.Column = column;
        }

        protected InnerChineseChessModel(bool isRed, ChessType type, int row, int column) : this(row, column)
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

        private bool _isEmpty;
        public bool IsEmpty
        {
            get => _isEmpty || Type == null;
            private set => SetProperty(ref _isEmpty, value);
        }

        public int GetIndex(int row, int column) => row * 9 + column;

        #region Current
        public bool? IsRed { get; }

        public ChessType? Type { get; }

        public ImageSource BackSource { get; private set; }

        public int Row { get; private set; }
        public int Column { get; private set; }
        #endregion

        #region Last
        public int LastRow { get; private set; }
        public int LastColumn { get; private set; }
        #endregion

        public void Backup()
        {
            this.LastRow = this.Row;
            this.LastColumn = this.Column;
        }

        public void Restore()
        {
            this.Row = this.LastRow;
            this.Column = this.LastColumn;

            this.IsEmpty = false;
        }

        public bool GoBack(IList<ChineseChessModel> datas)
        {
            datas.AssertNotEmpty(nameof(IList<ChineseChessModel>));

            int fromRow = this.LastRow, fromColumn = this.LastColumn;
            int toRow = this.Row, toColumn = this.Column;

            var fromModel = datas[GetIndex(fromRow, fromColumn)];
            var toModel = datas[GetIndex(toRow, toColumn)];

            fromModel.Data.Restore();
            toModel.Data.Restore();

            var temp = fromModel.Data;

            fromModel.Data = toModel.Data;
            toModel.Data = temp;

            return true;
        }

        public IChineseChess MoveTo(IList<ChineseChessModel> datas, int row, int column)
        {
            datas.AssertNotEmpty(nameof(IList<ChineseChessModel>));

            this.Row = row;
            this.Column = column;

            datas[GetIndex(row, column)].Data = this;

            return this;
        }

        public IChineseChess ResetAndMoveTo(IList<ChineseChessModel> datas, int row, int column)
        {
            datas.AssertNotEmpty(nameof(IList<ChineseChessModel>));

            this.IsEmpty = true;

            return this.MoveTo(datas, row, column);
        }

        #region TryPut
        /// <summary>
        /// 空 => 空 false
        /// 空 => 棋 false
        /// 棋 => 空 可以移动 ? true : false
        /// 棋 => 棋 同色 ? false : true
        /// </summary>
        public bool CheckPut(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            datas.AssertNotEmpty(nameof(IList<ChineseChessModel>));

            targetData.AssertNotNull(nameof(IChineseChess));

            if (this.IsEmpty || (!targetData.IsEmpty && this.IsRed == targetData.IsRed))
            {
                return false;
            }

            return this.TryPutToCore(datas, targetData);
        }

        protected virtual bool TryPutToCore(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            return false;
        }

        /// <summary>
        /// 空 => 空 false
        /// 空 => 棋 false
        /// 棋 => 空 可以移动 ? true : false
        /// 棋 => 棋 同色 ? false : true
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public bool TryPutTo(IList<ChineseChessModel> datas, IChineseChess targetData)
        {
            datas.AssertNotEmpty(nameof(IList<ChineseChessModel>));

            targetData.AssertNotNull(nameof(IChineseChess));

            if (this.CheckPut(datas, targetData))
            {
                int fromRow = this.Row, fromColumn = this.Column;
                int toRow = targetData.Row, toColumn = targetData.Column;

                this.Backup();

                // 移动或吃子
                this.MoveTo(datas, toRow, toColumn);

                targetData.Backup();

                targetData.ResetAndMoveTo(datas, fromRow, fromColumn);

                return true;
            }

            return false;
        }
        #endregion

        #region 预移动
        protected virtual bool TryMoveCore(IList<ChineseChessModel> datas) => false;

        public bool ClearReady(IList<ChineseChessModel> datas)
        {
            if (this.IsEmpty)
            {
                return false;
            }

            datas.ForEach(c => c.IsReadyToPut = false);

            return true;
        }

        public bool PreMove(IList<ChineseChessModel> datas)
        {
            if (!this.ClearReady(datas))
            {
                return false;
            }

            return TryMoveCore(datas);
        }
        #endregion
    }
}
