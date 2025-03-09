namespace Server.Serializer
{
    public interface IDataSerializer
    {
        byte[] Serialize<T>(T inObj);

        Task SerializeAsync<T>(Stream inStream, T inObj);

        Task<T> DeserializeAsync<T>(Stream inStream);

        Task<object> DeserializeAsync(Type type, Stream inStream);

        string ContentType { get; }
    }

}
