using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 23.06.21 TODO : ������ �ٽ� �ϱ�
/// 23.06.21 TODO : ȸ���� ���� �մ� �κ� �����ϱ�
/// 
/// 23.07.03 TODO : Ȧ��, GameOver�����
/// 
/// 2023.06.26 Ocean
/// </summary>
public class InGameManger : MonoBehaviour
{
    enum Direction
    {
        None,
        Right,
        Left,
        Down,
        FastDown,
        Hold,
    }

    enum Rotate
    {
        None,
        CW,     // �ð����
        CCW,    // �ݽð����
    }

    [SerializeField] private Material[] m_TetrominoColor;
    [SerializeField] private Transform m_BoardTransform;
    [SerializeField] private Transform m_HoldTransform;
    [SerializeField] private Tetromino m_Tetromino;
    [SerializeField] private GameObject m_GameOverPanel;
    [SerializeField] private TextMeshProUGUI m_ScoreText;
    [SerializeField] private TextMeshProUGUI m_ComboText;
    [SerializeField] private Animator m_ComboAni;

    private readonly int m_BoardWidth = 12;
    private readonly int m_BoardHeight = 22;
    private readonly int m_InitPosX = 4;                                // ���� ó�� ������ �ʱ� ��ġ 
    private readonly int m_InitPosY = 21;                               //
    private int m_PosX;                                                 // ���� ������ �� ����� ����
    private int m_PosY;                                                 //
    private int m_PrevPosX;                                             // ����� ���� �����ǰ��� �����ϴ� ����
    private int m_PrevPosY;                                             //
    private int m_HoldPosY = 3;
    private int m_Score;
    private int m_ComboCount;
    private bool m_Combo;                                               // �޺��� �ߴ��� Ȯ�����ִ� ����
    private bool m_GameOver;                                            // ���ӿ����� Ű �Է��� �����ֱ� ���� ����

    /// ����� ���Ⱑ �ƴ϶� �ٸ����� �־�α�
    private int[,] m_GameBoard = new int[22, 12];                       // ������ �����͸� �־��� �������迭 0 : ��ĭ, 1 : ��, 2 : ������ ��
    private int[,] m_HoldTetromino = new int[4, 4];                     // Ȧ�带 ���ѳ��� ��Ʈ�ι̳븦 ������� �������迭
    private Tetromino[,] m_RendererBoard = new Tetromino[22, 12];       // ȭ�鿡 �׷��� �������� �־��ִ� �迭. �����̴� ����� ���⿡?
    private Tetromino[,] m_HoldBoard = new Tetromino[4, 4];             // ȭ�鿡 �׷��� �������� �־��ִ� �迭. �����̴� ����� ���⿡?
    private int[,] m_TetrominoData = new int[4, 4];                     // ���� �����̷��� �ϴ� ����� �����͸� ���� �������迭
    private int[,] m_PrevTetrominoData = new int[4, 4];                 // ���� ȸ���Ϸ��� �ϴ� ����� ���� �����͸� ���� �������迭
    private int[] m_BlockIndex = new int[7];                            // ���� ��Ʈ�ι̳� �����Ͱ� ��� �迭�� �ε����� �������� �̾ƿ��� ���� �̸� �ε��� ���� ��Ƴ��� �迭
    ///

    private float m_Timer;                                              // 0.5�ʸ��� ��Ʈ�ι̳밡 ������ �� �ְ� ���ִ� ����
    private float m_KeyTimer;                                           // �Ʒ�Ű�� ������ �� �� ������ �� �ְ� ���ִ� ����, GetKey���� �ణ�� �����̸� ���.
    private int m_RandomTetrominoIndex;
    private int m_HoldTetrominoColorIndex;
    private int m_NonReduplicationRandomIndex;                          // �ߺ��� �ƴ� ������ ���� ����
    private bool m_IsCanHold;                                           // Ȧ��Ű�� �ѹ����� ���� �� �ִ�.
    private bool m_IsHoldFirst;                                         // Ȧ��Ű�� ���� �����ϰ� ó�� �������� �Ǻ����ִ� ����.
    private Direction m_InputDirection;
    private Rotate m_InputRotate;

