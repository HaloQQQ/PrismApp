using IceTea.Atom.Utils;
using System.Collections.Generic;

namespace CustomControlsDemoModule.Models
{
    internal class MenuNode
    {
        public MenuNode(string name)
        {
            Name = name;
            Items = new List<MenuNode>();
        }

        public string Name { get; }

        public string ParentName { get; private set; }

        public ICollection<MenuNode> Items { get; }

        public MenuNode Add(MenuNode item)
        {
            item.AssertNotNull(nameof(item));

            this.Items.Add(item);

            item.ParentName = this.Name;

            return this;
        }
    }
}
