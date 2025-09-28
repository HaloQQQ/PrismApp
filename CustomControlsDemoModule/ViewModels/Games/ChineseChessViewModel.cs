using CustomControlsDemoModule.Models;
using IceTea.Pure.Contracts;
using IceTea.Pure.Extensions;
using IceTea.Wpf.Atom.Utils.HotKey.App;
using Prism.Commands;
using Prism.Events;
using PrismAppBasicLib.Contracts;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Threading;

namespace CustomControlsDemoModule.ViewModels
{
    internal class ChineseChessViewModel : GameBaseViewModel<ChineseChessModel>
    {
        protected override string GameName => "象棋";

        private DispatcherTimer _timer;

        private ChineseChessModel _currentChess;
        public ChineseChessModel CurrentChess
        {
            get => _currentChess;
            private set => SetProperty<ChineseChessModel>(ref _currentChess, value);
        }

        public bool CanGoBack { get; }

        public ObservableCollection<IChessCommand> Stack { get; private set; }

        public ChineseChessViewModel(IAppConfigFileHotKeyManager appCfgHotkeyManager, IConfigManager configManager, IEventAggregator eventAggregator)
            : base(appCfgHotkeyManager, configManager, eventAggregator)
        {
            this.Stack = new();

            this.RevokeCommand = new DelegateCommand(() =>
            {
                IChessCommand current = default;
                if ((current = this.Stack.FirstOrDefault()) != null)
                {
                    this.From = null;
                    this.To = Datas[current.FromRow * 9 + current.FromColumn];

                    current.Back(this.Datas);

                    this.Stack.RemoveAt(0);

                    IsRedTurn = !IsRedTurn;

                    RaisePropertyChanged(nameof(CanGoBack));
                }
            },
            () => !IsGameOver && IsUsable && Stack.Count > 0
            )
            .ObservesProperty(() => this.CanGoBack)
            .ObservesProperty(() => this.IsUsable)
            .ObservesProperty(() => this.IsGameOver);

            this.SelectOrPutCommand = new DelegateCommand<ChineseChessModel>(model =>
            {
                var data = model.Data;
                var targetIsEmpty = data.IsEmpty;

                bool isSelected = !targetIsEmpty && model == CurrentChess;
                bool hasnotSelected = targetIsEmpty && CurrentChess == null;
                if (isSelected || hasnotSelected)
                {
                    return;
                }

                // 选中
                bool canSelect = !targetIsEmpty && data.IsRed == this.IsRedTurn;
                if (canSelect)
                {
                    if (model.TrySelect(Datas))
                    {
                        CurrentChess = model;

                        this.PlayMedia("select.mp3");
                    }

                    return;
                }

                // 移动棋子到这里 或 吃子
                if (this.CurrentChess.IsNotNullAnd(c => c.TryPutTo(Datas, model.Row, model.Column, Stack)))
                {
                    RaisePropertyChanged(nameof(CanGoBack));

                    if (targetIsEmpty)
                    {
                        this.PlayMedia("go.mp3");
                    }
                    else
                    {
                        this.PlayMedia("eat.mp3");
                    }

                    this.From = this.CurrentChess;
                    this.To = model;

                    this.CurrentChess = null;

                    var isShuai = data.Type == ChessType.帥;

                    if (isShuai || this.CheckGameOver())
                    {
                        IsGameOver = true;
                        var actor = IsRedTurn ? "红方" : "黑方";

                        if (!isShuai)
                        {
                            actor = IsRedTurn ? "黑方" : "红方";
                        }

                        this.Over_Wav();

                        CommonUtil.PublishMessage(_eventAggregator, $"{actor}获胜");
                        return;
                    }

                    this.IsRedTurn = !IsRedTurn;

                    return;
                }
            }, model => model != null && CurrentChess != model && !IsGameOver && IsUsable)
            .ObservesProperty(() => this.CurrentChess)
            .ObservesProperty(() => this.IsUsable)
            .ObservesProperty(() => this.IsGameOver);

            this.SwitchDirectionCommand = new DelegateCommand(() =>
            {
                Angle = Angle == 0 ? 180 : 0;
            });

            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += Timer_Tick;
            _timer.IsEnabled = true;
        }

