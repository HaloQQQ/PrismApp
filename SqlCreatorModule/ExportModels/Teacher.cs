using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// SqlServer.SchoolDB导出数据表Teacher
    /// </summary>
    [Table("Teacher")]
    public class Teacher : CloneableBase, ICloneable
    {
        [Column("TeacherNo")]
        public Int32 TeacherNo { get; set; }
        [Column("TeacherName")]
        public String? TeacherName { get; set; }
        [Column("TeacherSex")]
        public String? TeacherSex { get; set; }
        [Column("TeacherBirthDate")]
        public DateTime TeacherBirthDate { get; set; }
        [Column("TeacherId")]
        public String? TeacherId { get; set; }
        [Column("TeacherAddress")]
        public String? TeacherAddress { get; set; }
        [Column("TeacherTel")]
        public String? TeacherTel { get; set; }
        [Column("TeacherDepartment")]
        public String? TeacherDepartment { get; set; }
        [Column("TeacherTime")]
        public Int32 TeacherTime { get; set; }
        [Column("TeacherParty")]
        public String? TeacherParty { get; set; }
        [Column("TeacherUserPwd")]
        public String? TeacherUserPwd { get; set; }
        [Column("TeacherMgrNow")]
        public DateTime TeacherMgrNow { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM Teacher;"; }
        public override string ToString()
        {
            return $"Teacher: [TeacherNo={this.TeacherNo}, TeacherName={this.TeacherName}, TeacherSex={this.TeacherSex}, TeacherBirthDate={this.TeacherBirthDate}, TeacherId={this.TeacherId}, TeacherAddress={this.TeacherAddress}, TeacherTel={this.TeacherTel}, TeacherDepartment={this.TeacherDepartment}, TeacherTime={this.TeacherTime}, TeacherParty={this.TeacherParty}, TeacherUserPwd={this.TeacherUserPwd}, TeacherMgrNow={this.TeacherMgrNow}]";
        }
    }
}