using Prism.Mvvm;
using System.ComponentModel;

namespace MusicPlayerModule.ViewModels
{
    internal enum OperationType
    {
        [Description("已暂停")]
        Pause,
        [Description("快进")]
        FastForward,
        [Description("快退")]
        Rewind
    }

    internal class MediaOperationViewModel : BindableBase
    {
        private OperationType _operationType;
        public OperationType OperationType
        {
            get => this._operationType;
            set
            {
                if (SetProperty<OperationType>(ref _operationType, value))
                {
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
}
