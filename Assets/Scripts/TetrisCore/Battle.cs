using UnityEngine;
using UnityEngine.Tilemaps;

public class Battle : MonoBehaviour
{
    public Vector2Int MapSize;
    [Range(0.5f, 2f)]
    public float Step = 1f;
    public TBoard[] Boards;
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
        foreach(var b in Boards)
        {
            b.RefreshTile(RenderCells, Bounds, MapSize);
        }
        inputs[0] = default;
        inputs[1] = default;
    }

    private void UpdateInput()
    {
        inputs[0].horizontal = Input.GetAxisRaw("Horizontal");
    }
}