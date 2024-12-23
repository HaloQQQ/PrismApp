using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// 数据表导出mysqldatatype
    /// </summary>
    [Table("mysqldatatype")]
    public class Mysqldatatype : BaseModel, ICloneable
    {
        [Column("Bit")]
        public UInt64 Bit { get; set; }
        [Column("Bool")]
        public Boolean Bool { get; set; }
        [Column("BIGINT")]
        public Int64 BIGINT { get; set; }
        [Column("BIGINT UNSIGNED")]
        public UInt64 BIGINTUNSIGNED { get; set; }
        [Column("BINARY")]
        public Byte[]? BINARY { get; set; }
        [Column("BLOB")]
        public Byte[]? BLOB { get; set; }
        [Column("CHAR")]
        public String? CHAR { get; set; }
        [Column("DATE")]
        public DateTime DATE { get; set; }
        [Column("DATETIME")]
        public DateTime DATETIME { get; set; }
        [Column("DECIMAL")]
        public Decimal DECIMAL { get; set; }
        [Column("DOUBLE")]
        public Double DOUBLE { get; set; }
        [Column("DOUBLE PRECISION")]
        public Double DOUBLEPRECISION { get; set; }
        [Column("ENUM")]
        public Single ENUM { get; set; }
        [Column("FLOAT")]
        public Single FLOAT { get; set; }
        [Column("INT")]
        public Int32 INT { get; set; }
        [Column("INT UNSIGNED")]
        public UInt32 INTUNSIGNED { get; set; }
        [Column("INTEGER")]
        public Int32 INTEGER { get; set; }
        [Column("INTEGER UNSIGNED")]
        public UInt32 INTEGERUNSIGNED { get; set; }
        [Column("LONG VARBINARY")]
        public Byte[]? LONGVARBINARY { get; set; }
        [Column("LONG VARCHAR")]
        public String? LONGVARCHAR { get; set; }
        [Column("LONGBLOB")]
        public Byte[]? LONGBLOB { get; set; }
        [Column("LONGTEXT")]
        public String? LONGTEXT { get; set; }
        [Column("MEDIUMBLOB")]
        public Byte[]? MEDIUMBLOB { get; set; }
        [Column("MEDIUMINT")]
        public Int32 MEDIUMINT { get; set; }
        [Column("MEDIUMINT UNSIGNED")]
        public UInt32 MEDIUMINTUNSIGNED { get; set; }
        [Column("MEDIUMTEXT")]
        public String? MEDIUMTEXT { get; set; }
        [Column("NUMERIC")]
        public Decimal NUMERIC { get; set; }
        [Column("REAL")]
        public Double REAL { get; set; }
        [Column("SET")]
        public UInt64 SET { get; set; }
        [Column("SMALLINT")]
        public Int16 SMALLINT { get; set; }
        [Column("SMALLINT UNSIGNED")]
        public UInt16 SMALLINTUNSIGNED { get; set; }
        [Column("TEXT")]
        public String? TEXT { get; set; }
        [Column("TIME")]
        public TimeSpan TIME { get; set; }
        [Column("TIMESTAMP")]
        public DateTime TIMESTAMP { get; set; }
        [Column("TINYBLOB")]
        public Byte[]? TINYBLOB { get; set; }
        [Column("TINYINT")]
        public SByte TINYINT { get; set; }
        [Column("TINYINT UNSIGNED")]
        public Byte TINYINTUNSIGNED { get; set; }
        [Column("TINYTEXT")]
        public String? TINYTEXT { get; set; }
        [Column("VARBINARY")]
        public Byte[]? VARBINARY { get; set; }
        [Column("VARCHAR")]
        public String? VARCHAR { get; set; }
        [Column("YEAR")]
        public Int16 YEAR { get; set; }
        [Column("json")]
        public String? Json { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM mysqldatatype;"; }
        public override string ToString()
        {
            return $"Mysqldatatype: [Bit={this.Bit}, Bool={this.Bool}, BIGINT={this.BIGINT}, BIGINTUNSIGNED={this.BIGINTUNSIGNED}, BINARY={this.BINARY}, BLOB={this.BLOB}, CHAR={this.CHAR}, DATE={this.DATE}, DATETIME={this.DATETIME}, DECIMAL={this.DECIMAL}, DOUBLE={this.DOUBLE}, DOUBLEPRECISION={this.DOUBLEPRECISION}, ENUM={this.ENUM}, FLOAT={this.FLOAT}, INT={this.INT}, INTUNSIGNED={this.INTUNSIGNED}, INTEGER={this.INTEGER}, INTEGERUNSIGNED={this.INTEGERUNSIGNED}, LONGVARBINARY={this.LONGVARBINARY}, LONGVARCHAR={this.LONGVARCHAR}, LONGBLOB={this.LONGBLOB}, LONGTEXT={this.LONGTEXT}, MEDIUMBLOB={this.MEDIUMBLOB}, MEDIUMINT={this.MEDIUMINT}, MEDIUMINTUNSIGNED={this.MEDIUMINTUNSIGNED}, MEDIUMTEXT={this.MEDIUMTEXT}, NUMERIC={this.NUMERIC}, REAL={this.REAL}, SET={this.SET}, SMALLINT={this.SMALLINT}, SMALLINTUNSIGNED={this.SMALLINTUNSIGNED}, TEXT={this.TEXT}, TIME={this.TIME}, TIMESTAMP={this.TIMESTAMP}, TINYBLOB={this.TINYBLOB}, TINYINT={this.TINYINT}, TINYINTUNSIGNED={this.TINYINTUNSIGNED}, TINYTEXT={this.TINYTEXT}, VARBINARY={this.VARBINARY}, VARCHAR={this.VARCHAR}, YEAR={this.YEAR}, Json={this.Json}]";
        }
    }
}