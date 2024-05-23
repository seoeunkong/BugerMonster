using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] private GameObject _highlight;

    public bool IsWalkable { get { return _isWalkable; } set { _isWalkable = value; } }
    public int GCost { get { return _gCost; } set { _gCost = value; } }
    public int HCost { get { return _hCost; } set { _hCost = value; } }
    public int GridX { get { return _gridX; } set { _gridX = value; } }
    public int GridY { get { return _gridY; } set { _gridY = value; } }
    public Cell Parent { get { return _parent; } set { _parent = value; } }

    [SerializeField] private bool _isWalkable = true;
    private int _gCost; //A 알고리즘의 경로값 G(n) 
    private int _hCost; //A 알고리즘의 휴리스틱 H(n) 
    private int _gridX;
    private int _gridY;
    private Cell _parent;
    private SpriteRenderer _hightlightRenderer;


    public void Init(bool walkable, int gridX, int gridY)
    {
        this._isWalkable = walkable;
        this.GridX = gridX;
        this.GridY = gridY;
        if (_hightlightRenderer == null) _hightlightRenderer = _highlight.GetComponent<SpriteRenderer>();
    }

    public void MouseEnter()
    {
        _highlight.SetActive(true);
    }

    public void MouseExit()
    {
        _highlight.SetActive(false);
    }

    public int CompareTo(Cell otherCell)
    {
        if (this.fCost < otherCell.fCost) return -1;
        else if (this.fCost > otherCell.fCost) return 1;
        else return 0;
    }

    public int fCost
    {
        get
        {
            return GCost + HCost;
        }
    }
}
