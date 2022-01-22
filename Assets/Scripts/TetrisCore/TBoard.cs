using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TBoard : MonoBehaviour
{
    public Tilemap Tilemap;
    public TileBase WhiteTile;
    public TileBase BlackTile;
    public bool FlipX;
    public bool FlipY;

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
        switch (role)
        {
            case Role.White:
                return WhiteTile;
            case Role.Black:
                return BlackTile;
            default:
                return null;
        }
    }
}
