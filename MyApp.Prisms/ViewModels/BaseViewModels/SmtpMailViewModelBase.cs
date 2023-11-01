using IceTea.Atom.Mails.Contracts;
using IceTea.Atom.Mails.Dtos;
using MusicPlayerModule.MsgEvents;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using IceTea.Atom.Contracts;
using MyApp.Prisms.Helper;
using System.Collections.Generic;

namespace MyApp.Prisms.ViewModels.BaseViewModels
{
    internal abstract class SmtpMailViewModelBase : BindableBase
    {
        protected IEmailTransfer _emailTransfer;

        protected abstract void InitEmailTransfer();

        protected void Init()
        {
            this.InitEmailTransfer();

            _emailTransfer.ExceptionOccured += ex => PublishMessage(ex.Message);
            _emailTransfer.SendCompletedEventHandler += (sender, e) => { PublishMessage("发送完成"); this.Reset(); };
        }

        protected virtual string AppendSuffix(string number)
        {
            return number + MailSuffix;
        }

        public abstract string MailSuffix { get; }

        public SmtpMailViewModelBase(IEventAggregator eventAggregator, IConfigManager configManager)
        {
            this.Init();
            _eventAggregator = eventAggregator;
            _configManager = configManager;

            SendMailCommand = new DelegateCommand(() =>
            {
                _emailTransfer.SendInMailAsync(new Mail(AppendSuffix(From), Tos, Subject, Password, Body, false)
                {
                    CCList = Ccs,
                    BCCList = Bccs,
                    AttachmentList = Attachments.Select(filePath => new System.Net.Mail.Attachment(filePath))
                });
            }, () => !this.From.IsNullOrBlank()
                    && !this.Tos.IsNullOrEmpty()
                    && !this.Subject.IsNullOrBlank()
                    && !this.Body.IsNullOrBlank()
                    )
                    .ObservesProperty(() => this.From)
                    .ObservesProperty(() => this.Tos)
                    .ObservesProperty(() => this.Subject)
                    .ObservesProperty(() => this.Body);

            QueryMailCommand = new DelegateCommand(() =>
            {
                var result = _emailTransfer.GetMailMessage(this.AppendSuffix(this.From), this.Password);

                if (!result.IsSucceed)
                {
                    PublishMessage(result.ErrorMessage);
                }

                this.Mails = result.PopMails;
            }, () => !this.From.IsNullOrBlank())
                .ObservesProperty(() => this.From);
        }

        private void PublishMessage(string message)
        {
            _eventAggregator.GetEvent<DialogMessageEvent>().Publish(new IceTea.Wpf.Core.Contracts.DialogMessage(message));
        }

        private void Reset()
        {
            this.Tos.Clear();
            this.Ccs.Clear();
            this.Bccs.Clear();
            this.Attachments.Clear();

            this.Subject = string.Empty;
            this.Body = string.Empty;
        }

        #region Commands
        public ICommand SendMailCommand { get; private set; }
        public ICommand QueryMailCommand { get; private set; }
        #endregion

        #region Props
        public IEnumerable<PopMail> _mails;
        public IEnumerable<PopMail> Mails
        {
            get => this._mails;
            set => SetProperty<IEnumerable<PopMail>>(ref _mails, value);
        }

        public string Password
        {
            get
            {
                var str = this._configManager.ReadConfigNode(CustomConstants.MailAccounts);

                if (str.IsNullOrBlank())
                {
                    PublishMessage("未找到任何邮箱相关配置信息，请配置完成并重启程序后重试");
                    return string.Empty;
                }

                var pairs = str.DeserializeObject<IEnumerable<KeyValuePair<string, string>>>();

                var fromMail = this.AppendSuffix(From);
                if (pairs != null)
                {
                    foreach (var item in pairs)
                    {
                        if (fromMail.EqualsIgnoreCase(item.Key))
                        {
                            return item.Value;
                        }
                    }
                }

                PublishMessage($"未找到{fromMail}邮箱相关配置信息，请配置完成并重启程序后重试");

                return string.Empty;
            }
        }

        private string _from;

        public string From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        private ObservableCollection<string> _tos = new();

        public ObservableCollection<string> Tos
        {
            get => _tos;
            set => SetProperty(ref _tos, value);
        }

        private string _subject;

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        private string _body;

        public string Body
        {
            get => _body;
            set => SetProperty(ref _body, value);
        }


        private ObservableCollection<string> _ccs = new();

        public ObservableCollection<string> Ccs
        {
            get => _ccs;
            set => SetProperty(ref _ccs, value);
        }

        private ObservableCollection<string> _bccs = new();

        public ObservableCollection<string> Bccs
        {
            get => _bccs;
            set => SetProperty(ref _bccs, value);
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigManager _configManager;

        private ObservableCollection<string> _attachments = new();
        public ObservableCollection<string> Attachments
        {
            get => _attachments;
            set => SetProperty(ref _attachments, value);
        }

        #endregion
    }
}
