using Helper.AbstractModel;

namespace TcpSocket.Models
{
    public class PopupResult : BaseNotifyModel
    {
        private bool _backgroundImageSlider = true;

        public bool BackgroundImageSlider
        {
            get => this._backgroundImageSlider;
            set
            {
                this._backgroundImageSlider = value;
                CallModel();
            }
        }
    }
}