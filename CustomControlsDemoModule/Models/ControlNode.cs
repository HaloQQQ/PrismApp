using IceTea.Pure.Utils;
using Prism.Mvvm;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class ControlNode : BindableBase
    {
        public ControlNode(string name)
        {
            Name = name;
            Items = new List<ControlNode>();
        }

        public string Name { get; }

        public string ParentName { get; private set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { SetProperty(ref _isSelected, value); }
        }

        public IList<ControlNode> Items { get; }

        public ControlNode Add(ControlNode item)
        {
            item.AssertNotNull(nameof(item));

            this.Items.Add(item);

            item.ParentName = this.Name;

            return this;
        }
    }
}
