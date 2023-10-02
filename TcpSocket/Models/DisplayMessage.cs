using Prism.Mvvm;

namespace TcpSocket.Models
{
    public class DialogMessage : BindableBase
    {
        public DialogMessage(string dialogMessage, int seconds)
        {
            this.DisplayMessage = dialogMessage; 
            this.Seconds = seconds;
            this.IsDisplayingDialogMessage = true;
        }

        private bool _stopHien;

        public bool StopHide
        {
            get => this._stopHien;
            set => SetProperty<bool>(ref _stopHien, value);
        }


        private bool _isDisplayingDialogMessage;

        public bool IsDisplayingDialogMessage
        {
            get => this._isDisplayingDialogMessage;
            set => SetProperty<bool>(ref _isDisplayingDialogMessage, value);
        }

        private string _displayMessage;

        public string DisplayMessage
        {
            get => this._displayMessage;
            set => SetProperty<string>(ref _displayMessage, value);
        }

        private int _seconds;

        public int Seconds
        {
            get => this._seconds;
            set => SetProperty<int>(ref _seconds, value);
        }
    }
}
