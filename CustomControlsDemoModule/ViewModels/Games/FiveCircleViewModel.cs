using CustomControlsDemoModule.Models;
using IceTea.Atom.BaseModels;
using IceTea.Atom.Utils.HotKey.Contracts;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels
{
    internal class FiveCircleViewModel : BaseNotifyModel
    {
        public FiveCircleViewModel(IAppConfigFileHotKeyManager appHotKeyManager, IEventAggregator eventAggregator)
        {
            InitHotKeys(appHotKeyManager);

            this.Datas = new List<ChessModel>();

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

            CancelLastCommand = new DelegateCommand(
                    () => { LastModel?.Reset(); IsWhiteTurn = !IsWhiteTurn; },
                    () => !IsGameOver && IsUsable && LastModel != null
                )
                .ObservesProperty(() => this.LastModel)
                .ObservesProperty(() => this.IsUsable)
                .ObservesProperty(() => this.IsGameOver);

            StartPauseCommand = new DelegateCommand(() =>
            {
                IsUsable = !IsUsable;
            }, () => !IsGameOver).ObservesProperty(() => IsGameOver);

            RePlayCommand = new DelegateCommand(() =>
            {
                IsGameOver = false;
                IsUsable = true;

                LastModel = null;
                IsWhiteTurn = false;

                foreach (var item in this.Datas)
                {
                    item.Reset();
                }
            });

            ChessModel.SwitchEvent += model =>
            {
                LastModel = model;

                if (this.CheckSuccess(model))
                {
                    IsGameOver = true;
                    IsUsable = false;

                    var msg = (bool)model.IsWhite ? "白方" : "黑方";
                    eventAggregator.GetEvent<DialogMessageEvent>().Publish(new IceTea.Atom.Contracts.DialogMessage($"{msg}获胜"));

                    return;
                }

                IsWhiteTurn = !IsWhiteTurn;
            };
        }

        private bool CheckSuccess(ChessModel chessModel)
        {
            var row = chessModel.Row;
            var column = chessModel.Column;

            var isWhite = chessModel.IsWhite;

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

        public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; private set; }

        private void InitHotKeys(IAppConfigFileHotKeyManager appHotKeyManager)
        {
            var groupName = "五子棋";
            appHotKeyManager.TryAdd(groupName, new[] { "HotKeys", "App", groupName });

            appHotKeyManager.TryRegisterItem(groupName, new AppHotKey("重玩", Key.R, ModifierKeys.Alt));
            appHotKeyManager.TryRegisterItem(groupName, new AppHotKey("播放/暂停", Key.Space, ModifierKeys.None));
            appHotKeyManager.TryRegisterItem(groupName, new AppHotKey("悔棋", Key.Z, ModifierKeys.Control));

            KeyGestureDic = appHotKeyManager.First(g => g.GroupName == groupName).ToDictionary(hotKey => hotKey.Name);
        }

        #region Props
        private bool _isUsable = true;
        public bool IsUsable
        {
            get => _isUsable;
            set { SetProperty(ref _isUsable, value); }
        }

        private bool _isGameOver;
        public bool IsGameOver
        {
            get => _isGameOver;
            set { SetProperty(ref _isGameOver, value); }
        }

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
        public ICommand RePlayCommand { get; }
        public ICommand StartPauseCommand { get; }
        #endregion

        private ChessModel[][] chessModels;
        public IList<ChessModel> Datas { get; }
    }
}
