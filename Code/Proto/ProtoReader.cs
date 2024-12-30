using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Proto
{
    public class ProtoReader
    {
        public ProtoReader()
        {
            RegisterType();
        }

        public List<T> LoadCsv<T>(out Type pkType, out string pkName, string text) where T : class, new()
        {
            string[] lines = text.Split("\r\n");
            pkType = null;
            pkName = string.Empty;

            if (!LoadCsvField(out pkName, out List<string> names, out List<string> types, text))
            {
                Console.WriteLine("Load Csv Error");
                return null;
            }


            List<List<string>> columns = new List<List<string>>();
            for (int i = 2; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("#"))
                    continue;
                List<string> value = lines[i].Split(",").ToList<string>();
                if (value.Count < names.Count) continue;
                columns.Add(value);

            }

            pkType = GetTypeFromString(types[0]);

            List<T> results = new List<T>();
            for (int i = 0; i < columns.Count; i++)
            {
                T obj = new T();
                for (int j = 0; j < names.Count; j++)
                {
                    string propertyName = names[j];
                    string typeString = types[j];

                    string value = columns[i][j];


                    if (value == string.Empty) continue;

                    PropertyInfo property = typeof(T).GetProperty(propertyName);
                    if (property == null)
                    {
                        Console.WriteLine($"property Null {propertyName}");
                        return null;
                    }

                    property.SetValue(obj, ConvertToType(columns[i][j], GetTypeFromString(typeString)));
                }
                results.Add(obj);
            }
            return results;
        }

        public bool LoadCsvField(out string pkName, out List<string> fieldNames, out List<string> fieldTypes, string text)
        {
            string[] lines = text.Split("\r\n");
            pkName = string.Empty;
            fieldNames = new List<string>();
            fieldTypes = new List<string>();

            List<string> names = lines[0].Split(",").ToList<string>();
            List<string> types = lines[1].Split(",").ToList<string>();

            try
            {
                for (int i = 0; i < names.Count; i++)
                {

                    if (names[i].StartsWith("#")) continue;
                    fieldNames.Add(names[i]);

                    if (types[i].EndsWith(":pk"))
                    {
                        types[i] = types[i].Replace(":pk", "");
                        pkName = names[i];
                    }


                    fieldTypes.Add(types[i]);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private void RegisterType()
        {
            _typeMappingDict.Add("string", typeof(string));
            _typeMappingDict.Add("int", typeof(int));
            _typeMappingDict.Add("float", typeof(float));
            _typeMappingDict.Add("double", typeof(double));
            _typeMappingDict.Add("bool", typeof(bool));

            var enums = PrtEnum.GetEnums();
            foreach (var enumType in enums)
            {
                var name = enumType.Name;
                _typeMappingDict.Add($"enum:{name}", enumType);
            }
        }

        private Type GetTypeFromString(string typeString)
        {
            if (!_typeMappingDict.TryGetValue(typeString, out var type))
            {
                throw new Exception($"Unsupported type: {typeString}");
            }
            return type;
        }

        private object ConvertToType(string field, Type type)
        {
            try
            {
                return TypeDescriptor.GetConverter(type).ConvertFrom(field);
            }
            catch (Exception e)
            {
                throw new Exception($"Unsupported type(ConvertToType): {type.Name} {field} {e}");
            }
        }
        private Dictionary<string, Type> _typeMappingDict = new Dictionary<string, Type>();
    }
}
