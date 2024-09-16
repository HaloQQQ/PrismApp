using IceTea.Atom.Contracts;
using IceTea.Core.Utils.Mails;
using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Events;
using System;

namespace MyApp.Prisms.ViewModels
{
    internal class Smtp163MailViewModel : SmtpMailViewModelBase
    {
        public Smtp163MailViewModel(IEventAggregator eventAggregator, IConfigManager configManager, ISettingManager settingManager) : base(eventAggregator, configManager, settingManager)
        {
            this.TargetFolders = Enum.GetNames<Enum163MailOtherFolder>();
        }

        public override string MailSuffix => "@163.com";

        protected override void InitEmailManager()
        {
            var manager = new SmtpIMAP163Manager();

            base._emailManager = manager;
            base._imapClient = manager;
        }
    }
}
