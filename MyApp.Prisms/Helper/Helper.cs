using System.Windows;
using System.Windows.Controls.Primitives;

namespace MyApp.Prisms.Helper
{
    internal static partial class Helper
    {
        internal static bool IsInPopup(FrameworkElement element)
        {
            var current = element;
            while (current != null)
            {
                if (current is Popup)
                {
                    return true;
                }

                current = current.Parent as FrameworkElement;
            }

            return false;
        }
    }
}