using IceTea.Atom.BaseModels;

namespace CustomControlsDemoModule.Models
{
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    internal class DataItem :BaseNotifyModel
    {
		private bool _isSelected;
		public bool IsSelected
        {
			get => _isSelected;
			set => SetProperty<bool>(ref _isSelected, value);
		}


        private string _name;
        public string Name
		{
			get => _name;
			set => SetProperty<string>(ref _name, value);
		}


		private string _gender;
		public string Gender
        {
			get => _gender;
			set => SetProperty<string>(ref _gender, value);
		}


		private string _address;
		public string Address
		{
			get => _address;
			set => SetProperty<string>(ref _address, value);
		}
	}
}
