using System.Windows;
using System.Windows.Controls;

namespace TcpSocket.Helper
{
    public static class MyAttachProperty
    {
        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.RegisterAttached("CornerRadius", typeof(CornerRadius), typeof(MyAttachProperty));

        public static CornerRadius GetCornerRadius(DependencyObject obj)
        {
            return (CornerRadius) obj.GetValue(CornerRadiusProperty);
        }

        public static void SetCornerRadius(DependencyObject obj, CornerRadius value)
        {
            obj.SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty AttachProperty =
            DependencyProperty.RegisterAttached("Attach", typeof(bool),
                typeof(MyAttachProperty),
                new PropertyMetadata(false, new PropertyChangedCallback(
                    (d, args) =>
                    {
                        if (d is PasswordBox passwordBox)
                        {
                            bool oValue = (bool)args.OldValue;
                            bool nValue = (bool)args.NewValue;

                            if (oValue)
                            {
                                passwordBox.PasswordChanged -= PasswordChanged;
                            }

                            if (nValue)
                            {
                                passwordBox.PasswordChanged += PasswordChanged;
                            }
                        }
                    })));

        private static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (sender is PasswordBox passwordBox)
            {
                SetPassword(passwordBox, passwordBox.Password);
            }
        }
        
        public static bool GetAttach(DependencyObject obj)
        {
            return (bool) obj.GetValue(AttachProperty);
        }

        public static void SetAttach(DependencyObject obj, bool value)
        {
            obj.SetValue(AttachProperty, value);
        }


        public static readonly DependencyProperty PasswordProperty =
            DependencyProperty.RegisterAttached("Password", typeof(string),
                typeof(MyAttachProperty));

        public static string GetPassword(DependencyObject obj)
        {
            return (string) obj.GetValue(PasswordProperty);
        }

        public static void SetPassword(DependencyObject obj, string value)
        {
            obj.SetValue(PasswordProperty, value);
        }
    }
}