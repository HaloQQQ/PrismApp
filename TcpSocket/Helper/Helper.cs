using Hardcodet.Wpf.TaskbarNotification;
using Helper.FileOperation;
using Helper.Statics;
using Helper.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace TcpSocket.Helper
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

        internal static void ShowBalloonTip(string title, string message)
        {
            TaskbarIcon taskbarIcon =
                App.Current.MainWindow.FindResource(Constants.TaskBar_Resource_Key) as TaskbarIcon;
            taskbarIcon.ShowBalloonTip(
                title,
                message,
                BalloonIcon.Info
            );
        }

        internal static TaskbarIcon UseTaskBarIcon(this Window window)
        {
            TaskbarIcon taskbarIcon = (window.FindResource(Constants.TaskBar_Resource_Key) as TaskbarIcon)!;
            taskbarIcon.Loaded += (sender, e) =>
            {
                e.Handled = true;

                taskbarIcon.ShowBalloonTip(
                    "托盘程序",
                    "程序启动",
                    BalloonIcon.Info
                );
            };
            taskbarIcon.TrayMouseDoubleClick += (sender, e) =>
            {
                e.Handled = true;

                taskbarIcon.ShowBalloonTip(
                    "托盘程序",
                    "程序" + (window.Visibility == Visibility.Visible ? "隐藏" : "显示"),
                    BalloonIcon.Info
                );
            };

            return taskbarIcon;
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
                AppUtils.Kill(Constants.NOTE_PAD, GeneralFileTool.GetFileNameWithoutExtension(filePath));
                File.AppendAllText(filePath, wrapMsg + Environment.NewLine);
            }
        }

        internal static void OpenLog(string typeName)
        {
            var dir = Path.Combine(AppStatics.ExeDirectory, typeName);
            var filePath = Path.Combine(dir, DateTime.Now.ToString("yyyy-MM-dd") + typeName + ".log");

            if (!AppUtils.OpenWithNotePad(filePath))
            {
                ShowBalloonTip($"打开【{typeName}】日志", $"日志文件{filePath}不存在!");
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