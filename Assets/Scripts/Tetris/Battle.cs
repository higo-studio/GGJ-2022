using UnityEngine;
using UnityEngine.Tilemaps;

public class Battle : MonoBehaviour
{
    public Vector2Int MapSize;
    [Range(0.5f, 2f)]
    public float Step = 1f;
    public TBoard[] Boards;
    public int[] Scores;
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
        Scores = new int[2];
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

        Scores[0] = 0;
        Scores[1] = 0;
        for (var i = 0; i < MapSize.x; i++)
        {
            for (var j = 0; j < MapSize.y; j++)
            {
                var role = RenderCells[i, j];
                switch (role)
                {
                    case Role.Black:
                    case Role.White:
                        Scores[(int)role]++;
                        break;
                }
            }
        }

        for (var i = 0; i < Boards.Length; i++)
        {
            var b = Boards[i];
            b.RefreshTile(RenderCells, Bounds, MapSize);
            b.RefreshScore(Scores[i]);
        }
        inputs[0] = default;
        inputs[1] = default;
    }

    private void UpdateInput()
    {
        inputs[0].horizontal = Input.GetAxisRaw("Horizontal");
        inputs[0].vertical = -Input.GetAxisRaw("Vertical");
        if (Input.GetButtonUp("Rotate"))
        {
            inputs[0].applyRotate = true;
        }

        inputs[1].horizontal = Input.GetAxisRaw("Horizontal1");
        inputs[1].vertical = -Input.GetAxisRaw("Vertical1");
        if (Input.GetButtonUp("Rotate1"))
        {
            inputs[1].applyRotate = true;
        }
    }
}