using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using Hardcodet.Wpf.TaskbarNotification;
using Helper.Statics;
using Helper.FileOperation;
using Helper.Utils;

namespace TcpSocket.Helper
{
    internal static class Helper
    {
        internal static void Invoke(Action action)
        {
            App.Current.Dispatcher.Invoke(action);
        }

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
            TaskbarIcon taskbarIcon = App.Current.MainWindow.FindResource(Constants.TaskBar_Resource_Key) as TaskbarIcon;
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

        #region ThirdPartyDll

        // 锁屏 
        [DllImport("User32.dll")]
        public static extern bool LockWorkStation();


        private const int SW_SHOWNOMAL = 1;

        ///<summary>
        /// 该函数设置由不同线程产生的窗口的显示状态
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="cmdShow">指定窗口如何显示。查看允许值列表，请查阅ShowWindow函数的说明部分</param>
        /// <returns>如果函数原来可见，返回值为非零；如果函数原来被隐藏，返回值为零</returns>
        [DllImport("User32.dll")]
        internal static extern bool ShowWindowAsync(IntPtr hWnd, int cmdShow);

        /// <summary>
        ///  该函数将创建指定窗口的线程设置到前台，并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        ///  系统给创建前台窗口的线程分配的权限稍高于其他线程。 
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns>
        [DllImport("User32.dll", EntryPoint = "SetForegroundWindow")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern void SwitchToThisWindow(IntPtr hWnd, bool fAltTab);

        #endregion
    }
}