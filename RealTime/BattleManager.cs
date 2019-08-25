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

    //A temporary starting function
    private void Awake()
    {
        instance = this;
        playerUnitController = this.gameObject.GetComponent<PlayerUnitController>();

        StartCoroutine(LineProcessing());
    }

    //An actor acts itself to the line
    public void UnitReady(BattleActor me)
    {
        line.Add(me);
    }

    //A unit dies and it's stuff is removed here
    //Another version needs to be made for actors
    public void UnitDied(Unit me)
    {
        units.Remove(me);
        if (line.Contains(me))
            line.Remove(me);
    }
    
    //A coroutine that handles the real time portion of the battle mechanics
    IEnumerator LineProcessing()
    {
        //Before each loop we check if the battle is over
        while (!IsBattleOver())
        {
            //If the line is empty, we wait until someone joins
            while (line.Count == 0)
            {
                yield return null;
            }

            //Set the controlling boolean to true
            currentGoing = true;
            
            //Pop the first one in line
            current = line[0];
            line.RemoveAt(0);
            //Tell the popped unit to go
            current.Act();

            //Wait for the current to be done
            while (currentGoing)
                yield return null;
        }
        
        //Once we escape the above loop, the battle is over
        BattleEnded();
    }

    //This function returns true if the battle is over
    bool IsBattleOver()
    {
        bool over = false;

        bool enemiesAlive = false;
        bool playersAlive = false;

        //We go through the units list in order to determine who is still alive.
        //We determine which side their own based on what their parent object is.
        foreach(Unit u in units)
        {
            if (u.transform.parent == playerHolder)
                playersAlive = true;
            if (u.transform.parent == enemyHolder)
                enemiesAlive = true;
        }

        if (playersAlive != enemiesAlive)
            over = true;

        return over;
    }
    
    //A controlling bool, allowing the units to tell the manager when they are done by calling this function
    bool currentGoing = false;
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

    //This is a simple method to check who won the battle
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
