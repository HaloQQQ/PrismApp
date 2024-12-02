using Prism.Events;
using Prism.Ioc;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using MyApp.Prisms.MsgEvents;
using MyApp.Prisms.ViewModels;
using IceTea.Atom.Extensions;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using IceTea.Wpf.Atom.Utils;
using MyApp.Prisms.Helper;
using IceTea.Atom.Contracts;
using PrismAppBasicLib.Models;

namespace MyApp.Prisms.Views
{
    public partial class SwitchBackgroundView : UserControl
    {
        private ImageDisplayViewModel _imagesContext;

        private readonly IEventAggregator _eventAggregator;

        public SwitchBackgroundView()
        {
            InitializeComponent();

            this._imagesContext = this.DataContext as ImageDisplayViewModel;

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

            this._eventAggregator = ContainerLocator.Current.Resolve<IEventAggregator>();

            this._eventAggregator.GetEvent<BackgroundImageSelectorShowEvent>().Subscribe(() => this.SlideOut());
        }

        private void KeyBoard_OnExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Close)
            {
                SlideIn();

                e.Handled = true;
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
            SwitchBackgroundSlider_MoveTo(this.ActualWidth);
        }

        #endregion

        #region 设置背景

        private void OpenFileDialog()
        {
            var settingManager = ContainerLocator.Current.Resolve<ISettingManager<SettingModel>>();

            var openFileDialog = CommonAtomUtils.OpenFileDialog(settingManager[CustomConstants.IMAGE].Value, new PictureMedia());

            if (openFileDialog != null)
            {
                var file = openFileDialog.FileName;

                var data = this._imagesContext.Data;

                if (data.AddIfNotAnyWhile(item => file.EqualsIgnoreCase(item.URI), () => new MyImage(file)))
                {
                    SetBackgroundImage(file);

                    settingManager[CustomConstants.IMAGE].Value = file.GetParentPath();

                    this._imagesContext.RaisePropertyChangedEvent(nameof(ImageDisplayViewModel.ImagesCount));
                }
            }
        }

        private void SetBackgroundImage(string uri)
        {
            this._eventAggregator.GetEvent<BackgroundImageUpdateEvent>().Publish(uri);
        }

        private void ChangeBackground_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            if (e.OriginalSource is FrameworkElement element)
            {
                if (element.DataContext is MyImage image)
                {
                    if (e.RoutedEvent == MouseDoubleClickEvent && !image.IsEmpty)
                    {
                        SetBackgroundImage(image.URI);
                    }
                    else if (e.RoutedEvent == MouseLeftButtonUpEvent && image.IsEmpty)
                    {
                        OpenFileDialog();
                    }
                }
            }
        }

        #endregion
    }
}