    private void Start()
    {
        m_RandomTetrominoIndex = 0;
        m_KeyTimer = 0;
        m_Score = 0;
        m_ComboCount = 0;
        m_NonReduplicationRandomIndex = 7;
        m_IsCanHold = true;
        m_IsHoldFirst = true;
        m_Combo = false;
        m_GameOver = false;

        for (int i = 0; i < 7; i++)
        {
            m_BlockIndex[i] = i;
        }

        InitBoard();
        InitHoldBoard();
        MakeTetromino();
    }

    private void Update()
    {
        m_Timer += Time.deltaTime;
        m_KeyTimer += Time.deltaTime;

        KeyUpdate();
        GameLogicUpdate();
        RenderBoard();
    }

    private void InitBoard()
    {
        for (int y = 0; y < m_BoardHeight; y++)
        {
            for (int x = 0; x < m_BoardWidth; x++)
            {
                if (m_GameBoard[y, x] != 0 || y == 0 || x == 0 || x == 11)
                {
                    m_GameBoard[y, x] = 1;
                    Tetromino tetromino = Instantiate(m_Tetromino, new Vector3(x, y, 0), m_Tetromino.transform.rotation, m_BoardTransform);
                    tetromino.ChangeBlockColor(m_TetrominoColor[(int)BlockColor.Wall]);
                    tetromino.m_BlockType = BlockType.Wall;
                    tetromino.m_BlockColor = BlockColor.Wall;
                    m_RendererBoard[y, x] = tetromino;
                }
                else
                {
                    Tetromino tetromino = Instantiate(m_Tetromino, new Vector3(x, y, 0), m_Tetromino.transform.rotation, m_BoardTransform);
                    tetromino.gameObject.SetActive(false);
                    tetromino.m_BlockType = BlockType.Empty;
                    m_RendererBoard[y, x] = tetromino;
                }
            }
        }
    }

