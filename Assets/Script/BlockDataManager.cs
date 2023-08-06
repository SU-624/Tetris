using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockData
{
    public int[,] block;
}

/// <summary>
/// ���� ���� ��ġ���� ����� �����͸� �������ִ� Ŭ����
/// 
/// 2023.06.20  Ocean
/// </summary>
public class BlockDataManager : MonoBehaviour
{
    private BlockData[] m_BlockDatas = new BlockData[7];
    public BlockData[] BlockDatas { get { return m_BlockDatas; } }

    private void Start()
    {
        InitTetromino();
    }

    // ������ ����� �������ִ� ������ �� �ʱ�ȭ
    private void InitTetromino()
    {
        for (int i = 0; i < m_BlockDatas.Length; i++)
        {
            m_BlockDatas[i] = new BlockData();
        }

        // IBlock
        m_BlockDatas[0].block = new int[4, 4] {
            {0,0,0,0},
            {0,0,0,0},
            {1,1,1,1},
            {0,0,0,0}
        };

        // OBlock
        m_BlockDatas[1].block = new int[4, 4] {
            {0,0,0,0},
            {0,1,1,0},
            {0,1,1,0},
            {0,0,0,0}
        };

        // TBlock
        m_BlockDatas[2].block = new int[4, 4] {
            {0,0,0,0},
            {0,1,0,0},
            {1,1,1,0},
            {0,0,0,0}
        };

        // LBlock
        m_BlockDatas[3].block = new int[4, 4] {
            {0,0,0,0},
            {0,0,1,0},
            {1,1,1,0},
            {0,0,0,0}
        };

        // JBlock
        m_BlockDatas[4].block = new int[4, 4] {
            {0,0,0,0},
            {0,1,0,0},
            {0,1,1,1},
            {0,0,0,0}
        };

        // SBlock
        m_BlockDatas[5].block = new int[4, 4] {
            {0,0,0,0},
            {0,1,1,0},
            {1,1,0,0},
            {0,0,0,0}
        };

        // ZBlock
        m_BlockDatas[6].block = new int[4, 4]{
             {0,0,0,0 },
             {0,1,1,0 },
             {0,0,1,1 },
             {0,0,0,0 }
        };
    }
}
