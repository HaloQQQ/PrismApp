namespace MauiAppNet8.Views;

public partial class ColorView : ContentPage
{
	public ColorView()
	{
		InitializeComponent();
	}

    private void ColorBox_OnTapped(object sender, TappedEventArgs e)
    {
		this.DisplayAlert("Tapped弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnClick(object sender, EventArgs e)
    {
        this.DisplayAlert("Click弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        this.DisplayAlert("PinchUpdated弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        this.DisplayAlert("PanUpdated弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnSwiped(object sender, SwipedEventArgs e)
    {
        this.DisplayAlert($"{e.Direction}Swiped弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPointerEntered(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerEntered弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPointerExited(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerExited弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPointerMoved(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerMoved弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPointerPressed(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerPressed弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnPointerReleased(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerReleased弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnDragStarting(object sender, DragStartingEventArgs e)
    {
        this.DisplayAlert("DragStarting弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnDropCompleted(object sender, DropCompletedEventArgs e)
    {
        this.DisplayAlert("DropCompleted弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnDragLeave(object sender, DragEventArgs e)
    {
        this.DisplayAlert("DragLeave弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnDragOver(object sender, DragEventArgs e)
    {
        this.DisplayAlert("DragOver弹窗", "干啥?想偷看手机?", "嘿嘿");
    }

    private void ColorBox_OnDrop(object sender, DropEventArgs e)
    {
        this.DisplayAlert("Drop弹窗", "干啥?想偷看手机?", "嘿嘿");
    }
}