    // Ȧ�� ��ư�� ������ �� ���� ���� ��Ʈ�ι̳븦 Ȧ�� �гο� �����ֱ� ���� �̸� �ʱ�ȭ�����ش�.
    private void InitHoldBoard()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                Tetromino tetromino = Instantiate(m_Tetromino, m_HoldTransform);
                tetromino.transform.localPosition = new Vector3(x, m_HoldPosY - y, 0);
                tetromino.m_BlockType = BlockType.Empty;
                tetromino.gameObject.SetActive(false);
                m_HoldBoard[y, x] = tetromino;
            }
        }
    }

    // ���� �����Ǵ� �κп� �̹� ���� ���� �ִٸ� ���ӿ���.
    private void CheckGameOver()
    {
        for (int x = 4; x < m_BoardWidth; x++)
        {
            if (m_GameBoard[19, x] != 0 && m_GameBoard[19, x] != 1)
            {
                Time.timeScale = 0;
                m_GameOver = true;
                m_GameOverPanel.SetActive(true);
            }
        }
    }

    private IEnumerator DelayRender()
    {
        EraserPrevTetromino();

        DrawTetromino();

        yield return new WaitForSeconds(1f);

        RenderBoard();
    }

    private IEnumerator DelayMakeTetromino()
    {
        yield return new WaitForSeconds(1f);

        MakeTetromino();
    }

    // ��Ʈ�ι̳�� ���� ���带 ������ ���ִ� �Լ�
    private void RenderBoard()
    {
        EraserPrevTetromino();

        DrawRendererBoard();

        DrawTetromino();

        DrawGhostTetromino();
    }

    // ��Ʈ�ι̳밡 ���ų� ������ ���� �� �� ��� ������ ���带 ���� �����Ͱ��� �ִ� �κи� �׷��ִ� �Լ�
    private void DrawRendererBoard()
    {
        for (int y = 0; y < m_BoardHeight; y++)
        {
            for (int x = 0; x < m_BoardWidth; x++)
            {
                switch ((BlockColor)m_GameBoard[y, x])
                {
                    case BlockColor.Empty:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(false);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Empty;
                    }
                    break;

                    case BlockColor.Wall:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Wall;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.Wall;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.Wall]);
                    }
                    break;

                    case BlockColor.ILightBlue:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.ILightBlue;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.ILightBlue]);
                    }
                    break;

                    case BlockColor.OYellow:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.OYellow;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.OYellow]);
                    }
                    break;

                    case BlockColor.THotPink:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.THotPink;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.THotPink]);
                    }
                    break;

                    case BlockColor.LOrange:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.LOrange;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.LOrange]);
                    }
                    break;

                    case BlockColor.JBLue:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.JBLue;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.JBLue]);
                    }
                    break;

                    case BlockColor.SGreen:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.SGreen;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.SGreen]);
                    }
                    break;

                    case BlockColor.ZRedk:
                    {
                        m_RendererBoard[y, x].gameObject.SetActive(true);
                        m_RendererBoard[y, x].m_BlockType = BlockType.Block;
                        m_RendererBoard[y, x].m_BlockColor = BlockColor.ZRedk;
                        m_RendererBoard[y, x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.ZRedk]);
                    }
                    break;
                }
            }
        }
    }

    // ���� ������ִ� �Լ�
    private void MakeTetromino()
    {
        int _index = 0;

        if (m_NonReduplicationRandomIndex == 0)
        {
            m_NonReduplicationRandomIndex = 7;
        }

        _index = UnityEngine.Random.Range(0, m_NonReduplicationRandomIndex);

        m_RandomTetrominoIndex = m_BlockIndex[_index];
        m_TetrominoData = GameManager.Instance.m_BlockDataManager.BlockDatas[m_RandomTetrominoIndex].block;

        SwapArry(ref m_BlockIndex[_index], ref m_BlockIndex[m_NonReduplicationRandomIndex - 1]);
        m_NonReduplicationRandomIndex--;

        m_PosX = m_InitPosX;
        m_PosY = m_InitPosY;
    }

    // ������ �� �ִٸ� ��Ʈ�ι̳븦 �������ش�.
    private void MoveTetromino(int _x, int _y)
    {
        m_PosX += _x;
        m_PosY += _y;
    }

    // ���� �����̴� ��Ʈ�ι̳밡 �ٴ� ��� ���� �� �̸� �����ִ� ��Ʈ ����� �׷��ִ� �Լ�
    private void DrawGhostTetromino()
    {
        int _y = 0;

        while (CheckCanMove(0, _y - 1) == true)
        {
            _y -= 1;
        }

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // ��Ʈ�ι̳밡 ���� ��ġ���� �������°��� ǥ�����ش�.
                if (m_TetrominoData[y, x] != 0)
                {
                    // ���� ��Ʈ�ι̳�� ��Ʈ�� �������� �κ��� �ִٸ� ��Ʈ�� �׸��� �ʴ´�.
                    if (m_RendererBoard[m_PosY - y + _y, m_PosX + x].m_BlockType == BlockType.Block) continue;

                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].gameObject.SetActive(true);
                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].m_BlockType = BlockType.Ghost;
                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].m_BlockColor = BlockColor.Ghost;
                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.Ghost]);
                }
            }
        }
    }

    // ��Ʈ�ι̳밡 ������ �� �ִ��� ���� �Ǻ����ش�.
    private bool CheckCanMove(int _x, int _y)
    {
        bool _isCanMove = false;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // ���� ������ ��Ʈ�ι̳��� ���� �ִ°͸� �Ǻ��ϱ� ���ؼ�
                if (m_TetrominoData[y, x] == 0) continue;

                if (m_GameBoard[(m_PosY - y) + _y, (m_PosX + x) + _x] == 0)
                {
                    _isCanMove = true;
                }
                else
                {
                    return false;
                }
            }
        }

        return _isCanMove;
    }

    // ��Ʈ�ι̳밡 ������ �� �ִ��� ���� �Ǻ����ش�.
    private bool CheckCanRotate(int _rotateDirection)
    {
        bool _isCanRotate = true;

        // �̸� ��Ʈ�ι̳븦 �������� �װ��� ����̶�� ���� �� �ִٰ� true ���� �������ش�.
        int _rotateX = 0;
        int _rotateY = 0;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (m_TetrominoData[y, x] == 0) continue;

                if (_rotateDirection == 1)
                {
                    _rotateX = 4 - y - 1;
                    _rotateY = x;
                }
                else
                {
                    _rotateX = y;
                    _rotateY = 4 - x - 1;
                }

                // ������ ���� ����ó��
                if (m_PosX + _rotateX <= 0 || m_PosX + _rotateX >= 11)
                {
                    return false;
                }

                // �Ʒ� �ٴ� ����ó��
                if (m_PosY - _rotateY <= 0 || m_PosY - _rotateY > 21)
                {
                    return false;
                }

                if (m_GameBoard[m_PosY - _rotateY, m_PosX + _rotateX] != 0)
                {
                    return false;
                }
            }
        }

        return _isCanRotate;
    }

    // ������ ���� �����̴� �Լ�
    private void KeyUpdate()
    {
        m_InputDirection = Direction.None;
        m_InputRotate = Rotate.None;

        if (Input.GetKey(KeyCode.RightArrow) && m_KeyTimer > 0.1f)
        {
            m_InputDirection = Direction.Right;
            m_KeyTimer = 0;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && m_KeyTimer > 0.1f)
        {
            m_InputDirection = Direction.Left;
            m_KeyTimer = 0;
        }

        if (Input.GetKey(KeyCode.DownArrow) && m_KeyTimer > 0.1f)
        {
            m_InputDirection = Direction.Down;
            m_KeyTimer = 0;
        }

        if (Input.GetKeyDown(KeyCode.Space))
            m_InputDirection = Direction.FastDown;

        if (Input.GetKeyDown(KeyCode.LeftShift))
            m_InputDirection = Direction.Hold;

        // �ð�������� ȸ��
        if (Input.GetKeyDown(KeyCode.Z))
            m_InputRotate = Rotate.CW;

        // �ݽð�������� ȸ��
        if (Input.GetKeyDown(KeyCode.X))
            m_InputRotate = Rotate.CCW;
    }

    // ������ ��Ʈ�ι̳밡 ������ �� �ִ��� Ȯ�����ִ� �Լ�
    private void GameLogicUpdate()
    {
        m_PrevPosX = m_PosX;
        m_PrevPosY = m_PosY;

        m_PrevTetrominoData = m_TetrominoData;

        if (m_InputDirection == Direction.Right && !m_GameOver)
        {
            if (CheckCanMove(1, 0) == true)
                MoveTetromino(1, 0);
        }
        else if (m_InputDirection == Direction.Left && !m_GameOver)
        {
            if (CheckCanMove(-1, 0) == true)
                MoveTetromino(-1, 0);
        }

        if ((m_InputDirection == Direction.Down || m_Timer > 1f) && !m_GameOver)
        {
            MoveTetromino(0, -1);

            if (CheckFreezeBlock() == true)
                HardTetromino();

            m_Timer = 0;
        }

        if (m_InputDirection == Direction.FastDown && !m_GameOver)
        {
            int _y = 0;

            while (CheckCanMove(0, _y - 1) == true)
            {
                _y -= 1;
            }

            MoveTetromino(0, _y - 1);
            HardTetromino();
        }

        if (m_InputDirection == Direction.Hold && m_IsCanHold == true && !m_GameOver)
        {
            HoldTetromino();
        }

        // �ð�������� ȸ��
        if (m_InputRotate == Rotate.CW && !m_GameOver)
        {
            if (CheckCanRotate(-1) == true)
                RotationTetromino(-1);
        }

        // �ݽð�������� ȸ��
        if (m_InputRotate == Rotate.CCW && !m_GameOver)
        {
            if (CheckCanRotate(1) == true)
                RotationTetromino(1);
        }

        CheckGameOver();
    }

    // ��Ʈ�ι̳븦 ��� Ȧ�����ִ� �Լ�
    // �ѹ��� Ȧ�带 �� ���� ������ �ٷ� ���� ������ְ� �ƴ϶�� ������ Ȧ���س��� ������ ��������
    // �������ִ� ��Ʈ�ι̳뿡 �� �����͸� �־��ش�.
    private void HoldTetromino()
    {
        if (m_IsHoldFirst)
        {
            m_HoldTetromino = GameManager.Instance.m_BlockDataManager.BlockDatas[m_RandomTetrominoIndex].block;

            m_HoldTetrominoColorIndex = m_RandomTetrominoIndex + 2;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (m_HoldTetromino[y, x] == 0) continue;

                    if (m_HoldTetromino[y, x] != 0)
                    {
                        m_HoldBoard[y, x].gameObject.SetActive(true);
                        m_HoldBoard[y, x].m_BlockType = BlockType.Block;
                        m_HoldBoard[y, x].m_BlockColor = (BlockColor)m_HoldTetrominoColorIndex;
                        m_HoldBoard[y, x].ChangeBlockColor(m_TetrominoColor[m_HoldTetrominoColorIndex]);
                    }
                }
            }

            MakeTetromino();
            m_IsHoldFirst = false;
        }
        else
        {
            int[,] _tempArry = new int[4, 4];
            int _colorIndex = m_HoldTetrominoColorIndex - 2;            // �������ִ� �κп��� ���� 2�� �����ְ� ������ ���⼭�� ���ش�.

            _tempArry = GameManager.Instance.m_BlockDataManager.BlockDatas[m_RandomTetrominoIndex].block;
            m_TetrominoData = m_HoldTetromino;
            m_HoldTetromino = _tempArry;

            m_HoldTetrominoColorIndex = m_RandomTetrominoIndex + 2;
            m_RandomTetrominoIndex = _colorIndex;

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    if (m_HoldTetromino[y, x] == 0)
                        m_HoldBoard[y, x].gameObject.SetActive(false);

                    if (m_HoldTetromino[y, x] != 0)
                    {
                        m_HoldBoard[y, x].gameObject.SetActive(true);
                        m_HoldBoard[y, x].m_BlockType = BlockType.Block;
                        m_HoldBoard[y, x].m_BlockColor = (BlockColor)m_HoldTetrominoColorIndex;
                        m_HoldBoard[y, x].ChangeBlockColor(m_TetrominoColor[m_HoldTetrominoColorIndex]);
                    }
                }
            }
        }
        m_PosX = m_InitPosX;
        m_PosY = m_InitPosY;

        m_IsCanHold = false;
    }

    // 1�̸� �ð�������� ȸ��, -1�̸� �ݽð�������� ȸ��
    private void RotationTetromino(int _rotateDirection)
    {
        int[,] _tempTetrominoArry = new int[4, 4];

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (_rotateDirection == 1)
                {
                    _tempTetrominoArry[y, x] = m_TetrominoData[4 - x - 1, y];
                }
                else
                {
                    _tempTetrominoArry[y, x] = m_TetrominoData[x, 4 - y - 1];
                }
            }
        }

        m_TetrominoData = _tempTetrominoArry;
    }

    // �ٲ� ��ġ�� ��Ʈ�ι̳븦 �׷��ִ� �Լ�
    private void DrawTetromino()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // ������ �ű� �ڸ����� ���� �׷��ش�.
                if (m_TetrominoData[y, x] != 0)
                {
                    m_RendererBoard[m_PosY - y, m_PosX + x].gameObject.SetActive(true);
                    m_RendererBoard[m_PosY - y, m_PosX + x].m_BlockType = BlockType.Block;
                    m_RendererBoard[m_PosY - y, m_PosX + x].m_BlockColor = (BlockColor)m_RandomTetrominoIndex + 2;
                    m_RendererBoard[m_PosY - y, m_PosX + x].ChangeBlockColor(m_TetrominoColor[m_RandomTetrominoIndex + 2]);
                }
            }
        }
    }

    // ��ġ�� ���ϱ� ���� ������ �ִ� ��Ʈ�ι̳븦 ���� ��������Ѵ�.
    // ���� ��ġ�� ��Ʈ�ι̳븦 �����ִ� �Լ�
    private void EraserPrevTetromino()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // ������ �ű� �ڸ����� ���� �׷��ش�.
                if (m_PrevTetrominoData[y, x] != 0)
                {
                    m_RendererBoard[m_PrevPosY - y, m_PrevPosX + x].gameObject.SetActive(false);
                    m_RendererBoard[m_PrevPosY - y, m_PrevPosX + x].m_BlockType = BlockType.Empty;
                }
            }
        }
    }

    // ���� ��Ʈ�ι̳밡 ���� ������ Ȯ�����ִ� �Լ�
    private bool CheckFreezeBlock()
    {
        bool _isCanFreezen = false;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (m_TetrominoData[y, x] == 0) continue;

                if (m_GameBoard[m_PosY - y, m_PosX + x] != 0)
                {
                    _isCanFreezen = true;
                }
            }
        }

        return _isCanFreezen;
    }

    // �ٴ��̳� ��Ͽ� ������� ��������Ѵ�.
    private void HardTetromino()
    {
        m_PosY += 1;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (m_TetrominoData[y, x] == 0) continue;

                m_GameBoard[m_PosY - y, m_PosX + x] = m_RandomTetrominoIndex + 2;
            }
        }

        DeleteLine();
        MakeTetromino();

        m_IsCanHold = true;
    }

    // ��Ʈ�ι̳밡 �����ǿ� á�ٸ� ���������ִ� �Լ�
    private void DeleteLine()
    {
        int _tetrominoCount = 0;
        m_Combo = false;

        for (int PosY = 0; PosY < m_BoardHeight; PosY++)
        {
            _tetrominoCount = 0;

            for (int PosX = 0; PosX < m_BoardWidth; PosX++)
            {
                if (m_GameBoard[PosY, PosX] == 0) break;

                if (m_GameBoard[PosY, PosX] != 0
                    && m_GameBoard[PosY, PosX] != 1)
                {
                    _tetrominoCount++;
                }
            }

            if (_tetrominoCount == 10)
            {
                for (int y = 0; y + PosY < m_BoardHeight - 1; y++)
                {
                    for (int x = 0; x < m_BoardWidth; x++)
                    {
                        m_GameBoard[PosY + y, x] = m_GameBoard[PosY + 1 + y, x];
                    }
                }

                m_Score += 100;
                m_Combo = true;
                m_ComboCount += 1;
                PosY -= 1;
            }
        }

        if (m_Combo == false)
            m_ComboCount = 0;

        if (m_ComboCount != 0)
        {
            m_Score += (100 * m_ComboCount);
            m_ScoreText.text = m_Score.ToString();
            m_ComboText.text = "+" + m_ComboCount.ToString() + "Combo";
            m_ComboAni.Play("Combo", 0, 0f);
        }
    }

    private void SwapArry(ref int _to, ref int _from)
    {
        int temp = _to;
        _to = _from;
        _from = temp;
    }
}