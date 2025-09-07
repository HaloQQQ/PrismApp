using IceTea.Pure.BaseModels;
using IceTea.Pure.Contracts;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace MusicPlayerModule.Utils
{
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
    /// <summary>
    /// KRC文件行
    /// </summary>
    [DebuggerDisplay("{DebuggerDisplay}")]
    public class KRCLyricsLine : NotifyBase
    {
        private bool _isPlayingLine;

        public bool IsPlayingLine
        {
            get => _isPlayingLine; 
            set => SetProperty<bool>(ref _isPlayingLine, value);
        }

        public bool IsEnglish { get; private set; }

        private bool _isPlayed;

        public bool IsPlayed
        {
            get => this._isPlayed;
            set => SetProperty<bool>(ref _isPlayed, value);
        }

        private List<KRCLyricsWord> _chars = new List<KRCLyricsWord>();

        /// <summary>
        /// 行字符串
        /// </summary>
        public string KRCLineString
            => string.Format(@"[{0},{1}]{2}", this.LineStart.TotalMilliseconds, this.LineDuring.TotalMilliseconds,
                string.Join("", this.Chars.Select(x => x.KRCCharString)));

        /// <summary>
        /// 行开始时间
        /// </summary>
        public TimeSpan LineStart { get; set; }

#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
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
        public List<KRCLyricsWord> Chars => _chars;

        internal KRCLyricsLine(string krclinestring)
        {
            this.LineStart = TimeSpan.Zero;

            var regLineTime = new Regex(@"^\[(.*),(.*)\](.*)");

            var m1 = regLineTime.Match(krclinestring);
            if (m1.Success && m1.Groups.Count == 4)
            {
                var linestart = m1.Groups[1].Value;
                var linelength = m1.Groups[2].Value;

                this.LineStart = TimeSpan.FromMilliseconds(double.Parse(linestart));
                //this.LineDuring = TimeSpan.FromMilliseconds(double.Parse(linelength));

                var linecontent = m1.Groups[3].Value;

                var chars = Regex.Matches(linecontent, @"<(\d+),(\d+),(\d+)>[^<\r]+");

                foreach (Match m in chars)
                {
                    this.Chars.Add(new KRCLyricsWord(m.Value));
                }
            }
        }

        private string _words;
        public string Words => _words ??= this.ToString();

        private string _verticalWords;
        public string VerticalWords =>
            _verticalWords ??= string.Join(string.Empty, this.Chars.Select(item => item.Word)).Replace(' ', '\u00a0');

        public override string ToString()
        {
            var line = string.Join(string.Empty, this.Chars.Select(item => item.Word));
            this.IsEnglish = Regex.IsMatch(line, RegexConstants.EnglishPattern);

            return this.IsEnglish ? line : line.Replace(' ', '\u3000');
        }

        protected override void DisposeCore()
        {
            base.DisposeCore();

            this._chars.Clear();
            this._chars = null;
        }

        public string DebuggerDisplay
            => string.Format(@"{0:hh\:mm\:ss\.fff} {1:hh\:mm\:ss\.fff} {2}", this.LineStart, this.LineDuring,
                string.Join(",", this.Chars.Select(x => x.Word.ToString())));
    }
}
