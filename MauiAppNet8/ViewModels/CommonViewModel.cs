using IceTea.Atom.BaseModels;
using Prism.Commands;
using System.Windows.Input;

namespace MauiAppNet8.ViewModels
{
    internal class CommonViewModel : BaseNotifyModel
    {
        public CommonViewModel()
        {
            this.GoToCommand = new DelegateCommand<string>(pageName => Shell.Current.GoToAsync(pageName));
        }

        public ICommand GoToCommand { get; private set; }
    }
}
