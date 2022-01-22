using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

[Serializable]
public struct TileCollection
{
    public TileBase[] tiles;
}

public class TBoard : MonoBehaviour
{
    public Tilemap Tilemap;
    //public TileBase WhiteTile;
    //public TileBase BlackTile;
    //public TileBase FexiableWhiteTile;
    //public TileBase FixiableBlackTile;
    public TileCollection tileCollection;
    public bool FlipX;
    public bool FlipY;
    public TimeLerp timelerp;
    public Tilemap NextTilemap;

    public void RefreshScore(int score)
    {
        timelerp.TargetTime = score;
    }

    public void RefreshTile(Role[,] RenderCells, RectInt Bounds, Vector2Int MapSize)
    {
        var anthor = Bounds.position;
        Tilemap.ClearAllTiles();
        for (var i = 0; i < MapSize.x; i++)
        {
            for (var j = 0; j < MapSize.y; j++)
            {
                var fi = FlipX ? MapSize.x - i - 1 : i;
                var fj = FlipY ? MapSize.y - j - 1 : j;
                var role = RenderCells[i, j];
                Tilemap.SetTile(new Vector3Int(fi + anthor.x, fj + anthor.y, 0), GetTile(role));
            }
        }
    }

    TileBase GetTile(Role role)
    {
        var idx = (int)role;
        if (idx > tileCollection.tiles.Length || idx < 0) return null;
        return tileCollection.tiles[idx];
    }
}
