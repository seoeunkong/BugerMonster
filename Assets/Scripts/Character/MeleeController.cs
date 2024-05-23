using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class MeleeController : MeleeAreaController
{
    private Character _detect = null;
    public override void ThinkAction()
    {
        _detect = DetectClosestEnemy();
        if (_detect != null)
        {
            Attack();
        }
        else
        {
            Move();
        }
        UpdateIconAmount();
    }

    public override void Attack()
    {
        if(_detect != null)
        {
            CharacterAnimator.SetTrigger("onAttack");
            SpriteFlip(_detect.transform.position);
            _detect.Hit(AttackPower);
            _detect = null;

        }
    }

    private Character DetectClosestEnemy()
    {
        List<Character> enemies = DetectEnemiesAround();
        return GetClosestEnemy(enemies);
    }


    private Character GetClosestEnemy(List<Character> enemies)
    {
        Character closestEnemy = null;
        float closestDistance = float.MaxValue;

        foreach (Character enemy in enemies)
        {
            float distance = GetDistance(transform.position, enemy.transform.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy;
            }
        }

        return closestEnemy;
    }

}
