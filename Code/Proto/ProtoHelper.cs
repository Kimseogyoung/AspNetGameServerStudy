using ProtoBuf;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Proto
{
    public class ProtoHelper
    {
        private class ProtoList
        {
            public List<ProtoBase> List = new List<ProtoBase>();
            public Dictionary<object, int> PkIdxDict = new Dictionary<object, int>();
            public Dictionary<object, List<int>> MkIdxDict = new Dictionary<object, List<int>>();
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

        public bool TryGet<TProto>(object pk, out TProto? prt) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            var hash = GenerateKeyHash(pk);
            if (!protoData.PkIdxDict.TryGetValue(hash, out int idx))
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

        public TProto Get<TProto>(object pk) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();

            var hash = GenerateKeyHash(pk);

            if (!protoData.PkIdxDict.TryGetValue(hash, out int idx))
            {
                var prtName = typeof(TProto).Name;
                throw new Exception($"NOT_FOUND_PK:{prtName}:{pk}");
            }

            return (TProto)protoData.List[idx];
        }

        public List<TProto> GetByMk<TProto>(object mk) where TProto : ProtoBase, new()
        {
            var protoData = GetPrtDict<TProto>();
            var hash = GenerateKeyHash(mk);

            var prtList = new List<TProto>();
            if (!protoData.MkIdxDict.TryGetValue(hash, out var idxList))
            {
                return prtList;
            }


            foreach (var idx in idxList)
            {
                var prt = (TProto)protoData.List[idx];
                prtList.Add(prt);
            }

            return prtList;
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

        public IEnumerable<TProto> GetAll<TProto>() where TProto : ProtoBase, new()
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
            var prtList = _reader.LoadCsv<TProto>(out var pkNameList, out var mkNameList, text);

            _protoDict.Add(protoClassType, new ProtoList());

            if (comparer != null)
            {
                prtList.Sort(comparer);
            }

            for (int i = 0; i < prtList.Count; i++)
            {
                prtList[i].Idx = i;
                _protoDict[protoClassType].List.Add(prtList[i]);

                // pk 인덱스 딕셔너리 추가
                if (pkNameList.Count > 0)
                {
                    var pkValueList = new List<object>();
                    foreach (var pkName in pkNameList)
                    {
                        var property = typeof(TProto).GetProperty(pkName);
                        var value = property?.GetValue(prtList[i]);
                        if (value != null)
                        {
                            pkValueList.Add(value);
                        }
                    }

                    object hash = 0;
                    if (pkValueList.Count == 1)
                    {
                        // PK가 1개 있는 경우 
                        hash = pkValueList[0];
                    }
                    else if (pkValueList.Count >= 2)
                    {
                        // PK가 n개 있는 경우
                        hash = GenerateFastHashList(pkValueList);
                    }

                    if (_protoDict[protoClassType].PkIdxDict.ContainsKey(hash))
                    {
                        throw new Exception($"DUPLICATED_PK_HASH ClassType({protoClassType.Name}) Hash({hash}) Pk({string.Join(",", pkNameList)})");
                    }

                    _protoDict[protoClassType].PkIdxDict.Add(hash, i);
                }

                // mk 인덱스 딕셔너리 추가
                if (mkNameList.Count > 0)
                {
                    var mkValueList = new List<object>();
                    foreach (var mkName in mkNameList)
                    {
                        var property = typeof(TProto).GetProperty(mkName);
                        var value = property?.GetValue(prtList[i]);
                        if (value != null)
                        {
                            mkValueList.Add(value);
                        }
                    }

                    object hash = 0;
                    if (mkValueList.Count == 1)
                    {
                        // MK가 1개 있는 경우 
                        hash = mkValueList[0];
                    }
                    else if (mkValueList.Count >= 2)
                    {
                        // MK가 n개 있는 경우
                        hash = GenerateFastHashList(mkValueList);
                    }

                    if (!_protoDict[protoClassType].MkIdxDict.ContainsKey(hash))
                    {
                        _protoDict[protoClassType].MkIdxDict[hash] = new List<int>();
                    }

                    _protoDict[protoClassType].MkIdxDict[hash].Add(i);
                }
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

        private object GenerateKeyHash(object pkObj)
        {
            if(pkObj is ITuple tuple)
            {
                int hash = c_pkGenInitPrime; // 소수 기반 초기값
                for (int i = 0; i < tuple.Length; i++)
                {
                    int valueHash = tuple[i]?.GetHashCode() ?? 0;
                    hash = (hash * c_pkGenPrime) ^ valueHash; // XOR + 곱셈 조합
                }
                return hash;
            }

            return pkObj;
        }

        private int GenerateFastHashList(IEnumerable list)
        {
            int hash = c_pkGenInitPrime; // 소수 기반 초기값
            foreach (var value in list)
            {
                int valueHash = value?.GetHashCode() ?? 0;
                hash = (hash * c_pkGenPrime) ^ valueHash; // XOR + 곱셈 조합
            }
            return hash;
        }

        private Dictionary<Type, ProtoList> _protoDict = new Dictionary<Type, ProtoList>();
        private ProtoReader _reader;
        private string _csvPath;

        private const int c_pkGenInitPrime = 17;
        private const int c_pkGenPrime = 31;

    }
}
