using MusicPlayerModule.Models;
using System.Globalization;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
#pragma warning disable CS8603 // 可能返回 null 引用。
    internal class BatchAddAndPlayModelConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values != null && values.Length == 2 && values[0] is IEnumerable<FavoriteMusicViewModel> collection && values[1] is FavoriteMusicViewModel favorite)
            {
                return new BatchAddAndPlayModel(favorite, collection);
            }

            return values;
        }


        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
