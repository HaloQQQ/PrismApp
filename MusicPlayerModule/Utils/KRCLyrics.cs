using IceTea.Pure.BaseModels;
using IceTea.Pure.Extensions;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace MusicPlayerModule.Utils
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
#pragma warning disable CS8625 // 无法将 null 字面量转换为非 null 的引用类型。
    /// <summary>
    /// KRC歌词文件
    /// </summary>
    public class KRCLyrics : DisposableBase
    {
        private IList<KRCLyricsLine> _lines = new List<KRCLyricsLine>();
        public IList<KRCLyricsLine> Lines => _lines;

        private List<Tuple<Regex, Action<string>>> _properties;

        #region Props
        /// <summary>
        /// 歌词文本
        /// </summary>
        public string KRCString { get; }

        /// <summary>
        /// ID （总是$00000000，意义未知）
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// 艺术家
        /// </summary>
        public string Ar { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string Al { get; private set; }

        /// <summary>
        /// 标题
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// 歌词文件作者
        /// </summary>
        public string By { get; private set; }

        /// <summary>
        /// 歌曲文件Hash
        /// </summary>
        public string Hash { get; private set; }

        /// <summary>
        /// 总时长
        /// </summary>
        public TimeSpan Total
        {
            get
            {
                //计算总时间=所有行时间
                var sum = this.Lines.Sum(x => x.LineDuring.TotalMilliseconds);
                return TimeSpan.FromMilliseconds(sum);
            }
        }

        /// <summary>
        /// 偏移
        /// </summary>
        public TimeSpan Offset { get; private set; }
        #endregion

        /// <summary>
        /// 默认构造
        /// </summary>
        private KRCLyrics()
        {
            //this.Total = TimeSpan.Zero;
            this.Offset = TimeSpan.Zero;

            this._properties = new List<Tuple<Regex, Action<string>>>()
             {
                 new Tuple<Regex, Action<string>>(new Regex("\\[id:[^\\]]+\\]"), (s) => { this.ID = s; }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[al:[^\\n]+\\n"), (s) => { this.Al = s; }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[ar:[^\\]]+\\]"), (s) => { this.Ar = s; }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[ti:[^\\]]+\\]"), (s) => { this.Title = s; }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[hash:[^\\n]+\\n"), (s) => { this.Hash = s; }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[by:[^\\n]+\\n"), (s) => { this.By = s; }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[total:[^\\n]+\\n"), (s) =>
                 {
                     //this.Total = TimeSpan.FromMilliseconds(double.Parse(s));
                 }),
                 new Tuple<Regex, Action<string>>(new Regex("\\[offset:[^\\n]+\\n"), (s) =>
                 {
                     this.Offset = TimeSpan.FromMilliseconds(double.Parse(s));
                }),
            };
        }

        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="krcstring">KRC字符文本</param>
        private KRCLyrics(string krcstring) : this()
        {
            this.KRCString = krcstring;

            LoadProperties();
            void LoadProperties()
            {
                Regex regGetValueFromKeyValuePair = new Regex(@"\[(.*):(.*)\]");

                foreach (var prop in _properties)
                {
                    var m = prop.Item1.Match(this.KRCString);
                    if (m.Success)
                    {
                        var mm = regGetValueFromKeyValuePair.Match(m.Value);

                        if (mm.Success && mm.Groups.Count == 3)
                        {
                            prop.Item2(mm.Groups[2].Value);
                        }
                    }
                }
            }

            LoadLines();
            void LoadLines()
            {
                var linesMachCollection = Regex.Matches(this.KRCString, @"\[\d{1,}[^\n]+\n");
                foreach (Match m in linesMachCollection)
                {
                    this._lines.Add(new KRCLyricsLine(m.Value));
                }
            }
        }

        /// <summary>
        /// 保存到文件
        /// </summary>
        /// <param name="outputFilePath"></param>
        private void SaveToFile(string outputFilePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine(string.Format("[id:{0}]", this.ID));

            if (!string.IsNullOrEmpty(this.Al))
            {
                sb.AppendLine(string.Format("[al:{0}]", this.Al));
            }

            if (!string.IsNullOrEmpty(this.Ar))
            {
                sb.AppendLine(string.Format("[ar:{0}]", this.Ar));
            }

            if (!string.IsNullOrEmpty(this.Title))
            {
                sb.AppendLine(string.Format("[ti:{0}]", this.Title));
            }

            if (!string.IsNullOrEmpty(this.Hash))
            {
                sb.AppendLine(string.Format("[hash:{0}]", this.Hash));
            }

            if (!string.IsNullOrEmpty(this.By))
            {
                sb.AppendLine(string.Format("[by:{0}]", this.By));
            }

            if (this.Total != TimeSpan.Zero)
            {
                sb.AppendLine(string.Format("[total:{0}]", this.Total.TotalMilliseconds));
            }

            if (this.Offset != TimeSpan.Zero)
            {
                sb.AppendLine(string.Format("[offset:{0}]", this.Offset.TotalMilliseconds));
            }

            foreach (var line in this.Lines)
            {
                sb.AppendLine(line.KRCLineString);
            }

            var bytes = KRCFile.EncodeStringToBytes(sb.ToString());

            File.WriteAllBytes(outputFilePath, bytes);
        }

        /// <summary>
        /// 从文件加载
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <returns></returns>
        public static KRCLyrics LoadFromFile(string inputFilePath)
        {
            var str = KRCFile.DecodeFileToString(inputFilePath);

            return new KRCLyrics(str);
        }

        public static async Task<IEnumerable<string>> TryGetLyricPathsAsync(string directoryPath)
                => await Task.FromResult(directoryPath.GetFiles(true, str => str.EndsWithIgnoreCase(".krc"))).ConfigureAwait(false);

        protected override void DisposeCore()
        {
            foreach (var item in this._lines)
            {
                item.Dispose();
            }

            this._lines.Clear();
            this._lines = null;

            if (_properties != null)
            {
                _properties.Clear();
                _properties = null;
            }

            base.DisposeCore();
        }
    }
}
