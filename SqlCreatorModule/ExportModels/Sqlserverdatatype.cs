using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// SqlServer.MyDb导出数据表SqlServerDataType
    /// </summary>
    [Table("SqlServerDataType")]
    public class Sqlserverdatatype : CloneableBase, ICloneable
    {
        [Column("BigInt")]
        public Int64 Bigint { get; set; }
        [Column("Binary")]
        public Byte[] Binary { get; set; }
        [Column("Bit")]
        public Boolean Bit { get; set; }
        [Column("Char")]
        public String Char { get; set; }
        [Column("Date")]
        public DateTime Date { get; set; }
        [Column("DateTime")]
        public DateTime Datetime { get; set; }
        [Column("DateTime2")]
        public DateTime Datetime2 { get; set; }
        [Column("DateTimeOffset")]
        public DateTimeOffset Datetimeoffset { get; set; }
        [Column("Decimal")]
        public Decimal Decimal { get; set; }
        [Column("Float")]
        public Double Float { get; set; }
        [Column("Image")]
        public Byte[] Image { get; set; }
        [Column("Int")]
        public Int32 Int { get; set; }
        [Column("Money")]
        public Decimal Money { get; set; }
        [Column("NChar")]
        public String Nchar { get; set; }
        [Column("NText")]
        public String Ntext { get; set; }
        [Column("Numeric")]
        public Decimal Numeric { get; set; }
        [Column("Nvarchar")]
        public String Nvarchar { get; set; }
        [Column("Real")]
        public Single Real { get; set; }
        [Column("SmallDateTime")]
        public DateTime Smalldatetime { get; set; }
        [Column("SmallInt")]
        public Int16 Smallint { get; set; }
        [Column("SmallMoney")]
        public Decimal Smallmoney { get; set; }
        [Column("SqlVariant")]
        public Object Sqlvariant { get; set; }
        [Column("Sysname")]
        public String Sysname { get; set; }
        [Column("Text")]
        public String Text { get; set; }
        [Column("Time")]
        public TimeSpan Time { get; set; }
        [Column("TimeStamp")]
        public Byte[] Timestamp { get; set; }
        [Column("TinyInt")]
        public Byte Tinyint { get; set; }
        [Column("UniqueIdentifier")]
        public Guid Uniqueidentifier { get; set; }
        [Column("VarBinary")]
        public Byte[] Varbinary { get; set; }
        [Column("Varchar")]
        public String Varchar { get; set; }
        [Column("Xml")]
        public String Xml { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM SqlServerDataType;"; }
        public override string ToString()
        {
            return $"Sqlserverdatatype: [Bigint={Bigint}, Binary={Binary}, Bit={Bit}, Char={Char}, Date={Date}, Datetime={Datetime}, Datetime2={Datetime2}, Datetimeoffset={Datetimeoffset}, Decimal={Decimal}, Float={Float}, Image={Image}, Int={Int}, Money={Money}, Nchar={Nchar}, Ntext={Ntext}, Numeric={Numeric}, Nvarchar={Nvarchar}, Real={Real}, Smalldatetime={Smalldatetime}, Smallint={Smallint}, Smallmoney={Smallmoney}, Sqlvariant={Sqlvariant}, Sysname={Sysname}, Text={Text}, Time={Time}, Timestamp={Timestamp}, Tinyint={Tinyint}, Uniqueidentifier={Uniqueidentifier}, Varbinary={Varbinary}, Varchar={Varchar}, Xml={Xml}]";
        }
    }
}