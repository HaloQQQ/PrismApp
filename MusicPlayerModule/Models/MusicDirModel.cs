using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;

namespace MusicPlayerModule.Models
{
    internal class MusicDirModel
    {
        public MusicDirModel(string dirPath)
        {
            DirPath = dirPath.AssertNotNull(nameof(dirPath));
            DirName = dirPath.GetCurrentDirName();
        }

        public string DirName { get; private set; }
        public string DirPath { get; private set; }
    }
}
