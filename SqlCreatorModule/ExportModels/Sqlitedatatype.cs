using IceTea.Pure.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// PostgreSql.MyDb导出数据表SqliteDataType
    /// </summary>
    [Table("SqliteDataType")]
    public class Sqlitedatatype : CloneableBase, ICloneable
    {
        [Column("Integer")]
        public Int64 Integer { get; set; }
        [Column("Int")]
        public Int32 Int { get; set; }
        [Column("SmallInt")]
        public Int16 Smallint { get; set; }
        [Column("TinyInt")]
        public Byte Tinyint { get; set; }
        [Column("MEDIUMINT")]
        public Int32 Mediumint { get; set; }
        [Column("BIGINT")]
        public Int64 Bigint { get; set; }
        [Column("UNSIGNED BIG INT")]
        public Object UnsignedBigInt { get; set; }
        [Column("Int2")]
        public Object Int2 { get; set; }
        [Column("Int8")]
        public SByte Int8 { get; set; }
        [Column("NUMERIC")]
        public Decimal Numeric { get; set; }
        [Column("DECIMAL")]
        public Decimal Decimal { get; set; }
        [Column("BOOLEAN")]
        public Boolean Boolean { get; set; }
        [Column("DATE")]
        public DateTime Date { get; set; }
        [Column("DATETIME")]
        public DateTime Datetime { get; set; }
        [Column("REAL")]
        public Double Real { get; set; }
        [Column("DOUBLE")]
        public Double Double { get; set; }
        [Column("DOUBLE PRECISION")]
        public Object DoublePrecision { get; set; }
        [Column("FLOAT")]
        public Double Float { get; set; }
        [Column("CHARACTER")]
        public Object Character { get; set; }
        [Column("VARCHAR")]
        public String Varchar { get; set; }
        [Column("VARYING CHARACTER")]
        public Object VaryingCharacter { get; set; }
        [Column("NCHAR")]
        public String Nchar { get; set; }
        [Column("NATIVE CHARACTER")]
        public Object NativeCharacter { get; set; }
        [Column("NVARCHAR")]
        public String Nvarchar { get; set; }
        [Column("TEXTCLOB")]
        public Object Textclob { get; set; }
        [Column("BLOB")]
        public Byte[] Blob { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM SqliteDataType;"; }
        public override string ToString()
        {
            return $"Sqlitedatatype: [Integer={Integer}, Int={Int}, Smallint={Smallint}, Tinyint={Tinyint}, Mediumint={Mediumint}, Bigint={Bigint}, UnsignedBigInt={UnsignedBigInt}, Int2={Int2}, Int8={Int8}, Numeric={Numeric}, Decimal={Decimal}, Boolean={Boolean}, Date={Date}, Datetime={Datetime}, Real={Real}, Double={Double}, DoublePrecision={DoublePrecision}, Float={Float}, Character={Character}, Varchar={Varchar}, VaryingCharacter={VaryingCharacter}, Nchar={Nchar}, NativeCharacter={NativeCharacter}, Nvarchar={Nvarchar}, Textclob={Textclob}, Blob={Blob}]";
        }
    }
}