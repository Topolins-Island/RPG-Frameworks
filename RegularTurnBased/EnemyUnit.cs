using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public Targets myTarget;
    public string write;

    public override void Act()
    {
        Unit myTarget = FindTarget();
        print(name + ": attacks " + myTarget.name);
        myTarget.Damaged(this.damage);
        BattleManager.instance.Continue();
    }

    Unit FindTarget()
    {
        Unit target = null;
        if (myTarget == Targets.MYSELF)
            target = this;
        if (myTarget == Targets.MY_LEADER)
            target = BattleManager.instance.enemyLeader;
        if (myTarget == Targets.MY_SUPPORT)
            target = BattleManager.instance.enemySupport;
        if (myTarget == Targets.ENEMY_LEADER)
            target = BattleManager.instance.playerLeader;
        if (myTarget == Targets.ENEMY_SUPPORT)
            target = BattleManager.instance.playerSupport;

        return target;
    }
}
