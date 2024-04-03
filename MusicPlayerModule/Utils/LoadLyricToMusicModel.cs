using IceTea.Atom.Extensions;
using MusicPlayerModule.Common;
using MusicPlayerModule.Models;
using System.Diagnostics;
using System.IO;

namespace MusicPlayerModule.Utils
{
    internal class LoadLyricToMusicModel
    {
        public static async Task LoadLyricAsync(string lyricDir, IEnumerable<MusicModel> musics)
        {
            await Parallel.ForEachAsync(musics, async (music, token) =>
            {
                if (!token.IsCancellationRequested)
                {
                    await LoadLyricAsync(lyricDir, music);
                }
            });
        }

        public static async Task LoadLyricAsync(string lyricDir, MusicModel music)
        {
            Debug.Assert(music != null, "音乐实体不存在");

            if (music.IsPureMusic || music.IsLoadingLyric || music.Lyric != null)
            {
                return;
            }

            music.IsLoadingLyric = true;

            await Task.Run(() =>
            {
                IEnumerable<string> paths = GetLyricPaths(lyricDir);

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

        public static string TryGetLyricDir(string originDir)
        {
            var lyricFiles = GetLyricPaths(originDir);

            if (!lyricFiles.Any())
            {
                var rootPath = Path.GetPathRoot(originDir);
                var currentPath = Path.GetFullPath(originDir);

                if (rootPath.EqualsIgnoreCase(currentPath))
                {
                    return string.Empty;
                }

                return TryGetLyricDir(originDir.GetParentPath());
            }

            return CustomStatics.LyricDir = lyricFiles.First().GetParentPath();
        }

        public static IEnumerable<string> GetLyricPaths(string directoryPath)
                => GetLyricPaths(directoryPath, str => str.EndsWithIgnoreCase(".krc"));

        public static IEnumerable<string> GetLyricPaths(string directoryPath, Predicate<string> predicate)
        {
            Debug.Assert(Directory.Exists(directoryPath), "目录不存在");

            return directoryPath.GetFiles(predicate);
        }
    }
}