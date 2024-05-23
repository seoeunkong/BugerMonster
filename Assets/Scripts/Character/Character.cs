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

    public override void Create(CharacterData data, TeamType team) // 캐릭터 생성 
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


    public int UpdateTarget() //Enemy 중 캐릭터와 가장 가까이 있는 Enemy 인덱스로 업데이트
    {
        int targetIndex = -1;
        float dist = float.MaxValue;

        List<Character> characters = GameManager.Instance.Characters;
        for (int i = 0; i < characters.Count; i++)
        {
            if (IsEnemy(characters[i])) //속한 팀이 다르고, 유효한 타겟인 경우
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
        // 목표 위치 설정
        base.target = UpdateTarget();

        if ( base.target == -1)
        {
            isMoving = false;
            GameManager.Instance.StartGameOver(TeamType);
            return;
        }

        GameManager.Instance.GridManager.GetTileAtPosition(transform.position).IsWalkable = true; //이전 Cell의 isWalkable 활성화 
        var target = PathFinding.FindPath(base.target);
        if(target != null)
        {
            targetPosition = target.transform.position;
            GameManager.Instance.CharacterNextCell.Add(PathFinding.FindPath(base.target)); //GameManager에 다음 도착 지점 공유 
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

    private bool HasReachedTargetPosition() // 타겟 위치에 도달했는지 확인
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

    private void CheckDanagerState() //캐릭터가 위험 상태인지 확인
    {
        if (TeamType == TeamType.ENEMY) return;

        if (HP <= MaxHP * DANGERRATE)
        {
           GameManager.Instance.UIManager.FindPanelCell(Icon)?.ChangePanelColor();
        }
    }

    public void UpdateIconAmount() //캐릭터 정보창의 공격 아이콘 게이지
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
