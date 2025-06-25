

# PrismApp

## 介绍

PrismApp 是一个基于 Prism 框架的 WPF 应用程序，包含了多个模块：

- **CustomControlsDemoModule**: 演示了自定义控件的用法，包含按钮、文本框、面板等 UI 控件示例
- **MusicPlayerModule**: 实现了音乐播放器功能，支持播放控制、歌词显示、桌面歌词等特性
- **SqlCreatorModule**: 提供数据库结构导出功能，可以将数据库表结构导出为 C# 模型类

## 软件架构

项目采用模块化设计，使用 Prism 桌面框架进行模块管理和导航：

- **MyApp.Prisms**: 主应用程序，包含主窗口和全局设置
- **CustomControlsDemoModule**: 自定义控件演示模块
- **MusicPlayerModule**: 音乐播放器模块，包含播放控制、播放列表、歌词显示等
-Visible: public partial class WindowTitleView : UserControl
- **SqlCreatorModule**: 数据库结构导出模块，支持多种数据库类型

使用的技术和模式：
- Prism 框架（用于模块化、导航、事件聚合）
- MVVM 模式（分离视图和视图模型）
- Entity Framework（数据持久化）
- .NET 6（目标框架）

## 使用说明

1. **启动应用**:
   - 运行主程序，加载模块并初始化界面

2. **使用自定义控件**:
   - 在 `CustomControlsDemoModule` 中可以查看各种自DefinedColorBrush RecvBrush
    - internal static readonly ResourceDictionary Light
    - internal static readonly ResourceDictionary Dark
    - internal static readonly string[] WindowCornerRadius
    - internal static readonly string[] MailAccounts
    - internal static readonly string[] ConfigGlobalHotkeys
    - internal static class GlobalHotKeysConst
      - internal const string TogglePlay
      - internal const string Prev
      - internal const string Next
      - internal const string FastForward
      - internal const string Rewind
      - internal const string IncreaseVolume
      - internal const string DecreaseVolume
      - internal const string UpScreenBright
      - internal const string DownScreenBright
      - internal const string MusicLyricDesktop
      - internal const string ColorPicker
    - internal class GlobalHotKeyInfo
      - public GlobalHotKeyInfo(string name, CustomModifierKeys customModifierKeys, CustomKeys customKeys, bool isUsable = true)
      - public string Name { get; }
      - public CustomModifierKeys CustomModifierKeys { get; }
      - public CustomKeys CustomKeys { get; }
      - public bool IsUsable { get; }
    - internal static GlobalHotKeyInfo[] GlobalHotKeys

### MyApp.Prisms/Helper/Helper.cs
namespace MyApp.Prisms.Helper
  - internal static partial class Helper
    - internal static bool IsInPopup(FrameworkElement element)

