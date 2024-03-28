using IceTea.Atom.BaseModels;
using IceTea.Atom.Extensions;
using Prism.Commands;
using System.ComponentModel;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels
{
    internal class OrderViewModel : BaseNotifyModel
    {
        public OrderViewModel()
        {
            this.Steps = typeof(StepType).GetEnumDescriptions().Select(s => new StepModel(s));

            this.StartCommand = new DelegateCommand<StepModel>(model =>
            {
                model.UnStarted = !model.UnStarted;

                if (model.UnStarted)
                {
                    model.ExecuteTime = default(DateTime).ToString();
                }
                else
                {
                    model.ExecuteTime = DateTime.Now.ToString();
                }
            });

            this.CustomerServices = new List<CustomerService>() {
                 new CustomerService("moon.png", "小米客服", "24小时在线咨询", string.Empty, string.Empty),
                 new CustomerService("moon.png", "小米服务微博", string.Empty, string.Empty, string.Empty),
                 new CustomerService("moon.png", "小米服务微信", string.Empty, string.Empty, string.Empty),
                 new CustomerService("moon.png", "商城官方微信", string.Empty, string.Empty, string.Empty),
                 new CustomerService("moon.png", "小米售后服务热线", "400-100-5678", "手机、电视盒子、智能硬件等", "服务时间：8:00-18:00"),
                 new CustomerService("moon.png", "天星金融热线", "400-100-3399", "贷款、保险、理财、钱包", "服务时间：9:00-19:00"),
                 new CustomerService("moon.png", "小米移动热线", "10046", "任我行、吃到饱、米粉卡等", "服务时间：9:00-18:00"),
                 new CustomerService("moon.png", "小米游戏热线", "400-098-1666", "小米游戏相关", "服务时间：8:00-18:00")
            };
        }

        public ICommand StartCommand { get; private set; }

        public IEnumerable<StepModel> Steps { get; private set; }

        public IEnumerable<CustomerService> CustomerServices { get; private set; }
    }

    internal class CustomerService : BaseNotifyModel
    {
        public CustomerService(string imageSource, string subject, string body, string moreInfo, string serviceDuring)
        {
            ImageSource = imageSource;
            Subject = subject;
            Body = body;
            IsBodyNotEmpty = !Body.IsNullOrBlank();
            MoreInfo = moreInfo;
            ServiceDuring = serviceDuring;
            IsFooterNotEmpty = !(MoreInfo.IsNullOrBlank() && ServiceDuring.IsNullOrBlank());
        }

        public string ImageSource { get; private set; }

        public string Subject { get; private set; }

        public string Body { get; private set; }

        public bool IsBodyNotEmpty { get; private set; }

        public bool IsFooterNotEmpty { get; private set; }

        public string MoreInfo { get; private set; }

        public string ServiceDuring { get; private set; }
    }

    internal class StepModel : BaseNotifyModel
    {
        public string Step { get; private set; }
        private bool _unStarted;
        private DateTime _executeTime;

        public StepModel(string step)
        {
            Step = step;
            UnStarted = true;
        }

        public bool UnStarted
        {
            get => this._unStarted;
            set => SetProperty<bool>(ref _unStarted, value);
        }

        public string ExecuteTime
        {
            get => (!UnStarted && this._executeTime != default) ? this._executeTime.GetDateTimeFormats('f')[0].ToString() : string.Empty;
            set => SetProperty<DateTime>(ref _executeTime, DateTime.Parse(value));
        }
    }

    internal enum StepType
    {
        [Description("交易成功")]
        Over,
        [Description("出库")]
        Output,
        [Description("配货")]
        ProvideProduct,
        [Description("付款")]
        Pay,
        [Description("下单")]
        CreateOrder
    }
}
