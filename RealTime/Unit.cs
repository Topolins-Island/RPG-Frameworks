using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Unit : BattleActor
{
    public Image actionBar;
    public Image hpBar;

    public float maxHP;
    float currentHP;
    public int damage;

    [Header("Number of seconds to fill up the action bar")]
    public float speed;
    public float realSpeed;

    private void Start()
    {
        actionBar.fillAmount = 0;
        realSpeed = 1 / speed;

        BattleManager.instance.units.Add(this);
        StartCoroutine(Going());
    }

    public IEnumerator Going()
    {
        while(actionBar.fillAmount < 1)
        {
            actionBar.fillAmount += realSpeed * Time.deltaTime;
            yield return null;
        }
        
        if(actionBar.fillAmount >= 1)
        {
            ActionBarFilled();
        }
    }

    public virtual void ActionBarFilled()
    {
        BattleManager.instance.UnitReady(this);
    }

    public override void Act()
    {
        print(gameObject.name);
        Done();
    }

    public void Damaged(int dam)
    {
        currentHP += dam;

        hpBar.fillAmount = currentHP / maxHP;

        if (currentHP >= maxHP)
        {
            print(gameObject.name + " has died");
            BattleManager.instance.UnitDied(this);
            Destroy(this.gameObject);
        }
    }

    public void Done()
    {
        BattleManager.instance.Continue();
        actionBar.fillAmount = 0;
        StartCoroutine(Going());
    }

    public void Stop()
    {
        StopAllCoroutines();
    }
}
