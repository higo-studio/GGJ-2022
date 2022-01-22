using UnityEngine;
using UnityEngine.Tilemaps;

public class Battle : MonoBehaviour
{
    public Vector2Int MapSize;
    public Tilemap Tilemap;
    [Range(0.5f, 2f)]
    public float Step = 1f;
    public TileBase WhiteTile;
    public TileBase BlackTile;
    IGamePhase core = new TetrisCore();

    private Role[,] RenderCells;
    private PlayerInput[] inputs;

    public RectInt Bounds
    {
        get
        {
            Vector2Int position = new Vector2Int(-MapSize.x / 2, -MapSize.y / 2);
            return new RectInt(position, MapSize);
        }
    }

    private void Awake()
    {
        RenderCells = new Role[MapSize.x, MapSize.y];
        inputs = new PlayerInput[2];
        core.Init(Step, MapSize);
    }

    private void Update()
    {
        UpdateInput();
    }

    private void FixedUpdate()
    {
        core.Update(Time.fixedDeltaTime, inputs, ref RenderCells);
        RefreshTile();
        inputs[0] = default;
        inputs[1] = default;
    }

    private void UpdateInput()
    {

    }

    void RefreshTile()
    {
        var anthor = Bounds.position;
        Tilemap.ClearAllTiles();
        for (var i = 0; i < MapSize.x; i++)
        {
            for (var j = 0; j < MapSize.y; j++)
            {
                var role = RenderCells[i, j];
                Tilemap.SetTile(new Vector3Int(i + anthor.x, j + anthor.y, 0), GetTile(role));
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