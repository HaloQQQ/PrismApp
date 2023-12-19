using IceTea.Atom.Extensions;
using MusicPlayerModule.Models;
using System.Diagnostics;
using System.IO;

namespace MusicPlayerModule.Utils
{
    internal class LoadLyricToMusicModel
    {
        public static async Task LoadAsync(string dir, IEnumerable<MusicModel> musics)
        {
            await Task.Run(() =>
            {
                IEnumerable<string> paths = GetLyricPaths(dir,
                    str => str.EndsWithIgnoreCase(".krc"));

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
                        item.Lyric = KRCLyrics.LoadFromFile(filePath);
                    }
                }
            }).ConfigureAwait(false);
        }

        public static async Task LoadAsync(string dir, MusicModel music)
        {
            Debug.Assert(music != null, "音乐实体不存在");

            if (music.IsPureMusic || music.IsLoadingLyric || music.Lyric != null)
            {
                return;
            }

            music.IsLoadingLyric = true;

            await Task.Run(() =>
            {
                IEnumerable<string> paths = GetLyricPaths(dir,
                    str => str.EndsWithIgnoreCase(".krc"));

                string? lyricFilePath = paths.FirstOrDefault(path => path.Contains(music.Name) &&
                                                                (
                                                                    path.Contains(music.Singer)
                                                                    || path.Contains(
                                                                        music.Singer.Replace(" ", string.Empty))
                                                                )
                                                            );

                if (!(music.IsPureMusic = lyricFilePath == null))
                {
                    music.Lyric = KRCLyrics.LoadFromFile(lyricFilePath);
                }
            })
            .ContinueWith(task => music.IsLoadingLyric = false)
            .ConfigureAwait(false);
        }

        public static IEnumerable<string> GetLyricPaths(string directoryPath, Predicate<string> predicate)
        {
            Debug.Assert(Directory.Exists(directoryPath), "目录不存在");

            return directoryPath.GetFiles(predicate);
        }
    }
}