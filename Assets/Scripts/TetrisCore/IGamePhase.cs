using UnityEngine;

public struct PlayerInput
{
    public float horizontal;
    public float vertical;
    public bool applyRotate;
    public bool IsValid => horizontal != 0 || vertical != 0 || applyRotate;
    public override string ToString()
    {
        return $"H: {horizontal}, V: {vertical}, R: {applyRotate}";
    }
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
    public void Init(float step, Vector2Int size);
    public void Update(float time, PlayerInput[] input, ref Role[,] cells);
}
