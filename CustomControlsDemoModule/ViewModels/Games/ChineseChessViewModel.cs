using CustomControlsDemoModule.Models;
using IceTea.Atom.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;
using System.Linq;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels
{
    internal class ChineseChessViewModel : GameBaseViewModel<ChineseChessModel>
    {
        protected override string GameName => "象棋";

        public ChineseChessViewModel(IAppConfigFileHotKeyManager appConfigFileHotKeyManager, IConfigManager configManager, IEventAggregator eventAggregator)
            : base(appConfigFileHotKeyManager, configManager, eventAggregator)
        {
            this.CancelLastCommand = new DelegateCommand(() =>
            {
                From.Data.GoBack(this.Datas);
                From = null;
                To = null;

                if (CurrentChess != null)
                {
                    CurrentChess.Data.ClearReady(Datas);
                    CurrentChess = null;
                }

                IsRedTurn = !IsRedTurn;
            },
            () => !IsGameOver && IsUsable && From != null
            )
            .ObservesProperty(() => this.From)
            .ObservesProperty(() => this.IsUsable)
            .ObservesProperty(() => this.IsGameOver);

            this.SelectOrPutCommand = new DelegateCommand<ChineseChessModel>(model =>
            {
                var isJiangJun = model.Data.Type == ChessType.帥;

                // 移动棋子到这里
                if (this.CurrentChess != null && this.CurrentChess.Data.TryPutTo(Datas, model.Data))
                {
                    model.Data.ClearReady(Datas);

                    this.From = this.CurrentChess;

                    this.To = model;

                    this.CurrentChess = null;

                    if (isJiangJun || this.CheckGameOver())
                    {
                        IsGameOver = true;
                        var actor = IsRedTurn ? "红方" : "黑方";

                        if (!isJiangJun)
                        {
                            actor = IsRedTurn ? "黑方" : "红方";
                        }

                        eventAggregator.GetEvent<DialogMessageEvent>().Publish(new IceTea.Atom.Contracts.DialogMessage($"{actor}获胜"));
                        return;
                    }

                    this.IsRedTurn = !IsRedTurn;

                    return;
                }

                if (!model.Data.IsEmpty && model.Data.IsRed == IsRedTurn)
                {
                    CurrentChess = model;

                    CurrentChess.Data.PreMove(Datas);
                }
            }, model => model != null && CurrentChess != model && !IsGameOver && IsUsable)
            .ObservesProperty(() => this.CurrentChess)
            .ObservesProperty(() => this.IsUsable)
            .ObservesProperty(() => this.IsGameOver);

            this.SwitchDirectionCommand = new DelegateCommand(() =>
            {
                Angle = Angle == 0 ? 180 : 0;
            });
        }

        #region overrides
        protected override bool CheckGameOver()
        {
            bool faceToFace = false;

            var black = Datas.First(c => c.Data.Type == ChessType.帥 && !(bool)c.Data.IsRed).Data;
            var red = Datas.First(c => c.Data.Type == ChessType.帥 && (bool)c.Data.IsRed).Data;

            if (black.Column != red.Column)
            {
                return faceToFace;
            }

            // 将帅同列并相对
            var column = black.Column;
            var row = black.Row;

            var currentRow = row + 1;

            while (currentRow <= 9)
            {
                var currentData = Datas[GetIndex(currentRow, column)].Data;
                if (!currentData.IsEmpty)
                {
                    if (currentData.Type == ChessType.帥)
                    {
                        faceToFace = true;
                    }

                    break;
                }

                currentRow = currentRow + 1;

                int GetIndex(int row, int column) => row * 9 + column;
            }

            return faceToFace;
        }

        protected override void RePlay_CommandExecute()
        {
            base.RePlay_CommandExecute();

            this.InitDatas();

            CurrentChess = null;
            From = null;
            To = null;
            IsRedTurn = true;

            Angle = 0;
        }

        protected override void InitDatas()
        {
            this.Datas.Clear();

            for (int i = 0; i < 10; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    var isEmpty = true;

                    if (i == 0)
                    {
                        isEmpty = false;

                        if (j == 0 || j == 8)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.車, i, j));
                        }
                        else if (j == 1 || j == 7)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.馬, i, j));
                        }
                        else if (j == 2 || j == 6)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.相, i, j));
                        }
                        else if (j == 3 || j == 5)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.仕, i, j));
                        }
                        else if (j == 4)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.帥, i, j));
                        }
                    }
                    else if (i == 9)
                    {
                        isEmpty = false;

                        if (j == 0 || j == 8)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.車, i, j));
                        }
                        else if (j == 1 || j == 7)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.馬, i, j));
                        }
                        else if (j == 2 || j == 6)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.相, i, j));
                        }
                        else if (j == 3 || j == 5)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.仕, i, j));
                        }
                        else if (j == 4)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.帥, i, j));
                        }
                    }
                    else if (i == 3)
                    {
                        if (j == 0 || j == 2 || j == 4 || j == 6 || j == 8)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.兵, i, j));
                            isEmpty = false;
                        }
                    }
                    else if (i == 6)
                    {
                        if (j == 0 || j == 2 || j == 4 || j == 6 || j == 8)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.兵, i, j));
                            isEmpty = false;
                        }
                    }
                    else if (i == 2)
                    {
                        if (j == 1 || j == 7)
                        {
                            this.Datas.Add(new ChineseChessModel(false, ChessType.炮, i, j));
                            isEmpty = false;
                        }
                    }
                    else if (i == 7)
                    {
                        if (j == 1 || j == 7)
                        {
                            this.Datas.Add(new ChineseChessModel(true, ChessType.炮, i, j));
                            isEmpty = false;
                        }
                    }

                    if (isEmpty)
                    {
                        this.Datas.Add(new ChineseChessModel(i, j));
                    }
                }
            }
        }

        protected override void InitHotKeysCore(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
            appConfigFileHotKeyManager.TryRegisterItem(GameName, new AppHotKey("悔棋", Key.Z, ModifierKeys.Control));
            appConfigFileHotKeyManager.TryRegisterItem(GameName, new AppHotKey("棋盘换向", Key.D, ModifierKeys.Alt));
        }
        #endregion

        #region Props
        private bool _isRedTurn = true;
        public bool IsRedTurn
        {
            get => _isRedTurn;
            private set => SetProperty(ref _isRedTurn, value);
        }

        private double _angle;
        public double Angle
        {
            get => _angle;
            private set => SetProperty(ref _angle, value);
        }

        private ChineseChessModel _currentChess;
        public ChineseChessModel CurrentChess
        {
            get => _currentChess;
            private set => SetProperty(ref _currentChess, value);
        }

        private ChineseChessModel _from;
        public ChineseChessModel From
        {
            get => _from;
            private set => SetProperty(ref _from, value);
        }

        private ChineseChessModel _to;
        public ChineseChessModel To
        {
            get => _to;
            private set => SetProperty(ref _to, value);
        }
        #endregion

        #region Commands
        public ICommand SelectOrPutCommand { get; }

        public ICommand CancelLastCommand { get; }

        public ICommand SwitchDirectionCommand { get; }
        #endregion
    }
}
