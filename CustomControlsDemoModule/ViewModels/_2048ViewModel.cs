using IceTea.Atom.Contracts;
using IceTea.Atom.Extensions;
using IceTea.Atom.Utils.HotKey.Contracts;
using IceTea.General.Utils.HotKey.App;
using IceTea.General.Utils.HotKey.App.Contracts;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PrismAppBasicLib.MsgEvents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels
{
    internal class _2048ViewModel : BindableBase
    {
        public _2048ViewModel(IAppConfigFileHotKeyManager appHotKeyManager, IEventAggregator eventAggregator, IConfigManager configManager)
        {
            this.Data = new ObservableCollection<Model>();

            this.InitData(configManager);

            this.InitHotKeys(appHotKeyManager);

            this.StartPauseCommand = new DelegateCommand(() =>
            {
                this.IsUsable = !this.IsUsable;
            }, () => !this.IsGameOver).ObservesProperty(() => this.IsGameOver);

            this.RePlayCommand = new DelegateCommand(() => this.InitData(configManager));

            this.LeftCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = this.Move(this.Data[i * 4], this.Data[i * 4 + 1], this.Data[i * 4 + 2], this.Data[i * 4 + 3]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (this.Data[i * 4 + j] != arr[j])
                        {
                            changed = true;

                            this.Data[i * 4 + j].Value = arr[j];
                        }

                    }
                }

                if (changed)
                {
                    this.SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => this.IsUsable);

            this.RightCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = this.Move(this.Data[i * 4 + 3], this.Data[i * 4 + 2], this.Data[i * 4 + 1], this.Data[i * 4]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (this.Data[i * 4 + 3 - j] != arr[j])
                        {
                            changed = true;

                            this.Data[i * 4 + 3 - j].Value = arr[j];
                        }

                    }
                }

                if (changed)
                {
                    this.SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => this.IsUsable);

            this.UpCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = this.Move(this.Data[i], this.Data[i + 4], this.Data[i + 8], this.Data[i + 12]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (this.Data[i + j * 4] != arr[j])
                        {
                            changed = true;

                            this.Data[i + j * 4].Value = arr[j];
                        }
                    }
                }

                if (changed)
                {
                    this.SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => this.IsUsable);

            this.DownCommand = new DelegateCommand(() =>
            {
                bool changed = false;

                for (int i = 0; i < 4; i++)
                {
                    var arr = this.Move(this.Data[i + 12], this.Data[i + 8], this.Data[i + 4], this.Data[i]);

                    for (int j = 0; j < arr.Length; j++)
                    {
                        if (this.Data[i + (3 - j) * 4] != arr[j])
                        {
                            changed = true;

                            this.Data[i + (3 - j) * 4].Value = arr[j];
                        }
                    }
                }

                if (changed)
                {
                    this.SetRandomValue();
                }
                else
                {
                    CheckGameOver();
                }
            }).ObservesCanExecute(() => this.IsUsable);

            this._eventAggregator = eventAggregator;
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
        #endregion

        #region Logical
        public Dictionary<string, IHotKey<Key, ModifierKeys>> KeyGestureDic { get; private set; }

        private void InitHotKeys(IAppConfigFileHotKeyManager appHotKeyManager)
        {
            var groupName = "2048";
            appHotKeyManager.TryAdd(groupName, new[] { "HotKeys", "App", groupName }, "2048的快捷键");

            appHotKeyManager.TryRegisterItem(groupName, new AppHotKey("重玩", Key.R, ModifierKeys.Alt));
            appHotKeyManager.TryRegisterItem(groupName, new AppHotKey("播放/暂停", Key.Space, ModifierKeys.None));

            this.KeyGestureDic = appHotKeyManager.First(g => g.GroupName == groupName).ToDictionary(hotKey => hotKey.Name);
        }

        private Random _random = new Random();

        private void SetRandomValue()
        {
            var index = _random.Next(16);

            if (this.Data[index] == 0)
            {
                this.Data[index].Value = (index & 1) == 0 ? 2 : 4;

                if (this.Data[index].IsCreating)
                {
                    this.Data[index].IsCreating = false;
                }

                this.Data[index].IsCreating = true;
            }
            else
            {
                if (CheckGameOver())
                {
                    return;
                }

                this.SetRandomValue();
            }
        }

        private readonly IEventAggregator _eventAggregator;
        private bool CheckGameOver()
        {
            if (!this.Data.Any(x => x == 0))
            {
                this.IsGameOver = true;
                this.IsUsable = false;

                _eventAggregator.GetEvent<DialogMessageEvent>().Publish(new IceTea.Atom.Contracts.DialogMessage("游戏结束"));

                return true;
            }

            return false;
        }

        private void InitData(IConfigManager configManager)
        {
            this.Data.Clear();

            this.Data.AddRange(Enumerable.Repeat(0, 16).Select(v => new Model(v)));

            if (this.MaxScore == 0)
            {
                var configNodes = new string[] { "Games", "2048", nameof(MaxScore) };
                var maxScoreString = configManager.ReadConfigNode(configNodes);

                if (!maxScoreString.IsNullOrBlank() && int.TryParse(maxScoreString, out int maxScore))
                {
                    this.MaxScore = maxScore;
                }

                configManager.SetConfig += config => config.WriteConfigNode(this.MaxScore, configNodes);
            }

            this.MaxScore = Math.Max(this.MaxScore, this.Score);
            this.Score = 0;

            this.IsGameOver = false;
            this.IsUsable = true;

            this.SetRandomValue();
            this.SetRandomValue();
        }

        private int[] Move(Model a, Model b, Model c, Model d)
        {
            int[] values = { a, b, c, d };
            Model[] arr = { a, b, c, d };
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

                        this.Score += (temp[i] * 2);

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

                                this.Score += newV;

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
        public ObservableCollection<Model> Data { get; private set; }

        public ICommand RePlayCommand { get; }
        public ICommand StartPauseCommand { get; }

        public ICommand LeftCommand { get; }
        public ICommand RightCommand { get; }
        public ICommand UpCommand { get; }
        public ICommand DownCommand { get; }
        #endregion

        internal class Model : BindableBase
        {
            public Model(int value)
            {
                Value = value;
            }

            private int _value;
            public int Value
            {
                get => _value;
                set { SetProperty(ref _value, value); }
            }

            private bool _isCreating;
            public bool IsCreating
            {
                get => _isCreating;
                set { SetProperty(ref _isCreating, value); }
            }

            private bool _isUpdating;
            public bool IsUpdating
            {
                get => _isUpdating;
                set { SetProperty(ref _isUpdating, value); }
            }

            public static bool operator ==(Model first, Model second)
            {
                return first.Value == second.Value;
            }

            public static bool operator !=(Model first, Model second)
            {
                return first.Value != second.Value;
            }

            //public static implicit operator Model(int value)
            //{
            //    return new Model(value);
            //}

            public static implicit operator int(Model first)
            {
                return first.Value;
            }

        }
    }
}
