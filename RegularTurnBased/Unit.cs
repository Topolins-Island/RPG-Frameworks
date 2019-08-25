using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public Image hpBarFiller;

    [Header("Stats")]
    public float speed;
    public int damage;
    public int maxHP;
    public float currentHP;

    private void Awake()
    {
        //speed = Random.Range(0f, 7f);
        currentHP = maxHP;
    }

    //This is the function that the battlemanager calls and each type of unit override, these being EnemyUnit and PlayerUnit
    public virtual void Act()
    {
        print("Base Act function has been called");
    }

    public void Damaged(int _damage)
    {
        currentHP -= _damage;
        hpBarFiller.fillAmount = currentHP / maxHP;

        if (currentHP <= 0)
        {
            print(gameObject.name + " is dead!");
            BattleManager.instance.UnitDied(this);
            Destroy(this.gameObject);
        }
    }

    public void Healed(int _amount)
    {
        currentHP += _amount;

        if (currentHP > maxHP)
            currentHP = maxHP;

        hpBarFiller.fillAmount = currentHP / maxHP;

    }
}

public enum Targets
{
    MYSELF,
    MY_LEADER,
    MY_SUPPORT,
    ENEMY_LEADER,
    ENEMY_SUPPORT
}
