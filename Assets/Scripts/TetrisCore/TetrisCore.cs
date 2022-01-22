using System.Collections;
using System.Collections.Generic;
using UnityEngine;

struct Cube
{
    public Role color;
    public bool is_background;    
}

// 俄罗斯方块的主要逻辑
// 程序视角为 上黑下白
public class TetrisCore : IGamePhase
{
    public float step_time { get; private set; }
    private float curr_normal_time = 0;
    private Vector2Int size;
    private List<List<Cube>> cubes;
    Tetromino black_tetromino;
    Tetromino white_tetromino;

    //初始化 平分地图
    public void Init(float step, Vector2Int size)
    {
        step_time = step;
        this.size = size;
        cubes = new List<List<Cube>>(size.y);
        for(int i = 0; i < size.y / 2; ++i)
        {
            List<Cube> line = new List<Cube>(size.x);
            for(int n = 0; n < size.x; ++n)
            {
                line[n] = (new Cube() {
                    color = Role.White,
                    is_background = true
                });
            }
            cubes[i] = line;
        }
        for(int i = size.y / 2; i < size.y; ++i)
        {
            List<Cube> line = new List<Cube>(size.x);
            for(int n = 0; n < size.x; ++n)
            {
                line[n] = (new Cube() {
                    color = Role.Black,
                    is_background = true
                });
            }
            cubes[i] = line;
        }
    }

    public void SetStepTime(float time)
    {
        step_time = time;
    }

    //每帧被调用
    public void Update(float time, PlayerInput[] input, ref int[,] cells)
    {
        curr_normal_time += time;
        if(input[(int)Role.Black].IsValid)
        {
            TetrominoMove(ref input[0], 1);
        }
        if(input[(int)Role.White].IsValid)
        {
            TetrominoMove(ref input[1], -1);
        }
        //为渲染提供矩阵
        for(int i = 0; i < size.y; ++i)
        {
            for(int n = 0; n < size.x; ++n)
            {
                cells[i,n] = (int)cubes[i][n].color;
            }
        }
    }

    public void TetrominoMove(ref PlayerInput input, int director)
    {
        
    }

    //新的一轮
    private void NewRound()
    {

    }

    //每一个“下落”的周期调用，判断是否触底
    private bool Step()
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
}
