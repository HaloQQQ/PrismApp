using IceTea.Pure.BaseModels;
using NpgsqlTypes;
using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.NetworkInformation;

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// PostgreSql.postgres导出数据表postgresql_datatype
    /// </summary>
    [Table("postgresql_datatype")]
    public class PostgresqlDatatype : CloneableBase, ICloneable
    {
        [Column("Bigserial")]
        public Int64 Bigserial { get; set; }
        [Column("Bit")]
        public Boolean Bit { get; set; }
        [Column("Bool")]
        public Boolean Bool { get; set; }
        [Column("Box")]
        public NpgsqlBox Box { get; set; }
        [Column("Bytea")]
        public Byte[] Bytea { get; set; }
        [Column("Char")]
        public String Char { get; set; }
        [Column("Cidr")]
        public NpgsqlCidr Cidr { get; set; }
        [Column("Circle")]
        public NpgsqlCircle Circle { get; set; }
        [Column("Date")]
        public DateTime Date { get; set; }
        [Column("Decimal")]
        public Decimal Decimal { get; set; }
        [Column("Float4")]
        public Single Float4 { get; set; }
        [Column("Float8")]
        public Double Float8 { get; set; }
        [Column("Inet")]
        public IPAddress Inet { get; set; }
        [Column("Int2")]
        public Int16 Int2 { get; set; }
        [Column("Int4")]
        public Int32 Int4 { get; set; }
        [Column("Int8")]
        public Int64 Int8 { get; set; }
        [Column("Interval")]
        public TimeSpan Interval { get; set; }
        [Column("Json")]
        public String Json { get; set; }
        [Column("Jsonb")]
        public String Jsonb { get; set; }
        [Column("Line")]
        public NpgsqlLine Line { get; set; }
        [Column("Lseg")]
        public NpgsqlLSeg Lseg { get; set; }
        [Column("Macaddr")]
        public PhysicalAddress Macaddr { get; set; }
        [Column("Money")]
        public Decimal Money { get; set; }
        [Column("Numeric")]
        public Decimal Numeric { get; set; }
        [Column("Path")]
        public NpgsqlPath Path { get; set; }
        [Column("Point")]
        public NpgsqlPoint Point { get; set; }
        [Column("Polygon")]
        public NpgsqlPolygon Polygon { get; set; }
        [Column("Serial")]
        public Int32 Serial { get; set; }
        [Column("Serial2")]
        public Int16 Serial2 { get; set; }
        [Column("Serial8")]
        public Int64 Serial8 { get; set; }
        [Column("SmallSerial")]
        public Int16 Smallserial { get; set; }
        [Column("Text")]
        public String Text { get; set; }
        [Column("Time")]
        public TimeSpan Time { get; set; }
        [Column("TimeStamp")]
        public DateTime Timestamp { get; set; }
        [Column("TimeStamptz")]
        public DateTime Timestamptz { get; set; }
        [Column("Timetz")]
        public DateTimeOffset Timetz { get; set; }
        [Column("Tsquery")]
        public NpgsqlTsQuery Tsquery { get; set; }
        [Column("Tsvector")]
        public NpgsqlTsVector Tsvector { get; set; }
        [Column("Uuid")]
        public Guid Uuid { get; set; }
        [Column("Varbit")]
        public BitArray Varbit { get; set; }
        [Column("Varchar")]
        public String Varchar { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM postgresql_datatype;"; }
        public override string ToString()
        {
            return $"PostgresqlDatatype: [Bigserial={Bigserial}, Bit={Bit}, Bool={Bool}, Box={Box}, Bytea={Bytea}, Char={Char}, Cidr={Cidr}, Circle={Circle}, Date={Date}, Decimal={Decimal}, Float4={Float4}, Float8={Float8}, Inet={Inet}, Int2={Int2}, Int4={Int4}, Int8={Int8}, Interval={Interval}, Json={Json}, Jsonb={Jsonb}, Line={Line}, Lseg={Lseg}, Macaddr={Macaddr}, Money={Money}, Numeric={Numeric}, Path={Path}, Point={Point}, Polygon={Polygon}, Serial={Serial}, Serial2={Serial2}, Serial8={Serial8}, Smallserial={Smallserial}, Text={Text}, Time={Time}, Timestamp={Timestamp}, Timestamptz={Timestamptz}, Timetz={Timetz}, Tsquery={Tsquery}, Tsvector={Tsvector}, Uuid={Uuid}, Varbit={Varbit}, Varchar={Varchar}]";
        }
    }
}