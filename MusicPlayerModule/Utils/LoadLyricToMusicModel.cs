using IceTea.Atom.Extensions;
using MusicPlayerModule.Models;
using System.Diagnostics;
using System.IO;

namespace MusicPlayerModule.Utils
{
    internal class LoadLyricToMusicModel
    {
        private static IEnumerable<string> _paths;

        public static async Task LoadAsync(string dir, IEnumerable<MusicModel> musics)
        {
            await Task.Run(() =>
            {
                IEnumerable<string> paths = GetLyricPaths(dir,
                    str => str.EndsWith(".krc", StringComparison.CurrentCultureIgnoreCase));

                string? filePath;

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

        public static async Task LoadAsync(string dir, MusicModel music)
        {
            Debug.Assert(music != null, "音乐实体不存在");

            if (music.Lyric != null)
            {
                return;
            }

            await Task.Run(() =>
            {
                IEnumerable<string> paths = GetLyricPaths(dir,
                    str => str.EndsWith(".krc", StringComparison.CurrentCultureIgnoreCase));

                string? filePath = paths.FirstOrDefault(path => path.Contains(music.Name) &&
                                                                (
                                                                    path.Contains(music.Singer)
                                                                    || path.Contains(
                                                                        music.Singer.Replace(" ", string.Empty))
                                                                )
                );

                if (filePath != null)
                {
                    KRCLyrics krc = KRCLyrics.LoadFromFile(filePath);
                    music.Lyric = krc;
                }
            }).ConfigureAwait(false);
        }

        public static IEnumerable<string> GetLyricPaths(string directoryPath, Predicate<string> predicate)
        {
            if (_paths != null)
            {
                return _paths;
            }

            Debug.Assert(Directory.Exists(directoryPath), "目录不存在");

            List<string> paths = directoryPath.GetFiles(predicate);

            if (paths != null && paths.Count > 0)
            {
                _paths = paths;
            }

            return paths;
        }
    }
}