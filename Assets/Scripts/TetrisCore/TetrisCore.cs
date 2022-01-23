using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Cube
{
    public Role color;
    public bool is_background;    
}

public struct PlayerHandle
{
    public const float CD_DURATION = 0.15f;
    public TetrominoData tetromino_data;
    public TetrominoData next_tetromino_data;
    public float curr_time;
    public float input_cd_time;
    public int y_director;
    public bool IsMoveable()
    {
        return !tetromino_data.on_ground;
    }
}

//图搜索用的cube
public struct TraverseCube
{
    public bool is_valid;           //是否已遍历
}

//图搜索用的岛屿
public struct Island
{
    public List<Vector2Int> tcubes;
    public Role color;
    public bool part_of_mainland;   //是否邻接大面积方块

    public Island(Role _color)
    {
        tcubes = new List<Vector2Int>();
        color = _color;
        part_of_mainland = false;
    }
}

// 俄罗斯方块的主要逻辑
// 程序视角为 上黑下白
public class TetrisCore : IGamePhase
{
    public float step_time { get; private set; }
    private float curr_normal_time = 0;
    public int max_island = 6;

    private Vector2Int size;
    private Cube[,] cubes;
    PlayerHandle black_player;
    PlayerHandle white_player;
    private List<Island> fill_island;
    private List<Island> sinking_island;

    //初始化 平分地图
    public void Init(float step, Vector2Int size, int max_island)
    {
        step_time = step;
        this.size = size;
        this.max_island = max_island;
        //初始化方块场景
        cubes = new Cube[size.x, size.y];
        for(int i = 0; i < size.y / 2; ++i)
        {
            for(int n = 0; n < size.x; ++n)
            {
                cubes[n, i] = (new Cube() {
                    color = Role.White,
                    is_background = true
                });
            }
        }
        for(int i = size.y / 2; i < size.y; ++i)
        {
            for(int n = 0; n < size.x; ++n)
            {
                cubes[n, i] = (new Cube() {
                    color = Role.Black,
                    is_background = true
                });
            }
        }
        //初始化player handle
        black_player = new PlayerHandle(){
            curr_time = 0,
            y_director = 1
        };
        white_player = new PlayerHandle(){
            curr_time = 0,
            y_director = -1
        };
        NewRound();
    }

    public void SetStepTime(float time)
    {
        step_time = time;
    }

    //每帧被调用
    public void Update(float time, PlayerInput[] input, ref Role[,] cells, ref TetrominoData[] nextTDatas)
    {
        RevertActiveCells(ref black_player);
        RevertActiveCells(ref white_player);
        
        fill_island = null;
        sinking_island = null;

        //move & rotate
        if (black_player.IsMoveable() && input[(int)Role.Black].IsValid)
        {
            Move(input[0].GetMovement(), ref black_player);
            if (input[0].applyRotate)
                Rotate(ref black_player);
        }
        if(white_player.IsMoveable() && input[(int)Role.White].IsValid)
        {
            Move(input[1].GetMovement(), ref white_player);
            if (input[1].applyRotate)
                Rotate(ref white_player);
        }

        ApplyActiveCells(ref black_player);
        ApplyActiveCells(ref white_player);
        //step
        black_player.curr_time += time;
        black_player.input_cd_time -= time;
        white_player.curr_time += time;
        white_player.input_cd_time -= time;
        if(black_player.IsMoveable() && black_player.curr_time >= step_time)
        {
            black_player.curr_time -= step_time;
            Step(ref black_player);
        }
        if(white_player.IsMoveable() && white_player.curr_time >= step_time)
        {
            white_player.curr_time -= step_time;
            Step(ref white_player);
        }
        if(!black_player.IsMoveable() && !white_player.IsMoveable())
        {
            Filling();
        }

        //为渲染提供矩阵
        for (int i = 0; i < size.x; ++i)
        {
            for(int n = 0; n < size.y; ++n)
            {
                cells[i,n] = cubes[i, n].color;
            }
        }

        nextTDatas[0] = black_player.next_tetromino_data;
        nextTDatas[1] = white_player.next_tetromino_data;
    }

