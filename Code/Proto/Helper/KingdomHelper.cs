
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
        // 시작 칸부터 sizeX * sizeY 만큼의 칸을 훑는다.
        // 루프 경계가 y < sizeX, x < sizeY 로 뒤바뀌어 있었다. 지금 아이템이 전부
        // 정사각(3x3, 2x2, 1x1)이라 드러나지 않았을 뿐이다.
        public static IEnumerable<TilePos> GetTilePosRanges(int startX, int startY, int sizeX, int sizeY)
        {
            for (var y = 0; y < sizeY; y++)
            {
                for (var x = 0; x < sizeX; x++)
                {
                    yield return new TilePos { X = startX + x, Y = startY + y };
                }
            }
        }
    }
}
