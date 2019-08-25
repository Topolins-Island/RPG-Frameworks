using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public override void Act()
    {
        List<Unit> possibles = BattleManager.instance.GivePlayers();
        Unit target = possibles[Random.Range(0, possibles.Count - 1)];
        AttackTarget(target);
    }

    void AttackTarget(Unit target)
    {
        target.Damaged(this.damage);
        Done();
    }
}
