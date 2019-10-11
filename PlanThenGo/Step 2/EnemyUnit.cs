using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyUnit : Unit
{
    public override void Act()
    {
        List<PlayerUnit> players = BattleManager.instance.GivePlayers();
        print(gameObject.name + " attacks " + players[Random.Range(0, players.Count)].name);
    }
}
