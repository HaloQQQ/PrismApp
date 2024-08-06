using IceTea.Atom.Contracts;
using IceTea.Atom.Mails;
using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Events;

namespace MyApp.Prisms.ViewModels
{
    internal class Smtp163MailViewModel : SmtpMailViewModelBase
    {
        public Smtp163MailViewModel(IEventAggregator eventAggregator, IConfigManager configManager, ISettingManager settingManager) : base(eventAggregator, configManager, settingManager)
        {
        }

        public override string MailSuffix => "@163.com";

        protected override void InitEmailTransfer()
        {
            base._emailTransfer = new SmtpMail163Manager(null);
        }
    }
}
