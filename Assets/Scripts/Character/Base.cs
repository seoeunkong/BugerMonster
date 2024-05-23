using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Base : MonoBehaviour
{
    #region #ĳ���� ����
    public float AttackPower { get; protected set; } // ���ݷ�
    public float HP { get; protected set; } // ���� ü��
    public float MaxHP { get; protected set; } // �ִ� ü��
    public float Speed { get; protected set; } // �ӵ�
    public Animator CharacterAnimator { get; protected set; } // ĳ���� �ִϸ�����
    public Type Type { get; protected set; } // ĳ���� Ÿ��
    public TeamType TeamType { get; protected set; } // �� Ÿ��
    public Sprite Icon { get; protected set; } // ĳ���� ������

    public SpriteRenderer SpriteRenderer { get; protected set; } // ĳ������ SpriteRenderer
    public CharacterData CharacterData { get; protected set; } // ĳ���� ������
    #endregion

    private bool _isDead = false; // ��� ����
    public bool IsValid => !_isDead; // ��ȿ ����
    public const float DANGERRATE = 0.3f; // ĳ������ ���� ���� ���� ����

    protected int target; // Ÿ�� �ε���
    protected Vector3 targetPosition; // Ÿ�� ��ġ
    protected bool isMoving = false; // �̵� �� ����
    public int ZValue => -5;

    public abstract void Create(CharacterData characterData, TeamType team);

    protected void SpriteFlip(Vector3 target)
    {
        SpriteRenderer.flipX = target.x < transform.position.x;
    }

    public virtual void Die()
    {
        GameManager.Instance.GridManager.GetTileAtPosition(transform.position).IsWalkable = true; // ���� ��ġ�� Ÿ���� �ٽ� �̵� �����ϰ� ����
        _isDead = true;
        transform.gameObject.SetActive(false);
    }

}
