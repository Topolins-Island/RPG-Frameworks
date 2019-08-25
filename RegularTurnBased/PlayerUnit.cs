﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Introduced in episode 2, after I realize I need to have each unit choose their own thing
public class PlayerUnit : Unit
{
    [HideInInspector]
    public PlayerUnitController controller;

    public List<Ability> abilities;
    Ability currentAbility;

    public override void Act()
    {
        controller.DisplayAbilities(this, abilities);
        //print(name + ": can attack " + myTarget.name);
        //controller.PaintTargets(this, myTarget);
    }

    public void AbilityChosen(Ability ability)
    {
        currentAbility = ability;
        Unit myTarget = FindTarget(currentAbility.myTarget);
        controller.PaintTargets(this, myTarget);
    }

    public void PlayerSelected(Unit target)
    {
        //target.Damaged(this.damage);
        DoAbility(target);
        BattleManager.instance.Continue();
    }

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

//This is the start of abilities...
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

public class PlayerUnitEP02 : Unit
{
    public Targets myTarget;
    public string write;

    public override void Act()
    {
        Unit myTarget = FindTarget();
        myTarget.Damaged(this.damage);
        print(name + ": attacks " + myTarget.name);
    }

    Unit FindTarget()
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

public class PlayerUnitEp03 : Unit
{
    [HideInInspector]
    public PlayerUnitController controller;
    
    public string abilityName;
    public Targets myTarget;

    public override void Act()
    {
        Unit myTarget = FindTarget();
        //print(name + ": can attack " + myTarget.name);
        //controller.PaintTargets(this, myTarget);
    }

    public void PlayerSelected(Unit target)
    {
        target.Damaged(this.damage);
        BattleManager.instance.Continue();
    }

    Unit FindTarget()
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