using Prism.Mvvm;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MusicPlayerModule.Utils
{
    /// <summary>
    /// KRC文件行
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class KRCLyricsLine : BindableBase, IDisposable
    {
        private bool _isPlayingLine;

        public bool IsPlayingLine
        {
            get { return _isPlayingLine; }
            set { SetProperty<bool>(ref _isPlayingLine, value); }
        }

        private bool _isPlayed;

        public bool IsPlayed
        {
            get => this._isPlayed;
            set => SetProperty<bool>(ref _isPlayed, value);
        }

        private List<KRCLyricsChar> _chars = new List<KRCLyricsChar>();

        /// <summary>
        /// 行字符串
        /// </summary>
        public string KRCLineString
        {
            get
            {
                return string.Format(@"[{0},{1}]{2}", this.LineStart.TotalMilliseconds, this.LineDuring.TotalMilliseconds,
                    string.Join("", this.Chars.Select(x => x.KRCCharString)));
            }
        }

        /// <summary>
        /// 行开始事件
        /// </summary>
        public TimeSpan LineStart { get; set; }

        /// <summary>
        /// 行总时间
        /// </summary>
        public TimeSpan LineDuring
        {
            get
            {
                //计算行时间
                var sum = this.Chars.Select(x => x.CharDuring.TotalMilliseconds).Sum();
                return TimeSpan.FromMilliseconds(sum);
            }
        }

        /// <summary>
        /// 行内字符
        /// </summary>

        public List<KRCLyricsChar> Chars => _chars;

        public KRCLyricsLine()
        {
            this.LineStart = TimeSpan.Zero;
        }


        public KRCLyricsLine(string krclinestring) : this()
        {
            var regLineTime = new Regex(@"^\[(.*),(.*)\](.*)");

            var m1 = regLineTime.Match(krclinestring);
            if (m1.Success && m1.Groups.Count == 4)
            {
                var linestart = m1.Groups[1].Value;
                var linelength = m1.Groups[2].Value;

                this.LineStart = TimeSpan.FromMilliseconds(double.Parse(linestart));
                //this.LineDuring = TimeSpan.FromMilliseconds(double.Parse(linelength));

                var linecontent = m1.Groups[3].Value;

                var chars = Regex.Matches(linecontent, @"<(\d+),(\d+),(\d+)>(.?)");

                foreach (Match m in chars)
                {
                    this.Chars.Add(new KRCLyricsChar(m.Value));
                }
            }
        }

        public string DebuggerDisplay
        {
            get
            {
                return string.Format(@"{0:hh\:mm\:ss\.fff} {1:hh\:mm\:ss\.fff} {2}", this.LineStart, this.LineDuring,
                    string.Join(",", this.Chars.Select(x => x.Char.ToString())));
            }
        }

        public override string ToString()
        {
            return string.Join(string.Empty, this.Chars.Select(item => item.Char));
        }

        public string Words => this.ToString();

        public void Dispose()
        {
            this._chars.Clear();
            this._chars = null;
        }
    }
}
