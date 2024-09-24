using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MusicPlayerModule.Utils
{
    /// <summary>
    /// KRC文件行字符
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class KRCLyricsWord
    {
        /// <summary>
        /// 字符
        /// </summary>
        public string Word { get; }

        /// <summary>
        /// 字符KRC字符串
        /// </summary>
        public string KRCCharString
        {
            get
            {
                return string.Format(@"<{0},{1},{2}>{3}", this.CharStart.TotalMilliseconds, this.CharDuring.TotalMilliseconds, 0, this.Word);
            }
        }

        /// <summary>
        /// 字符起始时间(计算时加上字符所属行的起始时间)
        /// </summary>
        public TimeSpan CharStart { get; set; }

        /// <summary>
        /// 字符时长
        /// </summary>
        public TimeSpan CharDuring { get; set; }

        public KRCLyricsWord()
        {
            this.CharStart = TimeSpan.Zero;
            this.CharDuring = TimeSpan.Zero;
        }

        public KRCLyricsWord(string krcCharString)
            : this()
        {
            var chars = Regex.Match(krcCharString, @"<(\d+),(\d+),(\d+)>(.+)");

            if (chars.Success)
            {
                if (chars.Groups.Count >= 4)
                {
                    var charstart = chars.Groups[1].Value;
                    var charduring = chars.Groups[2].Value;
                    var unknowAlwaysZero = chars.Groups[3].Value;

                    this.CharStart = TimeSpan.FromMilliseconds(double.Parse(charstart));
                    this.CharDuring = TimeSpan.FromMilliseconds(double.Parse(charduring));

                    if (chars.Groups.Count >= 5)
                    {
                        this.Word = chars.Groups[4].Value;
                    }
                    else
                    {
                        this.Word = string.Empty;
                    }
                }
            }
        }

        public string DebuggerDisplay
        {
            get
            {
                return string.Format(@"{0:hh\:mm\:ss\.fff} {1:hh\:mm\:ss\.fff} {2}", this.CharStart, this.CharDuring, this.Word);
            }
        }
    }
}
