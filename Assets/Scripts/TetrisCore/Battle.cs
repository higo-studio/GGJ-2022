using UnityEngine;
using UnityEngine.Tilemaps;

public class Battle : MonoBehaviour
{
    public Vector2Int MapSize;
    public Tilemap Tilemap;
    [Range(0.5f, 2f)]
    public float Step = 1f;
    IGamePhase core = new TetrisCore();

    private void Awake()
    {
        core.Init(Step, MapSize);
    }
}