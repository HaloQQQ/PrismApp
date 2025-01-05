using System.Collections;

namespace MusicPlayerModule.Contracts
{
    internal interface IChildren : IDisposable
    {
        IList<IList> Parents { get; }

        bool TryAddTo(IList parent);

        bool RemoveFrom(IList parent);

        bool RemoveFromAll();
    }
}
