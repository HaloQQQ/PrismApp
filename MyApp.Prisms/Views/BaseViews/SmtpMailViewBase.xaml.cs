using IceTea.Atom.Extensions;
using IceTea.Wpf.Atom.Contracts;
using IceTea.Wpf.Atom.Contracts.MediaInfo;
using IceTea.Wpf.Atom.Utils;
using MyApp.Prisms.ViewModels.BaseViewModels;
using System;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyApp.Prisms.Views.BaseViews
{
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑添加 "required" 修饰符或声明为可为 null。
    /// <summary>
    /// SmtpMailViewModelBase.xaml 的交互逻辑
    /// </summary>
    public partial class SmtpMailViewBase : UserControl
    {
        private SmtpMailViewModelBase _viewModel;
        public SmtpMailViewBase()
        {
            InitializeComponent();

            this._viewModel = DataContext as SmtpMailViewModelBase;
        }

        private void StackPanel_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == CustomCommands.PostCommand)
            {
                e.Handled = true;
                if (e.Source is TextBox textBox && !textBox.Name.IsNullOrBlank())
                {
                    var text = textBox.Text;

                    if (!Regex.IsMatch(text, "^[A-Za-z0-9\\u4e00-\\u9fa5]+@[a-zA-Z0-9_-]+(\\.[a-zA-Z0-9_-]+)+$"))
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
#pragma warning disable CS8604 // 引用类型参数可能为 null。
                            this._viewModel.Tos.Remove(value);
                            break;
                        case "lstBxCcs":
                            this._viewModel.Ccs.Remove(value);
                            break;
                        case "lstBxBccs":
                            this._viewModel.Bccs.Remove(value);
                            break;
                        case "lstBxAttachments":
                            this._viewModel.Attachments.Remove(value);
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
            else if (e.Command == ApplicationCommands.Open)
            {
                e.Handled = true;

                var selectFile = CommonAtomUtils.OpenFileDialog(string.Empty, new AnyMedia());

                if (selectFile != null)
                {
                    this._viewModel.Attachments.AddRange(selectFile.FileNames);
                }
            }
        }
    }
}
