using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 23.06.21 TODO : 전광판 다시 하기
/// 23.06.21 TODO : 회전시 벽을 뚫는 부분 수정하기
/// 
/// 23.07.03 TODO : 홀드, GameOver만들기
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
        CW,     // 시계방향
        CCW,    // 반시계방향
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
    private readonly int m_InitPosX = 4;                                // 블럭이 처음 생성될 초기 위치 
    private readonly int m_InitPosY = 21;                               //
    private int m_PosX;                                                 // 블럭을 움직일 때 사용할 변수
    private int m_PosY;                                                 //
    private int m_PrevPosX;                                             // 블록의 이전 포지션값을 저장하는 변수
    private int m_PrevPosY;                                             //
    private int m_HoldPosY = 3;
    private int m_Score;
    private int m_ComboCount;
    private bool m_Combo;                                               // 콤보를 했는지 확인해주는 변수
    private bool m_GameOver;                                            // 게임오버시 키 입력을 막아주기 위한 변수

    /// 보드는 여기가 아니라 다른곳에 넣어두기
    private int[,] m_GameBoard = new int[22, 12];                       // 보드의 데이터를 넣어줄 이차원배열 0 : 빈칸, 1 : 벽, 2 : 고정된 블럭
    private int[,] m_HoldTetromino = new int[4, 4];                     // 홀드를 시켜놓을 테트로미노를 담고있을 이차원배열
    private Tetromino[,] m_RendererBoard = new Tetromino[22, 12];       // 화면에 그려줄 정보들을 넣어주는 배열. 움직이는 블록을 여기에?
    private Tetromino[,] m_HoldBoard = new Tetromino[4, 4];             // 화면에 그려줄 정보들을 넣어주는 배열. 움직이는 블록을 여기에?
    private int[,] m_TetrominoData = new int[4, 4];                     // 내가 움직이려고 하는 블록의 데이터를 담을 이차원배열
    private int[,] m_PrevTetrominoData = new int[4, 4];                 // 내가 회전하려고 하는 블록의 이전 데이터를 담을 이차원배열
    private int[] m_BlockIndex = new int[7];                            // 원본 테트로미노 데이터가 담긴 배열의 인덱스를 랜덤으로 뽑아오기 위해 미리 인덱스 값을 담아놓은 배열
    ///

    private float m_Timer;                                              // 0.5초마다 테트로미노가 떨어질 수 있게 해주는 변수
    private float m_KeyTimer;                                           // 아래키를 눌렀을 때 쭉 내려갈 수 있게 해주는 변수, GetKey에서 약간의 딜레이를 줬다.
    private int m_RandomTetrominoIndex;
    private int m_HoldTetrominoColorIndex;
    private int m_NonReduplicationRandomIndex;                          // 중복이 아닌 난수를 위한 변수
    private bool m_IsCanHold;                                           // 홀드키는 한번씩만 누를 수 있다.
    private bool m_IsHoldFirst;                                         // 홀드키를 게임 시작하고 처음 누른건지 판별해주는 변수.
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

    // 홀드 버튼을 눌렀을 때 내가 담을 테트로미노를 홀드 패널에 보여주기 위해 미리 초기화시켜준다.
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

    // 블럭이 생성되는 부분에 이미 굳은 블럭이 있다면 게임오버.
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

    // 테트로미노와 게임 보드를 렌더링 해주는 함수
    private void RenderBoard()
    {
        EraserPrevTetromino();

        DrawRendererBoard();

        DrawTetromino();

        DrawGhostTetromino();
    }

    // 테트로미노가 굳거나 라인이 삭제 될 때 모든 데이터 보드를 돌며 데이터값이 있는 부분만 그려주는 함수
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

    // 블럭을 만들어주는 함수
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

    // 움직일 수 있다면 테트로미노를 움직여준다.
    private void MoveTetromino(int _x, int _y)
    {
        m_PosX += _x;
        m_PosY += _y;
    }

    // 내가 움직이는 테트로미노가 바닥 어디에 놓일 지 미리 보여주는 고스트 블록을 그려주는 함수
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
                // 테트로미노가 현재 위치에서 떨어지는곳을 표시해준다.
                if (m_TetrominoData[y, x] != 0)
                {
                    // 만약 테트로미노와 고스트가 겹쳐지는 부분이 있다면 고스트는 그리지 않는다.
                    if (m_RendererBoard[m_PosY - y + _y, m_PosX + x].m_BlockType == BlockType.Block) continue;

                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].gameObject.SetActive(true);
                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].m_BlockType = BlockType.Ghost;
                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].m_BlockColor = BlockColor.Ghost;
                    m_RendererBoard[m_PosY - y + _y, m_PosX + x].ChangeBlockColor(m_TetrominoColor[(int)BlockColor.Ghost]);
                }
            }
        }
    }

    // 테트로미노가 움직일 수 있는지 먼저 판별해준다.
    private bool CheckCanMove(int _x, int _y)
    {
        bool _isCanMove = false;

        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // 내가 움직일 테트로미노의 값이 있는것만 판별하기 위해서
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

    // 테트로미노가 움직일 수 있는지 먼저 판별해준다.
    private bool CheckCanRotate(int _rotateDirection)
    {
        bool _isCanRotate = true;

        // 미리 테트로미노를 돌려보고 그곳이 빈곳이라면 돌릴 수 있다고 true 값을 리턴해준다.
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

                // 오른쪽 왼쪽 예외처리
                if (m_PosX + _rotateX <= 0 || m_PosX + _rotateX >= 11)
                {
                    return false;
                }

                // 아래 바닥 예외처리
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

    // 생성된 블럭을 움직이는 함수
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

        // 시계방향으로 회전
        if (Input.GetKeyDown(KeyCode.Z))
            m_InputRotate = Rotate.CW;

        // 반시계방향으로 회전
        if (Input.GetKeyDown(KeyCode.X))
            m_InputRotate = Rotate.CCW;
    }

    // 움직일 테트로미노가 움직일 수 있는지 확인해주는 함수
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

        // 시계방향으로 회전
        if (m_InputRotate == Rotate.CW && !m_GameOver)
        {
            if (CheckCanRotate(-1) == true)
                RotationTetromino(-1);
        }

        // 반시계방향으로 회전
        if (m_InputRotate == Rotate.CCW && !m_GameOver)
        {
            if (CheckCanRotate(1) == true)
                RotationTetromino(1);
        }

        CheckGameOver();
    }

    // 테트로미노를 잠시 홀드해주는 함수
    // 한번도 홀드를 한 적이 없으면 바로 새로 만들어주고 아니라면 이전에 홀드해놓은 정보를 가져오고
    // 움직여주는 테트로미노에 그 데이터를 넣어준다.
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
            int _colorIndex = m_HoldTetrominoColorIndex - 2;            // 렌더해주는 부분에서 색상에 2를 더해주고 있으니 여기서는 빼준다.

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

    // 1이면 시계방향으로 회전, -1이면 반시계방향으로 회전
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

    // 바뀐 위치에 테트로미노를 그려주는 함수
    private void DrawTetromino()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // 옆으로 옮긴 자리에서 블럭을 그려준다.
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

    // 위치가 변하기 전에 이전에 있던 테트로미노를 먼저 지워줘야한다.
    // 이전 위치의 테트로미노를 지워주는 함수
    private void EraserPrevTetromino()
    {
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                // 옆으로 옮긴 자리에서 블럭을 그려준다.
                if (m_PrevTetrominoData[y, x] != 0)
                {
                    m_RendererBoard[m_PrevPosY - y, m_PrevPosX + x].gameObject.SetActive(false);
                    m_RendererBoard[m_PrevPosY - y, m_PrevPosX + x].m_BlockType = BlockType.Empty;
                }
            }
        }
    }

    // 현재 테트로미노가 굳을 블럭인지 확인해주는 함수
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

    // 바닥이나 블록에 닿았으면 굳혀줘야한다.
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

    // 테트로미노가 보드판에 찼다면 삭제시켜주는 함수
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