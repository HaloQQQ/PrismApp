using System.Windows.Controls;
using MyApp.Prisms.ViewModels;

namespace MyApp.Prisms.Views
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