namespace MusicPlayerModule.MsgEvents.Video.Dtos
{
    internal struct BoolAndGuid
    {
        public BoolAndGuid(bool value, Guid guid)
        {
            Value = value;
            Guid = guid;
        }

        public bool Value { get; private set; }
        public Guid Guid { get; private set; }
    }
}
