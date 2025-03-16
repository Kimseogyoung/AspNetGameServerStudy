using System;
using System.Collections;
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

        public List<T> LoadCsv<T>(out List<string> pkNameList, out List<string> mkNameList, string text) where T : class, new()
        {
            string[] lines = text.Split("\r\n");
            pkNameList = new List<string>();
            mkNameList = new List<string>();

            if (!LoadCsvField(out pkNameList, out mkNameList, out List<string> names, out List<string> types, text))
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

            List<T> results = new List<T>();
            for (int i = 0; i < columns.Count; i++)
            {
                T obj = new T();
                var listValueDict = new Dictionary<string, List<string>>();
                for (int j = 0; j < names.Count; j++)
                {
                    string propertyName = names[j];
                    string typeString = types[j];

                    string value = columns[i][j];


                    if (value == string.Empty) continue;

                    var property = typeof(T).GetProperty(propertyName);
                    if (property == null)
                    {
                        Console.WriteLine($"property Null {propertyName}");
                        return null;
                    }

                    var propertyType = property.PropertyType;
                    var targetType = GetTypeFromString(typeString);

                    if (IsListType(propertyType))
                    {
                        if (!listValueDict.ContainsKey(propertyName))
                            listValueDict[propertyName] = new List<string>();
                        listValueDict[propertyName].Add(value);
                    }
                    else
                    {
                        property.SetValue(obj, ConvertToType(value, targetType));
                    }
                }

                // 리스트형 데이터 추가
                foreach (var kvp in listValueDict)
                {
                    var property = typeof(T).GetProperty(kvp.Key);
                    if (property != null && IsListType(property.PropertyType))
                    {
                        var elementType = property.PropertyType.GetGenericArguments()[0];
                        var listInstance = (IList)(Activator.CreateInstance(typeof(List<>).MakeGenericType(elementType)));

                        foreach (var val in kvp.Value)
                        {
                            listInstance.Add(ConvertToType(val, elementType));
                        }

                        property.SetValue(obj, listInstance);
                    }
                }


                results.Add(obj);
            }
            return results;
        }

        public bool LoadCsvField(out List<string> pkNameList, out List<string> mkNameList, out List<string> fieldNames, out List<string> fieldTypes, string text)
        {
            string[] lines = text.Split("\r\n");
            pkNameList = new List<string>();
            mkNameList = new List<string>();
            fieldNames = new List<string>();
            fieldTypes = new List<string>();

            List<string> names = lines[0].Split(",").ToList<string>();
            List<string> types = lines[1].Split(",").ToList<string>();

            try
            {
                for (int i = 0; i < names.Count; i++)
                {
                    var typeTxt = types[i];
                    var nameTxt = names[i];
                    if (nameTxt.StartsWith("#"))
                    {
                        continue;
                    }

                    fieldNames.Add(nameTxt);

                    if (typeTxt.EndsWith(":pk"))
                    {
                        types[i] = types[i].Replace(":pk", "");
                        pkNameList.Add(nameTxt);
                    }

                    if (typeTxt.EndsWith(":mk"))
                    {
                        types[i] = types[i].Replace(":mk", "");
                        mkNameList.Add(nameTxt);
                    }

                    fieldTypes.Add(typeTxt);
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
            foreach(var baseType in _baseTypeList)
            {
                _typeMappingDict.Add(baseType.Key, baseType.Value);
/*
                var genericListType = typeof(List<>).MakeGenericType(baseType.Value);
                _typeMappingDict.Add($"list:{baseType}", genericListType);*/
            }

            var enums = PrtEnum.GetEnums();
            foreach (var enumType in enums)
            {
                var name = enumType.Name;
                _typeMappingDict.Add($"{name}", enumType);
            }
        }

        private Type GetTypeFromString(string typeString)
        {
            typeString = typeString.Replace(":pk", "");
            typeString = typeString.Replace(":mk", "");

            var idx = typeString.IndexOf(":");
            if (idx != -1)
            {
                var mainType = typeString.Substring(0, idx);
                var subTypeName = typeString.Substring(idx + 1);
                var subType = GetTypeFromString(subTypeName);

                if (mainType == "enum")
                {
                    return subType;
                }
                else if (mainType == "list")
                {
                    var genericListType = typeof(List<>).MakeGenericType(subType);
                    return genericListType;
                }
            }

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

        // List<> 타입인지 확인하는 헬퍼 함수
        private bool IsListType(Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>);
        }

        private Dictionary<string, Type> _typeMappingDict = new Dictionary<string, Type>();
        private Dictionary<string, Type> _baseTypeList = new Dictionary<string, Type>
        { 
            {"int", typeof(int) }, 
            { "float", typeof(float) }, 
            { "double", typeof(double) }, 
            { "bool", typeof(bool) },
            {"DateTime", typeof(DateTime) },
            { "string", typeof(string) } 
        };
    }
}