    public void RevertActiveCells(ref PlayerHandle player)
    {
        Role reverse_color = (player.tetromino_data.color == Role.FixiableWhite) ? Role.Black : Role.White;
        for (int i = 0; i < 4; ++i)
        {
            Vector2Int position = player.tetromino_data.position + player.tetromino_data.cells[i];
            cubes[position.x, position.y].color = reverse_color;
            cubes[position.x, position.y].is_background = true;
        }
    }

    public void ApplyActiveCells(ref PlayerHandle player)
    {
        for (int i = 0; i < 4; ++i)
        {
            Vector2Int position = player.tetromino_data.position + player.tetromino_data.cells[i];
            cubes[position.x, position.y].color = player.tetromino_data.color;
            cubes[position.x, position.y].is_background = false;
        }
    }

    //move rotates
    public bool Move(Vector2Int input, ref PlayerHandle player, bool ignoreCd = false, bool clearTime = true)
    {
        //Vector2Int pre_pos = player.tetromino_data.position;
        //move
        Vector2Int offset = Vector2Int.zero;
        if (ignoreCd || player.input_cd_time <= 0)
        {
            if(input.x != 0)
                offset.x += (input.x > 0) ? 1 : -1;
            if (input.y != 0 && (input.y) * player.y_director > 0)
            {
                offset.y = player.y_director;
                if (clearTime)
                    player.curr_time = 0;
            }
            player.input_cd_time = PlayerHandle.CD_DURATION;
        }
        player.tetromino_data.position.x += offset.x;

        var valid = !IsOutOfIndex(ref player);
        if (!valid)
        {
            player.tetromino_data.position.x -= offset.x;
        }
        else
        {
            player.tetromino_data.position.y += offset.y;
            if(IsTetrominoGround(ref player)){
                player.tetromino_data.on_ground = true;
                player.tetromino_data.position.y -= offset.y;
            }
        }
        //Role reverse_color = (player.tetromino_data.color == Role.FixiableWhite) ? Role.Black : Role.White;
        //for (int i = 0; i < 4; ++i)
        //{
        //    Vector2Int position = pre_pos + player.tetromino_data.cells[i];
        //    cubes[position.x, position.y].color = reverse_color;
        //    cubes[position.x, position.y].is_background = true;
        //}
        //for (int i = 0; i < 4; ++i)
        //{
        //    Vector2Int position = player.tetromino_data.position + player.tetromino_data.cells[i];
        //    cubes[position.x, position.y].color = player.tetromino_data.color;
        //    cubes[position.x, position.y].is_background = false;
        //}

        return valid;
    }
    #region
    public void Rotate(ref PlayerHandle player)
    {
        //Vector2Int pre_pos = player.tetromino_data.position;
        int originalRotation = player.tetromino_data.rotationIndex;

        var direction = 1;
        player.tetromino_data.rotationIndex = Wrap(player.tetromino_data.rotationIndex + direction, 0, 4);
        ApplyRotationMatrix(direction, ref player);

        if (!TestWallKicks(player.tetromino_data.rotationIndex, direction, ref player))
        {
            player.tetromino_data.rotationIndex = originalRotation;
            ApplyRotationMatrix(-direction, ref player);
        }
        //Role reverse_color = (player.tetromino_data.color == Role.FixiableWhite) ? Role.Black : Role.White;
        //for (int i = 0; i < 4; ++i)
        //{
        //    Vector2Int position = pre_pos + player.tetromino_data.cells[i];
        //    cubes[position.x, position.y].color = reverse_color;
        //    cubes[position.x, position.y].is_background = true;
        //}
        //for (int i = 0; i < 4; ++i)
        //{
        //    Vector2Int position = player.tetromino_data.position + player.tetromino_data.cells[i];
        //    cubes[position.x, position.y].color = player.tetromino_data.color;
        //    cubes[position.x, position.y].is_background = false;
        //}
    }

