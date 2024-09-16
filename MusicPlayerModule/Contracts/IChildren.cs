using System.Collections;

namespace MusicPlayerModule.Contracts
{
    internal interface IChildren
    {
        IList<IList> Parents { get; }

        bool AddTo(IList parent);

        bool RemoveFrom(IList parent);

        bool RemoveFromAll();
    }
}
