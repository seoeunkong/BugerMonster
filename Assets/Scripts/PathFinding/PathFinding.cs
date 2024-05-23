using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PathFinding : MonoBehaviour
{
    public Cell FindPath(int Index)
    {
        Vector3 tarPos = GameManager.Instance.Characters[Index].transform.position;

        Cell startCell = GameManager.Instance.GridManager.GetTileAtPosition(transform.position);
        Cell targetCell = GameManager.Instance.GridManager.GetTileAtPosition(tarPos);

        //List<Cell> openSet = new List<Cell>();
        List<Cell> openSet = new List<Cell>();
        HashSet<Cell> closedSet = new HashSet<Cell>();

        openSet.Add(startCell);
        while (openSet.Count > 0)
        {
            #region ���� ���� ���� ���� ��带 �����Ѵ�.
            Cell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentCell.fCost || (openSet[i].fCost == currentCell.fCost && openSet[i].HCost < currentCell.HCost))
                {
                    currentCell = openSet[i];
                }
            }
            #endregion

            #region ���� ���� ���� ���� ��尡 �������� Ž���� �����Ѵ�.
            if (currentCell == targetCell)
            {
                List<Cell> paths= RetracePath(startCell, targetCell);
   
                Vector2 pos = transform.position;
                Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(pos);
                if(paths.Count > 1) //���� ��ġ�� Ÿ���� �������� ���� ���
                {
                    return paths[0];
                }
                return null;
            }
            #endregion

            #region ���� ��带 ���� �¿��� ���� Ŭ����� ������ �̵��Ѵ�.
            openSet.Remove(currentCell);
            closedSet.Add(currentCell);
            #endregion

            #region �̿���带 �����ͼ� ���� ����� �� ���� �¿� �߰��Ѵ�.
            foreach (Cell n in GameManager.Instance.GridManager.GetNeighbours(currentCell))
            {
                if (n != targetCell && (!n.IsWalkable || closedSet.Contains(n) || GameManager.Instance.CharacterNextCell.Contains(n))) 
                {
                    continue;
                }

                int g = currentCell.GCost + GetDistance(currentCell, n);
                int h = GetDistance(n, targetCell);
                int f = g + h;

                // ���� �¿� �̹� �ߺ� ��尡 �ִ� ��� ���� ���� ������ �����Ѵ�.
                if (!openSet.Contains(n))
                {
                    n.GCost = g;
                    n.HCost = h;
                    n.Parent = currentCell;
                    openSet.Add(n);
                }
                else
                {
                    if (n.fCost > f)
                    {
                        n.GCost = g;
                        n.Parent = currentCell;
                    }
                }
            }
            #endregion
        }
        return null;
    }

    private List<Cell> RetracePath(Cell startCell, Cell endCell)
    {
        List<Cell> path = new List<Cell>();
        Cell currentCell = endCell;

        while (currentCell != startCell)
        {
            path.Add(currentCell);
            currentCell = currentCell.Parent;
        }

        //���� ��忡�� ���� ������
        //�ִܰŸ� ��ã�⿡ �ʿ��� ��� ������ �迭�� ����� ����.
        path.Reverse();
        return path;
    }

    private int GetDistance(Cell cellA, Cell cellB) 
    {
        int dstX = Mathf.Abs(cellA.GridX - cellB.GridX);
        int dstY = Mathf.Abs(cellA.GridY - cellB.GridY);

        return dstX > dstY ? (dstX - dstY) : (dstY - dstX);
    }
}
