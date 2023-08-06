using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// BlockData == �����ϰ� ��Ʈ���� ��Ͽ� ���� ����..
// Tetromino == �ϼ��� ����� �̸�.
// 

/// <summary>
/// ��Ʈ���� ������ �ۼ��Ѵ�.
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
