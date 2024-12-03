using System.Runtime.InteropServices;
using System;
using System.Windows;
using System.Windows.Input;
using IceTea.Wpf.Atom.Extensions;

namespace MyApp.Prisms.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        [DllImport("user32.dll")] //导入user32.dll函数库
        public static extern bool GetCursorPos(out System.Drawing.Point lpPoint);

        [DllImport("user32.dll")]//取设备场景 
        private static extern IntPtr GetDC(IntPtr hwnd);//返回设备场景句柄 
        [DllImport("gdi32.dll")]//取指定点颜色 
        private static extern int GetPixel(IntPtr hdc, System.Drawing.Point p);

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (GetCursorPos(out System.Drawing.Point lpPoint))
            {
                System.Drawing.Point p = new System.Drawing.Point(lpPoint.X, lpPoint.Y);//取置顶点坐标 
                IntPtr hdc = GetDC(new IntPtr(0));//取到设备场景(0就是全屏的设备场景) 
                int c = GetPixel(hdc, p);//取指定点颜色 
                byte r = (byte)(c & 0xFF);//转换R 
                byte g = (byte)((c & 0xFF00) / 256);//转换G 
                byte b = (byte)((c & 0xFF0000) / 65536);//转换B 
                //this.Title = CommonExtensions.GetStringFromRGBA(r, g, b);

                this.Title = p.ToString();
            }
        }
    }
}