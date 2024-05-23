using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class RangedController : Character
{
    private Character _detect = null;
    private Bullet _bullet = null;

    public override void ThinkAction()
    {
        _detect = DetectEnemy();
        if (_detect != null)
        {
            if (_bullet == null)
            {
                BulletSetting();
            }
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
        if (_detect != null)
        {
            _bullet.gameObject.SetActive(true);
            _bullet.Init(this, _detect);

            CharacterAnimator.SetTrigger("onAttack");
            SpriteFlip(_detect.transform.position);
            _detect.Hit(AttackPower);
            _detect = null;
        }
    }

    private Character DetectEnemy()
    {
        int targetIndex = UpdateTarget();
        if(targetIndex != -1)
        {
            float distance = GetDistance(transform.position, GameManager.Instance.Characters[targetIndex].transform.position);
            if (distance > 0 && distance < 3) return GameManager.Instance.Characters[targetIndex];
        } 
        return null;
    }

    private void BulletSetting()
    {
        _bullet = this.GetComponentInChildren<Bullet>();
        _bullet.SetAttackPower(AttackPower);
        _bullet.gameObject.SetActive(false);
    }

}
