using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public class PlayerController : MonoBehaviour
{
    public string DestinationTag = "Cell"; // ������ �±�
    private Vector3 _offset; // ���콺 Ŭ�� �� ������
    private Vector3 _startPos; // ���� ��ġ
    private Cell _preCell; // ���� ��
    private Collider2D _collider2d; // �ݶ��̴�

    private void Start()
    {
        _collider2d = GetComponent<Collider2D>();
    }

    private bool IsValidMove(Vector3 pos)    // ��ȿ�� �̵����� Ȯ��
    {
        Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(pos);
        return cell != null && cell.IsWalkable;
    }

    private Vector3 NormalizeMousePos(Vector3 pos)  // ���콺 ��ġ ����ȭ
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
                cell.IsWalkable = false; //�ش� �� �̵� �Ұ����� ���·� ��ȯ 
                GameManager.Instance.UpdateCharacterInfo(this.GetComponent<Character>()); //�Ʊ� ����â�� ������Ʈ

                // �ֺ� ���� ���̶���Ʈ ����
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

    private void HandleCellHighlighting()  // �� ���̶���Ʈ ó��
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

    private Vector3 GetMouseWorldPosition()  // ���콺�� ���� ��ǥ�� ��ȯ
    {
        var mouseScreenPos = Input.mousePosition;
        mouseScreenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(mouseScreenPos);
    }
  
}
