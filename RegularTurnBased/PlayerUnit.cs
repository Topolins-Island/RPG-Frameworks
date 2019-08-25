using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Introduced in episode 2, after I realize I need to have each unit choose their own thing
public class PlayerUnit : Unit
{
    [HideInInspector]
    public PlayerUnitController controller;

    public List<Ability> abilities;
    Ability currentAbility;

    //Instead of doing whatever, the units ask their controller to show their abilities
    public override void Act()
    {
        controller.DisplayAbilities(this, abilities);
    }

    //This is a function called by the unit's controller, telling it what ability the player has chosen
    public void AbilityChosen(Ability ability)
    {
        currentAbility = ability;
        Unit myTarget = FindTarget(currentAbility.myTarget);
        controller.PaintTargets(this, myTarget);
    }

    //This function is where the controller tells the unit what unit the player selected
    public void PlayerSelected(Unit target)
    {
        DoAbility(target);
        BattleManager.instance.Continue();
    }

    //This funciton is a where the chosen ability's data is parsed and what is required is done
    void DoAbility(Unit target)
    {
        if (currentAbility.type == AbilityType.ATTACK)
            target.Damaged(this.damage);
        if (currentAbility.type == AbilityType.HEAL)
            target.Healed(this.damage);
    }

    Unit FindTarget(Targets myTarget)
    {
        Unit target = null;
        if (myTarget == Targets.MYSELF)
            target = this;
        if (myTarget == Targets.MY_LEADER)
            target = BattleManager.instance.playerLeader;
        if (myTarget == Targets.MY_SUPPORT)
            target = BattleManager.instance.playerSupport;
        if (myTarget == Targets.ENEMY_LEADER)
            target = BattleManager.instance.enemyLeader;
        if (myTarget == Targets.ENEMY_SUPPORT)
            target = BattleManager.instance.enemySupport;

        return target;
    }
}

[System.Serializable]
public class Ability
{
    public string abilityName;
    public AbilityType type;
    public Targets myTarget;

    public Ability(string abilityName, AbilityType type, Targets myTarget)
    {
        this.abilityName = abilityName;
        this.type = type;
        this.myTarget = myTarget;
    }
}

public enum AbilityType
{
    ATTACK,
    HEAL
}
