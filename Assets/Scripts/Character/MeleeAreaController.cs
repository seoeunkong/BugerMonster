using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAreaController : Character
{
    private List<Character> _detects = new List<Character>(); // Ž���� �� ĳ���� ���
    public override void ThinkAction()
    {
        _detects = DetectEnemiesAround(); // �ֺ� �� Ž��

        if (_detects.Count > 0) // ���� �ִ� ���
        {
            Attack();
        }
        else // ���� ���� ���
        {
            Move();
        }
        UpdateIconAmount(); // ������ ���� ������Ʈ
    }

    public override void Attack()
    {
        CharacterAnimator.SetTrigger("onAttack");
        foreach (Character character in _detects)
        {
            SpriteFlip(character.transform.position);
            character.Hit(AttackPower);
        }
        _detects.Clear();
    }


    protected List<Character> DetectEnemiesAround()  // �ֺ� �� ĳ���� Ž��
    {
        List<Character> enemies = new List<Character>();
        float x = transform.position.x;
        float y = transform.position.y;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;  // ���� ��ġ�� ����

                Vector3 detectionPosition = new Vector3(x + i, y + j, transform.position.z);
                Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(detectionPosition);
                if (cell == null) continue; // ��ġ�� �׸��� ������ ������� Ȯ��

                Character detectedCharacter = GameManager.Instance.GetCharacterAtPosition(detectionPosition);
                if (detectedCharacter != null && detectedCharacter.TeamType != TeamType)
                {
                    enemies.Add(detectedCharacter);
                }
            }
        }

        return enemies;
    }

}
