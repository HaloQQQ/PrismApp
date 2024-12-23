using IceTea.Atom.Extensions;
using MusicPlayerModule.Models;
using System.Diagnostics;
using System.IO;

namespace MusicPlayerModule.Utils
{
    internal class LoadLyricToMusicModel
    {
        public static async Task TryLoadLyricAsync(string lyricDir, IEnumerable<MusicModel> musics)
        {
            await Parallel.ForEachAsync(musics, async (music, token) =>
            {
                if (!token.IsCancellationRequested)
                {
                    await TryLoadLyricAsync(lyricDir, music);
                }
            });
        }

        public static async Task TryLoadLyricAsync(string lyricDir, MusicModel music)
        {
            Debug.Assert(music != null, "音乐实体不存在");

            if (music.IsPureMusic || music.IsLoadingLyric || music.Lyric != null)
            {
                return;
            }

            music.IsLoadingLyric = true;

            IEnumerable<string> paths = await TryGetLyricPathsAsync(lyricDir);

            string? lyricFilePath = paths.FirstOrDefault(path => path.Contains(music.Name) &&
                                                            (
                                                                path.Contains(music.Singer)
                                                                || path.Contains(
                                                                    music.Singer.Replace(" ", string.Empty))
                                                            )
                                                        );

            if (!(music.IsPureMusic = lyricFilePath == null))
            {
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                music.Lyric = KRCLyrics.LoadFromFile(lyricFilePath);
            }

            music.IsLoadingLyric = false;
        }

        public static async Task<string> TryGetLyricDir(string originDir)
        {
            var lyricFiles = await TryGetLyricPathsAsync(originDir);

            if (!lyricFiles.Any())
            {
                return string.Empty;
            }

            return lyricFiles.First().GetParentPath();
        }

        public static async Task<IEnumerable<string>> TryGetLyricPathsAsync(string directoryPath)
                => await TryGetLyricPathsAsync(directoryPath, str => str.EndsWithIgnoreCase(".krc"));

        public static async Task<IEnumerable<string>> TryGetLyricPathsAsync(string directoryPath, Predicate<string> predicate)
        {
            Debug.Assert(Directory.Exists(directoryPath), "目录不存在");

            return await Task.FromResult(directoryPath.GetFiles(predicate));
        }
    }
}