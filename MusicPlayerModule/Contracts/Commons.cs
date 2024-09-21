using System.Windows.Controls;

namespace MusicPlayerModule.Contracts
{
    internal static class Commons
    {
        internal static void IncreaseVolume(MediaElement mediaPlayer)
        {
            var value = mediaPlayer.Volume + 0.05;

            if (value > 1)
            {
                value = 1;
            }

            mediaPlayer.Volume = value;
        }

        internal static void DecreaseVolume(MediaElement mediaPlayer)
        {
            var value = mediaPlayer.Volume - 0.05;

            if (value < 0)
            {
                value = 0;
            }

            mediaPlayer.Volume = value;
        }

        internal static void ResetMediaPlayer(Slider slider, MediaElement mediaElement)
        {
            slider.Value = 0;
            mediaElement.Stop();
            mediaElement.Position = TimeSpan.Zero;
        }
    }
}
