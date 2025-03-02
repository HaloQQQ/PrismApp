using Prism.Commands;
using Prism.Events;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using IceTea.Atom.Extensions;
using IceTea.Atom.Contracts;
using System.Collections.Generic;
using IceTea.Atom.Mails;
using IceTea.Wpf.Atom.Utils;
using System;
using IceTea.Atom.BaseModels;
using PrismAppBasicLib.Contracts;
using IceTea.Core.Contracts;
using IceTea.Core.Utils.Mails;

namespace MyApp.Prisms.ViewModels.BaseViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal abstract class SmtpMailViewModelBase : BaseNotifyModel
    {
        protected IEmailManager _emailManager;
        protected IMAPEmailReceiver _imapClient;

        /// <summary>
        /// 初始化IEmailManager
        /// </summary>
        protected abstract void InitEmailManager();

        public IEnumerable<string> TargetFolders { get; protected set; }

        public IEnumerable<string> MessageFlags { get; }

        public abstract string MailSuffix { get; }

        protected string FromMail => this.From + this.MailSuffix;

        public SmtpMailViewModelBase(IEventAggregator eventAggregator, IConfigManager configManager, ISettingManager settingManager)
        {
            _eventAggregator = eventAggregator;
            _configManager = configManager;
            _settingManager = settingManager;

            this.MessageFlags = Enum.GetNames<EnumSearch>();

            this.InitEmail();

            this.InitCommands();
        }

        protected void InitEmail()
        {
            this.InitEmailManager();

            _emailManager.ExceptionOccured += (sender, ex) => CommonUtil.PublishMessage(_eventAggregator, ex.Message);
            _emailManager.SendCompletedEventHandler += (sender, e) =>
            {
                CommonUtil.PublishMessage(_eventAggregator, "发送完成");

                CommonAtomUtils.BeginInvoke(this.Reset);
            };

            MailOutDto.SelectStatusChanged += newValue =>
            {
                RaisePropertyChanged(nameof(NotUse));

                if (newValue != IsMailsSelectedAll)
                {
                    if (newValue && Mails.Any(m => !m.IsSelected))
                    {
                        return;
                    }

                    _isMailsSelectedAll = newValue;

                    RaisePropertyChanged(nameof(IsMailsSelectedAll));
                }
            };
        }

        private void InitCommands()
        {
            SetFlagCommand = new DelegateCommand<IList<string>>(async strList =>
            {
                var list = this.Mails.Where(m => m.IsSelected);

                if (list.Any())
                {
                    var flag = Enum.Parse<EnumMessageFlags>(strList[0]);
                    var addOrRemove = bool.Parse(strList[1]);

                    await _imapClient.MarkFlagsAsync(list.Select(m => m.Id.To<uint>()), flag, CurrentFolder, addOrRemove);

                    if (flag != EnumMessageFlags.Deleted)
                    {
                        list.ForEach(m => m.IsSelected = false);
                    }
                    else
                    {
                        this.QueryMailCommand.Execute(null);
                    }
                }
            }, _ => Mails.Any(m => m.IsSelected))
               .ObservesProperty(() => NotUse);

            SelectAllCommand = new DelegateCommand(() => { }, () => this.Mails.Count > 0).ObservesProperty(() => this.Mails.Count);

            SendMailCommand = new DelegateCommand(async () =>
            {
                var dto = new MailInDto(this.FromMail.FillToArray(), this.Tos, this.Subject, Password, this.Body, string.Empty);

                this.Ccs.ForEach(c => dto.TryAddCC(c));
                this.Bccs.ForEach(bc => dto.TryAddBCC(bc));

                dto.Attachments.AddRange(Attachments.Select(filePath => new System.Net.Mail.Attachment(filePath)));

                await _emailManager.SendAsync(dto);
            }, () => !Password.IsNullOrBlank()
                    && !this.Tos.IsNullOrEmpty()
                    && !this.Subject.IsNullOrBlank()
                    && !this.Body.IsNullOrBlank()
                    && !this.IsLoading
                    )
                    .ObservesProperty(() => this.From)
                    .ObservesProperty(() => this.Tos)
                    .ObservesProperty(() => this.Subject)
                    .ObservesProperty(() => this.Body)
                    .ObservesProperty(() => this.IsLoading);

            QueryMailCommand = new DelegateCommand(async () =>
            {
                this.IsLoading = true;

                _emailManager.SetCredentials(this.FromMail, this.Password);

                var result = await _imapClient.SearchMessageAsync(Enum.Parse<EnumSearch>(CurrentFlag), CurrentFolder);

                if (!result.IsSucceed)
                {
                    CommonUtil.PublishMessage(_eventAggregator, result.ErrorMessage);
                }
                else
                {
                    this.IsMailsSelectedAll = false;

                    this.Mails.Clear();
                    this.Mails.AddRange(result.PopMails);
                }

                this.IsLoading = false;
            }, () => !Password.IsNullOrBlank() && !this.IsLoading)
                .ObservesProperty(() => this.From)
                .ObservesProperty(() => this.IsLoading);

            DeleteCommand = new DelegateCommand(async () =>
            {
                var list = this.Mails.Where(m => m.IsSelected).ToList();

                if (list.Any())
                {
                    var faileds = await _imapClient.DeleteAsync(list.Select(m => m.Id.To<uint>()), CurrentFolder);

                    list.RemoveAll(m => faileds.Contains(m.Id.To<uint>()));

                    this.Mails.RemoveAll(m => list.Contains(m));
                }
            }, () => Mails.Any(m => m.IsSelected))
               .ObservesProperty(() => NotUse);

            DeleteDeletionCommand = new DelegateCommand(async () =>
            {
                _emailManager.SetCredentials(this.FromMail, this.Password);

                await _imapClient.DeleteAsync(CurrentFolder);

                this.QueryMailCommand.Execute(null);
            },
            () => !Password.IsNullOrBlank())
                .ObservesProperty(() => From);

            MoveToCommand = new DelegateCommand<string>(async targetFolder =>
            {
                var list = this.Mails.Where(m => m.IsSelected).ToList();

                if (list.Any())
                {
                    await _imapClient.MoveToAsync(list.Select(m => m.Id.To<uint>()), targetFolder, CurrentFolder);
                    list.ForEach(m => m.IsSelected = false);

                    this.QueryMailCommand.Execute(null);
                }
            }, _ => Mails.Any(m => m.IsSelected))
            .ObservesProperty(() => NotUse);

            CopyToCommand = new DelegateCommand<string>(async targetFolder =>
            {
                var list = this.Mails.Where(m => m.IsSelected).ToList();

                if (list.Any())
                {
                    await _imapClient.CopyToAsync(list.Select(m => m.Id.To<uint>()), targetFolder, CurrentFolder);
                    list.ForEach(m => m.IsSelected = false);
                }
            }, _ => Mails.Any(m => m.IsSelected))
            .ObservesProperty(() => NotUse);
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
        public ICommand SetFlagCommand { get; private set; }

        public ICommand SelectAllCommand { get; private set; }

        public ICommand MoveToCommand { get; private set; }
        public ICommand CopyToCommand { get; private set; }

        /// <summary>
        /// 删除指定的标记删除的消息
        /// </summary>
        public ICommand DeleteCommand { get; private set; }
        /// <summary>
        /// 删除标记为删除的消息
        /// </summary>
        public ICommand DeleteDeletionCommand { get; private set; }

        public ICommand SendMailCommand { get; private set; }
        public ICommand QueryMailCommand { get; private set; }
        #endregion

        #region Fields
        private readonly IEventAggregator _eventAggregator;
        private readonly IConfigManager _configManager;
        private readonly ISettingManager _settingManager;

        private bool _isLoading;
        private bool _isMailsSelectedAll;

        private string _from;
        private string _subject;
        private string _body;
        #endregion

        #region Props
        public bool NotUse { get; }

        private string _currentFolder;
        public string CurrentFolder
        {
            get => _currentFolder;
            set => SetProperty(ref _currentFolder, value);
        }

        private string _currentFlag;
        public string CurrentFlag
        {
            get => _currentFlag;
            set => SetProperty(ref _currentFlag, value);
        }

        private string _subFolderName;
        public string SubFolderName
        {
            get => _subFolderName;
            set => SetProperty(ref _subFolderName, value);
        }

        public bool IsLoading
        {
            get => this._isLoading;
            set => SetProperty<bool>(ref _isLoading, value);
        }

        public bool IsMailsSelectedAll
        {
            get => _isMailsSelectedAll;
            set
            {
                if (SetProperty(ref _isMailsSelectedAll, value))
                {
                    this.Mails.ForEach(i => i.IsSelected = value);
                }
            }
        }

        public ObservableCollection<MailOutDto> Mails { get; } = new();


        public string From
        {
            get => _from;
            set => SetProperty(ref _from, value);
        }

        public string Password
        {
            get
            {
                var fromMail = this.FromMail;
                if (!this._settingManager.TryGetValue(fromMail, out var password))
                {
                    return string.Empty;
                }

                return password;
            }
        }

        public ObservableCollection<string> Tos { get; } = new();

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Body
        {
            get => _body;
            set => SetProperty(ref _body, value);
        }

        public ObservableCollection<string> Ccs { get; } = new();

        public ObservableCollection<string> Bccs { get; } = new();

        public ObservableCollection<string> Attachments { get; } = new();
        #endregion
    }
}
