using CustomControlsDemoModule.Models;
using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Atom.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels
{
    internal class _2048ViewModel : GameBaseViewModel<_2048Model>
    {
        protected override string GameName => "2048";

        public _2048ViewModel(IAppConfigFileHotKeyManager appConfigFileHotKeyManager, IConfigManager configManager, IEventAggregator eventAggregator)
            : base(appConfigFileHotKeyManager, configManager, eventAggregator)
        {
            LeftCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = Move(Datas[i * 4], Datas[i * 4 + 1], Datas[i * 4 + 2], Datas[i * 4 + 3]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (Datas[i * 4 + j] != arr[j])
                        {
                            changed = true;

                            Datas[i * 4 + j].Value = arr[j];
                        }

                    }
                }

                if (changed)
                {
                    SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => IsUsable);

            RightCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = Move(Datas[i * 4 + 3], Datas[i * 4 + 2], Datas[i * 4 + 1], Datas[i * 4]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (Datas[i * 4 + 3 - j] != arr[j])
                        {
                            changed = true;

                            Datas[i * 4 + 3 - j].Value = arr[j];
                        }

                    }
                }

                if (changed)
                {
                    SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => IsUsable);

            UpCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = Move(Datas[i], Datas[i + 4], Datas[i + 8], Datas[i + 12]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (Datas[i + j * 4] != arr[j])
                        {
                            changed = true;

                            Datas[i + j * 4].Value = arr[j];
                        }
                    }
                }

                if (changed)
                {
                    SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => IsUsable);

            DownCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = Move(Datas[i + 12], Datas[i + 8], Datas[i + 4], Datas[i]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (Datas[i + (3 - j) * 4] != arr[j])
                        {
                            changed = true;

                            Datas[i + (3 - j) * 4].Value = arr[j];
                        }
                    }
                }

                if (changed)
                {
                    SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => IsUsable);
        }

        #region Props
        private int _score;
        public int Score
        {
            get => _score;
            set { SetProperty(ref _score, value); }
        }

        private int _maxScore;
        public int MaxScore
        {
            get => _maxScore;
            set { SetProperty(ref _maxScore, value); }
        }
        #endregion

        #region overrides
        protected override void LoadConfig(IConfigManager configManager)
        {
            base.LoadConfig(configManager);

            if (MaxScore == 0)
            {
                var configNodes = new string[] { "Games", "2048", nameof(MaxScore) };
                var maxScoreString = configManager.ReadConfigNode(configNodes);

                if (!maxScoreString.IsNullOrBlank() && int.TryParse(maxScoreString, out int maxScore))
                {
                    MaxScore = maxScore;
                }

                configManager.SetConfig += config => config.WriteConfigNode(MaxScore, configNodes);
            }
        }

        protected override void RePlay_CommandExecute()
        {
            base.RePlay_CommandExecute();

            this.InitDatas();

            Score = 0;
        }

        protected override void InitDatas()
        {
            Datas.Clear();

            Datas.AddRange(Enumerable.Repeat(0, 16).Select(v => new _2048Model(v)));

            MaxScore = Math.Max(MaxScore, Score);

            SetRandomValue();
            SetRandomValue();
        }
        #endregion

        #region Logicals
        private Random _random = new Random();

        private void SetRandomValue()
        {
            var index = _random.Next(16);

            if (Datas[index] == 0)
            {
                Datas[index].Value = (index & 1) == 0 ? 2 : 4;

                if (Datas[index].IsCreating)
                {
                    Datas[index].IsCreating = false;
                }

                Datas[index].IsCreating = true;
            }
            else
            {
                if (CheckGameOver())
                {
                    return;
                }

                SetRandomValue();
            }
        }

        private bool CheckGameOver()
        {
            if (!Datas.Any(x => x == 0))
            {
                IsGameOver = true;

                MaxScore = Math.Max(MaxScore, Score);

                _eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage("游戏结束"));

                return true;
            }

            return false;
        }

        private int[] Move(_2048Model a, _2048Model b, _2048Model c, _2048Model d)
        {
            int[] values = { a, b, c, d };
            _2048Model[] arr = { a, b, c, d };
            a.IsUpdating = false;
            b.IsUpdating = false;
            c.IsUpdating = false;
            d.IsUpdating = false;

            var list = new List<(int v, bool b)>();

            ExecuteShallow();

            void ExecuteShallow()
            {
                var temp = new List<int>();
                foreach (var item in values)
                {
                    if (item != 0)
                    {
                        temp.Add(item);
                    }
                }

                for (int i = 0; i < temp.Count; i++)
                {
                    if (temp[i] == 0)
                    {
                        continue;
                    }

                    if (i + 1 < temp.Count && temp[i] == temp[i + 1])
                    {
                        list.Add((temp[i] * 2, true));

                        Score += temp[i] * 2;

                        temp[i + 1] = 0;
                    }
                    else
                    {
                        list.Add((temp[i], false));
                    }
                }
            }

            void ExecuteDeep()
            {
                foreach (var item in arr)
                {
                    if (item != 0)
                    {
                        var current = list.LastOrDefault((0, false));

                        if (current.Item1 != item)
                        {
                            list.Add((item, false));
                        }
                        else
                        {
                            var newV = item.Value;
                            while (current.Item1 == newV)
                            {
                                list.RemoveAt(list.Count - 1);

                                var last = list.LastOrDefault((0, false));

                                newV += current.Item1;

                                if (newV != last.Item1)
                                {
                                    list.Add((newV, true));
                                }

                                Score += newV;

                                current = last;
                            }
                        }
                    }
                }
            }

            for (int i = 0; i < arr.Length; i++)
            {
                if (i < list.Count)
                {
                    values[i] = list[i].v;
                    arr[i].IsUpdating = list[i].b;
                }
                else
                {
                    values[i] = 0;
                }
            }

            return values;
        }
        #endregion

        #region 公共
        public ICommand LeftCommand { get; }
        public ICommand RightCommand { get; }
        public ICommand UpCommand { get; }
        public ICommand DownCommand { get; }
        #endregion
    }
}
