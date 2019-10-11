using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    public EnemyUnit target;

    public override void FigureOutMove()
    {
        BattleManager.instance.playerController.ShowMyStuff(this);
    }

    public void MoveChosen(Move move, EnemyUnit _target)
    {
        currentMove = move;
        target = _target;
        BattleManager.instance.UnitReady(this);
    }

    public override void Act()
    {
        EnemyUnit temp = target;
        target = null;

        if(currentMove.type == MoveType.DAMAGE)
        {
            temp.Damage(currentMove.damage);
        }
    }
}