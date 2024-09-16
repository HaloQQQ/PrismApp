using System.Windows.Controls;

namespace MusicPlayerModule.Contracts
{
    internal static class Commons
    {
        public static void IncreaseVolume(MediaElement mediaPlayer)
        {
            var value = mediaPlayer.Volume + 0.05;

            if (value > 1)
            {
                value = 1;
            }

            mediaPlayer.Volume = value;
        }

        public static void DecreaseVolume(MediaElement mediaPlayer)
        {
            var value = mediaPlayer.Volume - 0.05;

            if (value < 0)
            {
                value = 0;
            }

            mediaPlayer.Volume = value;
        }
    }
}
