using MusicPlayerModule.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using WpfStyleResources.Helper;

namespace MusicPlayerModule.Utils
{
    internal class LoadLyricToMusicModel
    {
        public static async Task LoadAsync(string dir, IEnumerable<MusicModel> musics)
        {
            await Task.Run(() =>
            {
                List<string> paths = GetLyricPaths(dir, str => str.EndsWith(".krc", StringComparison.CurrentCultureIgnoreCase));

                string? filePath = null;

                foreach (var item in musics.Where(m => m.Lyric == null))
                {
                    filePath = paths.FirstOrDefault(path => path.Contains(item.Name) &&
                                    (
                                        path.Contains(item.Singer)
                                        || path.Contains(item.Singer.Replace(" ", string.Empty))
                                    )
                                );

                    if (filePath != null)
                    {
                        KRCLyrics krc = KRCLyrics.LoadFromFile(filePath);
                        item.Lyric = krc;
                    }
                }
            }).ConfigureAwait(false);
        }

        public static List<string> GetLyricPaths(string directoryPath, Predicate<string> predicate)
        {
            Debug.Assert(Directory.Exists(directoryPath), "目录不存在");

            List<string> paths = new List<string>();

            CommonUtils.GetFiles(directoryPath, paths, predicate);

            return paths;
        }
    }
}
