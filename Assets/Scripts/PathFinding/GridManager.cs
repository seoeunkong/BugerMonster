using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Cell> Path { get; private set; } = new List<Cell>(); // 경로를 저장하는 리스트

    public int Width => _width;  // 그리드의 너비
    public int Height => _height; // 그리드의 높이

    [SerializeField] private int _width, _height; // 그리드의 너비와 높이 설정
    [SerializeField] private Cell _cellPrefab; // 셀 프리팹
    private Transform _cam; // 카메라 

    private Dictionary<Vector2, Cell> _tiles; // 위치와 셀을 매핑하는 딕셔너리

    void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()   // 그리드를 생성
    {
        CameraInit();

        _tiles = new Dictionary<Vector2, Cell>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var spawnedTile = Instantiate(_cellPrefab, new Vector3(x, y), Quaternion.identity);
                spawnedTile.name = $"Cell {x} {y}";
                spawnedTile.transform.parent = transform;
                spawnedTile.Init(true, x, y);

                _tiles[new Vector2(x, y)] = spawnedTile;
            }
        }
    }

    public List<Cell> GetNeighbours(Cell cell)   //상하좌우로 이웃한 4개의 이웃노드 반환 
    {
        //f_cost와 h_cost가 같은 경우를 대비해서 아래 노드가 먼저 우선순위를 가지게끔 구현
        int[] offX = { 0, -1, 1, 0 };
        int[] offY = { -1, 0, 0, 1 };

        List<Cell> neighbours = new List<Cell>();
        for (int i = 0; i < 4; i++)
        {
            int checkX = cell.GridX + offX[i];
            int checkY = cell.GridY + offY[i];            

            if (checkX >= 0 && checkX < _width && checkY >= 0 && checkY < _height)
            {
                neighbours.Add(GetTileAtPosition(new Vector2(checkX, checkY)));
            }
        }

        return neighbours;
    }

    public Cell GetTileAtPosition(Vector2 pos)   // 특정 위치의 타일 정보 반환
    {
        Vector2 position = new Vector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        if (_tiles.TryGetValue(position, out var tile)) return tile;
        return null;
    }

    public void ActiveGrid() //씬 이동 시 그리드 재활성화
    {
        CameraInit();

        for (int i = 0; i < Width * Height; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);

            Cell cell = transform.GetChild(i).GetComponent<Cell>();
            if (cell != null) cell.IsWalkable = true;
        }

    }

    void CameraInit() //카메라 위치 초기화
    {
        if (_cam == null) _cam = Camera.main.transform;
        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

}
