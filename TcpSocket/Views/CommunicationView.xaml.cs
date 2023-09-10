using System.Windows.Controls;
using TcpSocket.ViewModels;

namespace TcpSocket.Views
{
    public partial class CommunicationView : UserControl
    {
        public CommunicationView()
        {
            InitializeComponent();

            if (this.DataContext is CommunicationViewModel context)
            {
                context.AddServer(this.MachineServer);
                context.AddServer(this.AppServer);
            }
        }
    }
}