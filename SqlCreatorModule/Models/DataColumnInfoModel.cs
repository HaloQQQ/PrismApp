using IceTea.Pure.Extensions;
using System.Data;

namespace SqlCreatorModule.Models
{
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8602
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class DataColumnInfoModel : IEquatable<DataColumnInfoModel>
    {
        public DataColumnInfoModel(DataColumn column)
        {
            this.ColumnName = column.ColumnName;
            this.DefaultValue = column.DefaultValue.ToString();

            this.Unique = column.Unique;
            this.AllowDBNull = column.AllowDBNull;

            this.AutoIncrement = column.AutoIncrement;
            this.AutoIncrementSeed = column.AutoIncrementSeed;
            this.AutoIncrementStep = column.AutoIncrementStep;

            this.Expression = column.Expression;
            this.ReadOnly = column.ReadOnly;
            this.DataType = column.DataType;
            this.DateTimeMode = column.DateTimeMode;
            this.MaxLength = column.MaxLength;
            this.Namespace = column.Namespace;
            this.ExtendedProperties = column.ExtendedProperties;

            this.Caption = column.Caption;
        }

        public PropertyCollection ExtendedProperties { get; }

        public string Caption { get; }

        public string ColumnName { get; }
        /// <summary>
        /// 属性数据类型
        /// </summary>
        public Type DataType { get; }

        /// <summary>
        /// Db字段数据类型
        /// </summary>
        public string DbDataType { get; set; }
        public string Comment { get; set; }

        public string DefaultValue { get; }
        public bool Unique { get; }
        public bool AllowDBNull { get; }
        public bool AutoIncrement { get; }
        public long AutoIncrementSeed { get; }
        public long AutoIncrementStep { get; }
        public int MaxLength { get; }

        public bool ReadOnly { get; }
        public DataSetDateTime DateTimeMode { get; }
        public string Namespace { get; }
        public string Expression { get; }

        public bool Equals(DataColumnInfoModel? other)
        {
            return other.IsNotNullAnd(_ => _.ColumnName.EqualsIgnoreCase(this.ColumnName));
        }
    }
}
