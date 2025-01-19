using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using IceTea.SqlStandard.Contracts;
using IceTea.SqlStandard.DbModels;
using IceTea.Wpf.Core.Utils;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using SqlCreatorModule.Models;
using System.Data;
using System.Windows.Input;
using IceTea.Wpf.Atom.Utils;
using PrismAppBasicLib.Contracts;
using System.Collections.ObjectModel;
using IceTea.Wpf.Atom.Contracts.FileFilters;

namespace SqlCreatorModule.ViewModels
{
#pragma warning disable CS8600 // 将 null 字面量或可能为 null 的值转换为非 null 类型。
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8603 // 可能返回 null 引用。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
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

                    CommonUtil.PublishMessage(eventAggregator, "连接成功");
                }
                catch (Exception ex)
                {
                    this.CurrentDbName = string.Empty;
                    CommonUtil.PublishMessage(eventAggregator, ex.Message);
                }
            });

            this.GetTablesCommand = new DelegateCommand(() =>
            {
                try
                {
                    this.CurrentTableName = string.Empty;

                    using (IDb db = this.IsSqlite ? this.GetSqliteDb(false) : this.GetDb())
                    {
                        this.TableNames = db.GetTables();
                        this.CurrentTableName = this.TableNames.FirstOrDefault();
                    }

                    CommonUtil.PublishMessage(eventAggregator, "查询表名集合成功");
                }
                catch (Exception ex)
                {
                    CommonUtil.PublishMessage(eventAggregator, ex.Message);
                }
            });

            this.ShowTableStructureCommand = new DelegateCommand(() =>
            {
                try
                {
                    this.TableColumnsStructure.Clear();

                    using (IDb db = this.IsSqlite ? this.GetSqliteDb(false) : this.GetDb())
                    {
                        var dataTable = db.ExecuteQueryAtOnce($"select * from {this.CurrentTableName};", null).Tables[0];

                        var list = new List<DataColumnInfoModel>();
                        foreach (DataColumn item in dataTable.Columns)
                        {
                            list.Add(new DataColumnInfoModel(item));
                        }

                        if (!IsSqlite)
                        {
                            var fields = db.GetColumns(this.CurrentTableName);
                            foreach (var item in fields)
                            {
                                var array = item.Split(',');

                                var column = list.FirstOrDefault(t => t.ColumnName.EqualsIgnoreCase(array[0]));

                                if (column != null)
                                {
                                    column.DbDataType = array[1];

                                    column.Comment = array[2];
                                }
                            }
                        }

                        this.TableColumnsStructure.AddIfItemsNotWhileOrNotContains(list, _ => this.TableColumnsStructure.Any(t => t.Equals(_)));

                        CommonUtil.PublishMessage(eventAggregator, "查询表结构成功");
                    }
                }
                catch (Exception ex)
                {
                    CommonUtil.PublishMessage(eventAggregator, ex.Message);
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

                    ModelHelper.BuildTheModelClassFromColumns(TableColumnsStructure.Select(column => new ModelHelper.ColumnProperty(CurrentTableName.ToPascal(), column.ColumnName, column.DataType)),
                        this.CurrentTableName, $"{CurrentDbType}.{CurrentDbName}导出数据表{CurrentTableName}", ModelExportDir, "SqlCreatorModule.ExportModels");

                    CommonUtil.PublishMessage(eventAggregator, "导出成功");
                }
                catch (Exception ex)
                {
                    CommonUtil.PublishMessage(eventAggregator, ex.Message);
                }
            });
        }

        private IDb GetSqliteDb(bool openFileDialog = true)
        {
            if (openFileDialog)
            {
                var fileDialog = CommonAtomUtils.OpenFileDialog(null, new AnyFilter());

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

        private IList<DataColumnInfoModel> _tableColumnsStructure = new ObservableCollection<DataColumnInfoModel>();

        public IList<DataColumnInfoModel> TableColumnsStructure
        {
            get => this._tableColumnsStructure;
        }

        public ICommand OpenExportDirCommand { get; }
        public ICommand ExportTableStructureToFileCommand { get; }

        public ICommand ShowTableStructureCommand { get; }
        public ICommand GetTablesCommand { get; }
        public ICommand ConnectCommand { get; }


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
