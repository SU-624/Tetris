using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum BlockType
{
    Empty,
    Wall,
    Block,
    Ghost,
}

public enum BlockColor
{
    Empty,
    Wall,
    ILightBlue,
    OYellow,
    THotPink,
    LOrange,
    JBLue,
    SGreen,
    ZRedk,
    Ghost,
}

public class Tetromino : MonoBehaviour
{
    [SerializeField] private Renderer m_TetrominoColor;

    public BlockType m_BlockType;
    public BlockColor m_BlockColor;

    public void ChangeBlockColor(Material tetrominoColor)
    {
        m_TetrominoColor.material = tetrominoColor;
    }
}