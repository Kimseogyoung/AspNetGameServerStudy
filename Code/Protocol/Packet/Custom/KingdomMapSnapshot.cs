using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Protocol.Packet.Custom
{
    [ProtoContract]
    public class KingdomMapSnapshotPacket
    {
        [ProtoMember(1)]
        public int Ver { get; set; } = c_curVer;

        [ProtoMember(2)]
        public ulong ObjIdCounter { get; set; } = 0;

        [ProtoMember(3)]
        public List<List<ulong>> TileMap { get; set; } = new List<List<ulong>>();

        [ProtoMember(4)]
        public Dictionary<ulong, PlacedKingdomItemPacket> PlacedObjDict { get; set; } = new Dictionary<ulong, PlacedKingdomItemPacket>();

        public KingdomMapSnapshotPacket()
        {
        }

        public KingdomMapSnapshotPacket(int ver, ulong objIdCounter, List<ulong> placedObjIdList, List<List<ulong>> tileMap, Dictionary<ulong, PlacedKingdomItemPacket> placedObjDict)
        {
            this.Ver = c_curVer;
            this.ObjIdCounter = objIdCounter;
            this.TileMap = tileMap;
            this.PlacedObjDict = placedObjDict;
        }

        public KingdomMapSnapshotPacket DeepCopy()
        {
            var copy = new KingdomMapSnapshotPacket
            {
                Ver = this.Ver,
                ObjIdCounter = this.ObjIdCounter
            };

            // TileMap 깊은 복사
            copy.TileMap = new List<List<ulong>>();
            foreach (var innerList in this.TileMap)
            {
                copy.TileMap.Add(new List<ulong>(innerList)); // 내부 리스트 복사
            }

            // PlacedObjDict 깊은 복사
            copy.PlacedObjDict = new Dictionary<ulong, PlacedKingdomItemPacket>();
            foreach (var kvp in this.PlacedObjDict)
            {
                var copyObj = new PlacedKingdomItemPacket
                {
                    Id = kvp.Value.Id,
                    StructureItemId = kvp.Value.StructureItemId,
                    Type = kvp.Value.Type,
                    Num = kvp.Value.Num,
                    State = kvp.Value.State,
                    StartTileX = kvp.Value.StartTileX,
                    StartTileY = kvp.Value.StartTileY,
                    SizeX = kvp.Value.SizeX,
                    SizeY = kvp.Value.SizeY,
                    Rotation = kvp.Value.Rotation
                };
                copy.PlacedObjDict.Add(kvp.Key, copyObj);
            }

            return copy;
        }

        public const int c_curVer = c_ver_1;
        public const int c_ver_1 = 1;
    }
}
