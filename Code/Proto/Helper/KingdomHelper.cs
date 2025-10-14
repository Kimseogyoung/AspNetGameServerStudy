
using System.Collections.Generic;

namespace Proto.Helper
{
    public struct TilePos
    {
        public int X;
        public int Y;
    }

    public static class KingdomHelper
    {
        public static IEnumerable<TilePos> GetTilePosRanges(int startX, int startY, int sizeX, int sizeY)
        {
            var tilePosRangeList = new List<TilePos>();
            for (var y = 0; y < sizeX; y++)
            {
                for (var x = 0; x < sizeY; x++)
                {
                    /*                    tilePosRangeList.Add(new TilePosPacket
                                        {
                                            X = startPos.X + x,
                                            Y = startPos.Y + y,
                                        });
                    */
                    var posX = startX + x;
                    var posY = startY + y;
                    yield return new TilePos { X = posX, Y = posY };
                }
            }
        }
    }
}
