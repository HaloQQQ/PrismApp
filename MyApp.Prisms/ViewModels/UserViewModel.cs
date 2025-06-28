using IceTea.Atom.BaseModels;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Windows.Controls;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class UserViewModel : ValidationBase
    {
        public bool SignIn { get; set; }

        private string _account;
        [Required(ErrorMessage = "账号不能为空")]
        [RegularExpression("^[a-zA-Z\\d_]+$", ErrorMessage = "英文、数字、下划线")]
        [StringLength(16, MinimumLength = 6, ErrorMessage = "长度不对（6-16）")]
        public string Account
        {
            get => this._account;
            set
            {
                if (SetProperty<string>(ref _account, value))
                {
                    ValidateNotifyDataError();
                }
            }
        }

        private string _password;
        [Required(ErrorMessage = "密码不能为空")]
        [RegularExpression("^[a-zA-Z\\d_]+$", ErrorMessage = "英文、数字、下划线")]
        [StringLength(12, MinimumLength =8, ErrorMessage ="长度不对（8-12）")]
        public string Password
        {
            get => this._password;
            set
            {
                if (SetProperty<string>(ref _password, value))
                {
                    ValidateNotifyDataError();
                }
            }
        }
    }

    class CustomValidationRule : ValidationRule
    {
        public override System.Windows.Controls.ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            if (value != null && value.ToString() == "admin")
            {
                return new System.Windows.Controls.ValidationResult(false, "不应是admin");
            }

            return new System.Windows.Controls.ValidationResult(true, null);
        }
    }
}