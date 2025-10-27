using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class KingdomMap : ScriptBase
{
    private GameObject _tileRootGO;
    private GameObject _kingdomItemRootGO;
    private GameObject _tilePrefab;

    private int _tileXSize = 0;
    private int _tileYSize = 0;
    private List<List<Transform>> _tileMapTransforms = new();
    private Dictionary<ulong, KingdomItemGO> _kingdomItemGODict = new();

    private const float tileXPosXDistance = 0.76f;
    private const float tileXPosYDistance = 0.34f;
    private const float tileYPosXDistance = -0.76f; 
    private const float tileYPosYDistance = 0.34f;
    protected override bool OnCreateScript()
    {
        _tileRootGO = UTIL.FindChild(gameObject, "TileRoot", true);
        _kingdomItemRootGO = UTIL.FindChild(gameObject, "KingdomItemRoot", true);
        _tilePrefab = UTIL.LoadRes<GameObject>(AppPath.GetPrefabPath("Tile"));

        RefreshMap();
        return true;
    }

    protected override void OnDestroyScript()
    {
    }

    public void RefreshMap()
    {
        var sizeX = APP.Ctx.Player.KingdomMap.SizeX;
        var sizeY = APP.Ctx.Player.KingdomMap.SizeY;

        if (_tileXSize == sizeX && _tileYSize == sizeY)
        {
            return;
        }

        if(_tileXSize > sizeX || _tileYSize > sizeY)
        {
            LOG.E("Tile이 더 작아지는 경우가 있으면 안됨");
            // 초기화
            _tileXSize = 0;
            _tileYSize = 0;
        }

        for (var tileY = _tileXSize; tileY < sizeY; tileY++)
        {
            _tileMapTransforms.Add(new List<Transform>());
            for (var tileX = _tileYSize; tileX < sizeX; tileX++)
            {
                var tileGO = UTIL.Instantiate(_tilePrefab, _tileRootGO, $"Tile({tileX},{tileY})");
                tileGO.transform.localPosition = new Vector3(
                    (tileX * tileXPosXDistance) + (tileY * tileYPosXDistance),
                   (tileX * tileXPosYDistance) + (tileY * tileYPosYDistance), 0);
                _tileMapTransforms[tileY].Add(tileGO.transform);
            }
        }

        // TODO: 개선 필요. 일단 매번 삭제 -> 재생성하도록함.
        foreach (var go in _kingdomItemGODict.Values)
            UTIL.Destroy(go.GameObject);
        _kingdomItemGODict.Clear();

        foreach (var placedKingdomItem in APP.Ctx.Player.KingdomMap.PlacedKingdomItemList)
        {
            var prt = APP.Prt.GetKingdomItemPrt(placedKingdomItem.Num);
            var pos = GetTileCenterPos(placedKingdomItem.StartTileX, placedKingdomItem.StartTileY, placedKingdomItem.SizeX, placedKingdomItem.SizeY);
            var gameObj = UTIL.Instantiate(AppPath.GetPrefabPath("KingdomItem"), Vector3.zero, _kingdomItemRootGO, $"KingdomItem({placedKingdomItem.Id})");
            var spriteRenderer = UTIL.FindChild<SpriteRenderer>(gameObj);
            var sprite = UTIL.LoadSprite(prt.Sprite);
            gameObj.transform.localPosition = pos;
            spriteRenderer.sprite = sprite;
            var kingdomeItemGO = new KingdomItemGO
            {
                GameObject = gameObj,
                Sprite = sprite,
                Id = placedKingdomItem.Id,
                KingdomItemNum = prt.Num
            };

            _kingdomItemGODict.Add(kingdomeItemGO.Id, kingdomeItemGO);
        }
    }

    private Vector3 GetTilePos(float x, float y)
    {
        return new Vector3((x * tileXPosXDistance) + (y * tileYPosXDistance), (x * tileXPosYDistance) + (y * tileYPosYDistance), 0);
    }

    private Vector3 GetTileCenterPos(int x, int y, int sizeX, int sizeY)
    {
        return GetTilePos(x + sizeX/2f, y + sizeX / 2f);
    }

    private struct KingdomItemGO
    {
        public GameObject GameObject;
        public Sprite Sprite;
        public ulong Id;
        public int KingdomItemNum;
    }
}
