using IceTea.Atom.BaseModels;
using IceTea.Atom.Utils;
using Prism.Commands;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels
{
    internal class ClickCounter : BaseNotifyModel
    {
        private bool _isExit = false;

        public ClickCounter()
        {
            this.Ip = AppUtils.GetInternalIp().ToString();

            this.IncreaseCommand = new DelegateCommand<ClickCounter>(data => data.Count++, data => data?.Count < 30)
                                                                            .ObservesProperty(() => this.Count);
            this.DecreaseCommand = new DelegateCommand<ClickCounter>(data => data.Count--, data => data?.Count > 0)
                                                                            .ObservesProperty(() => this.Count);

            Application.Current.Windows[0].Destroying += (sender, e) => this._isExit = true;

            Task.Run(async () =>
            {
                while (!_isExit)
                {
                    //current.Post(state => this.CurrentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"), null);

                    await Application.Current.Dispatcher.DispatchAsync(() => this.CurrentTime = DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));

                    await Task.Delay(1000);
                }
            });
        }

        public string Ip { get; private set; }

        private string _currentTime;

        public string CurrentTime
        {
            get => this._currentTime;
            set => SetProperty<string>(ref _currentTime, value);
        }


        private int _count;

        public int Count
        {
            get => this._count;
            set => SetProperty<int>(ref _count, value);
        }

        #region Commands
        public ICommand IncreaseCommand { get; private set; }
        public ICommand DecreaseCommand { get; private set; }
        #endregion
    }
}
