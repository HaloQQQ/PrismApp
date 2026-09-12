using System.Windows;

namespace MusicPlayerModule.Utils
{
    /// <summary>
    /// 绑定代理：为游离在逻辑/可视树之外的 Freezable（如 <see cref="GradientStop"/>）桥接 DataContext。
    /// Freezable 不在逻辑/可视树中，RelativeSource FindAncestor 无法向上查找祖先，
    /// 因此把 DataContext 缓存到该代理的 <see cref="Data"/> 属性，再通过 Source 绑定给它们。
    /// </summary>
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register(nameof(Data), typeof(object), typeof(BindingProxy), new PropertyMetadata(null));

        /// <summary>
        /// 被代理的数据上下文
        /// </summary>
        public object Data
        {
            get => GetValue(DataProperty);
            set => SetValue(DataProperty, value);
        }

        protected override Freezable CreateInstanceCore() => new BindingProxy();
    }
}
