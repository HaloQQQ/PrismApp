using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using System.Diagnostics;

namespace MusicPlayerModule.Models
{
    [DebuggerDisplay("DirName={DirName}, DirPath={DirPath}")]
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
