using System.IO;
using UnityEngine;

public class FileReader
{
    public string ReadFile(string path)
    {
        string result = File.ReadAllText(path);

        if(result == null || result == string.Empty)
        {
            Debug.Log($"{path} is Null");
            return string.Empty;
        }
        return result;
    }
    public string ReadTextAsset(string path)
    {
        Debug.Log(path);
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        string result = textAsset.text;

        if (result == null || result == string.Empty)
        {
            Debug.Log($"{path} is Null");
            return string.Empty;
        }
        return result;
    }
}
