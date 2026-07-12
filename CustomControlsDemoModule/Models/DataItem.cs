using IceTea.Pure.BaseModels;

namespace CustomControlsDemoModule.Models
{
    internal class DataItem :NotifyBase
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
