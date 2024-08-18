using IceTea.Atom.Extensions;
using IceTea.Atom.Contracts;
using IceTea.Atom.Utils;
using IceTea.SqlStandard.Contracts;
using IceTea.SqlStandard.DbModels;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using IceTea.Wpf.Core.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using PrismAppBasicLib.MsgEvents;
using SqlCreatorModule.Models;
using System.Data;
using System.Windows.Input;
using IceTea.Wpf.Atom.Utils;

namespace SqlCreatorModule.ViewModels
{
    internal class CreateModelViewModel : BindableBase
    {
        public CreateModelViewModel(IEventAggregator eventAggregator)
        {
            this.ConnectCommand = new DelegateCommand(() =>
            {
                try
                {
                    if (this.IsSqlite)
                    {
                        IDb sqlite = this.GetSqliteDb();

                        if (sqlite == null)
                        {
                            return;
                        }

                        using (sqlite)
                        {
                            this.DbNames = sqlite.GetDBsName();
                            this.CurrentDbName = this.DbNames.FirstOrDefault();
                        }

                        return;
                    }

                    using (IDb db = this.GetDb())
                    {
                        this.DbNames = db.GetDBsName();
                    }

                    this.PublishMsg(eventAggregator, "连接成功");
                }
                catch (Exception ex)
                {
                    this.CurrentDbName = string.Empty;
                    this.PublishMsg(eventAggregator, ex.Message);
                }
            });

            this.GetTablesCommand = new DelegateCommand(() =>
            {
                try
                {
                    this.CurrentTableName = string.Empty;

                    if (this.IsSqlite)
                    {
                        using (IDb sqlite = this.GetSqliteDb(false))
                        {
                            this.TableNames = sqlite.GetTables();
                            this.CurrentTableName = this.TableNames.FirstOrDefault();
                        }

                        return;
                    }

                    using (IDb db = this.GetDb())
                    {
                        this.TableNames = db.GetTables();
                        this.CurrentTableName = this.TableNames.FirstOrDefault();
                    }

                    this.PublishMsg(eventAggregator, "查询表名集合成功");
                }
                catch (Exception ex)
                {
                    this.PublishMsg(eventAggregator, ex.Message);
                }
            });

            this.ShowTableStructureCommand = new DelegateCommand(() =>
            {
                try
                {
                    this.TableColumnsStructure = null;

                    IDb db = null;
                    using (IDisposable disposable = new DisposeAction(() => db?.Dispose()))
                    {
                        if (this.IsSqlite)
                        {
                            db = this.GetSqliteDb(false);
                        }
                        else
                        {
                            db = this.GetDb();
                        }

                        var dataTable = db.ExecuteQueryAtOnce($"select * from {this.CurrentTableName};", null).Tables[0];

                        var list = new List<DataColumnInfoModel>();
                        foreach (DataColumn item in dataTable.Columns)
                        {
                            list.Add(new DataColumnInfoModel(item));
                        }

                        this.TableColumnsStructure = list;

                        this.PublishMsg(eventAggregator, "查询表结构成功");
                    }
                }
                catch (Exception ex)
                {
                    this.PublishMsg(eventAggregator, ex.Message);
                }
            });

            this.OpenExportDirCommand = new DelegateCommand(() =>
            {
                var targetDir = CommonCoreUtils.OpenFolderDialog(null);

                if (targetDir.IsNullOrBlank())
                {
                    return;
                }

                this.ModelExportDir = targetDir;
            });

            this.ExportTableStructureToFileCommand = new DelegateCommand(() =>
            {
                try
                {
                    ModelExportDir.AssertArgumentNotNull(nameof(ModelExportDir));

                    this.TableColumnsStructure.AssertNotEmpty(nameof(TableColumnsStructure));

                    ModelHelper.BuildTheModelClassFromColumns(TableColumnsStructure.Select(column=>new ModelHelper.ColumnProperty(column.ColumnName, column.DataType)),
                        this.CurrentTableName, $"{CurrentDbType}.{CurrentDbName}导出数据表{CurrentTableName}", ModelExportDir, "SqlCreatorModule.ExportModels");

                    this.PublishMsg(eventAggregator, "导出成功");
                }
                catch (Exception ex)
                {
                    this.PublishMsg(eventAggregator, ex.Message);
                }
            });
        }

        private IDb GetSqliteDb(bool openFileDialog = true)
        {
            if (openFileDialog)
            {
                var fileDialog = CommonAtomUtils.OpenFileDialog(null, new AnyMedia());

                if (fileDialog == null)
                {
                    return null;
                }

                this.DbFilePath = fileDialog.FileName;
            }

            return new SqliteDb($"datasource={DbFilePath.AssertArgumentNotNull("sqlite数据库文件未指定")}");
        }

