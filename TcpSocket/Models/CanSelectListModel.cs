using System;
using System.Collections.ObjectModel;
using Helper.AbstractModel;

namespace TcpSocket.Models
{
    public interface ICanSelect
    {
        void Select();
    }

    public interface ISelectNotify
    {
        event Action<bool>? SelecteChanged;
        void Select(bool value);
    }


    public class CanSelectListModel<T> : BaseNotifyModel, ICanSelect where T : class, ISelectNotify
    {
        public ObservableCollection<T> List { get; }

        public CanSelectListModel()
        {
            this.List = new ObservableCollection<T>();
        }

        private bool _isSelectedAll;

        public bool IsSelectedAll
        {
            get => this._isSelectedAll;
            set
            {
                this._isSelectedAll = value;

                this.Select();
            }
        }

        public void Clear()
        {
            this.List.Clear();
        }

        public int AllCount => this.List.Count;

        public int SelectedCount { get; set; }

        public void Add(T context)
        {
            this.List.Add(context);
            context.SelecteChanged += result =>
            {
                if (result)
                {
                    if (++this.SelectedCount == this.AllCount)
                    {
                        this._isSelectedAll = true;

                        CallModel(nameof(this.IsSelectedAll));
                    }
                }
                else
                {
                    this.SelectedCount--;

                    var oldValue = this._isSelectedAll;

                    this._isSelectedAll = result;
                    
                    if (oldValue != result)
                    {
                        CallModel(nameof(this.IsSelectedAll));
                    }
                }
            };
        }

        public void Select()
        {
            foreach (var canSelect in this.List)
            {
                canSelect.Select(this.IsSelectedAll);
            }
        }
    }
}