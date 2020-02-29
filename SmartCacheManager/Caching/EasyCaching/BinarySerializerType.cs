namespace SmartCacheManager.Caching.EasyCaching
{
    /// <summary>
    /// Binary serializer type for distributed caching
    /// </summary>
    public enum BinarySerializerType
    {
        MessagePack,
        Protobuf,
        Json,
    }
}
