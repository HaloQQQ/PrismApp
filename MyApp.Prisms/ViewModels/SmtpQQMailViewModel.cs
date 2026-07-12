using MyApp.Prisms.ViewModels.BaseViewModels;
using Prism.Events;
using System;
using IceTea.Pure.Businesses.Config;
using IceTea.Pure.Businesses.Setting;
using IceTea.Core.Businesses.Mail;

namespace MyApp.Prisms.ViewModels
{
    internal class SmtpQQMailViewModel : SmtpMailViewModelBase
    {
        public SmtpQQMailViewModel(IEventAggregator eventAggregator, IConfigManager configManager, ISettingManager settingManager) : base(eventAggregator, configManager, settingManager)
        {
            this.TargetFolders = Enum.GetNames(typeof(EnumQQMailOtherFolder));
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
