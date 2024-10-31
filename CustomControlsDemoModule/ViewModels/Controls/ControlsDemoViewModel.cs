using CustomControlsDemoModule.Models;
using CustomControlsDemoModule.Views.Controls;
using CustomControlsDemoModule.Views.Controls.TextBoxes;
using IceTea.Atom.Extensions;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

namespace CustomControlsDemoModule.ViewModels.Controls
{
    internal class ControlsDemoViewModel : BindableBase
    {
        public ControlsDemoViewModel(IRegionManager regionManager)
        {
            InitTreeViewData();

            InitMenuData();

            InitComamnds(regionManager);
        }

        private void InitComamnds(IRegionManager regionManager)
        {
            NavigateToCommand = new DelegateCommand<object>(param =>
            {
                if (param is ControlNode node && node.Items.Count == 0)
                {
                    var uri = node.Name;

                    if ("选择器控件".EqualsIgnoreCase(node.ParentName))
                    {
                        uri = nameof(Selectors);
                    }
                    else if ("集合控件".EqualsIgnoreCase(node.ParentName))
                    {
                        uri = nameof(ItemsControls);
                    }
                    else if ("虚拟化面板".EqualsIgnoreCase(node.ParentName))
                    {
                        uri = nameof(VirtualizingPanels);
                    }
                    else if (Controls.First(n => n.Name == "面板").Items.SelectMany(n =>
                    {
                        var list = new List<string>();

                        GetNames(list, n);

                        return list;

                        void GetNames(IList<string> names, ControlNode node)
                        {
                            if (node.Items.Count == 0)
                            {
                                names.Add(node.Name);
                                return;
                            }

                            foreach (var item in node.Items)
                            {
                                GetNames(names, item);
                            }
                        }

                    }).Contains(node.Name))
                    {
                        uri = nameof(Panels);
                    }

                    switch (node.Name)
                    {
                        case "单击按钮":
                            uri = nameof(ButtonsView);
                            break;
                        case "双击按钮":
                            uri = nameof(ToggleButtonsView);
                            break;

                        case "单行文本":
                            uri = nameof(TextBox);
                            break;
                        case "多行文本":
                            uri = nameof(RichTextBox);
                            break;
                        case "密码框":
                            uri = nameof(PasswordBox);
                            break;
                        case "选项卡":
                            uri = nameof(TabControls);
                            break;
                        default:
                            break;
                    }

                    regionManager.RequestNavigate("ContentRegion", uri, nr => { }, new NavigationParameters()
                {
                    { "Key", "Value" }
                });
                }
            });
        }

        public IEnumerable<ControlNode> Controls { get; private set; }

        public IEnumerable<MenuNode> Menus { get; private set; }

        private void InitMenuData()
        {
            var list = new List<MenuNode>()
            {
                new MenuNode("文件")
                    .Add(new MenuNode("新建").Add(new MenuNode("项目"))
                    .Add(new MenuNode("仓库")))
                    .Add(new("打开")),

                new MenuNode("编辑").Add(new("转到")).Add(new("查找和替换")),

                new MenuNode("视图").Add(new("代码")).Add(new("设计器"))
            };

            Menus = list;
        }

        private void InitTreeViewData()
        {
            var list = new List<ControlNode>()
            {
                new ControlNode("按钮").Add(new("单击按钮")).Add(new("双击按钮")),

                new ControlNode("文本框").Add(new("单行文本")).Add(new("多行文本")).Add(new("密码框")),

                new ControlNode("面板")
                        .Add(
                                new ControlNode("网格面板")
                                    .Add(new("不平均网格")).Add(new("平均网格"))
                            )
                        .Add(
                                new ControlNode("平铺面板")
                                    .Add(new("堆叠面板")).Add(new("折叠面板")).Add(new("停靠面板"))
                            )
                        .Add(new("画布"))
                        .Add(
                                new ControlNode("虚拟化面板")
                                    .Add(new("垂直堆叠虚拟化面板"))
                                    .Add(new("垂直折叠虚拟化面板"))
                                    .Add(new("垂直平均网格虚拟化面板"))
                                    .Add(new("水平堆叠虚拟化面板"))
                                    .Add(new("水平折叠虚拟化面板"))
                                    .Add(new("水平平均网格虚拟化面板"))
                            )
                        .Add(new("边框"))
                        .Add(new("滚动视图")),

                new ControlNode("选择器控件").Add(new("选项卡")).Add(new("下拉框")).Add(new("表格")).Add(new("列表")),

                new ControlNode("集合控件").Add(new("菜单")).Add(new("上下文菜单")).Add(new("树形列表")),
            };

            Controls = list;
        }

        public ICommand NavigateToCommand { get; private set; }
    }
}
