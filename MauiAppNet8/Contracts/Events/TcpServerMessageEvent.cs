using IceTea.Atom.Utils.Events;

namespace MauiAppNet8.Contracts.Events
{
    internal class TcpServerReceiveMessageEvent : EventBase<string>
    {
    }

    internal class TcpServerSendMessageEvent : EventBase<string>
    {
    }

    internal class MessageEvent : EventBase<string>
    {
    }
}
