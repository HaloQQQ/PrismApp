using IceTea.Atom.Utils;
using System.Windows.Media.Imaging;

namespace CustomControlsDemoModule.Models
{
    internal class Picture
    {
        public Picture(string description, string filePath, BitmapSource source)
        {
            Description = description.AssertNotNull(nameof(description));
            FilePath = filePath.AssertNotNull(nameof(filePath));
            Source = source.AssertNotNull(nameof(source));
        }

        public string Description { get; }

        public string FilePath { get; }

        public BitmapSource Source { get; }
    }
}
