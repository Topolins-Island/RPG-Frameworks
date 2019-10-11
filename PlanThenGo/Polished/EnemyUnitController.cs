using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnitController : MonoBehaviour
{
    public void NeedNewTarget(EnemyUnit me, Move move)
    {
        if (move.type == MoveType.DAMAGE)
        {
            List<PlayerUnit> players = BattleManager.instance.GivePlayers();
            me.target = players[Random.Range(0, players.Count)];
        }
    }
}
