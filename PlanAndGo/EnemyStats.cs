using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Enemy Monster", menuName = "Monster/Enemy")]
public class EnemyStats : ScriptableObject
{
    public Sprite sprite;

    public BattlerStats stats;

    public Equipment myEquip = Equipment.EMPTY;
    public List<Ability> myAbility;
}
