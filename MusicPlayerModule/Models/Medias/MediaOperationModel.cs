using IceTea.Pure.BaseModels;
using System.ComponentModel;

namespace MusicPlayerModule.Models;

internal enum OperationType
{
    [Description("已暂停")]
    Pause,
    [Description("快进")]
    FastForward,
    [Description("快退")]
    Rewind
}

internal class MediaOperationModel : NotifyBase
{
    private OperationType _operationType;
    public OperationType OperationType
    {
        get => _operationType;
        set
        {
            if (_operationType != value)
            {
                _operationType = value;

                RaisePropertyChanged(nameof(CurrentOperation));
            }
        }
    }

    public string CurrentOperation
    {
        get
        {
            switch (OperationType)
            {
                case OperationType.Pause:
                    return "已暂停";
                case OperationType.FastForward:
                    return "快进";
                case OperationType.Rewind:
                    return "快退";
                default:
                    throw new IndexOutOfRangeException();
            }
        }
    }
}
