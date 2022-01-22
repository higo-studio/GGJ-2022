using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

struct Cube
{
    public Role color;
    public bool is_background;    
}

public struct PlayerHandle
{
    public TetrominoData tetromino_data;
    public float curr_time;
    public int y_director;
    public bool IsMoveable()
    {
        return !tetromino_data.on_ground;
    }
}

// 俄罗斯方块的主要逻辑
// 程序视角为 上黑下白
public class TetrisCore : IGamePhase
{
    public float step_time { get; private set; }
    private float curr_normal_time = 0;
    private Vector2Int size;
    private Cube[,] cubes;
    PlayerHandle black_player;
    PlayerHandle white_player;

    //初始化 平分地图
    public void Init(float step, Vector2Int size)
    {
        step_time = step;
        this.size = size;
        //初始化方块场景
        cubes = new Cube[size.x, size.y];
        for(int i = 0; i < size.x / 2; ++i)
        {
            for(int n = 0; n < size.y; ++n)
            {
                cubes[i, n] = (new Cube() {
                    color = Role.White,
                    is_background = true
                });
            }
        }
        for(int i = size.x / 2; i < size.x; ++i)
        {
            for(int n = 0; n < size.y; ++n)
            {
                cubes[i, n] = (new Cube() {
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
        black_player = new PlayerHandle(){
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
    public void Update(float time, PlayerInput[] input, ref int[,] cells)
    {
        //move & rotate
        if(black_player.IsMoveable() && input[(int)Role.Black].IsValid)
        {
            TetrominoMoveRotate(ref input[0], ref black_player);
        }
        if(white_player.IsMoveable() && input[(int)Role.White].IsValid)
        {
            TetrominoMoveRotate(ref input[1], ref white_player);
        }
        //step
        black_player.curr_time += time;
        white_player.curr_time += time;
        if(black_player.IsMoveable() && black_player.curr_time >= step_time)
        {
            black_player.curr_time -= step_time;
            Step(black_player);
        }
        if(white_player.IsMoveable() && white_player.curr_time >= step_time)
        {
            white_player.curr_time -= step_time;
            Step(white_player);
        }
        //为渲染提供矩阵
        for(int i = 0; i < size.x; ++i)
        {
            for(int n = 0; n < size.y; ++n)
            {
                cells[i,n] = (int)cubes[i, n].color;
            }
        }
    }

    //move rotates
    public void TetrominoMoveRotate(ref PlayerInput input, ref PlayerHandle player)
    {
        //move
        Vector2Int offset = Vector2Int.zero;
        if(input.horizontal != 0)
            offset.x += (input.horizontal > 0) ? 1 : -1;
        if(input.vertical != 0 && (input.vertical) * player.y_director > 0)
        {
            offset.y = player.y_director;
            player.curr_time = 0;
        }
        player.tetromino_data.position += offset;
        if(IsTetrominoGround(ref player)){
            player.tetromino_data.on_ground = true;
            return;
        }
        //rotate

    }

    //新的一轮
    private void NewRound()
    {

    }

    //每一个“下落”的周期调用，判断是否触底
    private bool Step(PlayerHandle player)
    {
        return false;
    }

    //填充/吃
    private bool Filling()
    {
        return false;
    }

    //下沉
    private bool Sinking()
    {
        return false;
    }

    //判断触底
    private bool IsTetrominoGround(ref PlayerHandle player)
    {
        for(int i = 0; i < 4; ++i)
        {
            Vector2Int cur_position = player.tetromino_data.cells[i];
            if((cubes[cur_position.x, cur_position.y].color == player.tetromino_data.color &&
                cubes[cur_position.x, cur_position.y].is_background) ||
                (cubes[cur_position.x, cur_position.y].color != player.tetromino_data.color &&
                !cubes[cur_position.x, cur_position.y].is_background))
            {
                return true;
            }
        }
        return false;
    }
}
