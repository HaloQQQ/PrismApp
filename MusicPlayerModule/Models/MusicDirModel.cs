using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using System.Diagnostics;

namespace MusicPlayerModule.Models;

[DebuggerDisplay("DirName={DirName}, DirPath={DirPath}")]
internal class MusicDirModel
{
    public MusicDirModel(string dirPath)
    {
        DirPath = dirPath.AssertNotNull(nameof(dirPath));
        DirName = dirPath.GetCurrentDirName();
    }

    public string DirName { get; }
    public string DirPath { get; }
}
