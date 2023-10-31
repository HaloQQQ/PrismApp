using IceTea.Atom.Contracts;
using IceTea.Atom.Mails;
using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Events;

namespace MyApp.Prisms.ViewModels
{
    internal class SmtpQQMailViewModel : SmtpMailViewModelBase
    {
        public SmtpQQMailViewModel(IEventAggregator eventAggregator, IConfigManager configManager) : base(eventAggregator, configManager)
        {
        }

        public override string MailSuffix => "@qq.com";

        protected override void InitEmailTransfer()
        {
            base._emailTransfer = new SmtpMailQQTransfer(null);
        }
    }
}
