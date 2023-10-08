using Prism.Events;

namespace TcpSocket.MsgEvents
{
    /// <summary>
    /// 背景选择界面通知更换背景
    /// </summary>
    internal class BackgroundImageUpdateEvent : PubSubEvent<string>
    {
    }
}
