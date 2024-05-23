using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAreaController : Character
{
    private List<Character> _detects = new List<Character>(); // 탐지된 적 캐릭터 목록
    public override void ThinkAction()
    {
        _detects = DetectEnemiesAround(); // 주변 적 탐지

        if (_detects.Count > 0) // 적이 있는 경우
        {
            Attack();
        }
        else // 적이 없는 경우
        {
            Move();
        }
        UpdateIconAmount(); // 아이콘 상태 업데이트
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


    protected List<Character> DetectEnemiesAround()  // 주변 적 캐릭터 탐지
    {
        List<Character> enemies = new List<Character>();
        float x = transform.position.x;
        float y = transform.position.y;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;  // 현재 위치는 제외

                Vector3 detectionPosition = new Vector3(x + i, y + j, transform.position.z);
                Cell cell = GameManager.Instance.GridManager.GetTileAtPosition(detectionPosition);
                if (cell == null) continue; // 위치가 그리드 범위를 벗어나는지 확인

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
