using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// 数据表导出Module
    /// </summary>
    [Table("Module")]
    public class Module : BaseModel, ICloneable
    {
        [Column("id")]
        public Int64 Id { get; set; }
        [Column("group_name")]
        public String GroupName { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM Module;"; }
        public object Clone() { return base.MemberwiseClone(); }
        public override string ToString()
        {
            return $"Module: [Id={this.Id}, GroupName={this.GroupName}]";
        }
    }
}