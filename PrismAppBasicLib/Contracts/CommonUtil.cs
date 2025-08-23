using IceTea.Pure.Contracts;
using IceTea.Pure.Extensions;
using IceTea.Pure.Utils;
using IceTea.Wpf.Atom.Utils;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace PrismAppBasicLib.Contracts
{
    public static class CommonUtil
    {
        public static void PublishMessage(IEventAggregator eventAggregator, string msg, int seconds = 3)
        {
            eventAggregator.AssertNotNull(nameof(IEventAggregator)).GetEvent<DialogMessageEvent>().Publish(new DialogMessage(msg, seconds));
        }

        public static void SubscribeMessage(IEventAggregator eventAggregator, Action<DialogMessage> action)
        {
            eventAggregator.AssertNotNull(nameof(IEventAggregator)).GetEvent<DialogMessageEvent>().Subscribe(action);
        }

        public static void Log(string typeName, string message)
        {
            var dir = Path.Combine(AppStatics.ExeDirectory, typeName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var filePath = Path.Combine(dir, DateTime.Now.FormatTime("yyyy-MM-dd") + typeName + ".log");

            var wrapMsg = GetWrapMsg(message);
            try
            {
                File.AppendAllText(filePath, wrapMsg + Environment.NewLine);
            }
            catch
            {
                AppUtils.Kill("notepad");
                File.AppendAllText(filePath, wrapMsg + Environment.NewLine);
            }

            string GetWrapMsg(string __message)
            {
                var process = Process.GetCurrentProcess();
                return
                    $"【{AppStatics.Ip}】【{process.ProcessName}】【{process.Id}】【{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}】【{__message}】【{Environment.OSVersion.VersionString}】";
            }
        }

        public static void OpenLog(string typeName)
        {
            var dir = Path.Combine(AppStatics.ExeDirectory, typeName);
            var filePath = Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd") + typeName + ".log");

            if (AppUtils.OpenWithNotePad(filePath) == null)
            {
                MessageBox.Show($"日志文件{filePath}不存在!");
            }
        }

        public static void Clear(this RichTextBox txt)
        {
            txt.Document.Blocks.Clear();
        }

        public static void Info(this RichTextBox txt, bool isLogging, string name, string message)
        {
            if (isLogging)
            {
                Log(name, message);
            }

            WpfAtomUtils.BeginInvoke(() =>
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " " + message);

                PrintMsg(txt, paragraph);
            });
        }

        public static void PrintMsg(this RichTextBox txt,
            Paragraph paragraph)
        {
            paragraph.LineHeight = 5;
            paragraph.FontSize = 14;
            txt.Document.Blocks.Add(paragraph);

            if (txt.Document.Blocks.Count > 500)
            {
                txt.Document.Blocks.Clear();
            }

            txt.ScrollToEnd();
        }
    }
}
