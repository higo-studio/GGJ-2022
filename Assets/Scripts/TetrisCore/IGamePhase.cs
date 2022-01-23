using UnityEngine;

public struct PlayerInput
{
    public float horizontal;
    public float vertical;
    public bool applyRotate;
    public bool IsValid => horizontal != 0 || vertical != 0 || applyRotate;
    public bool IsMovementValid => horizontal != 0 || vertical != 0 || applyRotate;
    public override string ToString()
    {
        return $"H: {horizontal}, V: {vertical}, R: {applyRotate}";
    }

    public Vector2Int GetMovement() => new Vector2Int((int)horizontal, (int)vertical);
}
public enum Role
{
    Black = 0,
    White = 1,
    FexiableBlack = 2, 
    FixiableWhite = 3
}

public interface IGamePhase
{
    public void Init(float step, Vector2Int size, int _max_island);
    public void Update(float time, PlayerInput[] input, ref Role[,] cells, ref TetrominoData[] nextTDatas);
    public bool IsGameOver { get; }
}
