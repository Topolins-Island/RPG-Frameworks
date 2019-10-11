using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUnit : Unit
{
    public override void Act()
    {
        List<EnemyUnit> enemies = BattleManager.instance.GiveEnemies();
        print(gameObject.name + " attacks " + enemies[Random.Range(0, enemies.Count)].name);
    }
}
