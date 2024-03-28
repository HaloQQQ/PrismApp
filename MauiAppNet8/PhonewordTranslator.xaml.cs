using MauiAppNet8.ViewModels;

namespace MauiAppNet8;

public partial class PhonewordTranslator : ContentPage
{
    private PhoneNumTranslateViewModel _viewModel;

	public PhonewordTranslator()
	{
		InitializeComponent();

        this._viewModel = this.BindingContext as PhoneNumTranslateViewModel;
	}

    private async void OnCall(object sender, EventArgs e)
    {                                
        if (await this.DisplayAlert(
                        "Dial a Number",
                        "Would you like to call " + this._viewModel.PhoneNum + "?",
                        "Yes",
                        "No"))
        {
            try
            {
                if (PhoneDialer.Default.IsSupported)
                    PhoneDialer.Default.Open(this._viewModel.PhoneNum);
            }
            catch (ArgumentNullException)
            {
                await DisplayAlert("Unable to dial", "Phone number was not valid.", "OK");
            }
            catch (Exception)
            {
                await DisplayAlert("Unable to dial", "Phone dialing failed.", "OK");
            }
        }
    }
}