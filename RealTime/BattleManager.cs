using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;
    [HideInInspector]
    public PlayerUnitController playerUnitController;

    public Transform enemyHolder;
    public Transform playerHolder;

    public List<Unit> units;
    public List<BattleActor> line;

    BattleActor current;

    private void Awake()
    {
        instance = this;
        playerUnitController = this.gameObject.GetComponent<PlayerUnitController>();

        StartCoroutine(LineProcessing());
    }

    public void UnitReady(BattleActor me)
    {
        line.Add(me);
    }

    public void UnitDied(Unit me)
    {
        units.Remove(me);
        if (line.Contains(me))
            line.Remove(me);
    }

    bool currentGoing = false;
    IEnumerator LineProcessing()
    {
        //this should be replaced by checking for unit death
        while (!IsBattleOver())
        {
            while (line.Count == 0)
            {
                yield return null;
            }

            currentGoing = true;

            current = line[0];
            line.RemoveAt(0);
            current.Act();

            while (currentGoing)
                yield return null;
        }
        
        BattleEnded();
    }

    bool IsBattleOver()
    {
        bool bep = false;

        bool enemiesAlive = false;
        bool playersAlive = false;

        foreach(Unit u in units)
        {
            if (u.transform.parent == playerHolder)
                playersAlive = true;
            if (u.transform.parent == enemyHolder)
                enemiesAlive = true;
        }

        if (playersAlive != enemiesAlive)
            bep = true;

        return bep;
    }

    public void Continue()
    {
        currentGoing = false;
    }

    public List<Unit> GiveEnemies()
    {
        List<Unit> enems = new List<Unit>();
        foreach (Unit u in units)
        {
            if (u.transform.parent == enemyHolder)
                enems.Add(u);
        }
        return enems;
    }

    public List<Unit> GivePlayers()
    {
        List<Unit> playas = new List<Unit>();
        foreach (Unit u in units)
        {
            if (u.transform.parent == playerHolder)
                playas.Add(u);
        }
        return playas;
    }

    void BattleEnded()
    {
        foreach (Unit u in units)
            u.Stop();

        if (enemyHolder.childCount == 0)
            print("player won");
        if (playerHolder.childCount == 0)
            print("enemy won");
    }
}
