using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Prism.Events;
using Prism.Ioc;
using MyApp.Prisms.MsgEvents;
using IceTea.Atom.Constants;
using IceTea.Atom.Utils;
using IceTea.Atom.Extensions;

namespace MyApp.Prisms.Helper
{
    internal static partial class Helper
    {
        internal static bool IsInPopup(FrameworkElement element)
        {
            var current = element;
            while (current != null)
            {
                if (current is Popup)
                {
                    return true;
                }

                current = current.Parent as FrameworkElement;
            }

            return false;
        }

        internal static ResourceDictionary GetDict(Uri uri)
        {
            return Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(dict => dict.Source.ToString() == uri.ToString())!;
        }

        private static string GetWrapMsg(string message)
        {
            var process = Process.GetCurrentProcess();
            return
                $"【{AppStatics.Ip}】【{process.ProcessName}】【{process.Id}】【{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}】【{message}】【{Environment.OSVersion.VersionString}】";
        }

        internal static void Log(string typeName, string message)
        {
            var dir = Path.Combine(AppStatics.ExeDirectory, typeName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            var filePath = Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd") + typeName + ".log");

            var wrapMsg = GetWrapMsg(message);
            try
            {
                File.AppendAllText(filePath, wrapMsg + Environment.NewLine);
            }
            catch
            {
                AppUtils.Kill(Constants.NOTE_PAD, filePath.GetFileNameWithoutExtension());
                File.AppendAllText(filePath, wrapMsg + Environment.NewLine);
            }
        }

        internal static void OpenLog(string typeName)
        {
            var dir = Path.Combine(AppStatics.ExeDirectory, typeName);
            var filePath = Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd") + typeName + ".log");

            if (!AppUtils.OpenWithNotePad(filePath))
            {
                ContainerLocator.Current.Resolve<IEventAggregator>()
                    ?.GetEvent<DialogMessageEvent>()
                    ?.Publish(new Models.DialogMessage($"日志文件{filePath}不存在!", 2));
            }
        }

        internal static ResourceDictionary GetDict(string url)
        {
            return new ResourceDictionary()
            {
                Source = new Uri(url, UriKind.RelativeOrAbsolute)
            };
        }

        /// <summary>
        /// 给长城贴瓷砖这事，算是完了
        /// </summary>
        /// <param name="element"></param>
        /// <returns></returns>
        internal static Popup SetPopupIn(UserControl element)
        {
            var popup = new Popup();
            popup.AllowsTransparency = true;
            popup.PlacementTarget = element;
            popup.Placement = PlacementMode.Center;
            popup.StaysOpen = false;

            var child = element.Content as UIElement;
            element.Content = null;

            popup.Child = child;

            // popup.Height = element.Height;
            // popup.SetBinding(Popup.HeightProperty, new Binding("Height") {Source = element});


            element.Content = popup;

            return popup;
        }
    }
}