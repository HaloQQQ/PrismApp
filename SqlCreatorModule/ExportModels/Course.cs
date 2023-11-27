using IceTea.Atom.BaseModels;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
    /// <summary>
    /// SqlServer.SchoolDB导出数据表Course
    /// </summary>
    [Table("Course")]
    public class Course : BaseModel, ICloneable
    {
        [Column("CourseNo")]
        public Int32 CourseNo { get; set; }
        [Column("CourseName")]
        public String CourseName { get; set; }
        [Column("CourseScore")]
        public Decimal CourseScore { get; set; }
        [Column("CourseMakerName")]
        public String CourseMakerName { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM Course;"; }
        public object Clone() { return base.MemberwiseClone(); }
        public override string ToString()
        {
            return $"Course: [CourseNo={this.CourseNo}, CourseName={this.CourseName}, CourseScore={this.CourseScore}, CourseMakerName={this.CourseMakerName}]";
        }
    }
}