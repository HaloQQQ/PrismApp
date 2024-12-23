using IceTea.Atom.BaseModels;

namespace MyApp.Prisms.ViewModels
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class UserViewModel : BaseNotifyModel
    {
        public bool SignIn { get; set; }

        private string _account;

        public string Account
        {
            get => this._account;
            set => SetProperty<string>(ref _account, value);
        }

        private string _password;

        public string Password
        {
            get => this._password;
            set => SetProperty<string>(ref _password, value);
        }
    }
}