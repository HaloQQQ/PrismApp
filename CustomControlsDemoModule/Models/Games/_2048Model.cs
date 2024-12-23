using IceTea.Atom.BaseModels;

namespace CustomControlsDemoModule.Models
{
#pragma warning disable CS0660 // 类型定义运算符 == 或运算符 !=，但不重写 Object.Equals(object o)
#pragma warning disable CS0661 // 类型定义运算符 == 或运算符 !=，但不重写 Object.GetHashCode()
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
