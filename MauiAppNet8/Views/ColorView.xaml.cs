namespace MauiAppNet8.Views;

public partial class ColorView : ContentPage
{
	public ColorView()
	{
		InitializeComponent();
	}

    private void ColorBox_OnTapped(object sender, TappedEventArgs e)
    {
		this.DisplayAlert("Tapped����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnClick(object sender, EventArgs e)
    {
        this.DisplayAlert("Click����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
    {
        this.DisplayAlert("PinchUpdated����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        this.DisplayAlert("PanUpdated����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnSwiped(object sender, SwipedEventArgs e)
    {
        this.DisplayAlert($"{e.Direction}Swiped����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPointerEntered(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerEntered����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPointerExited(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerExited����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPointerMoved(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerMoved����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPointerPressed(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerPressed����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnPointerReleased(object sender, PointerEventArgs e)
    {
        this.DisplayAlert("PointerReleased����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnDragStarting(object sender, DragStartingEventArgs e)
    {
        this.DisplayAlert("DragStarting����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnDropCompleted(object sender, DropCompletedEventArgs e)
    {
        this.DisplayAlert("DropCompleted����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnDragLeave(object sender, DragEventArgs e)
    {
        this.DisplayAlert("DragLeave����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnDragOver(object sender, DragEventArgs e)
    {
        this.DisplayAlert("DragOver����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }

    private void ColorBox_OnDrop(object sender, DropEventArgs e)
    {
        this.DisplayAlert("Drop����", "��ɶ?��͵���ֻ�?", "�ٺ�");
    }
}