using CustomControlsDemoModule.Models;
using Prism.Mvvm;
using System.Collections.Generic;

namespace CustomControlsDemoModule.ViewModels
{
    internal class ControlsDemoViewModel : BindableBase
    {
        public ControlsDemoViewModel()
        {
            this.InitTreeViewData();

            this.InitMenuData();
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

            this.Menus = list;
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
                                    .Add(new("堆叠虚拟化面板")).Add(new("折叠虚拟化面板"))
                            )
                        .Add(new("边框"))
                        .Add(new("滚动视图")),

                new ControlNode("选择器控件").Add(new("选项卡")).Add(new("下拉框")).Add(new("表格")).Add(new("列表")),

                new ControlNode("集合控件").Add(new("菜单")).Add(new("上下文菜单")).Add(new("树形列表")),
            };

            this.Controls = list;
        }
    }
}
