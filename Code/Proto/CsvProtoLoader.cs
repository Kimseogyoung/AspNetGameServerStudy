using System.IO;

namespace Proto
{
    public sealed class CsvProtoLoader : IProtoLoader
    {
        private readonly string _csvPath;
        private readonly ProtoParser _parser = new ProtoParser();

        public CsvProtoLoader(string csvPath)
        {
            _csvPath = csvPath;
        }

        public ProtoLoadResult<T> Load<T>(string name) where T : ProtoBase, new()
        {
            var filePath = System.IO.Path.Combine(_csvPath, $"{name}.csv");
            var text = File.ReadAllText(filePath);
            return _parser.Parse<T>(text);
        }
    }
}
