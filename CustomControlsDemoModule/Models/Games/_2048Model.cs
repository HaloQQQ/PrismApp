using IceTea.Atom.BaseModels;

namespace CustomControlsDemoModule.Models
{
    internal class _2048Model : BaseNotifyModel
    {
        public _2048Model(int value)
        {
            Value = value;
        }

        private int _value;
        public int Value
        {
            get => _value;
            set { SetProperty(ref _value, value); }
        }

        private bool _isCreating;
        public bool IsCreating
        {
            get => _isCreating;
            set { SetProperty(ref _isCreating, value); }
        }

        private bool _isUpdating;
        public bool IsUpdating
        {
            get => _isUpdating;
            set { SetProperty(ref _isUpdating, value); }
        }

        public static bool operator ==(_2048Model first, _2048Model second)
        {
            return first.Value == second.Value;
        }

        public static bool operator !=(_2048Model first, _2048Model second)
        {
            return first.Value != second.Value;
        }

        public static implicit operator int(_2048Model first)
        {
            return first.Value;
        }

    }
}
