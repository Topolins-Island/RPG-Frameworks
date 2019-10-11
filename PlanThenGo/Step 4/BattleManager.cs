using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public PlayerUnitController playerController;
    public EnemyUnitController enemyController;

    public List<Unit> units;
    public List<Unit> line;
    Unit current;

    private void Start()
    {
        instance = this;
        playerController.BattleStarted();
        StartRound();
    }
    
    void StartRound()
    {
        line = new List<Unit>();
        for (int i = 0; i < units.Count; i++)
        {
            units[i].FigureOutMove();
        }
    }

    public void UnitReady(Unit me)
    {
        line.Add(me);
        if(line.Count == units.Count)
        {
            StartCoroutine(LineProcessing());
        }
    }

    IEnumerator LineProcessing()
    {
        SortUnits();

        while (line.Count > 0)
        {
            current = line[0];
            line.RemoveAt(0);
            current.Act();
            yield return new WaitForSeconds(0.5f);
        }

        if (BattleOver())
        {
            //BattleEnding();
            yield break;
        }
        else
            StartRound();
    }

    void SortUnits()
    {
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

    bool BattleOver()
    {
        bool over = true;

        bool enemiesLeft = false;
        bool playersLeft = false;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].gameObject.GetComponent<EnemyUnit>() != null)
                enemiesLeft = true;
            if (units[i].gameObject.GetComponent<PlayerUnit>() != null)
                playersLeft = true;
        }

        if (enemiesLeft && playersLeft)
            over = false;

        return over;
    }

    public void UnitDied(Unit me)
    {
        units.Remove(me);
        if (line.Contains(me))
            line.Remove(me);

        for (int i = 0; i < units.Count; i++)
        {
            units[i].AnotherUnitDied(me);
        }
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
