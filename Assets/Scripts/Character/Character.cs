using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using static UnityEditor.PlayerSettings;

public abstract class Character : Base
{
    public PathFinding PathFinding { get; private set; }
    private Slider _healthBar;

    private void Start()
    {
        PathFinding = this.GetComponent<PathFinding>();
    }

    private void Update()
    {
        Moving();
    }

    private void Init()
    {
        CharacterAnimator = GetComponent<Animator>();
        SpriteRenderer = GetComponent<SpriteRenderer>();
        PathFinding = this.GetComponent<PathFinding>();
        _healthBar = this.GetComponentInChildren<Slider>();
    }

    public override void Create(CharacterData data, TeamType team) // ĳ���� ���� 
    {
        Init();
        AssignCharacterData(data, team);
    }

    private void AssignCharacterData(CharacterData data, TeamType team)
    {
        CharacterData = data;
        TeamType = team;
        Type = data.JobType;
        CharacterAnimator.runtimeAnimatorController = data.Ani;
        AttackPower = data.AttackPower;
        MaxHP = data.MaxHp;
        HP = data.MaxHp;
        Speed = data.Speed;
        Icon = data.IconSprite;

        if (team == TeamType.ENEMY)
            SpriteRenderer.color = Color.red;

        UpdateHealthBar(HP, MaxHP);
    }

    private bool IsEnemy(Character character) => character.TeamType != TeamType && character.IsValid;


    public int UpdateTarget() //Enemy �� ĳ���Ϳ� ���� ������ �ִ� Enemy �ε����� ������Ʈ
    {
        int targetIndex = -1;
        float dist = float.MaxValue;

        List<Character> characters = GameManager.Instance.Characters;
        for (int i = 0; i < characters.Count; i++)
        {
            if (IsEnemy(characters[i])) //���� ���� �ٸ���, ��ȿ�� Ÿ���� ���
            {
                float distance = GetDistance(transform.position, characters[i].transform.position);
                if (distance < dist) { 
                    dist = distance;
                    targetIndex = i;
                }
            }
        }
        return targetIndex;
    }

    public void Move()
    {
        // ��ǥ ��ġ ����
        base.target = UpdateTarget();

        if ( base.target == -1)
        {
            isMoving = false;
            GameManager.Instance.StartGameOver(TeamType);
            return;
        }

        GameManager.Instance.GridManager.GetTileAtPosition(transform.position).IsWalkable = true; //���� Cell�� isWalkable Ȱ��ȭ 
        var target = PathFinding.FindPath(base.target);
        if(target != null)
        {
            targetPosition = target.transform.position;
            GameManager.Instance.CharacterNextCell.Add(PathFinding.FindPath(base.target)); //GameManager�� ���� ���� ���� ���� 
            SpriteFlip(targetPosition);
            StartMoving();
        }
    }

    private void StartMoving()
    {
        if (!isMoving)
        {
            isMoving = true;
            CharacterAnimator.SetFloat("isMove", Speed);
        }
    }

    private void Moving()
    {
        if (!isMoving) return;

        Vector2 moveVec = Vector2.MoveTowards(transform.position, targetPosition, Speed * Time.deltaTime);
        transform.position = new Vector3(moveVec.x, moveVec.y, ZValue);

        if (HasReachedTargetPosition())
        {
            StopMoving();
        }

    }

    private bool HasReachedTargetPosition() // Ÿ�� ��ġ�� �����ߴ��� Ȯ��
    {
        Vector2 delta = targetPosition - transform.position;
        return Mathf.Abs(delta.x) + Mathf.Abs(delta.y) < 0.01f;
    }

    private void StopMoving()
    {
        isMoving = false;
        CharacterAnimator.SetFloat("isMove", 0);
        var cell = GameManager.Instance.GridManager.GetTileAtPosition(transform.position);
        cell.IsWalkable = false;
        transform.position = cell.transform.position + new Vector3(0, 0, ZValue);
    }

    public float GetDistance(Vector2 start, Vector2 target)
    {
        Vector2 vec = GameManager.Instance.NormalizePosition(start);
        Vector2 targetPos = GameManager.Instance.NormalizePosition(target);
        return Mathf.Max(Mathf.Abs(vec.x - targetPos.x), Mathf.Abs(vec.y - targetPos.y));
    }

    public void Hit(float damage)
    {
        if (HP > 0)
        {
            HP = HP - damage < 0 ? 0 : HP - damage;
            CheckDanagerState();
            UpdateHealthBar(HP, MaxHP);
            if (HP <= 0) Die();
        }
    }

    private void UpdateHealthBar(float current, float max)
    {
        _healthBar.value = current / max;
    }

    private void CheckDanagerState() //ĳ���Ͱ� ���� �������� Ȯ��
    {
        if (TeamType == TeamType.ENEMY) return;

        if (HP <= MaxHP * DANGERRATE)
        {
           GameManager.Instance.UIManager.FindPanelCell(Icon)?.ChangePanelColor();
        }
    }

    public void UpdateIconAmount() //ĳ���� ����â�� ���� ������ ������
    {
        if (TeamType == TeamType.ENEMY) return;

        var panelIcon = GameManager.Instance.UIManager.FindPanelCell(this.Icon)?.IconImg;
        GameManager.Instance.UIManager.UpdateFillIcon(isMoving,panelIcon);
    }

    public override void Die()
    {
        if (TeamType == TeamType.KNIGHT) GameManager.Instance.RemovePanelCell(this);
        base.Die();
    }


    public abstract void ThinkAction();
    public abstract void Attack();

}


public enum Type
{
    MELEE = 0,
    MELEEAREA = 1,
    RANGED = 2
}

public enum TeamType
{
    KNIGHT = 0,
    ENEMY
}
