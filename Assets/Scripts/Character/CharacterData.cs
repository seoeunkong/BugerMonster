using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Character Data", menuName = "Scriptable Object/Character Data", order = int.MaxValue)]
public class CharacterData : ScriptableObject
{
    public float MaxHp { get { return _maxHp; } }
    public float AttackPower { get { return _attackPower; } }
    public float Speed { get { return _speed; } }
    public Type JobType { get { return _type; } }
    public RuntimeAnimatorController Ani { get { return _anim; } }
    public Sprite IconSprite { get { return _iconSprite; } }

    [SerializeField] private float _attackPower;
    [SerializeField] private float _maxHp;
    [SerializeField] private float _speed;
    [SerializeField] private Type _type;
    [SerializeField] private RuntimeAnimatorController _anim;
    [SerializeField] private Sprite _iconSprite;
}
