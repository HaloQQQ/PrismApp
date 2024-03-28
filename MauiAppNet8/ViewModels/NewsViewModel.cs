using IceTea.Atom.Extensions;
using IceTea.Atom.Utils;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels
{
    internal class NewsViewModel : BindableBase
    {
        public NewsViewModel()
        {
            this.DeleteNewsItemCommand = new DelegateCommand<NewsItemModel>(item => this.News.Remove(item), item => item != null);
            this.FavoriteNewsItemCommand = new DelegateCommand<NewsItemModel>(async item => await Shell.Current.DisplayAlert("提示", "已收藏", "取消"));
            this.RefreshCommand = new DelegateCommand(() =>
            {
                this.News.Clear();

                this.News.AddRange(
                    new List<NewsItemModel>
                    {
                        new NewsItemModel("新闻1", "正文1"),
                        new NewsItemModel("新闻2", "正文2"),
                        new NewsItemModel("新闻3", "正文3"),
                        new NewsItemModel("新闻4", "正文4"),
                        new NewsItemModel("新闻5", "正文5"),
                        new NewsItemModel("新闻6", "正文6")
                    }
                );

                this.IsRefreshing = false;
            });

            this.News.AddRange(
                    new List<NewsItemModel>
                    {
                        new NewsItemModel("新闻1", "正文1"),
                        new NewsItemModel("新闻2", "正文2"),
                        new NewsItemModel("新闻3", "正文3"),
                        new NewsItemModel("新闻4", "正文4"),
                        new NewsItemModel("新闻5", "正文5"),
                        new NewsItemModel("新闻6", "正文6")
                    }
                );
        }

        private bool _isRefreshing;

        public bool IsRefreshing
        {
            get => this._isRefreshing;
            set => SetProperty<bool>(ref _isRefreshing, value);
        }


        private ObservableCollection<NewsItemModel> _news = new();

        public ObservableCollection<NewsItemModel> News
        {
            get => this._news;
            set => SetProperty<ObservableCollection<NewsItemModel>>(ref _news, value.AssertNotNull(nameof(News)));
        }


        #region Commands
        public ICommand DeleteNewsItemCommand { get; }
        public ICommand FavoriteNewsItemCommand { get; }
        public ICommand RefreshCommand { get; }
        #endregion
    }

    internal class NewsItemModel
    {
        public NewsItemModel(string title, string body)
        {
            Title = title;
            Body = body;
        }

        public string Title { get; }
        public string Body { get; }
    }
}
