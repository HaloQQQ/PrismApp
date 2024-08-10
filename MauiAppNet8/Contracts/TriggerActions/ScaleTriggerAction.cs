
namespace MauiAppNet8.Contracts.TriggerActions
{
    internal class ScaleTriggerAction : TriggerAction<VisualElement>
    {
        public double To { get; set; }

        public uint Duration { get; set; }

        public bool AutoReverse { get; set; }

        protected override void Invoke(VisualElement sender)
        {
            sender.ScaleTo(this.To, this.Duration, Easing.CubicInOut);

            if (this.AutoReverse)
            {
                sender.ScaleTo(1, this.Duration, Easing.CubicInOut);
            }
        }
    }
}
