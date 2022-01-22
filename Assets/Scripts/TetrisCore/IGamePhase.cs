using UnityEngine;

public struct PlayerInput
{
    public int horizontal;
    public int vertical;
    public bool applyRotate;
    public bool IsValid => horizontal != 0 || vertical != 0 || applyRotate;
}
public enum Role
{
    Black = 0,
    White = 1
}

public interface IGamePhase
{
    public void Init(float step, Vector2Int size);
    public void Update(float time, PlayerInput[] input, ref int[,] cells);
}
