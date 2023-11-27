using IceTea.Atom.BaseModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// 数据表导出student
    /// </summary>
    [Table("student")]
    public class Student : BaseModel, ICloneable
    {
        [Column("id")]
        public Int64 Id { get; set; }
        [Column("name")]
        public String Name { get; set; }
        [Column("age")]
        public Int16 Age { get; set; }
        [Column("male")]
        public Boolean Male { get; set; }
        [Column("tid")]
        public Int64 Tid { get; set; }
        [Column("address")]
        public String Address { get; set; }
        [Column("last_access_time")]
        public DateTime LastAccessTime { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM student;"; }
        public object Clone() { return base.MemberwiseClone(); }
        public override string ToString()
        {
            return $"Student: [Id={this.Id}, Name={this.Name}, Age={this.Age}, Male={this.Male}, Tid={this.Tid}, Address={this.Address}, LastAccessTime={this.LastAccessTime}]";
        }
    }
}