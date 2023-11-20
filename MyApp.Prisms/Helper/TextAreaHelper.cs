using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using IceTea.Wpf.Core.Utils;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Base;

namespace MyApp.Prisms.Helper
{
    internal static partial class Helper
    {
        internal static void Clear(this RichTextBox txt)
        {
            txt.Document.Blocks.Clear();
        }

        internal static void Info(this RichTextBox txt, bool isLogging, string name, string message)
        {
            if (isLogging)
            {
                Helper.Log(name, message);
            }

            CommonUtils.BeginInvoke(() =>
            {
                Paragraph paragraph = new Paragraph();
                paragraph.Inlines.Add(
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                    + " " + message);

                PrintMsg(txt, paragraph);
            });
        }

        private static Paragraph GetParagraph(string type, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
        {
            if (isLogging)
            {
                Helper.Log(socket.Name, message);
            }

            Paragraph paragraph = new Paragraph();
            Run title = new Run($"[{DateTime.Now.FormatTime()}]# {type} {socket.Encoding.BodyName} {from}=>{to}>".AppendLineOr());
            paragraph.Inlines.Add(title);
            Run item = new Run(message);
            item.FontWeight = FontWeights.Bold;
            item.FontSize = 16;
            paragraph.Inlines.Add(item);

            return paragraph;
        }

        internal static void Send(this RichTextBox txt, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
        {
            CommonUtils.BeginInvoke(() =>
            {
                var paragraph = GetParagraph("Send", from, to, isLogging, socket, message);
                paragraph.Inlines.LastInline.Foreground = CustomConstants.SendBrush;

                PrintMsg(txt, paragraph);
            });
        }

        internal static void Recv(this RichTextBox txt, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
        {
            CommonUtils.BeginInvoke(() =>
            {
                var paragraph = GetParagraph("Recv", from, to, isLogging, socket, message);
                paragraph.Inlines.LastInline.Foreground = CustomConstants.RecvBrush;
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