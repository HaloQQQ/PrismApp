using IceTea.Atom.Extensions;
using MusicPlayerModule.Models;
using System.Globalization;
using System.Windows.Data;

namespace MusicPlayerModule.Converters
{
    internal class MusicDistributeMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.IsNotNullAnd(arr => arr.Length > 1))
            {
                var musicMove = new MusicMoveModel();

                if (values[0] is MusicModel music)
                {
                    musicMove.Music = music;
                }

                if (values[1] is string path)
                {
                    musicMove.MoveToDir = path;
                }

                return musicMove;
            }

            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
