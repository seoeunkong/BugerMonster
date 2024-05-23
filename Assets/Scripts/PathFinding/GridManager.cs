using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public List<Cell> Path { get; private set; } = new List<Cell>(); // ��θ� �����ϴ� ����Ʈ

    public int Width => _width;  // �׸����� �ʺ�
    public int Height => _height; // �׸����� ����

    [SerializeField] private int _width, _height; // �׸����� �ʺ�� ���� ����
    [SerializeField] private Cell _cellPrefab; // �� ������
    private Transform _cam; // ī�޶� 

    private Dictionary<Vector2, Cell> _tiles; // ��ġ�� ���� �����ϴ� ��ųʸ�

    void Start()
    {
        GenerateGrid();
    }

    public void GenerateGrid()   // �׸��带 ����
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

    public List<Cell> GetNeighbours(Cell cell)   //�����¿�� �̿��� 4���� �̿���� ��ȯ 
    {
        //f_cost�� h_cost�� ���� ��츦 ����ؼ� �Ʒ� ��尡 ���� �켱������ �����Բ� ����
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

    public Cell GetTileAtPosition(Vector2 pos)   // Ư�� ��ġ�� Ÿ�� ���� ��ȯ
    {
        Vector2 position = new Vector2(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));
        if (_tiles.TryGetValue(position, out var tile)) return tile;
        return null;
    }

    public void ActiveGrid() //�� �̵� �� �׸��� ��Ȱ��ȭ
    {
        CameraInit();

        for (int i = 0; i < Width * Height; i++)
        {
            transform.GetChild(i).gameObject.SetActive(true);

            Cell cell = transform.GetChild(i).GetComponent<Cell>();
            if (cell != null) cell.IsWalkable = true;
        }

    }

    void CameraInit() //ī�޶� ��ġ �ʱ�ȭ
    {
        if (_cam == null) _cam = Camera.main.transform;
        _cam.transform.position = new Vector3((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f, -10);
    }

}
