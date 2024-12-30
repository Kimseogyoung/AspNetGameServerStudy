using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Proto
{
    public class ProtoHelper
    {
        private class ProtoList
        {
            public List<object> List = new List<object>();
            public Dictionary<object, int> IdxDict = new Dictionary<object, int>();
        }

        public void Init(string csvPath)
        {
            _reader = new ProtoReader();
            _csvPath = csvPath;
        }

        public IEnumerable GetEnumerable<TProto>() where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            return protoData.List;
        }

        public int GetCount<TProto>() where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            return protoData.List.Count();
        }

        public bool TryGet<TProto>(object key, out TProto? prt) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            if (!protoData.IdxDict.TryGetValue(key, out int idx))
            {
                prt = null;
                //var prtName = typeof(TProto).Name;
                //Console.WriteLine($"NOT_FOUND_PK:{prtName}:{key}");
            }
            else
            {
                prt = (TProto)protoData.List[idx];
            }
            return prt != null;
        }

        public TProto Get<TProto>(object key) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();

            if (!protoData.IdxDict.TryGetValue(key, out int idx))
            {
                var prtName = typeof(TProto).Name;
                throw new Exception($"NOT_FOUND_PK:{prtName}:{key}");
            }

            return (TProto)protoData.List[idx];
        }

        public TProto GetNext<TProto>(TProto prt) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();

            var idx = protoData.List.IndexOf(prt);
            if (protoData.List.Count <= idx + 1)
                return prt;

            return (TProto)protoData.List[idx + 1];
        }

        public int GetIndexOf<TProto>(TProto prt) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();

            var idx = protoData.List.IndexOf(prt);

            return idx;
        }

        public TProto GetByIndex<TProto>(int idx) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            return (TProto)protoData.List[idx];
        }

        public TProto GetFirst<TProto>() where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            return (TProto)protoData.List[0];
        }

        public List<TProto> GetAll<TProto>() where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            return protoData.List.Cast<TProto>().ToList();
        }

        public void Bind<TProto>(string className = "", IComparer<TProto>? comparer = null) where TProto : ProtoBase, new()
        {
            if (className == "") 
            {
                className = typeof(TProto).Name.Replace("Proto", "");
            }
            
            Type protoClassType = typeof(TProto);

            if (_protoDict.ContainsKey(typeof(TProto)))
            {
                Console.WriteLine($"{typeof(TProto).Name} is existed");
                return;
            }

            var filePath = Path.Join(_csvPath, $"{className}.csv");
            var text = File.ReadAllText(filePath);
            var prtList = _reader.LoadCsv<TProto>(out Type pkType, out string pkName, text);

            _protoDict.Add(protoClassType, new ProtoList());

            if (comparer != null)
            {
                prtList.Sort(comparer);
            }

            for (int i = 0; i < prtList.Count; i++)
            {
                prtList[i].Idx = i;

                if (!string.IsNullOrEmpty(pkName))
                {
                    // PK가 없는 경우
                    var property = typeof(TProto).GetProperty(pkName);
                    var value = property.GetValue(prtList[i]);
                    _protoDict[protoClassType].IdxDict.Add(value, i); // TODO: Hash 찾기로 바꾸기
                }

                _protoDict[protoClassType].List.Add(prtList[i]);
            }

        }

        private ProtoList GetPrtDict<TProto>() where TProto : ProtoBase, new()
        {
            if (!_protoDict.TryGetValue(typeof(TProto), out var list))
            {
                //Bind<TProto>();
                Console.WriteLine($"{typeof(TProto).Name} is not exist.");
                return new ProtoList();
                //list = _protoDict[typeof(TProto)];
            }
            return list;
        }

        private Dictionary<Type, ProtoList> _protoDict = new Dictionary<Type, ProtoList>();
        private ProtoReader _reader;
        private string _csvPath;

    }
}
