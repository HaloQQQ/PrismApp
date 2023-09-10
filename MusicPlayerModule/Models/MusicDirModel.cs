using System.IO;

namespace MusicPlayerModule.Models
{
    internal class MusicDirModel
    {
        public MusicDirModel(string dirPath)
        {
            DirPath = dirPath;
            DirName = new DirectoryInfo(dirPath).Name;
        }

        public string DirName { get; private set; }
        public string DirPath { get; private set; }
    }
}