        #region Timer
        private void Timer_Tick(object sender, EventArgs e)
        {
            _seconds++;
            if (IsRedTurn)
            {
                RaisePropertyChanged(nameof(this.RedTimeSpan));
            }
            else
            {
                RaisePropertyChanged(nameof(this.BlackTimeSpan));
            }
        }

        protected override void OnUsableChanged(bool newValue)
        {
            base.OnUsableChanged(newValue);

            this._timer.IsEnabled = newValue;
        }

        private int _seconds;

        public string BlackTimeSpan => TimeSpan.FromSeconds(_seconds).FormatTimeSpan();

        public string RedTimeSpan => TimeSpan.FromSeconds(_seconds).FormatTimeSpan();
        #endregion

        #region overrides
        protected override bool CheckGameOver()
        {
            bool faceToFace = false;

            var black = Datas.First(c => c.Data.Type == ChessType.帥 && !(bool)c.Data.IsRed);
            var red = Datas.First(c => c.Data.Type == ChessType.帥 && (bool)c.Data.IsRed);

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

            _currentChess = null;
            IsRedTurn = true;

            Angle = 0;

            _seconds = 0;
            RaisePropertyChanged(nameof(BlackTimeSpan));
            RaisePropertyChanged(nameof(RedTimeSpan));

            foreach (var item in this.Stack)
            {
                item.Dispose();
            }
            this.Stack.Clear();
        }

        protected override void InitDatas()
        {
            foreach (var item in this.Datas)
            {
                item.Dispose();
            }
            this.Datas.Clear();

            for (int row = 0; row < 10; row++)
            {
                for (int column = 0; column < 9; column++)
                {
                    this.Datas.Add(new ChineseChessModel(row, column));
                }
            }
        }

        protected override void InitHotKeysCore(IAppHotKeyGroup group)
        {
            group.TryRegister(new AppHotKey("悔棋", Key.Z, ModifierKeys.Control));
            group.TryRegister(new AppHotKey("棋盘换向", Key.D, ModifierKeys.Alt));
        }
        #endregion

        #region Props
        private ChineseChessModel _from;
        public ChineseChessModel From
        {
            get => _from;
            private set => SetProperty<ChineseChessModel>(ref _from, value);
        }

        private ChineseChessModel _to;
        public ChineseChessModel To
        {
            get => _to;
            private set => SetProperty<ChineseChessModel>(ref _to, value);
        }

        private bool _isRedTurn = true;
        public bool IsRedTurn
        {
            get => _isRedTurn;
            private set
            {
                if (SetProperty(ref _isRedTurn, value))
                {
                    _seconds = 0;

                    if (value)
                    {
                        RaisePropertyChanged(nameof(BlackTimeSpan));
                    }
                    else
                    {
                        RaisePropertyChanged(nameof(RedTimeSpan));
                    }
                }
            }
        }

        private double _angle;
        public double Angle
        {
            get => _angle;
            private set => SetProperty(ref _angle, value);
        }
        #endregion

        #region Commands
        public ICommand SelectOrPutCommand { get; }

        public ICommand RevokeCommand { get; }

        public ICommand SwitchDirectionCommand { get; }
        #endregion

        protected override void DisposeCore()
        {
            _timer.IsEnabled = false;
            _timer.Tick -= Timer_Tick;
            _timer = null;

            foreach (var item in this.Stack)
            {
                item.Dispose();
            }
            this.Stack.Clear();
            this.Stack = null;

            this.CurrentChess = null;
            this.From = null;
            this.To = null;

            base.DisposeCore();
        }
    }
}