        private IDb GetDb()
        {
            this.Host.AssertNotNull(nameof(Host));
            this.CurrentDbName.AssertNotNull(nameof(CurrentDbName));
            this.Uid.AssertNotNull(nameof(Uid));
            this.Pwd.AssertNotNull(nameof(Pwd));

            if (this.IsMysql)
            {
                ushort port = 3306;
                if (ushort.TryParse(this.Port, out ushort p))
                {
                    port = p;
                }

                return new MySqlDb(this.Host, this.CurrentDbName, this.Uid, this.Pwd, port);
            }
            else if (this.IsSqlServer)
            {
                ushort port = 1433;
                if (ushort.TryParse(this.Port, out ushort p))
                {
                    port = p;
                }

                return new SqlServerDb(this.Host, this.CurrentDbName, this.Uid, this.Pwd, port);
            }
            else
            {
                ushort port = 5432;
                if (ushort.TryParse(this.Port, out ushort p))
                {
                    port = p;
                }

                return new PostgreSQLDb(this.Host, this.CurrentDbName, this.Uid, this.Pwd, port);
            }
        }

        private void PublishMsg(IEventAggregator eventAggregator, string message)
        {
            eventAggregator.GetEvent<DialogMessageEvent>().Publish(new DialogMessage(message, 3));
        }

        private IEnumerable<DataColumnInfoModel> _tableColumnsStructure;

        public IEnumerable<DataColumnInfoModel> TableColumnsStructure
        {
            get => this._tableColumnsStructure;
            set => SetProperty<IEnumerable<DataColumnInfoModel>>(ref _tableColumnsStructure, value);
        }

        public ICommand OpenExportDirCommand { get; private set; }
        public ICommand ExportTableStructureToFileCommand { get; private set; }

        public ICommand ShowTableStructureCommand { get; private set; }
        public ICommand GetTablesCommand { get; private set; }
        public ICommand ConnectCommand { get; private set; }

        private string _modelExportDir;

        public string ModelExportDir
        {
            get => this._modelExportDir;
            set => SetProperty<string>(ref _modelExportDir, value);
        }


        private string _currentTableName;

        public string CurrentTableName
        {
            get => this._currentTableName;
            set => SetProperty<string>(ref _currentTableName, value);
        }

        private IEnumerable<string> _tableNames;

        public IEnumerable<string> TableNames
        {
            get => this._tableNames;
            set => SetProperty<IEnumerable<string>>(ref _tableNames, value);
        }


        private string _currentDbName;

        public string CurrentDbName
        {
            get => this._currentDbName;
            set => SetProperty<string>(ref _currentDbName, value);
        }

        private IEnumerable<string> _dbNames;

        public IEnumerable<string> DbNames
        {
            get => this._dbNames;
            set => SetProperty<IEnumerable<string>>(ref _dbNames, value);
        }

        private string _currentDbType;

        public string CurrentDbType
        {
            get => this._currentDbType;
            set
            {
                if (SetProperty<string>(ref _currentDbType, value))
                {
                    this.IsMysql = EnumDbType.mysql.EqualsIgnoreCase(value);
                    this.IsSqlServer = EnumDbType.sqlserver.EqualsIgnoreCase(value);
                    this.IsSqlite = EnumDbType.sqlite.EqualsIgnoreCase(value);
                    this.IsPostgresql = EnumDbType.postgresql.EqualsIgnoreCase(value);
                }
            }
        }

        private bool _isMysql;

        public bool IsMysql
        {
            get => this._isMysql;
            set => SetProperty<bool>(ref _isMysql, value);
        }

        private bool _isSqlServer;

        public bool IsSqlServer
        {
            get => this._isSqlServer;
            set => SetProperty<bool>(ref _isSqlServer, value);
        }

        private bool _isSqlite;

        public bool IsSqlite
        {
            get => this._isSqlite;
            set => SetProperty<bool>(ref _isSqlite, value);
        }

        private bool _isPostgresql;

        public bool IsPostgresql
        {
            get => this._isPostgresql;
            set => SetProperty<bool>(ref _isPostgresql, value);
        }

        private string _host;

        public string Host
        {
            get => this._host;
            set => SetProperty<string>(ref _host, value);
        }


        private string _port;

        public string Port
        {
            get => this._port;
            set => SetProperty<string>(ref _port, value);
        }

        private string _uid;

        public string Uid
        {
            get => this._uid;
            set => SetProperty<string>(ref _uid, value);
        }

        private string _pwd;

        public string Pwd
        {
            get => this._pwd;
            set => SetProperty<string>(ref _pwd, value);
        }

        private string _dbFilePath;

        public string DbFilePath
        {
            get => this._dbFilePath;
            set => SetProperty<string>(ref _dbFilePath, value);
        }
    }
}
