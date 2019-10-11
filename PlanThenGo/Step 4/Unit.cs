using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : MonoBehaviour
{
    public Image hpBar;

    public float maxHP;
    public float currentHP;
    public float speed = 0;
    public List<Move> moves;
    public Move currentMove;

    private void Start()
    {
        currentHP = maxHP;
    }

    public virtual void FigureOutMove()
    {
        print(gameObject.name);
    }

    public virtual void Act()
    {
        print(gameObject.name);
    }

    public void Damage(int _dam)
    {
        currentHP -= _dam;

        if (currentHP <= 0)
        {
            BattleManager.instance.UnitDied(this);
            Destroy(this.gameObject);
        }

        RefreshHPBar();
    }

    void RefreshHPBar()
    {
        hpBar.fillAmount = currentHP / maxHP;
    }

    public virtual void AnotherUnitDied(Unit unit)
    {
        print("i need to ask my controller for a new target!");
    }
}

[System.Serializable]
public class Move
{
    public MoveType type;
    public int damage;
}

public enum MoveType
{
    DAMAGE
}
