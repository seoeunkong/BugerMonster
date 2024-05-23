using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base : MonoBehaviour
{
    #region #캐릭터 스탯
    public float AttackPower { get; protected set; } // 공격력
    public float HP { get; protected set; } // 현재 체력
    public float MaxHP { get; protected set; } // 최대 체력
    public float Speed { get; protected set; } // 속도
    public Animator CharacterAnimator { get; protected set; } // 캐릭터 애니메이터
    public Type Type { get; protected set; } // 캐릭터 타입
    public TeamType TeamType { get; protected set; } // 팀 타입
    public Sprite Icon { get; protected set; } // 캐릭터 아이콘

    public SpriteRenderer SpriteRenderer { get; protected set; } // 캐릭터의 SpriteRenderer
    public CharacterData CharacterData { get; protected set; } // 캐릭터 데이터
    #endregion

    private bool _isDead = false; // 사망 여부
    public bool IsValid => !_isDead; // 유효 여부
    public const float DANGERRATE = 0.3f; // 캐릭터의 위험 상태 기준 비율

    protected int target; // 타겟 인덱스
    protected Vector3 targetPosition; // 타겟 위치
    protected bool isMoving = false; // 이동 중 여부
    public int ZValue => -5;

    public abstract void Create(CharacterData characterData, TeamType team);

    protected void SpriteFlip(Vector3 target)
    {
        SpriteRenderer.flipX = target.x < transform.position.x;
    }

    public virtual void Die()
    {
        GameManager.Instance.GridManager.GetTileAtPosition(transform.position).IsWalkable = true; // 현재 위치의 타일을 다시 이동 가능하게 설정
        _isDead = true;
        transform.gameObject.SetActive(false);
    }

}
