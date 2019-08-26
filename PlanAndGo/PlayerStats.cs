using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Monster", menuName = "Monster/Player")]
public class PlayerStats : ScriptableObject
{
    public Sprite sprite;
    public Sprite roleSprite;
    public MonsterStrains lowerStrain;
    public MonsterStrains higherStrain;

    public BattlerStats stats;
    public List<Ability> myAbility;

    [TextArea]
    public string descriptionOfMonster;
}

[System.Serializable]
public class Ability
{
    public MoveType moveType;

    public GameObject buff;
    public BuffTarget buffTarget;

    public int abilityValue;

    public string abilityMessage;

    public Ability(MoveType moveType, GameObject buff, BuffTarget buffTarget, int abilityValue, string abilityMessage)
    {
        this.moveType = moveType;
        this.buff = buff;
        this.buffTarget = buffTarget;
        this.abilityValue = abilityValue;
        this.abilityMessage = abilityMessage;
    }
}

public enum MoveType
{
    Regular,
    Healing,
    None,
    HealMyself,
    Block,
    Guard,
    HitAll,
    Stun,
    Double_Hit,
    IgnoreDef,
    SpawnAds,
    BuffThenAttack
}

public enum BuffTarget
{
    SELF,
    TARGET,
    SPAWN
}