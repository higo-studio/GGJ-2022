using UnityEngine;
using UnityEngine.Tilemaps;

public class Battle : MonoBehaviour
{
    public Vector2Int MapSize;
    [Range(0.1f, 2f)]
    public float Step = 1f;
    public float MaxStep = 2f;
    public float MinStep = 0.1f;
    public float SpeedUpTotalRound = 10;
    public int Max_Island = 6;
    public TBoard[] Boards;
    public int[] Scores;
    public Tilemap[] NextTilemaps;
    public AnimationCurve Curve;

    TetrisCore core = new TetrisCore();

    private Role[,] RenderCells;
    private TetrominoData[] NextTDatas;
    private PlayerInput[] inputs;
    private bool isRunning = true;

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
        Init();
    }

    private void Init()
    {
        Scores = new int[2];
        NextTDatas = new TetrominoData[2];
        RenderCells = new Role[MapSize.x, MapSize.y];
        inputs = new PlayerInput[2];
        core.Init(Step, MapSize, Max_Island);
    }

    private void Update()
    {
        if (!isRunning) return;
        UpdateInput();
    }

    private void FixedUpdate()
    {
        if (!isRunning) return;
        core.Update(Time.fixedDeltaTime, inputs, ref RenderCells, ref NextTDatas);
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

        RefreshNextView();
        inputs[0] = default;
        inputs[1] = default;  
        
        if (core.IsGameOver)
        {
            isRunning = false;
            Restart();
        }

        var factor = Mathf.Clamp01(core.RoundCount / SpeedUpTotalRound);
        Step = MinStep + (MaxStep - MinStep) * Curve.Evaluate(factor);
        Step = MaxStep - Step;
        core.SetStepTime(Step);
    }

    private void UpdateInput()
    {
        inputs[1].horizontal = Input.GetAxisRaw("Horizontal");
        inputs[1].vertical = Input.GetAxisRaw("Vertical");
        if (Input.GetButtonUp("Rotate"))
        {
            inputs[1].applyRotate = true;
        }

        inputs[0].horizontal = -Input.GetAxisRaw("Horizontal1");
        inputs[0].vertical = Input.GetAxisRaw("Vertical1");
        if (Input.GetButtonUp("Rotate1"))
        {
            inputs[0].applyRotate = true;
        }
    }

    private void RefreshNextView()
    {
        var n0 = NextTilemaps[0];
        var n1 = NextTilemaps[1];

        var d0 = NextTDatas[0];
        var d1 = NextTDatas[1];

        var b0 = Boards[0];
        var b1 = Boards[1];

        n0.ClearAllTiles();
        n1.ClearAllTiles();

        //n0.SetTile(Vector3Int.zero, b0.tileCollection.tiles[(int)Role.Black]);
        //n1.SetTile(Vector3Int.zero, b1.tileCollection.tiles[(int)Role.Black]);
        for (var i = 0; i < d0.cells.Length; i++)
        {
            var cell = d0.cells[i];
            var pos = new Vector3Int(cell.x - 1, cell.y - 1, 0);
            n0.SetTile(pos, b0.tileCollection.tiles[(int)Role.FexiableBlack]);
        }

        for (var i = 0; i < d1.cells.Length; i++)
        {
            var cell = d1.cells[i];
            var pos = new Vector3Int(cell.x - 1, cell.y - 1, 0);
            n1.SetTile(pos, b1.tileCollection.tiles[(int)Role.FixiableWhite]);
        }
    }

    public void Stop()
    {
        isRunning = false;
    }

    public void Resume()
    {
        isRunning = true;
    }

    public void Restart()
    {
        core = new TetrisCore();
        Init();
    }
}