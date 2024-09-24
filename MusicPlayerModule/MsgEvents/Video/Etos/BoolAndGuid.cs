namespace MusicPlayerModule.MsgEvents.Video.Dtos
{
    internal struct BoolAndGuid
    {
        public BoolAndGuid(bool value, Guid guid)
        {
            Value = value;
            Guid = guid;
        }

        public bool Value { get; }
        public Guid Guid { get; }
    }
}
