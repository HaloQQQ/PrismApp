using IceTea.Atom.BaseModels;
using System.Collections;

namespace MusicPlayerModule.Contracts
{
    internal abstract class ChildrenBase : BaseNotifyModel, IChildren
    {
        private IList<IList> _parents = new List<IList>();
        public IList<IList> Parents => _parents;

        public bool TryAddTo(IList parent)
        {
            if (parent.Contains(this))
            {
                return false;
            }

            if (!Parents.Contains(parent))
            {
                Parents.Add(parent);
            }

            parent.Add(this);

            return true;
        }

        public bool RemoveFrom(IList parent)
        {
            if (Parents.Remove(parent))
            {
                parent.Remove(this);

                return true;
            }

            return false;
        }

        private bool _isRemovedAll;
        public bool RemoveFromAll()
        {
            if (!_isDisposed || !_isRemovedAll)
            {
                foreach (var item in Parents)
                {
                    item.Remove(this);
                }

                _isRemovedAll = true;

                this.Dispose();

                return true;
            }

            return false;
        }

        private bool _isDisposed;
        /// <summary>
        /// 从父集合移除
        /// </summary>
        public virtual void Dispose()
        {
            if (!_isDisposed || !_isRemovedAll)
            {
                _isDisposed = true;

                this.RemoveFromAll();

                this.Parents.Clear();
            }
        }
    }
}