    private void ApplyRotationMatrix(int direction, ref PlayerHandle player)
    {
        float[] matrix = Data.RotationMatrix;
        var cells = player.tetromino_data.cells;
        // Rotate all of the cells using the rotation matrix
        for (int i = 0; i < cells.Length; i++)
        {
            var cellInt2 = cells[i];
            var cell = new Vector3(cellInt2.x, cellInt2.y, 0);
            int x, y;

            switch (player.tetromino_data.tetromino)
            {
                case Tetromino.I:
                case Tetromino.O:
                    // "I" and "O" are rotated from an offset center point
                    cell.x -= 0.5f;
                    cell.y -= 0.5f;
                    x = Mathf.CeilToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.CeilToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;

                default:
                    x = Mathf.RoundToInt((cell.x * matrix[0] * direction) + (cell.y * matrix[1] * direction));
                    y = Mathf.RoundToInt((cell.x * matrix[2] * direction) + (cell.y * matrix[3] * direction));
                    break;
            }

            cells[i] = new Vector2Int(x, y);
        }
    }

    private bool TestWallKicks(int rotationIndex, int rotationDirection, ref PlayerHandle player)
    {
        int wallKickIndex = GetWallKickIndex(rotationIndex, rotationDirection, ref player);

        for (int i = 0; i < player.tetromino_data.wallKicks.GetLength(1); i++)
        {
            Vector2Int translation = player.tetromino_data.wallKicks[wallKickIndex, i];

            if (Move(translation, ref player, true, false))
            {
                return true;
            }
        }

        return false;
    }

    private int GetWallKickIndex(int rotationIndex, int rotationDirection, ref PlayerHandle player)
    {
        int wallKickIndex = rotationIndex * 2;

        if (rotationDirection < 0)
        {
            wallKickIndex--;
        }

        return Wrap(wallKickIndex, 0, player.tetromino_data.wallKicks.GetLength(0));
    }

    private int Wrap(int input, int min, int max)
    {
        if (input < min)
        {
            return max - (min - input) % (max - min);
        }
        else
        {
            return min + (input - min) % (max - min);
        }
    }
    #endregion
    void GenTData(out TetrominoData wd, out TetrominoData bd)
    {
        var offset = 2;
        Tetromino white_t = (Tetromino)Random.Range(0, 7);
        Tetromino black_t = (Tetromino)Random.Range(0, 7);
        //去冲突
        while (Data.Conflict[white_t].ContainsKey(black_t))
        {
            black_t = (Tetromino)Random.Range(0, 7);
        }
        wd = new TetrominoData()
        {
            position = new Vector2Int(size.x / 2, size.y - offset),
            color = Role.FixiableWhite,
            tetromino = white_t,
            on_ground = false
        };
        wd.Initialize();
        //
        bd = new TetrominoData()
        {
            //position  color  tetromino  onground
            position = new Vector2Int(size.x / 2, offset - 2),
            color = Role.FexiableBlack,
            tetromino = black_t,
            on_ground = false
        };
        bd.Initialize();
    }

    //新的一轮
    private void NewRound()
    {
        Debug.Log("New Round");
        if (!white_player.next_tetromino_data.Valid)
        {
            GenTData(out var next_white_data, out var next_black_data);
            white_player.next_tetromino_data = next_white_data;
            black_player.next_tetromino_data = next_black_data;
        }

        white_player.tetromino_data = white_player.next_tetromino_data;
        black_player.tetromino_data = black_player.next_tetromino_data;

        white_player.curr_time = 0;
        black_player.curr_time = 0;

        GenTData(out var white_data, out var black_data);
        white_player.next_tetromino_data = white_data;
        black_player.next_tetromino_data = black_data;


        Vector2Int white_pos = white_player.tetromino_data.position;
        for(int i = 0; i < 4; ++i){
            Vector2Int position = white_pos + white_player.tetromino_data.cells[i];
            cubes[position.x, position.y].color = white_player.tetromino_data.color;
            cubes[position.x, position.y].is_background = false;
        }
        Vector2Int black_position = black_player.tetromino_data.position;
        for(int i = 0; i < 4; ++i){
            Vector2Int position = black_position + black_player.tetromino_data.cells[i];
            cubes[position.x, position.y].color = black_player.tetromino_data.color;
            cubes[position.x, position.y].is_background = false;
        }
    }

