using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// Mysql.MyDb导出数据表mysqldatatype
    /// </summary>
    [Table("mysqldatatype")]
    public class Mysqldatatype : BaseModel, ICloneable
    {
        [Column("BigInt")]
        public Int64 BigInt { get; set; }
        [Column("BigIntUnsigned")]
        public UInt64 BigIntUnsigned { get; set; }
        [Column("Binary")]
        public Byte[]? Binary { get; set; }
        [Column("Bit")]
        public UInt64 Bit { get; set; }
        [Column("Blob")]
        public Byte[]? Blob { get; set; }
        [Column("Bool")]
        public Boolean Bool { get; set; }
        [Column("Char")]
        public String? Char { get; set; }
        [Column("Date")]
        public DateTime Date { get; set; }
        [Column("DateTime")]
        public DateTime DateTime { get; set; }
        [Column("Decimal")]
        public Decimal Decimal { get; set; }
        [Column("Double")]
        public Double Double { get; set; }
        [Column("DoublePrecision")]
        public Double DoublePrecision { get; set; }
        [Column("Float")]
        public Single Float { get; set; }
        [Column("Int")]
        public Int32 Int { get; set; }
        [Column("IntUnsigned")]
        public UInt32 IntUnsigned { get; set; }
        [Column("LongVarBinary")]
        public Byte[]? LongVarBinary { get; set; }
        [Column("LongVarChar")]
        public String? LongVarChar { get; set; }
        [Column("LongBlob")]
        public Byte[]? LongBlob { get; set; }
        [Column("LongText")]
        public String? LongText { get; set; }
        [Column("MedimumBlob")]
        public Byte[]? MedimumBlob { get; set; }
        [Column("MedimumInt")]
        public Int32 MedimumInt { get; set; }
        [Column("MedimumUnsigned")]
        public UInt32 MedimumUnsigned { get; set; }
        [Column("MediumText")]
        public String? MediumText { get; set; }
        [Column("Numeric")]
        public Decimal Numeric { get; set; }
        [Column("Real")]
        public Double Real { get; set; }
        [Column("SmallInt")]
        public Int16 SmallInt { get; set; }
        [Column("SmallIntUnsigned")]
        public UInt16 SmallIntUnsigned { get; set; }
        [Column("Text")]
        public String? Text { get; set; }
        [Column("Time")]
        public TimeSpan Time { get; set; }
        [Column("TimeStamp")]
        public DateTime TimeStamp { get; set; }
        [Column("TinyBlob")]
        public Byte[]? TinyBlob { get; set; }
        [Column("TinyInt")]
        public SByte TinyInt { get; set; }
        [Column("TinyIntUnsigned")]
        public Byte TinyIntUnsigned { get; set; }
        [Column("TinyText")]
        public String? TinyText { get; set; }
        [Column("VarBinary")]
        public Byte[]? VarBinary { get; set; }
        [Column("VarChar")]
        public String? VarChar { get; set; }
        [Column("Year")]
        public Int16 Year { get; set; }
        [Column("Json")]
        public String? Json { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM mysqldatatype;"; }
        public override string ToString()
        {
            return $"Mysqldatatype: [BigInt={this.BigInt}, BigIntUnsigned={this.BigIntUnsigned}, Binary={this.Binary}, Bit={this.Bit}, Blob={this.Blob}, Bool={this.Bool}, Char={this.Char}, Date={this.Date}, DateTime={this.DateTime}, Decimal={this.Decimal}, Double={this.Double}, DoublePrecision={this.DoublePrecision}, Float={this.Float}, Int={this.Int}, IntUnsigned={this.IntUnsigned}, LongVarBinary={this.LongVarBinary}, LongVarChar={this.LongVarChar}, LongBlob={this.LongBlob}, LongText={this.LongText}, MedimumBlob={this.MedimumBlob}, MedimumInt={this.MedimumInt}, MedimumUnsigned={this.MedimumUnsigned}, MediumText={this.MediumText}, Numeric={this.Numeric}, Real={this.Real}, SmallInt={this.SmallInt}, SmallIntUnsigned={this.SmallIntUnsigned}, Text={this.Text}, Time={this.Time}, TimeStamp={this.TimeStamp}, TinyBlob={this.TinyBlob}, TinyInt={this.TinyInt}, TinyIntUnsigned={this.TinyIntUnsigned}, TinyText={this.TinyText}, VarBinary={this.VarBinary}, VarChar={this.VarChar}, Year={this.Year}, Json={this.Json}]";
        }
    }
}