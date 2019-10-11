using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public PlayerUnit target;
    public override void FigureOutMove()
    {
        List<PlayerUnit> players = BattleManager.instance.GivePlayers();
        currentMove = moves[Random.Range(0, moves.Count)];
        target = players[Random.Range(0, players.Count)];
        BattleManager.instance.UnitReady(this);
    }

    public override void Act()
    {
        PlayerUnit temp = target;
        target = null;
        if (currentMove.type == MoveType.DAMAGE)
        {
            temp.Damage(currentMove.damage);
        }
    }

    public override void AnotherUnitDied(Unit unit)
    {
        if (unit == target)
            BattleManager.instance.enemyController.NeedNewTarget(this, currentMove);
    }
}