    //每一个“下落”的周期调用，判断是否触底
    private bool Step(ref PlayerHandle player)
    {
        Debug.Log("A Step");
        Vector2Int pre_pos = player.tetromino_data.position;
        player.tetromino_data.position.y += player.y_director;
        if(IsTetrominoGround(ref player))
        {
            player.tetromino_data.position.y -= player.y_director;
            player.tetromino_data.on_ground = true;
            return true;
        }
        Role reverse_color = (player.tetromino_data.color == Role.FixiableWhite) ? Role.Black : Role.White;
        for(int i = 0; i < 4; ++i){
            Vector2Int position = pre_pos + player.tetromino_data.cells[i];
            cubes[position.x, position.y].color = reverse_color;
            cubes[position.x, position.y].is_background = true;
        }
        for(int i = 0; i < 4; ++i){
            Vector2Int position = player.tetromino_data.position + player.tetromino_data.cells[i];
            cubes[position.x, position.y].color = player.tetromino_data.color;
            cubes[position.x, position.y].is_background = false;
        }
        return false;
    }

    //填充/吃
    private bool Filling()
    {
        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                cubes[x, y].is_background = true;
            }
        }
        white_player.tetromino_data.color = Role.White;
        black_player.tetromino_data.color = Role.Black;
        for(int i = 0; i < 4; ++i){
            Vector2Int positionW = white_player.tetromino_data.position + white_player.tetromino_data.cells[i];
            Vector2Int positionB = black_player.tetromino_data.position + black_player.tetromino_data.cells[i];
            cubes[positionW.x, positionW.y].color = white_player.tetromino_data.color;
            cubes[positionB.x, positionB.y].color = black_player.tetromino_data.color;
        }
        //填充
        int white_complete_line = 0;
        for(int y = 0; y < size.y; ++y)
        {
            bool quit = false;
            for(int x = 0; x < size.x; ++x)
            {
                if(cubes[x, y].color != Role.White)
                    quit = true;
            }
            if(quit)
                break;
            white_complete_line++;
        }
        int black_complete_line = 0;
        for(int y = size.y - 1; y >= 0; --y)
        {
            bool quit = false;
            for(int x = 0; x < size.x; ++x)
            {
                if(cubes[x, y].color != Role.Black)
                    quit = true;
            }
            if(quit)
                break;
            black_complete_line++;
        }

        fill_island = new List<Island>();
        //界线附近的区域
        int top = size.y - black_complete_line;
        int bottom = white_complete_line - 1;
        int traverse_y_size = top - bottom + 1;
        //搜索白色岛屿
        List<Island> white_islands = new List<Island>();
        Traverse(bottom, traverse_y_size, Role.White, ref white_islands, false);
        //搜索黑色岛屿
        List<Island> black_islands = new List<Island>();
        Traverse(bottom, traverse_y_size, Role.Black, ref black_islands, false);
        //白块被吃
        foreach(Island island in white_islands)
        {
            for(int i = 0; i < island.tcubes.Count; ++i)
            {
                Vector2Int pos = island.tcubes[i];
                cubes[pos.x, pos.y].color = Role.Black;   
            }
            fill_island.Add(island);
        }
        //黑块被吃
        foreach(Island island in black_islands)
        {
            for(int i = 0; i < island.tcubes.Count; ++i)
            {
                Vector2Int pos = island.tcubes[i];
                cubes[pos.x, pos.y].color = Role.White;   
            }
            fill_island.Add(island);
        }
        //吃完后再次搜索有无需要沉底的岛屿
        List<Island> sinking_white_island = new List<Island>();
        Traverse(bottom, traverse_y_size, Role.White, ref sinking_white_island, true);
        List<Island> sinking_black_island = new List<Island>();
        Traverse(bottom, traverse_y_size, Role.Black, ref sinking_black_island, true);

        if (sinking_white_island.Count > 0 || sinking_black_island.Count > 0)
        {
            Sinking(ref sinking_white_island, ref sinking_black_island);
        }
        else
        {
            NewRound();
        }
        return false;
    }

    //遍历,生成Island
    private void Traverse(int bottom, int y_size, Role island_color, ref List<Island> islands, bool is_sinking)
    {
        TraverseCube[,] tcubes = new TraverseCube[size.x, y_size];
        for(int y = 0; y < y_size; ++y)
        {
            for(int x = 0; x < size.x; ++x)
            {
                if(tcubes[x, y].is_valid)           //已遍历则不再遍历
                    continue;
                if(bottom + y >= size.y || bottom + y < 0)
                {
                    tcubes[x, y].is_valid = true;
                    continue;
                }
                if(cubes[x, bottom + y].color == island_color)    //搜索到岛屿
                {
                    Island island = new Island(island_color);
                    TraverseIsland(x, y, bottom, y_size, in cubes[x, bottom + y].color, ref tcubes, ref island);
                    if(!island.part_of_mainland)
                    {
                        if(is_sinking && island.tcubes.Count > max_island)
                            islands.Add(island);
                        else if(!is_sinking && island.tcubes.Count <= max_island)
                            islands.Add(island);
                    }
                }
                tcubes[x, y].is_valid = true;
            }
        }
    }

    private void TraverseIsland(int x, int y, int bottom, int y_size, in Role color, ref TraverseCube[,] tcubes, ref Island island)
    {
        if(x >= size.x || x < 0 || y >= y_size || y < 0)
        {
            return;
        }
        if(y + bottom >= size.y || y + bottom < 0)
        {
            tcubes[x, y].is_valid = true;
            return;
        }
        if(tcubes[x, y].is_valid){
            return;
        }
        tcubes[x, y].is_valid = true;
        if(cubes[x, y + bottom].color == color)
        {
            //是否邻接大陆
            if(color == Role.Black)
            {
                if(y >= y_size - 1)
                    island.part_of_mainland = true;
            }
            else
            {
                if(y <= 0)
                    island.part_of_mainland = true;
            }
            //深度遍历
            int[] x_offset = { 0, 0, 1, -1};
            int[] y_offset = { 1, -1, 0, 0};
            island.tcubes.Add(new Vector2Int(x, y + bottom));
            for(int i = 0; i < 4; ++i)
            {
                TraverseIsland(x + x_offset[i], y + y_offset[i], bottom, y_size, in color, ref tcubes, ref island);
            }
        }
    }

    //下沉
    private bool Sinking(ref List<Island> white_islands, ref List<Island> black_islands)
    {
        sinking_island = new List<Island>();
        //白块沉底
        foreach(Island island in white_islands)
        {
            IslandSinking(island, Role.White);
            sinking_island.Add(island);
        }
        //黑块沉底
        foreach(Island island in black_islands)
        {
            IslandSinking(island, Role.Black);
            sinking_island.Add(island);
        }
        NewRound();
        return false;
    }

    //将一个Island沉底
    private void IslandSinking(Island island, Role color)
    {   
        Debug.Log("Sinking : " + color);
        int y_director = (color == Role.White) ? -1 : 1;
        Role reverse_color = (color == Role.White) ? Role.Black : Role.White;
        Vector2Int offset = Vector2Int.zero;
        for (int i = 0; i < island.tcubes.Count; ++i)
        {
            cubes[island.tcubes[i].x, island.tcubes[i].y].is_background = false;
        }
        while(true)
        {   
            offset.y += y_director;
            if(IsSinkingGround(island, color, offset))
                break;
            for(int i = 0; i < island.tcubes.Count; ++i)
            {
                cubes[island.tcubes[i].x, island.tcubes[i].y].color = reverse_color;
                cubes[island.tcubes[i].x, island.tcubes[i].y].is_background = true;
                var pos = island.tcubes[i] + offset;
                island.tcubes[i] = pos;
            }
            for(int i = 0; i < island.tcubes.Count; ++i)
            {
                cubes[island.tcubes[i].x, island.tcubes[i].y].color = color;
                cubes[island.tcubes[i].x, island.tcubes[i].y].is_background = false;
            }
            
        }
        for(int x = 0; x < size.x; ++x)
        {
            for(int y = 0; y < size.y; ++y)
            {
                cubes[x, y].is_background = true;
            }
        }
    }

    private bool IsSinkingGround(Island island, Role color, Vector2Int offset)
    {
        for(int i = 0; i < island.tcubes.Count; ++i)
        {
            Vector2Int cur_position = island.tcubes[i] + offset;
            bool out_of_index = false;
            switch(color)
                {
                    case Role.Black: out_of_index = (island.tcubes[i].y >= size.y); break;
                    case Role.White: out_of_index = (island.tcubes[i].y < 0); break;
                }
            if(out_of_index)
                return true;
            if((cubes[cur_position.x, cur_position.y].color == color &&
                cubes[cur_position.x, cur_position.y].is_background))
            {
                return true;
            }
        }
        return false;
    }

    //判断触底
    private bool IsTetrominoGround(ref PlayerHandle player)
    {
        for(int i = 0; i < 4; ++i)
        {
            Vector2Int cur_position = player.tetromino_data.cells[i] + player.tetromino_data.position;
            bool out_of_index = false;
            switch(player.y_director)
            {
                case 1: out_of_index = (cur_position.y >= size.y); break;
                case -1: out_of_index = (cur_position.y < 0); break;
            }
            if(out_of_index)
                return true;
            if((cubes[cur_position.x, cur_position.y].color == player.tetromino_data.color - 2 &&
                cubes[cur_position.x, cur_position.y].is_background) ||
                (cubes[cur_position.x, cur_position.y].color != player.tetromino_data.color &&
                cubes[cur_position.x, cur_position.y].color != player.tetromino_data.color - 2 &&
                !cubes[cur_position.x, cur_position.y].is_background))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsOutOfIndex(ref PlayerHandle player)
    {
        var IsVerticalOutofIndex = false;
        for (int i = 0; i < 4; ++i)
        {
            Vector2Int cur_position = player.tetromino_data.cells[i] + player.tetromino_data.position;
            if (cur_position.y < 0  || cur_position.y >= size.y)
            {
                IsVerticalOutofIndex = true;
                break;
            }
        }

        
        return IsVerticalOutofIndex || IsHorizontalOutOfIndex(ref player);
    }

    private bool IsHorizontalOutOfIndex(ref PlayerHandle player)
    {
        for(int i = 0; i < 4; ++i)
        {
            Vector2Int cur_position = player.tetromino_data.cells[i] + player.tetromino_data.position;
            if(cur_position.x >= size.x || cur_position.x < 0)
                return true;
            if((cubes[cur_position.x, cur_position.y].color == player.tetromino_data.color - 2 &&
                cubes[cur_position.x, cur_position.y].is_background) ||
                (cubes[cur_position.x, cur_position.y].color != player.tetromino_data.color &&
                cubes[cur_position.x, cur_position.y].color != player.tetromino_data.color - 2 &&
                !cubes[cur_position.x, cur_position.y].is_background))
            {
                return true;
            }
        }
        return false;
    }

    private void GameOver()
    {
        Debug.Log("玩完了!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
    }

    public ref List<Island> GetTillIslands()
    {
        return ref fill_island;
    }

    public ref List<Island> GetSinkingIslands()
    {
        return ref sinking_island;
    }
}