### MyApp.Prisms/Helper/TextAreaHelper.cs
namespace MyApp.Prisms.Helper
  - internal static partial class Helper
    - private static Paragraph GetParagraph(string type, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
    - internal static void Send(this RichTextBox txt, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
    - internal static void Recv(this RichTextBox txt, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)

### MyApp.Prisms/Models/ProcessContext.cs
namespace MyApp.Prisms.Models
  - public class ProcessContext : BaseNotifyModel
    - private bool _isChecked
    - public bool IsChecked
        {
            get => _isChecked;
            set
    - public int Id { get; set; }
    - public string Name { get; set; }
    - public static event Action<bool> SelectStatusChanged;
    - public ProcessContext(Process process)
  - public class ServiceContext : IDisposable
    - public string ServiceName { get;  }
    - public string DisplayName { get; }
    - public string ServiceControllerStatus { get;  }
    - public string ServiceType { get; }
    - public ServiceController ServiceController { get; private set; }
    - public ServiceContext(ServiceController service)
    - public void Dispose()

### MyApp.Prisms/MsgEvents/BackgroundImageSelectorShowEvent.cs
namespace MyApp.Prisms.MsgEvents
  - public class BackgroundImageSelectorShowEvent : PubSubEvent

### MyApp.Prisms/MsgEvents/BackgroundImageUpdateEvent.cs
namespace MyApp.Prisms.MsgEvents
  - internal class BackgroundImageUpdateEvent : PubSubEvent<string>

### MyApp.Prisms/MsgEvents/SwitchThemeEvent.cs
namespace MyApp.Prisms.MsgEvents
  - internal class SwitchThemeEvent : PubSubEvent<bool>

### MyApp.Prisms/MsgEvents/UpdateScreenBrightEvent.cs
namespace MyApp.Prisms.MsgEvents
  - internal class UpdateScreenBrightEvent : PubSubEvent<int>

### MyApp.Prisms/MyApp.Prisms.csproj
[NO MAP]

### MyApp.Prisms/Resources/MyDataTemplate.xaml
[NO MAP]

### MyApp.Prisms/Resources/MyDictionary.xaml
[NO MAP]

### MyApp.Prisms/Resources/Penguin.ico
[NO MAP]

### MyApp.Prisms/Resources/apple.ico
[NO MAP]

### MyApp.Prisms/ViewModels/AnotherTcpServerViewModel.cs
namespace MyApp.Prisms.ViewModels
  - internal class AnotherTcpServerViewModel : TcpServerViewModel
    - public AnotherTcpServerViewModel(IConfigManager config, string name = "程序服务器") : base(config, name)

### MyApp.Prisms/ViewModels/BaseViewModels/BaseSocketViewModel.cs
namespace MyApp.Prisms.ViewModels.BaseViewModels
  - internal abstract class BaseSocketViewModel : BaseNotifyModel
    - private ISocket _socket
    - public ISocket Socket
        {
            get => this._socket;
            protected set => SetProperty<ISocket>(ref _socket, value);
        }
    - public BaseSocketViewModel(IConfigManager config, string name)
    - private string _sendMessage
    - public string Name { get; protected set; } = "Socket";
    - private bool _isLogging
    - protected ushort _port
    - public uint MaxMessageLength { get; set; } = 256;
    - public Encoding Encoding { get; set; } = Encoding.UTF8;
    - public bool IsHex { get; set; }
    - public bool IsLogging
        {
            get => this._isLogging;
            set => SetProperty<bool>(ref _isLogging, value);
        }
    - public string Ip { get; set; }
    - public string Port { get; set; } = "50000";
    - public string SendMessage
        {
            get => this._sendMessage;
            set => SetProperty<string>(ref _sendMessage, value);
        }
    - public ICommand ConnectCommand { get; protected set; }
    - public ICommand SendCommand { get; protected set; }
    - public ICommand OpenLogCommand { get; private set; }
    - protected abstract bool InitSocket();
    - protected void Connect()
    - public static IEnumerable<string> Ips { get; } =
            AppUtils.GetIpAddressColl().Select(address => address.ToString());

### MyApp.Prisms/ViewModels/BaseViewModels/SmtpMailViewModelBase.cs
namespace MyApp.Prisms.ViewModels.BaseViewModels
  - internal abstract class SmtpMailViewModelBase : BaseNotifyModel
    - protected IEmailManager _emailManager
    - protected IMAPEmailReceiver _imapClient
    - protected abstract void InitEmailManager();
    - public IEnumerable<string> TargetFolders { get; protected set; }
    - public IEnumerable<string> MessageFlags { get; }
    - public abstract string MailSuffix { get; }
    - protected string FromMail => this.From + this.MailSuffix;
    - public SmtpMailViewModelBase(IEventAggregator eventAggregator, IConfigManager configManager, ISettingManager settingManager)
    - protected void InitEmail()
    - private void InitCommands()
    - private void Reset()
    - public ICommand SetFlagCommand { get; private set; }
    - public ICommand SelectAllCommand { get; private set; }
    - public ICommand MoveToCommand { get; private set; }
    - public ICommand CopyToCommand { get; private set; }
    - public ICommand DeleteCommand { get; private set; }
    - public ICommand DeleteDeletionCommand { get; private set; }
    - public ICommand SendMailCommand { get; private set; }
    - public ICommand QueryMailCommand { get; private set; }
    - private readonly IEventAggregator _eventAggregator
    - private readonly IConfigManager _configManager
    - private readonly ISettingManager _settingManager
    - private bool _isLoading
    - private bool _isMailsSelectedAll
    - private string _from
    - private string _subject
    - private string _body
    - public bool NotUse { get; }
    - private string _currentFolder
    - public string CurrentFolder
        {
            get => _currentFolder;
            value => SetProperty(ref _currentFolder, value);
        }
    - private string _currentFlag
    - public string CurrentFlag
        {
            get => _currentFlag;
            value => SetProperty(ref _currentFlag, value);
        }
    - private string _subFolderName
    - public string SubFolderName
        {
            get => _subFolderName;
            value => SetProperty(ref _subFolderName, value);
        }
    - public bool IsLoading
        {
            get => this._isLoading;
            value => SetProperty<bool>(ref _isLoading, value);
        }
    - public bool IsMailsSelectedAll
        {
            get => _isMailsSelectedAll;
            value
    - public ObservableCollection<MailOutDto> Mails { get; } = new();
    - public string From
        {
            get => _from;
            value => SetProperty(ref _from, value);
        }
    - public string Password
        {
            get
    - public ObservableCollection<string> Tos { get; } = new();
    - public string Subject
        {
            get => _subject;
            value => SetProperty(ref _subject, value);
        }
    - public string Body
        {
            get => _body;
            value => SetProperty(ref _body, value);
        }
    - public ObservableCollection<string> Ccs { get; } = new();
    - public ObservableCollection<string> Bccs { get; } = new();
    - public ObservableCollection<string> Attachments { get; } = new();

### MyApp.Prisms/ViewModels/CommunicationViewModel.cs
namespace MyApp.Prisms.ViewModels
  - internal class CommunicationViewModel : BaseNotifyModel
    - public CommunicationViewModel(
                UdpSocketViewModel udpContext,
                TcpClientViewModel applicationClientContext,
                TcpServerViewModel machineServerContext,
                AnotherTcpServerViewModel applicationServerContext
            )
    - public UdpSocketViewModel UdpContext { get; private set; }
    - public TcpServerViewModel MachineServerContext { get; private; }
    - public AnotherTcpServerViewModel ApplicationServerContext { get; private set; }
    - public TcpClientViewModel ApplicationClientContext { get; private set; }
    - public ISet<TcpServerView> Servers
    - public void AddServer(TcpServerView server)
    - public void TransmitFrom(TcpServerView server, byte[] data)
    - private bool _isTcp
    - public bool IsTcp
        {
            get => this._isTcp;
            set => SetProperty<bool>(ref _isTcp, value);
        }
    - private bool _isServer
    - public bool IsTcpServer
        {
            get => this._isServer;
            value => SetProperty<bool>(ref _isServer, value);
        }
    - private bool _isHex
    - public bool IsHex
        {
            get => this._isHex;
            value
    - public ushort MaxClientsCount
        {
            set

### MyApp.Prisms/ViewModels/ImageDisplayViewModel.cs
namespace MyApp.Prisms.ViewModels
  - internal class MyImage : BaseNotifyModel
    - public bool InList { get; }
    - private bool _selected
    - public bool Selected
        {
            get => this._selected;
            internal set => SetProperty<bool>(ref _selected, value);
        }
    - private BitmapSource? _source
    - public BitmapSource? Source
        {
            get => _source;
            private set => SetProperty(ref _source, value);
        }
    - public bool IsEmpty { get; }
    - public string URI { get; set; } = null!;
    - public string FileType { get; set; } = null!;
    - public string Name { get; set; } = null!;
    - public string Size { get; }
    - internal MyImage()
    - public MyImage(string path)
  - internal class ImageDisplayViewModel : BaseNotifyModel, IDisposable
    - public ObservableCollection<MyImage> Data { get; private set; } = new ObservableCollection<MyImage>();
    - private Random _random
    - public int ImagesCount => Math.Max(this.Data.Count - 1, 0);
    - public void RaisePropertyChangedEvent(string propName)
    - internal string GetRandomImage()
    - internal void SelectImage(string selectedImage)
    - private bool _showInList
    - public bool ShowInList
        {
            get => _showInList;
            value => SetProperty<bool>(ref _showInList, value);
        }
    - public ImageDisplayViewModel(IConfigManager config, ISettingManager<SettingModel> settingManager, IEventAggregator eventAggregator)
    - public ICommand RefreshCommand { get; }
    - private bool _isLoading
    - public bool IsLoading
        {
            get => this._isLoading;
            private value => SetProperty<bool>(ref _isLoading, value);
        }
    - private bool _disposed
    - public void Dispose()

### MyApp.Prisms/ViewModels/ProcessServiceViewModel.cs
namespace MyApp.Prisms.ViewModels
  - internal class ProcessServiceViewModel : BaseNotifyModel, IDisposable
    - public ObservableCollection<ProcessContext> ProcessList { get; private set; } = new ObservableCollection<ProcessContext>();
    - public void RefreshProcesses()
    - public ObservableCollection<ServiceContext> ServiceList { get; private set; } = new ObservableCollection<ServiceContext>();
    - public void RefreshServices()
    - private bool _disposed
    - public void Dispose()
    - public ProcessServiceViewModel()
    - private bool _isSelectedAllProcesses
    - public bool IsSelectedAllProcesses
        {
            get => _isSelectedAllProcesses;
            value
    - public ProcessContext CurrentProcess { get; set; }
    - public ServiceContext CurrentService { get; set; }
    - public ICommand StartServiceCommand { get; private set; }
    - public ICommand StopServiceCommand { get; private set; }
    - public ICommand ReloadServiceCommand { get; private set; }
    - public ICommand StopProcessCommand { get; private set; }
    - public ICommand ReloaProcessCommand { get; private set; }

### MyApp.Prisms/ViewModels/SettingsViewModel.cs
namespace MyApp.Prisms.ViewModels
  - internal class SettingsViewModel : BaseNotifyModel
    - public SettingsViewModel(
                IConfigManager configManager,
                ISettingManager settingManager,
                ISettingManager<SettingModel> settingModels,
                IAppConfigFileHotKeyManager appConfigFileHotKeyManager,
                IEventAggregator eventAggregator,
                IDialogService dialogService
            )
    - public ISettingManager<SettingModel> SettingModels { get; }
    - public IAppConfigFileHotKeyManager AppConfigFileHotKeyManager { get; }
    - private IGlobalConfigFileHotKeyManager _globalConfigFileHotKeyManager
    - public IGlobalConfigFileHotKeyManager GlobaConfigFilelHotKeyManager
        {
            get => _globalConfigFileHotKeyManager;
            internal value => SetProperty(ref _globalConfigFileHotKeyManager, value);
        }
    - private void InitCommands(IEventAggregator eventAggregator, ISettingManager settingManager, IConfigManager configManager, IDialogService dialogService)
    - public Pair CurrentMailPair { get; } = new();
    - public ObservableCollection<Pair> MailAccounts { get; } = new();
    - private void LoadMailAccounts(IConfigManager configManager, ISettingManager settingManager)
    - public ICommand ShowDialogCommand { get; private set; }
    - public ICommand ColorPickerCommand { get; private set; }
    - public ICommand CleanConfigWhenExitAppCommand { get; private set; }
    - public ICommand AddMailAccountCommand { get; private set; }
    - public ICommand RemoveMailAccountCommand { get; private; }
    - public ICommand FindImageDirCommand { get; private set; }
    - public ICommand FindMusicDirCommand { get; private set; }
    - public ICommand FindVideoDirCommand { get; private set; }
    - public ICommand ResetGlobalHotKeyGroupCommand { get; private set; }
    - public ICommand ResetAppHotKeyGroupCommand { get; private set; }
    - public ICommand RestartComputerCommand { get; private set; }
    - public ICommand ShutdownComputerCommand { get; private set; }
    - public ICommand CancelCommand { get; private set; }
    - public ICommand SubmitCommand { get; private set; }
    - private void LoadConfig(IConfigManager configManager, ISettingManager<SettingModel> settingModels)
    - private void InitSetting(IConfigManager configManager, ISettingManager<SettingModel> settingModels, string key, string description, params string[] configNode)
    - private void LoadWindowCornerRadius(IConfigManager configManager)
    - private bool _isLightSysTheme
    - public bool IsLightSysTheme
        {
            get => _isLightSysTheme;
            value
    - private bool _isColorPicker
    - public bool IsColorPicker
        {
            get => _isColorPicker;
            private value => SetProperty(ref _isColorPicker, value);
        }
    - private CornerRadius _cornerRadius
    - public CornerRadius CornerRadius
        {
            get => this._cornerRadius;
            value => SetProperty<CornerRadius>(ref _cornerRadius, value);
        }
    - private bool _isEditingSetting
    - public bool IsEditingSetting
        {
            get => this._isEditingSetting;
            value
  - internal class Pair : BaseNotifyModel
    - public Pair()
    - public Pair(string key, string value)
    - private string _key
    - public string Key
        {
            get => this._key;
            value => SetProperty<string>(ref _key, value);
        }
    - private string _value
    - public string Value
        {
            get => this._value;
            value => SetProperty<string>(ref _value, value);
        }
    - public void Clear()

### MyApp.Pris; internal class SwitchThemeEvent : PubSubEvent<bool> prism 模块和功能演示。

3. **音乐播放器**:
   - 在 `MusicPlayerModule` 中可以播放音乐、设置播放列表、调整音量、显示歌词等

4. **数据库结构导出**:
   - 使用 `SqlCreatorModule` 可以连接数据库并导出表结构为 C# 模型类
   - 支持多种数据库类型（MySQL、SQL Server、SQLIite 签署事件

### MyApp.Prisms/MsgEvents/BackgroundImageUpdateEvent.cs
namespace MyApp.Prisms.MsgEvents
  - internal class BackgroundImageUpdateEvent : PubSubEvent<string>  - public override void OnInitialized()

### README.md
PrismApp
介绍
软件架构
使用说明

### SqlCreatorModule/DBConfig.xml
[NO MAP]

### SqlCreatorModule/ExportModels/Abpauditlogs.cs
namespace SqlCreatorModule.ExportModels
  - [Table("abpauditlogs")]
    public class Abpauditlogs : BaseModel, ICloneable
    - [Column("Id")]
        public Guid Id { get; set; }
    - [Column("ApplicationName")]
        public String ApplicationName { get; set; }
    - [Column("UserId")]
        public Guid UserId { get; set; }
    - [Column("UserName")]
        public String UserName { get; set; }
    - [Column("TenantId")]
        public Guid TenantId { get; set; }
    - [Column("TenantName")]
        public String TenantName { get; set; }
    - [Column("ImpersonatorUserId")]
        public Guid ImpersonatorUserId { get; set; }
    - [Column("ImpersonatorUserName")]
        public String ImpersonatorUserName { get; set; }
    - [Column("ImpersonatorTenantId")]
        public Guid ImpersonatorTenantId { get; set; }
    - [Column("ImpersonatorTenantName")]
        public String ImpersonatorTenantName { get; set; }
    - [Column("ExecutionTime")]
        public DateTime ExecutionTime { get; set; }
    - [Column("ExecutionDuration")]
        public Int32 ExecutionDuration { get; set; }
    - [Column("ClientIpAddress")]
        public String ClientIpAddress { get; set; }
    - [Column("ClientName")]
        public String ClientName { get; set; }
    - [Column("ClientId")]
        public String ClientId { get; set; }
    - [Column("CorrelationId")]
        public String CorrelationId { get; set; }
    - [Column("BrowserInfo")]
        public String BrowserInfo { get; set; }
    - [Column("HttpMethod")]
        public String HttpMethod { get; set; }
    - [Column("Url")]
        public String Url { get; set; }
    - [Column("Exceptions")]
        public String Exceptions { get; set; }
    - [Column("Comments")]
        public String Comments { get; set; }
    - [Column("HttpStatusCode")]
        public Int32 HttpStatusCode { get; set; }
    - [Column("ExtraProperties")]
        public String ExtraProperties { get; set; }
    - [Column("ConcurrencyStamp")]
        public String ConcurrencyStamp { get; set; }
    - public virtual string GetSelectCmdText()
    - public override string ToString()

### SqlCreatorModule/ExportModels/Course.cs
namespace SqlCreatorModule.ExportModels
  - [Table("Course")]
    public class Course : BaseModel, ICloneable
    - [Column("CourseNo")]
        public Int32 CourseNo { get; set; }
    - [Column("CourseName")]
        public String? CourseName { get; set; }
    - [Column("CourseScore")]
        public Decimal CourseScore { get; set; }
    - [Column("CourseMakerName")]
        public String? CourseMakerName { get; set; }
    - public virtual string GetSelectCmdText()
    - public override string ToString()

### SqlCreatorModule/ExportModels/Module.cs
namespace SqlCreatorModule.ExportModels
  - [Table("Module")]
    public class Module : BaseModel, ICloneable
    - [Column("id")]
        public Int64 Id { get; set; }
    - [Column("group_name")]
        public String? GroupName { get; set; }
    - public virtual string GetSelectCmdText()
    - public override string ToString()

### SqlCreatorModule/ExportModels/Mysqldatatype.cs
namespace SqlCreatorModule.ExportModels
  - [Table("mysqldatatype")]
    public class Mysqldatatype : BaseModel, ICloneable
    - [Column("BigInt")]
        public Int64 BigInt { get; set; }
    - [Column("BigIntUnsigned")]
        public UInt64 BigIntUnsigned { get; set; }
    - [Column("Binary")]
        public Byte[]? Binary { get; set; }
    - [Column("Bit")]
        public UInt64 Bit { get; set; }
    - [Column("Blob")]
        public Byte[]? Blob { get; set; }
    - [Column("Bool")]
        public Boolean Bool { get; set; }
    - [Column("Char")]
        public String? Char { get; set; }
    - [Column("Date")]
        public DateTime Date { get; set; }
    - [Column("DateTime")]
        public DateTime DateTime { get; set; }
    - [Column("Decimal")]
        public Decimal Decimal { get; set; }
    - [Column("Double")]
        public Double Double { get; set; }
    - [Column("DoublePrecision")]
        public Double DoublePrecision { get; set; }
    - [Column("Float")]
        public Single Float { get; set; }
    - [Column("Int")]
        public Int32 Int { get; set; }
    - [Column("IntUnsigned")]
        public UInt32 IntUnsigned { get; set; }
    - [Column("LongVarBinary")]
        public Byte[]? LongVarBinary { get; set; }
    - [Column("LongVarChar")]
        public String? LongVarChar { get; set; }
    - [Column("LongBlob")]
        public Byte[]? LongBlob { get; set; }
    - [Column("LongText")]
        public String? LongText { get; set; }
    - [Column("MedimumBlob")]
        public Byte[]? MedimumBlob { get; set; }
    - [Column("MedimumInt")]
        public Int32 MedimumInt { get; set; }
    - [Column("MedimumUnsigned")]
        public UInt32 MedimumUnsigned { get; set; }
    - [Column("MediumText")]
        public String? MediumText { get; set; }
    - [Column("Numeric")]
        public Decimal Numeric { get; set; }
    - [Column("Real")]
        public Double Real { get; set; }
    - [Column("SmallInt")]
        public Int16 SmallInt { get; set; }
    - [Column("SmallIntUnsigned")]
        public UInt16 SmallIntUnsigned { get; set; }
    - [Column("Text")]
        public String? Text { get; set; }
    - [Column("Time")]
        public TimeSpan Time { get; set; }
    - [Column("TimeStamp")]
        public DateTime TimeStamp { get; set; }
    - [Column("TinyBlob")]
        public Byte[]? TinyBlob { get; set; }
    - [Column("TinyInt")]
        public SByte TinyInt { get; set; }
    - [Column("TinyIntUnsigned")]
        public Byte TinyIntUnsigned { get; set; }
    - [Column("TinyText")]
        public String? TinyText { get; set; }
    - [Column("VarBinary")]
        public Byte[]? VarBinary { get; set; }
    - [Column("VarChar")]
        public String? VarChar { get; set; }
    - [Column("Year")]
        public Int16 Year { get; set; }
    - [Column("Json")]
        public String? Json { get; set; }
    - public virtual string GetSelectCmdText()
    - public override string ToString()

### SqlCreatorModule/ExportModels/Student.cs
namespace SqlCreatorModule.ExportModels
  - [Table("student")]
    public class Student : BaseModel, ICloneable
    - [Column("id")]
        public Int64 Id { get; set; }
    - [Column("name")]
        public String? Name { get; set; }
    - [Column("age")]
        public Int16 Age { get; set; }
    - [Column("male")]
        public Boolean Male { get; set; }
    - [Column("tid")]
        public Int64 Tid { get; set; }
    - [Column("address")]
        public String? Address { get; set; }
    - [Column("last_access_time")]
        public DateTime LastAccessTime { get; set; }
    - public virtual string GetSelectCmdText()
    - public override string ToString()

### SqlCreatorModule/ExportModels/Teacher.cs
namespace SqlCreatorModule.ExportModels
  - [Table("Teacher")]
    public class Teacher : BaseModel, ICloneable
    - [Column("TeacherNo")]
        public Int32 TeacherNo { get; set; }
    - [Column("TeacherName")]
        public String? TeacherName { get; set; }
    - [Column("TeacherSex")]
        public String? TeacherSex { get; set; }
    - [Column("TeacherBirthDate")]
        public DateTime TeacherBirthDate { get; set; }
    - [Column("TeacherId")]
        public String? TeacherId { get; set; }
    - [Column("TeacherAddress")]
        public String? TeacherAddress { get; set; }
    - [Column("TeacherTel")]
        public String? TeacherTel { get; set; }
    - [Column("TeacherDepartment")]
        public String? TeacherDepartment { get; set; }
    - [Column("TeacherTime")]
        public Int32 TeacherTime { get; set; }
    - [Column("TeacherParty")]
        public String? TeacherParty { get; set; }
    - [Column("TeacherUserPwd")]
        public String? TeacherUserPwd { get; set; }
    - [Column("TeacherMgrNow")]
        public DateTime TeacherMgrNow { get; set; }
    - public virtual string GetSelectCmdText()
    - public override string ToString()

### SqlCreatorModule/Models/DataColumnInfoModel.cs
namespace SqlCreatorModule.Models
  - internal class DataColumnInfoModel : IEquatable<DataColumnInfoModel>
    - public DataColumnInfoModel(DataColumn column)
    - public PropertyCollection ExtendedProperties { get; }
    - public string Caption { get; }
    - public string ColumnName { get; }
    - public string DataType { get; }
    - public string DbDataType { get; set; }
    - public string Comment { get; set; }
    - public string DefaultValue { get; }
    - public bool Unique { get; }
    - public bool AllowDBNull { get; }
    - public bool AutoIncrement { get; }
    - public long AutoIncrementSeed { get; }
    - public long AutoIncrementStep { get; }
    - public int MaxLength { get; }
    - public bool ReadOnly { get; }
    - public DataSetDateTime DateTimeMode { get; }
    - public string Namespace { get; }
    - public string Expression { get; }
    - public bool Equals(DataColumnInfoModel? other)

### SqlCreatorModule/SqlCreatorModule.cs
namespace SqlCreatorModule
  - public class SqlCreatorModule : IModule
    - private readonly IRegionManager _regionManager
    - public SqlCreatorModule(IRegionManager regionManager)
    - public void OnInitialized(IContainerProvider containerProvider)
    - public void RegisterTypes(IContainerRegistry containerRegistry)

### SqlCreatorModule/SqlCreatorModule.csproj
[NO MAP]

### SqlCreatorModule/ViewModels/CreateModelViewModel.cs
namespace SqlCreatorModule.ViewModels
  - internal class CreateModelViewModel : BindableBase
    - public CreateModelViewModel(IEventAggregator eventAggregator)
    - private IDb GetSqliteDb(bool openFileDialog = true)
    - private IDb GetDb()
    - private IList<DataColumnInfoModel> _tableColumnsStructure
    - public IList<DataColumnInfoModel> TableColumnsStructure
        {
            get => this._tableColumnsStructure;
        }
    - public ICommand OpenExportDirCommand { get; }
    - public ICommand ExportTableStructureToFileCommand { get; }
    - public ICommand ShowTableStructureCommand { get; }
    - public ICommand GetTablesCommand { get; }
    - public ICommand ConnectCommand { get; }
    - private string _modelExportDir
    - public string ModelExportDir
        {
            get => this._modelExportDir;
            set => SetProperty<string>(ref _modelExportDir, value);
        }
    - private string _currentTableName
    - public string CurrentTableName
        {
            get => this._currentTableName;
            set => SetProperty<string>(ref _currentTableName, value);
        }
    - private IEnumerable<string> _tableNames
    - public IEnumerable<string> TableNames
        {
            get => this._tableNames;
            set => SetProperty<IEnumerable<string>>(ref _tableNames, value);
        }
    - private string _currentDbName
    - public string CurrentDbName
        {
            get => this._currentDbName;
            value => SetProperty<string>(ref _currentDbName, value);
        }
    - private IEnumerable<string> _dbNames
    - public IEnumerable<string> DbNames
        {
            get => this._dbNames;
            value => SetProperty<IEnumerable<string>>(ref _dbNames, value);
        }
    - private string _currentDbType
    - public string CurrentDbType
        {
            get => this._currentDbType;
            value
    - private bool _isMysql
    - public bool IsMysql
        {
            get => this._isMysql;
            value => SetProperty<bool>(ref _isMysql, value);
        }
    - private bool _isSqlServer
    - public bool IsSqlServer
        {
            get => this._isSqlServer;
            value => SetProperty<bool>(ref _isSqlServer, value);
        }
    - private bool _isSqlite
    - public bool IsSqlite
        {
            get => this._isSqlite;
            value => SetProperty<bool>(ref _isSqlite, value);
        }
    - private bool _isPostgresql
    - public bool IsPostgresql
        {
            get => this._isPostgresql;
            value => SetProperty<bool>(ref _isPostgresql, value);
        }
    - private string _host
    - public string Host
        {
            get => this._host;
            value => SetProperty<string>(ref _host, value);
        }
    - private string _port
    - public string Port
        {
            get => this._port;
            value => SetProperty<string>(ref _port, value);
        }
    - private string _uid
    - public string Uid
        {
            get => this._uid;
            value => SetProperty<string>(ref _uid, value);
        }
    - private string _pwd
    - public string Pwd
        {
            get => this._pwd;
            value => SetProperty<string>(ref _pwd, value);
        }
    - private string _dbFilePath
    - public string DbFilePath
        {
            get => this._dbFilePath;
            value => SetProperty<string>(ref _dbFilePath, value);
        }

### SqlCreatorModule/Views/CreateModelView.xaml
[NO MAP]

### SqlCreatorModule/Views/CreateModelView.xaml.cs
namespace SqlCreatorModule.Views
  - public partial class CreateModelView : UserControl
    - public CreateModelView()

### SqlCreatorModule/Views/ModelView.xaml
[NO MAP]

### SqlCreatorModule/Views/ModelView.xaml.cs
namespace SqlCreatorModule.Views
  - public partial class ModelView : UserControl
    - public ModelView()

### SqlCreatorModule/Views/ModelViewBase.cs
namespace SqlCreatorModule.Views
  - public abstract class ModelViewBase : UserControl
    - protected ModelViewBase()
    - protected void Init()
    - protected abstract void OnInitialized()

### MyApp.Prisms/Views/AnotherTcpServerView.cs
namespace MyApp.Prisms.Views
  - internal class AnotherTcpServerView : TcpServerView

### MyApp.Prisms/Views/BaseViews/SmtpMailViewBase.xaml
[NO MAP]

### MyApp.Prisms/Views/BaseViews/SmtpMailViewBase.xaml.cs
namespace MyApp.Prisms.Views.BaseViews
  - public partial class SmtpMailViewBase : UserControl
    - private SmtpMailViewModelBase _viewModel
    - public SmtpMailViewBase()
    - private void StackPanel_Executed(object sender, ExecutedRoutedEventArgs e)

### MyApp.Prisms/Views/CommunicationView.xaml
[NO MAP]

### MyApp.Prisms/Views/CommunicationView.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class CommunicationView : UserControl
    - public CommunicationView()

### MyApp.Prisms/Views/MailManager.xaml
[NO MAP]

### MyApp.Prisms/Views/MailManager.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class MailManager : UserControl
    - public MailManager()

### MyApp.Prisms/Views/MainWindow.xaml
[NO MAP]

### MyApp.Prisms/Views/MainWindow.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class MainWindow : Window
    - public MainWindow()

### MyApp.Prisms/Views/MusicWindow.xaml
[NO MAP]

### MyApp.Prisms/Views/MusicWindow.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class MusicWindow : Window
    - public MusicWindow()

### MyApp.Prisms/Views/ProcessServiceView.xaml
[NO MAP]

### MyApp.Prisms/Views/ProcessServiceView.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class ProcessServiceView : UserControl
    - private ProcessServiceViewModel _context
    - public ProcessServiceView()
    - private void CommandBinding_OnExecuted(object sender, ExecutedRoutedEventArgs e)

### MyApp.Prisms/Views/SettingsView.xaml
[NO MAP]

### MyApp.Prisms/Views/SettingsView.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class SettingsView : UserControl
    - public SettingsView()
    - private void GlobalHotKeyTextBox_OnKeyUp(object sender, KeyEventArgs e)
    - private void AppHotKeyTextBox_OnKeyUp(object sender, KeyEventArgs e)

### MyApp.Prisms/Views/SwitchBackgroundView.xaml
[NO MAP]

### MyApp.Prisms/Views/SwitchBackgroundView.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class SwitchBackgroundView : UserControl
    - private ImageDisplayViewModel _imagesContext
    - private readonly IEventAggregator _eventAggregator
    - public SwitchBackgroundView()
    - private void KeyBoard_OnExecuted(object sender, ExecutedRoutedEventArgs e)
    - private void SwitchBackgroundSlider_MoveTo(double to)
    - public void SlideOut()
    - private void SlideIn()
    - private void OpenFileDialog()
    - private void SetBackgroundImage(string uri)

### MyApp.Prisms/Views/TcpClientView.xaml
[NO MAP]

### MyApp.Prisms/Views/TcpClientView.xaml.cs
namespace MyApp.Prisms.Views
  - public partial class TcpClientView : UserControl
    - private TcpClientViewModel _tcpSocketViewModel
   