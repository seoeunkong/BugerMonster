using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    public string DestinationTag = "Cell"; // 도착지 태그
    private Vector3 _offset; // 마우스 클릭 시 오프셋
    private Vector3 _startPos; // 시작 위치
    private Cell _preCell; // 이전 셀
    private Collider2D _collider2d; // 콜라이더

    private void Start()
    {
        _collider2d = GetComponent<Collider2D>();
    }

    private bool IsValidMove(Vector3 pos)    // 유효한 이동인지 확인
    {
        Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(pos);
        return cell != null && cell.IsWalkable;
    }

    private Vector3 NormalizeMousePos(Vector3 pos)  // 마우스 위치 정규화
    {
        return new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), pos.z);
    }

    private void OnMouseDown()
    {
        _offset = transform.position - GetMouseWorldPosition();
        _startPos = transform.position;
        _preCell = GameManager.Instance.GridManager.GetTileAtPosition(_startPos);
        if(_preCell != null) _preCell.IsWalkable = true;
    }

    private void OnMouseDrag()
    {
        transform.position = GetMouseWorldPosition() + _offset;
        HandleCellHighlighting();
    }

    private void OnMouseUp()
    {
        _collider2d.enabled = false;

        Vector3 pos = GetMouseWorldPosition();
        Vector3 normalizePos = NormalizeMousePos(pos);

        if (IsValidMove(normalizePos))
        {
            transform.position = normalizePos;

            Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(normalizePos);
            if (cell != null)
            {
                cell.IsWalkable = false; //해당 셀 이동 불가능한 상태로 전환 
                GameManager.Instance.UpdateCharacterInfo(this.GetComponent<Character>()); //아군 정보창에 업데이트

                // 주변 셀의 하이라이트 제거
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        GameManager.Instance.GridManager.GetTileAtPosition(new Vector2(normalizePos.x + i, normalizePos.y + j))?.MouseExit();
                    }
                }
            }
        }
        else
        {
            transform.position = _startPos;
        }
        
        _collider2d.enabled = true;
    }

    private void HandleCellHighlighting()  // 셀 하이라이트 처리
    {
        Cell currentCell = GameManager.Instance.GridManager.GetTileAtPosition(transform.position);

        if (currentCell != null && currentCell.IsWalkable)
        {
            currentCell.MouseEnter();

            if (_preCell != currentCell)
            {
                _preCell?.MouseExit();
                _preCell = currentCell;
            }
        }
    }

    private Vector3 GetMouseWorldPosition()  // 마우스의 월드 좌표를 반환
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
  
}
