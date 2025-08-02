using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using IceTea.Atom.Extensions;
using IceTea.SocketStandard.Contracts;
using IceTea.Wpf.Atom.Utils;
using PrismAppBasicLib.Contracts;

namespace MyApp.Prisms.Helper
{
    internal static partial class Helper
    {
        private static Paragraph GetParagraph(string type, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
        {
            if (isLogging)
            {
#pragma warning disable CA1416 // 验证平台兼容性
                CommonUtil.Log(socket.Name, message);
            }

            Paragraph paragraph = new Paragraph();
            Run title = new Run($"[{DateTime.Now.FormatTime()}]# {type} {socket.EncodingName} {from}=>{to}>".AppendLineOr());
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
            WpfAtomUtils.BeginInvoke(() =>
            {
                var paragraph = GetParagraph("Send", from, to, isLogging, socket, message);
                paragraph.Inlines.LastInline.Foreground = CustomConstants.SendBrush;

                txt.PrintMsg(paragraph);
            });
        }

        internal static void Recv(this RichTextBox txt, EndPoint from, EndPoint to, bool isLogging, ISocket socket,
            string message)
        {
            WpfAtomUtils.BeginInvoke(() =>
            {
                var paragraph = GetParagraph("Recv", from, to, isLogging, socket, message);
                paragraph.Inlines.LastInline.Foreground = CustomConstants.RecvBrush;
                paragraph.TextAlignment = TextAlignment.Right;

                txt.PrintMsg(paragraph);
            });
        }
    }
}