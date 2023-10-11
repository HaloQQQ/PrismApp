using IceTea.Core.Utils;
using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using MyApp.Prisms.ViewModels.BaseViewModels;
using IceTea.Wpf.Core.Helper;

namespace MyApp.Prisms.Helper
{
    internal static partial class Helper
    {
        internal static void Clear(this RichTextBox txt)
        {
            txt.Document.Blocks.Clear();
        }

        internal static void Info(this RichTextBox txt, BaseSocketViewModel context, string message)
        {
            if (context.IsLogging)
            {
                Helper.Log(context.Name, message);
            }

            CommonUtils.Invoke(() =>
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " " + message);

                PrintMsg(txt, paragraph);
            });
        }

        private static Paragraph GetParagraph(string type, EndPoint from, EndPoint to, BaseSocketViewModel context,
            string message)
        {
            if (context.IsLogging)
            {
                Helper.Log(context.Name, message);
            }

            Paragraph paragraph = new Paragraph();
            Run title = new Run($"[{DateTime.Now.FormatTime()}]# {type} {context.Encoding.BodyName} {from}=>{to}>" +
                                Environment.NewLine);
            paragraph.Inlines.Add(title);
            Run item = new Run(message);
            item.FontWeight = FontWeights.Bold;
            item.FontSize = 16;
            paragraph.Inlines.Add(item);

            return paragraph;
        }

        internal static void Send(this RichTextBox txt, EndPoint from, EndPoint to, BaseSocketViewModel context,
            string message)
        {
            CommonUtils.Invoke(() =>
            {
                var paragraph = GetParagraph("Send", from, to, context, message);
                paragraph.Inlines.LastInline.Foreground = Constants.SendBrush;

                PrintMsg(txt, paragraph);
            });
        }

        internal static void Recv(this RichTextBox txt, EndPoint from, EndPoint to, BaseSocketViewModel context,
            string message)
        {
            CommonUtils.Invoke(() =>
            {
                var paragraph = GetParagraph("Recv", from, to, context, message);
                paragraph.Inlines.LastInline.Foreground = Constants.RecvBrush;
                paragraph.TextAlignment = TextAlignment.Right;

                PrintMsg(txt, paragraph);
            });
        }

        private static void PrintMsg(this RichTextBox txt,
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