using Helper.AbstractModel;

namespace TcpSocket.ViewModels
{
    public class UserViewModel : BaseNotifyModel
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