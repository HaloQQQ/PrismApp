using System;
using System.Windows.Forms;
using System.Windows.Threading;

namespace MusicPlayerModule.Utils
{
    internal static class CommonFileUtils
    {
        internal static void InvokeAtOnce(Action action)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(action);
        }

        internal static void Invoke(Action action)
        {
            System.Windows.Application.Current?.Dispatcher?.Invoke(DispatcherPriority.Background, action);
        }

        /// <summary>
        /// 如何成功则返回选中的目录，否则返回string.Empty
        /// </summary>
        /// <returns></returns>
        internal static string OpenFolderDialog(string orginalPath)
        {
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = orginalPath;
            //folderDialog.ShowNewFolderButton = true;

            if (folderDialog.ShowDialog() == DialogResult.OK)
            {
                return folderDialog.SelectedPath;
            }

            return string.Empty;
        }

        internal enum MediaType
        {
            mp3,
            mp4
        }

        /// <summary>
        /// 如何成功则返回OpenFileDialog，否则返回null
        /// </summary>
        /// <returns></returns>
        internal static OpenFileDialog OpenFileDialog(string orginalPath, MediaType mediaType)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = $"文件 (*.{mediaType})|*.{mediaType}|所有文件 (*.*)|*.*";
            openFileDialog.InitialDirectory = orginalPath;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Title = "选择文件";
            openFileDialog.Multiselect = true;

            if (DialogResult.OK == openFileDialog.ShowDialog())
            {
                return openFileDialog;
            }

            return null;
        }
    }
}
