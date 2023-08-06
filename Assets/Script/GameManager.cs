using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BlockData == 순수하게 테트리스 블록에 대한 정보..
// Tetromino == 완성된 블록의 이름.
// 

/// <summary>
/// 테트리스 로직을 작성한다.
/// 
/// 2023.06.20 Ocean
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                return null;
            }
            return instance;
        }
    }

    public BlockDataManager m_BlockDataManager;
    public InGameManger m_InGameManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }


}
