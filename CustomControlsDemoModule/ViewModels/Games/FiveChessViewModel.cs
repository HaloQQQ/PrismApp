using CustomControlsDemoModule.Models;
using IceTea.Pure.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using PrismAppBasicLib.Contracts;
using System.Collections.Generic;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels
{
    internal class FiveChessViewModel : GameBaseViewModel<ChessModel>
    {
        protected override string GameName => "五子棋";

        public FiveChessViewModel(IAppConfigFileHotKeyManager appConfigFileHotKeyManager, IConfigManager configManager, IEventAggregator eventAggregator)
            : base(appConfigFileHotKeyManager, configManager, eventAggregator)
        {
            CancelLastCommand = new DelegateCommand(() =>
            {
                LastModel?.Reset();
                LastModel = null;
                IsWhiteTurn = !IsWhiteTurn;
            },
            () => !IsGameOver && IsUsable && LastModel != null
            )
            .ObservesProperty(() => this.LastModel)
            .ObservesProperty(() => this.IsUsable)
            .ObservesProperty(() => this.IsGameOver);

            ChessModel.SwitchEvent += model =>
            {
                LastModel = model;

                if (this.CheckGameOver())
                {
                    IsGameOver = true;

                    var msg = (bool)model.IsWhite ? "白方" : "黑方";

                    CommonUtil.PublishMessage(eventAggregator, $"{msg}获胜");

                    return;
                }

                IsWhiteTurn = !IsWhiteTurn;
            };
        }

        #region overrides
        protected override bool CheckGameOver()
        {
            var row = LastModel.Row;
            var column = LastModel.Column;

            var isWhite = LastModel.IsWhite;

            if (isWhite == null)
            {
                return false;
            }

            var list = new List<ChessModel>();
            // 水平
            for (int i = column - 4; i < column + 5; i++)
            {
                if (i >= 0 && i < chessModels[0].Length)
                {
                    list.Add(chessModels[row][i]);
                }
            }

            if (IsSuccess(list, isWhite))
            {
                return true;
            }

            // 垂直
            list.Clear();
            for (int i = row - 4; i < row + 5; i++)
            {
                if (i >= 0 && i < chessModels.Length)
                {
                    list.Add(chessModels[i][column]);
                }
            }

            if (IsSuccess(list, isWhite))
            {
                return true;
            }

            // 正斜
            list.Clear();
            for (int i = -4; i < 5; i++)
            {
                if (row + i >= 0 && row + i < chessModels.Length
                    && column + i >= 0 && column + i < chessModels[0].Length)
                {
                    list.Add(chessModels[row + i][column + i]);
                }
            }

            if (IsSuccess(list, isWhite))
            {
                return true;
            }

            // 反斜
            list.Clear();
            for (int i = -4; i < 5; i++)
            {
                if (row - i >= 0 && row - i < chessModels.Length
                    && column + i >= 0 && column + i < chessModels[0].Length)
                {
                    list.Add(chessModels[row - i][column + i]);
                }
            }

            if (IsSuccess(list, isWhite))
            {
                return true;
            }

            return false;

            bool IsSuccess(IList<ChessModel> list, bool? isWhite)
            {
                if (isWhite == null)
                {
                    return false;
                }

                var count = 0;
                var succeedList = new List<ChessModel>();
                foreach (var item in list)
                {
                    if (item.IsWhite == isWhite)
                    {
                        count++;
                        succeedList.Add(item);

                        if (count == 5)
                        {
                            succeedList.ForEach(item => item.Success());

                            return true;
                        }
                    }
                    else
                    {
                        count = 0;

                        succeedList.Clear();
                    }
                }

                return false;
            }
        }

        protected override void RePlay_CommandExecute()
        {
            base.RePlay_CommandExecute();

            LastModel = null;
            IsWhiteTurn = false;

            foreach (var item in this.Datas)
            {
                item.Reset();
            }
        }

        protected override void InitDatas()
        {
            chessModels = new ChessModel[15][];
            for (int i = 0; i < chessModels.Length; i++)
            {
                chessModels[i] = new ChessModel[15];
                for (int j = 0; j < chessModels[i].Length; j++)
                {
                    chessModels[i][j] = new ChessModel(i, j);
                    this.Datas.Add(chessModels[i][j]);
                }
            }
        }

        protected override void InitHotKeysCore(IAppConfigFileHotKeyManager appConfigFileHotKeyManager)
        {
            appConfigFileHotKeyManager.TryRegisterItem(GameName, new AppHotKey("悔棋", Key.Z, ModifierKeys.Control));
        }
        #endregion

        #region Props
        private bool _isWhiteTurn;
        public bool IsWhiteTurn
        {
            get => _isWhiteTurn;
            private set => SetProperty(ref _isWhiteTurn, value);
        }

        private ChessModel _lastModel;
        private ChessModel LastModel
        {
            get => _lastModel;
            set => SetProperty(ref _lastModel, value);
        }
        #endregion

        #region Commands
        public ICommand CancelLastCommand { get; }
        #endregion

        private ChessModel[][] chessModels;
    }
}
