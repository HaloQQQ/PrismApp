using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Helper.Utils;
using TcpSocket.Helper;
using TcpSocket.UsefulControl;
using TcpSocket.UserControls.Function;

namespace TcpSocket
{
    public partial class MainWindow : Window
    {
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            this.UseTaskBarIcon();

            this.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;

            this.DataContext = Statics.DataContext;

            // 标题栏事件订阅
            this.TitleBarEventSubscribe();

            // 背景选择面板事件订阅
            this.SwitchBackgroundEventSubscribe();

            this.Loaded += (sender, e) =>
            {
                // 增加调整窗口大小的装饰
                //将装饰器添加到窗口的Content控件上
                var c = this.Content as UIElement;
                var layer = AdornerLayer.GetAdornerLayer(c);
                Debug.Assert(layer != null, nameof(layer) + " != null");
                layer.Add(new WindowResizeAdorner(c));
            };

            this.LoadDefaultTheme();

            this.InitBackgroundSwitchTimer();
        }

        private Random random = new Random();
        private DispatcherTimer _timer = null!;

        private void InitBackgroundSwitchTimer()
        {
            this._timer = new DispatcherTimer();
            this._timer.Tick += (sender, e) =>
            {
                DateTime now = DateTime.Now;
                var context = Statics.DataContext;
                context.SoftwareContext.CurrentTime = now.FormatTime();

                if (Statics.DataContext.SoftwareContext.BackgroundSwitch)
                {
                    if (context.ImagesContext != null && context.ImagesContext.Block.Count > 0)
                    {
                        if (now.TimeOfDay.Seconds == 0 || now.TimeOfDay.Seconds == 30)
                        {
                            var totalCount = context.ImagesContext.Block.Count;
                            var uri = context.ImagesContext.Block[this.random.Next(0, totalCount)].URI;
                            this.Container.Background = new ImageBrush(new BitmapImage(new Uri(uri)))
                            {
                                Stretch = Stretch.UniformToFill
                            };
                        }
                    }
                }
            };
            this._timer.Interval = TimeSpan.FromSeconds(1);
            this._timer.Start();
        }

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Theme

        private void LoadDefaultTheme()
        {
            var defaultThemeUri = Statics.DataContext.SoftwareContext.DefaultThemeURI;
            if (string.IsNullOrEmpty(defaultThemeUri))
            {
                this.RefreshTheme();
            }
            else
            {
                var currentUri = new Uri(defaultThemeUri, UriKind.RelativeOrAbsolute);

                if (Helper.Helper.Equals(Statics.Dark.Source.ToString(), currentUri.ToString()))
                {
                    Application.Current.Resources.MergedDictionaries.Add(Statics.Dark);
                }
                else
                {
                    Application.Current.Resources.MergedDictionaries.Add(Statics.Light);
                    currentUri = Statics.Light.Source;
                }

                var dict = Helper.Helper.GetDict(currentUri);

                this.Container.Background = dict[Constants.Container_Background] as SolidColorBrush;
            }

            var bkgrdImage = Statics.DataContext.SoftwareContext.CurrentBkGrd;
            if (File.Exists(bkgrdImage))
            {
                this.Container.Background = new ImageBrush(new BitmapImage(new Uri(bkgrdImage)))
                {
                    Stretch = Stretch.UniformToFill
                }; 
            }
        }

        private void RefreshTheme()
        {
            var dict = Helper.Helper.GetDict(Statics.Light.Source);

            if (dict == null)
            {
                Application.Current.Resources.MergedDictionaries.Remove(Statics.Dark);
                Application.Current.Resources.MergedDictionaries.Add(Statics.Light);

                dict = Helper.Helper.GetDict(Statics.Light.Source);
            }
            else
            {
                Application.Current.Resources.MergedDictionaries.Remove(Statics.Light);
                Application.Current.Resources.MergedDictionaries.Add(Statics.Dark);

                dict = Helper.Helper.GetDict(Statics.Dark.Source);
            }

            Statics.DataContext.SoftwareContext.DefaultThemeURI = dict.Source.ToString();
            this.Container.Background = dict[Constants.Container_Background] as SolidColorBrush;
        }

        #endregion

        #region KeyShortCut

        private void KeyBoard_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            // e.Handled = true;

            string? str1 = e.Parameter?.ToString();

            if (e.Command == ApplicationCommands.Open)
            {
                if (Helper.Helper.Equals(str1, Constants.MAX))
                {
                    this.WindowState = WindowState.Maximized;
                }
                else if (Helper.Helper.Equals(str1, Constants.NORMAL))
                {
                    this.WindowState = WindowState.Normal;
                }
                else if (Helper.Helper.Equals(str1, Constants.MIN))
                {
                    this.WindowState = WindowState.Minimized;
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                if (Helper.Helper.Equals(str1, Constants.CLOSE))
                {
                    this.Close();
                }
            }
        }

        #endregion

        #region 标题栏事件订阅

        private void TitleBarEventSubscribe()
        {
            this.TitleBarSlider.SwitchTheme += (sender, e) =>
            {
                e.Handled = true;

                this.RefreshTheme();
            };

            this.TitleBarSlider.SwitchBackground += (sender, e) =>
            {
                e.Handled = true;

                this.SwitchBackgroundSlider.SlideOut();
            };
        }

        #endregion

        #region 背景图选择面板事件订阅

        private void SwitchBackgroundEventSubscribe()
        {
            this.SwitchBackgroundSlider.SetBackImage += uri =>
            {
                Statics.DataContext.SoftwareContext.CurrentBkGrd = uri;
                this.Container.Background = new ImageBrush(new BitmapImage(new Uri(uri)))
                {
                    Stretch = Stretch.UniformToFill
                };
            };
        }

        #endregion

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            e.Handled = true;

            if (e.Source is TabControl tabControl)
            {
                var item = tabControl.SelectedItem as TabItem;

                if (item.Header.ToString() == "通讯工具")
                {
                    if (item.Content == null)
                    {
                        item.Content = new UsrCommunication()
                        {
                            DataContext = Statics.DataContext
                        };
                    }
                }
                else if (item.Header.ToString() == "图片列表")
                {
                    if (item.Content == null)
                    {
                        item.Content = new VirtualUI()
                        {
                            DataContext = Statics.DataContext.ImagesContext
                        };
                    }
                }
                else if (item.Header.ToString() == "进程服务")
                {
                    if (item.Content == null)
                    {
                        item.Content = new UsrProcessService()
                        {
                            DataContext = Statics.DataContext.ProcessServiceContext
                        };
                    }
                }
            }
        }
    }
}