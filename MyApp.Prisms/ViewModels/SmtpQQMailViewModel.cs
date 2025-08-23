using IceTea.Pure.Contracts;
using IceTea.Core.Utils.Mails;
using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Events;
using System;

namespace MyApp.Prisms.ViewModels
{
    internal class SmtpQQMailViewModel : SmtpMailViewModelBase
    {
        public SmtpQQMailViewModel(IEventAggregator eventAggregator, IConfigManager configManager, ISettingManager settingManager) : base(eventAggregator, configManager, settingManager)
        {
            this.TargetFolders = Enum.GetNames<EnumQQMailOtherFolder>();
        }

        public override string MailSuffix => "@qq.com";

        protected override void InitEmailManager()
        {
            var manager = new SmtpIMAPQQManager();

            base._emailManager = manager;
            base._imapClient = manager;
        }
    }
}
