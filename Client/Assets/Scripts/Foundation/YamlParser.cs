using System.Collections;
using YamlDotNet.Serialization;

public class YamlParser 
{ 
    public T GetConfig<T>(string data)  where T : Config.ConfigBase
    {
        var deserializer = new DeserializerBuilder()
           .Build();

        //yml contains a string containing your YAML
        T config = deserializer.Deserialize<T> (data);
        return config;
    }

}
