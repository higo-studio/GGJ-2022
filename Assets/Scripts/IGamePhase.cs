public struct PlayerInput
{
    public int horizontal;
    public int vertical;
    public bool applyRotate;
    public bool IsValid => horizontal != 0 || vertical != 0 || applyRotate;
}
public enum Role
{
    Black,
    White
}

public interface IGamePhase
{
    void Init(float step);
    // 0
    // ึ๐ึกต๗ำร
    void Update(float time, PlayerInput[] input, out int[,] cells);
}
