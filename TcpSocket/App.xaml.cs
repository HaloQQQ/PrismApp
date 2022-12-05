using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using TcpSocket.Helper;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace TcpSocket
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Mutex? mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            App.Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (ConfigurationHelper.Instance.IsOnlyOne())
            {
                string processName = Process.GetCurrentProcess().ProcessName;
                mutex = new Mutex(true, processName, out bool isNew);
                {
                    if (!isNew)
                    {
                        Helper.Helper.Log(Constants.Software_Log_Dir, "当前已有软件运行，启动失败!");

                        foreach (var process in Process.GetProcessesByName(processName))
                        {
                            if (process.Id != Process.GetCurrentProcess().Id)
                            {
                                Helper.Helper.ShowWindowAsync(process.MainWindowHandle, 1);
                            }
                        }

                        MessageBox.Show("当前已有软件运行，启动失败!");

                        Environment.Exit(1);
                    }

                    Helper.Helper.Log(Constants.Software_Log_Dir, $"启动成功!");
                }
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            base.OnExit(e);

            Helper.Helper.Log(Constants.Software_Log_Dir, $"退出成功!");
        }

        private IEnumerable<string> GetMessageList(Exception exception)
        {
            var list = new List<string>();
            list.Add(exception.Message);

            while ((exception = exception.InnerException) != null)
            {
                list.Add(exception.Message);
            }

            return list;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var list = this.GetMessageList(e.ExceptionObject as Exception);
            
            var message = "Domain出现异常:\r\n" + string.Join("\r\n", list);
            Helper.Helper.Log("Domain异常日志", message);
            MessageBox.Show(message);
        }

        private void Current_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            var list = this.GetMessageList(e.Exception);
            
            var message = "App出现异常:\r\n" + string.Join("\r\n", list);
            Helper.Helper.Log("App异常日志", message);
            MessageBox.Show(message);
        }
    }
}