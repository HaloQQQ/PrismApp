using IceTea.NetCore.Utils.AppHotKey;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Input;

namespace MusicPlayerModule.Converters
{
    internal class KeyGestureDicConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values[0] is IDictionary<ICommand, AppHotKeyModel> dic && values[1] is ICommand command)
            {
                return dic[command].SelectedKey;
            }

            return values;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    internal class ModifierKeyGestureDicConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values[0] is IDictionary<ICommand, AppHotKeyModel> dic && values[1] is ICommand command)
            {
                return dic[command].ModifierKey;
            }

            return values;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
