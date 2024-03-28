namespace MauiAppNet8.Contracts
{
    public class AppAttachProperty
    {
        public static readonly BindableProperty IsAutowireProperty =
            BindableProperty.CreateAttached("IsAutowire", typeof(bool), typeof(AppAttachProperty), false, BindingMode.OneWay, null, (view, oV, nV) =>
            {
                if ((bool)nV)
                {
                    Prism.Mvvm.ViewModelLocationProvider.AutoWireViewModelChanged(view, Bind);

                    void Bind(object view, object viewModel)
                    {
                        if (view is BindableObject element)
                            element.BindingContext = viewModel;
                    }
                }
            });

        public static bool GetIsAutowire(BindableObject obj)
        {
            return (bool)obj.GetValue(IsAutowireProperty);
        }

        public static void SetIsAutowire(BindableObject obj, bool value)
        {
            obj.SetValue(IsAutowireProperty, value);
        }
    }
}
