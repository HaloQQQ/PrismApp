using IceTea.Atom.BaseModels;
using IceTea.Atom.Utils;
using System.Collections;

namespace MusicPlayerModule.Contracts
{
    internal abstract class ChildrenBase : BaseNotifyModel, IChildren
    {
        private IList<IList> _parents = new List<IList>();
        public IList<IList> Parents => _parents;

        public bool AddTo(IList parent)
        {
            if (!Parents.Contains(parent.AssertNotNull(nameof(parent))))
            {
                Parents.Add(parent);
            }

            parent.Add(this);

            return true;
        }

        public bool RemoveFrom(IList parent)
        {
            parent.Remove(this);

            Parents.Remove(parent);

            return true;
        }

        public bool RemoveFromAll()
        {
            foreach (var item in Parents)
            {
                item.Remove(this);
            }

            return true;
        }
    }
}
