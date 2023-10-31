using IceTea.Atom.Extensions;
using IceTea.Wpf.Core.Contracts;
using MyApp.Prisms.ViewModels.BaseViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyApp.Prisms.Views.BaseViews
{
    /// <summary>
    /// SmtpMailViewModelBase.xaml 的交互逻辑
    /// </summary>
    public partial class SmtpMailViewBase : UserControl
    {
        private SmtpMailViewModelBase _viewModel;
        public SmtpMailViewBase()
        {
            InitializeComponent();

            this._viewModel = this.DataContext as SmtpMailViewModelBase;
        }

        private void StackPanel_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            if (e.Command == CustomCommands.PostCommand)
            {
                e.Handled = true;
                if (e.Source is TextBox textBox && !textBox.Name.IsNullOrBlank())
                {
                    var text = textBox.Text;

                    if(!Regex.IsMatch(text, "^[A-Za-z0-9\\u4e00-\\u9fa5]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$"))
                    {
                        System.Windows.MessageBox.Show("添加内容必须为邮箱");
                        return;
                    }

                    switch (textBox.Name)
                    {
                        case "TosTextBox":
                            this._viewModel.Tos.Add(text);
                            break;
                        case "CcsTextBox":
                            this._viewModel.Ccs.Add(text);
                            break;
                        case "BccsTextBox":
                            this._viewModel.Bccs.Add(text);
                            break;
                        default:
                            throw new NotSupportedException();
                    }

                    textBox.Clear();
                }
            }
            else if (e.Command == ApplicationCommands.Close)
            {
                e.Handled = true;

                if (e.Source is ListBox listBox && e.OriginalSource is Button button)
                {
                    var value = button.DataContext.ToString();

                    switch (listBox.Name)
                    {
                        case "lstBxTos":
                            this._viewModel.Tos.Remove(value);
                            break;
                        case "lstBxCcs":
                            this._viewModel.Ccs.Remove(value);
                            break;
                        case "lstBxBccs":
                            this._viewModel.Bccs.Remove(value);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
        }
    }
}
