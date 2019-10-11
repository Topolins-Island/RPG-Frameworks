using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public List<Unit> units;
    public List<Unit> line;
    Unit current;

    private void Start()
    {
        instance = this;
        StartCoroutine(LineProcessing());
    }

    IEnumerator LineProcessing()
    {
        SortUnits();

        while (line.Count > 0)
        {
            current = line[0];
            line.RemoveAt(0);
            current.Act();
            yield return null;
        }
    }

    void SortUnits()
    {
        line = new List<Unit>();
        line.AddRange(units);

        bool swapped = false;
        do
        {
            swapped = false;
            for (int i = 0; i < line.Count - 1; i++)
            {
                if (line[i].speed < line[i + 1].speed)
                {
                    Unit temp = line[i];
                    line[i] = line[i + 1];
                    line[i + 1] = temp;
                    swapped = true;
                }
            }
        } while (swapped);

    }

    public List<EnemyUnit> GiveEnemies()
    {
        List<EnemyUnit> enemies = new List<EnemyUnit>();
        foreach(Unit u in units)
        {
            if (u.gameObject.GetComponent<EnemyUnit>() != null)
                enemies.Add(u.gameObject.GetComponent<EnemyUnit>());
        }
        return enemies;
    }

    public List<PlayerUnit> GivePlayers()
    {
        List<PlayerUnit> players = new List<PlayerUnit>();
        foreach (Unit u in units)
        {
            if (u.gameObject.GetComponent<PlayerUnit>() != null)
                players.Add(u.gameObject.GetComponent<PlayerUnit>());
        }
        return players;
    }
}
