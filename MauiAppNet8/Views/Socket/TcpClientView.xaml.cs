using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using MauiAppNet8.ViewModels.Socket;

namespace MauiAppNet8.Views.Socket;

public partial class TcpClientView : ContentPage
{
    public TcpClientView()
    {
        InitializeComponent();
    }

    private void GetIps_Click(object sender, EventArgs e)
    {
        var ips = AppUtils.GetIpAddressColl().Select(ip => ip.ToString());

        this.DisplayAlert("ÏÔÊ¾IP", Environment.NewLine.Join(ips), "¹Ø±Õ");
    }

    protected override void OnDisappearing()
    {
        if (this.BindingContext is BaseSocketViewModel viewModel && viewModel.Socket.IsNotNullAnd(socket => socket.IsConnected))
        {
            viewModel.ConnectCommand.Execute(null);
        }

        base.OnDisappearing();
    }
}