using UnityEngine;
using UnityEngine.Tilemaps;

public class Battle : MonoBehaviour
{
    public Vector2Int MapSize;
    public Tilemap Tilemap;
    [Range(0.5f, 2f)]
    public float Step = 1f;
    IGamePhase core = new TetrisCore();

    public int[,] RenderCells;

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
        RenderCells = new int[MapSize.x, MapSize.y];
        inputs = new PlayerInput[2];
        core.Init(Step, MapSize);
    }

    private void FixedUpdate()
    {
        core.Update(Time.fixedDeltaTime, inputs, ref RenderCells);
    }
}