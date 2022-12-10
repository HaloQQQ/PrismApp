using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Helper.Utils;
using TcpSocket.Models;

namespace TcpSocket.Helper
{
    internal static partial class Helper
    {
        internal static void Clear(this RichTextBox txt)
        {
            txt.Document.Blocks.Clear();
        }
        internal static void Info(this RichTextBox txt, BaseSocketContext context, string message)
        {
            if (context.IsLogging)
            {
                Helper.Log(context.Name, message);
            }

            Invoke(() =>
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(message);

                PrintMsg(txt, paragraph);
            });
        }

        private static Paragraph GetParagraph(string type, EndPoint from, EndPoint to, BaseSocketContext context, string message)
        {
            if (context.IsLogging)
            {
                Helper.Log(context.Name, message);
            }

            Paragraph paragraph = new Paragraph();
            Run title = new Run($"[{DateTime.Now.FormatTime()}]# {type} {context.Encoding.BodyName} {from}=>{to}>" + Environment.NewLine);
            paragraph.Inlines.Add(title);
            Run item = new Run(message);
            paragraph.Inlines.Add(item);

            return paragraph;
        }

        internal static void Send(this RichTextBox txt, EndPoint from, EndPoint to, BaseSocketContext context, string message)
        {
            Invoke(() =>
            {
                var paragraph = GetParagraph("Send", from, to, context, message);
                paragraph.Inlines.LastInline.Foreground = Constants.SendBrush;

                PrintMsg(txt, paragraph);
            });
        }

        internal static void Recv(this RichTextBox txt, EndPoint from, EndPoint to, BaseSocketContext context, string message)
        {
            Invoke(() =>
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
            txt.Document.LineHeight = 1;
            txt.Document.Blocks.Add(paragraph);

            if (txt.Document.Blocks.Count > 500)
            {
                txt.Document.Blocks.Clear();
            }

            txt.ScrollToEnd();
        }
    }
}