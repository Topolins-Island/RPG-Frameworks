using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Battler : MonoBehaviour
{
    public float currentHP;
    public int damage;
    public AttackDistance attackDistance;
    public int speed;
    public BattlerAnimator animator;

    public void Attacked(int _dam)
    {
        currentHP -= _dam;

        if(currentHP <= 0)
        {
            BattleManager.instance.UnitDied(this);
            Destroy(this.gameObject);
        }
    }
}

[System.Serializable]
public class AttackDistance
{
    public int outer;
    public int inner;
}