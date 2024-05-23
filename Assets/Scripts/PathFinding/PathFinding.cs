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
            #region 가장 낮은 값을 가진 노드를 선택한다.
            Cell currentCell = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].fCost < currentCell.fCost || (openSet[i].fCost == currentCell.fCost && openSet[i].HCost < currentCell.HCost))
                {
                    currentCell = openSet[i];
                }
            }
            #endregion

            #region 가장 낮은 값을 가진 노드가 종착노드면 탐색을 종료한다.
            if (currentCell == targetCell)
            {
                List<Cell> paths= RetracePath(startCell, targetCell);
   
                Vector2 pos = transform.position;
                Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(pos);
                if(paths.Count > 1) //현재 위치와 타겟이 인접하지 않은 경우
                {
                    return paths[0];
                }
                return null;
            }
            #endregion

            #region 현재 노드를 오픈 셋에서 빼서 클로즈드 셋으로 이동한다.
            openSet.Remove(currentCell);
            closedSet.Add(currentCell);
            #endregion

            #region 이웃노드를 가져와서 값을 계산한 후 오픈 셋에 추가한다.
            foreach (Cell n in GameManager.Instance.GridManager.GetNeighbours(currentCell))
            {
                if (n != targetCell && (!n.IsWalkable || closedSet.Contains(n) || GameManager.Instance.CharacterNextCell.Contains(n))) 
                {
                    continue;
                }

                int g = currentCell.GCost + GetDistance(currentCell, n);
                int h = GetDistance(n, targetCell);
                int f = g + h;

                // 오픈 셋에 이미 중복 노드가 있는 경우 값이 작은 쪽으로 변경한다.
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

        //시작 노드에서 종착 노드까지
        //최단거리 길찾기에 필요한 모든 노드들의 배열을 만들어 낸다.
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
