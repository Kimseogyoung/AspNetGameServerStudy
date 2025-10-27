using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class KingdomMap : ScriptBase
{
    private GameObject _tileMapGO;
    private List<List<Transform>> _tileMapTransforms = new();

    private const float tileXPosXDistance = 0.76f;
    private const float tileXPosYDistance = 0.34f;
    private const float tileYPosXDistance = -0.76f; 
    private const float tileYPosYDistance = 0.34f;
    protected override bool OnCreateScript()
    {
        _tileMapGO = UTIL.FindChild(gameObject, "TileRoot", true);
        var tilePrefab = UTIL.LoadRes<GameObject>($"{AppPath.PrefabDir}/Tile");

        var sizeX = APP.Ctx.Player.KingdomMap.SizeX;
        var sizeY = APP.Ctx.Player.KingdomMap.SizeY;
        for (var tileY= 0; tileY < sizeY; tileY++)
        {
            _tileMapTransforms.Add(new List<Transform>());
            for (var tileX = 0; tileX < sizeX; tileX++)
            {
                var tileGO = UTIL.Instantiate(tilePrefab, _tileMapGO, $"Tile({tileX},{tileY})");
                tileGO.transform.localPosition = new Vector3(
                    (tileX * tileXPosXDistance) + (tileY * tileYPosXDistance),
                   (tileX * tileXPosYDistance) + (tileY * tileYPosYDistance),
                    0);
                _tileMapTransforms[tileY].Add(tileGO.transform);
            }
        }
        return true;
    }

    protected override void OnDestroyScript()
    {
    }
}
