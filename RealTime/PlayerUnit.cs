using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnit : Unit
{
    public Actions actions;
    PlayerAction currentAction;

    //Instead of immediately going on action bar filled,
    //WE need the player to choose what they are doing and then
    //go into the line. To do this we need to store our chosen
    //action and do it in Act();
    public override void ActionBarFilled()
    {
        BattleManager.instance.playerUnitController.UnitReady(this);
    }

    public void HeresAction(PlayerAction playerAction)
    {
        currentAction = playerAction;
        BattleManager.instance.UnitReady(this);
    }

    public override void Act()
    {
        //We need to check if the unit's target is still alive!
        //Might need to get permission from the player unit controller
        Go(currentAction.action, currentAction.target);
    }

    void Go(Action action, Unit target)
    {
        if(action == Action.SCARE)
        {
            AttackTarget(target);
        }

        //I'm done with my turn, so...
        Done();
    }

    void AttackTarget(Unit target)
    {
        target.Damaged(this.damage);
    }
}

[System.Serializable]
public class Actions
{
    public GameObject holder;
    public Button attack;
    public Button ability;
    public Button run;
}

public class PlayerAction
{
    public Action action;
    public Unit target;

    public PlayerAction(Action action, Unit target)
    {
        this.action = action;
        this.target = target;
    }
}

public enum Action
{
    SCARE,
    ABILITY,
    RUN
}
