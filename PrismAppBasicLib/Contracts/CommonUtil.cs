using IceTea.Atom.Contracts;
using IceTea.Atom.Utils;
using Prism.Events;
using PrismAppBasicLib.MsgEvents;
using System;

namespace PrismAppBasicLib.Contracts
{
    public static class CommonUtil
    {
        public static void PublishMessage(IEventAggregator eventAggregator, string msg, int seconds = 3)
        {
            eventAggregator.AssertNotNull(nameof(IEventAggregator)).GetEvent<DialogMessageEvent>().Publish(new DialogMessage(msg, seconds));
        }

        public static void SubscribeMessage(IEventAggregator eventAggregator, Action<DialogMessage> action)
        {
            eventAggregator.AssertNotNull(nameof(IEventAggregator)).GetEvent<DialogMessageEvent>().Subscribe(action);
        }
    }
}
