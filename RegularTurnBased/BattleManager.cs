﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public PlayerUnit playerLeader;
    public PlayerUnit playerSupport;
    public EnemyUnit enemyLeader;
    public EnemyUnit enemySupport;
    public List<EnemyUnit> enemies;

    public List<Unit> battlers;

    //A temporary start function
    private void Start()
    {
        instance = this;
        SortUnits();
        PrepUnits();
        StartCoroutine(LineProcessing());
    }

    //A coroutine that handles the turn-based portion of the battle mechanics
    IEnumerator LineProcessing()
    {
        //Before each loop we check if the battle is over
        while (!BattleOver())
        {
            //We fill the temporary list if it's empty,
            if (battlers.Count == 0)
                SortUnits();

            //And pop the first one in line.
            Unit current = battlers[0];
            battlers.RemoveAt(0);
            
            //We set the controlling boolean,
            currentDone = false;

            //Then tell the unit to go,
            current.Act();

            //And wait for it to be done.
            while (!currentDone)
                yield return null;

            yield return null;
        }
        
        //Here is where the end of battle code would begin.
    }

    //A controlling bool, allowing the units to tell the manager when they are done by calling this function
    bool currentDone = false;
    public void Continue()
    {
        currentDone = true;
    }

    //This function returns true if the battle is over
    bool BattleOver()
    {
        bool over = false;

        if (playerLeader == null || enemyLeader == null)
            over = true;

        return over;
    }

    //One of the most important parts of turn based combat is to ensure unit death is handled well.
    //Luckily in this format of battles we have a pretty easy time doing so.
    public void UnitDied(Unit theDeceased)
    {
        //We check who it is
        //In this system's logic if the player leader dies and he has no support unit,
        //it means the player lost.
        if (theDeceased == playerLeader)
        {
            if (playerSupport != null)
            {
                playerLeader = playerSupport;
                playerSupport = null;
            }
            else
                print("enemies won!");
        }
        if (theDeceased == playerSupport)
            playerSupport = null;

        //The same is true for enemies.
        if (theDeceased == enemyLeader)
        {
            if (enemySupport != null)
            {
                enemyLeader = enemySupport;
                enemySupport = null;
            }
            else
                print("player's won!");
        }

        if (theDeceased == enemySupport)
            enemySupport = null;

        //If the temporary list includes the unit that died, we just remove it here.
        battlers.Remove(theDeceased);
    }

    //This is a method to set up the battler objects
    void PrepUnits()
    {
        if (playerLeader != null)
            playerLeader.controller = GetComponent<PlayerUnitController>();
        if (playerSupport != null)
            playerSupport.controller = GetComponent<PlayerUnitController>();
    }

    //This is a simple bubble sort by each unit's speed value.
    void SortUnits()
    {
        battlers = new List<Unit>();
        if (playerLeader != null)
            battlers.Add(playerLeader);
        if (playerSupport != null)
            battlers.Add(playerSupport);
        if (enemyLeader != null)
            battlers.Add(enemyLeader);
        if(enemySupport != null)
            battlers.Add(enemySupport);

        bool swapped = false;
        do
        {
            swapped = false;
            for (int i = 0; i < battlers.Count - 1; i++)
            {
                if (battlers[i].speed < battlers[i + 1].speed)
                {
                    Unit temp = battlers[i];
                    battlers[i] = battlers[i + 1];
                    battlers[i + 1] = temp;
                    swapped = true;
                }
            }
        } while (swapped);
    }
}
