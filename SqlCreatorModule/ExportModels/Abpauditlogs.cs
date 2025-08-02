using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlCreatorModule.ExportModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    /// <summary>
    /// 数据表导出abpauditlogs
    /// </summary>
    [Table("abpauditlogs")]
    public class Abpauditlogs : CloneableBase, ICloneable
    {
        [Column("Id")]
        public Guid Id { get; set; }
        [Column("ApplicationName")]
        public String ApplicationName { get; set; }
        [Column("UserId")]
        public Guid UserId { get; set; }
        [Column("UserName")]
        public String UserName { get; set; }
        [Column("TenantId")]
        public Guid TenantId { get; set; }
        [Column("TenantName")]
        public String TenantName { get; set; }
        [Column("ImpersonatorUserId")]
        public Guid ImpersonatorUserId { get; set; }
        [Column("ImpersonatorUserName")]
        public String ImpersonatorUserName { get; set; }
        [Column("ImpersonatorTenantId")]
        public Guid ImpersonatorTenantId { get; set; }
        [Column("ImpersonatorTenantName")]
        public String ImpersonatorTenantName { get; set; }
        [Column("ExecutionTime")]
        public DateTime ExecutionTime { get; set; }
        [Column("ExecutionDuration")]
        public Int32 ExecutionDuration { get; set; }
        [Column("ClientIpAddress")]
        public String ClientIpAddress { get; set; }
        [Column("ClientName")]
        public String ClientName { get; set; }
        [Column("ClientId")]
        public String ClientId { get; set; }
        [Column("CorrelationId")]
        public String CorrelationId { get; set; }
        [Column("BrowserInfo")]
        public String BrowserInfo { get; set; }
        [Column("HttpMethod")]
        public String HttpMethod { get; set; }
        [Column("Url")]
        public String Url { get; set; }
        [Column("Exceptions")]
        public String Exceptions { get; set; }
        [Column("Comments")]
        public String Comments { get; set; }
        [Column("HttpStatusCode")]
        public Int32 HttpStatusCode { get; set; }
        [Column("ExtraProperties")]
        public String ExtraProperties { get; set; }
        [Column("ConcurrencyStamp")]
        public String ConcurrencyStamp { get; set; }
        public virtual string GetSelectCmdText() { return "SELECT * FROM abpauditlogs;"; }
        public override string ToString()
        {
            return $"Abpauditlogs: [Id={this.Id}, ApplicationName={this.ApplicationName}, UserId={this.UserId}, UserName={this.UserName}, TenantId={this.TenantId}, TenantName={this.TenantName}, ImpersonatorUserId={this.ImpersonatorUserId}, ImpersonatorUserName={this.ImpersonatorUserName}, ImpersonatorTenantId={this.ImpersonatorTenantId}, ImpersonatorTenantName={this.ImpersonatorTenantName}, ExecutionTime={this.ExecutionTime}, ExecutionDuration={this.ExecutionDuration}, ClientIpAddress={this.ClientIpAddress}, ClientName={this.ClientName}, ClientId={this.ClientId}, CorrelationId={this.CorrelationId}, BrowserInfo={this.BrowserInfo}, HttpMethod={this.HttpMethod}, Url={this.Url}, Exceptions={this.Exceptions}, Comments={this.Comments}, HttpStatusCode={this.HttpStatusCode}, ExtraProperties={this.ExtraProperties}, ConcurrencyStamp={this.ConcurrencyStamp}]";
        }
    }
}