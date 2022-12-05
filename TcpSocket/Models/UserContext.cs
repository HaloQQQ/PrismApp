using Helper.AbstractModel;

namespace TcpSocket.Models
{
    public class UserContext : BaseNotifyModel
    {
        public bool SignIn { get; set; }

        private string _account;

        public string Account
        {
            get => this._account;
            set
            {
                this._account = value;
                CallModel();
            }
        }

        private string _password;

        public string Password
        {
            get => this._password;
            set
            {
                this._password = value;
                CallModel();
            }
        }
    }
}