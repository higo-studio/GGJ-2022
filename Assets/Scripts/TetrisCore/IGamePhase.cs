using UnityEngine;

public struct PlayerInput
{
    public float horizontal;
    public float vertical;
    public bool applyRotate;
    public bool IsValid => horizontal != 0 || vertical != 0 || applyRotate;
}
public enum Role
{
    Black,
    White,
    FexiableBlack,
    FixiableWhite
}

public interface IGamePhase
{
    public void Init(float step, Vector2Int size);
    public void Update(float time, PlayerInput[] input, ref Role[,] cells);
}
