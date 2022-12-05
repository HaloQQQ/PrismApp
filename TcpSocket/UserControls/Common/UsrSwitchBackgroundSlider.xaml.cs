using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Helper.Utils;
using Microsoft.Win32;
using TcpSocket.Helper;
using TcpSocket.Models;

namespace TcpSocket.UserControls.Common
{
    public partial class UsrSwitchBackgroundSlider : UserControl
    {
        private const string HideSlider = "HideSlider";

        public UsrSwitchBackgroundSlider()
        {
            InitializeComponent();

            this.MouseEnter += (sender, e) =>
            {
                e.Handled = true;

                this.SlideOut();
            };

            this.MouseLeave += (sender, e) =>
            {
                e.Handled = true;

                this.SlideIn();
            };

            // 功能性字体图标点击事件
            AddHandler(MouseLeftButtonUpEvent, new MouseButtonEventHandler((sender, e) =>
            {
                e.Handled = true;

                if (e.OriginalSource is TextBlock textBlock)
                {
                    switch (textBlock.ToolTip)
                    {
                        case "向右隐藏":
                            this.SlideIn();
                            break;
                        case "布局排列":
                            if (DataContext is ImagesContext context)
                            {
                                context.ShowInList = !context.ShowInList;
                            }

                            break;
                        default:
                            AppUtils.Assert(false, "背景选择器控件功能字体图标异常!");
                            break;
                    }
                }
            }));
        }


        private void KeyBoard_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            e.Handled = true;

            string? str1 = e.Parameter.ToString();
            if (Helper.Helper.Equals(str1, HideSlider))
            {
                SlideIn();
            }
        }

        #region 背景选择侧边栏

        private void SwitchBackgroundSlider_MoveTo(double to)
        {
            var storyBoard = new Storyboard();

            var x = ((RenderTransform as TranslateTransform)!).X;
            var translateAnimation = new DoubleAnimation(x, to,
                new Duration(TimeSpan.FromSeconds(1)));
            if (to != 0)
            {
                translateAnimation.BeginTime = TimeSpan.FromMilliseconds(500);
            }

            Storyboard.SetTarget(translateAnimation, this);
            Storyboard.SetTargetProperty(translateAnimation,
                new PropertyPath("(UIElement.RenderTransform).(TranslateTransform.X)"));

            storyBoard.Children.Add(translateAnimation);

            storyBoard.Begin();
        }

        // 唤出侧边栏
        public void SlideOut()
        {
            SwitchBackgroundSlider_MoveTo(0);
        }

        // 隐藏侧边栏
        private void SlideIn()
        {
            SwitchBackgroundSlider_MoveTo(Width);
        }

        #endregion

        #region 设置背景

        private void OpenFileDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "选择背景图片";
            openFileDialog.Multiselect = false;
            openFileDialog.InitialDirectory = Constants.Image_Dir;
            openFileDialog.Filter = "图像文件|*.jpg|图像文件|*.png|所有文件|*.*";

            bool? result = openFileDialog.ShowDialog();
            if (result != null && (bool) result)
            {
                SetBackgroundImage(openFileDialog.FileName);
                Statics.DataContext.ImagesContext.ImageDir = Directory.GetParent(openFileDialog.FileName)?.ToString()!;
            }
        }

        /// <summary>
        /// 需设置背景者务必订阅
        /// </summary>
        public event Action<string> SetBackImage = null!;

        private void SetBackgroundImage(string uri)
        {
            SetBackImage?.Invoke(uri);
        }

        private void ChangeBackground_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.OriginalSource is FrameworkElement element)
            {
                if (element.DataContext is MyImage image)
                {
                    if (e.RoutedEvent.Name == MouseDoubleClickEvent.Name && image.URI != null)
                    {
                        SetBackgroundImage(image.URI);
                    }
                    else if (e.RoutedEvent.Name == MouseLeftButtonUpEvent.Name && image.URI == null)
                    {
                        OpenFileDialog();
                    }
                }
            }
        }

        #endregion
    }
}