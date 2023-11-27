using System.Data;

namespace SqlCreatorModule.Models
{
    internal class DataColumnInfoModel
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
            this.DataType = column.DataType.Name;
            this.DateTimeMode = column.DateTimeMode;
            this.MaxLength = column.MaxLength;
            this.Namespace = column.Namespace;
            //this.ExtendedProperties = column.ExtendedProperties;
        }

        public string ColumnName { get; }
        public string DefaultValue { get; }
        public bool Unique { get; }
        public bool AllowDBNull { get; }
        public bool AutoIncrement { get; }
        public long AutoIncrementSeed { get; }
        public long AutoIncrementStep { get; }
        public string Expression { get; }
        public bool ReadOnly { get; }
        public string DataType { get; }
        public DataSetDateTime DateTimeMode { get; }
        public int MaxLength { get; }
        public string Namespace { get; }
        //public PropertyCollection ExtendedProperties { get; }
    }
}
