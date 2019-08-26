using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class Battler : MonoBehaviour
{
    public Sprite mySprite;
    public Equipment myEquip;
    public int maxHP;
    public float currentHP;
    public IntRange defense;
    public IntRange damage;
    public IntRange accuracy = new IntRange(100, 100);
    public IntRange evasion = new IntRange(0, 0);
    public float speed;

    public Image hpBar;

    [HideInInspector]
    public UnityEvent myMove;
    [HideInInspector]
    public UnityEvent ability;
    [HideInInspector]
    public UnityEvent tookDamage;

    [HideInInspector]
    public int numberOfBlocks = 0;
    [HideInInspector]
    public bool stunned = false;
    [HideInInspector]
    public List<Battler> guards = new List<Battler>();

    //Transform targetHolder;

    List<Battler> target;
    GameObject currentBuff;

    //[HideInInspector]
    public List<Ability> myAbilities;
    public int currentAbility = 0;

    Vector3 attackDirection;

    public IconBar iconBar;

    private void Start()
    {
        target = new List<Battler>();
        myMove.AddListener(RefreshTargets);
    }

    bool disableSkipIfFirst;
    private void Update()
    {
        if (disableSkipIfFirst && this.transform.GetSiblingIndex() == 0 && myEquip == Equipment.SKIP)
            myEquip = Equipment.EMPTY;
    }

    public void SetStats(Transform oppositePartyHolder)
    {

    }

    public void SetStats(Transform oppositePartyHolder, EnemyStats _myStats, Sprite _sprite)
    {
        maxHP = _myStats.stats.maxHP;
        currentHP = maxHP;
        speed = _myStats.stats.speed;

        defense = new IntRange(_myStats.stats.defense.m_Min, _myStats.stats.defense.m_Max);
        damage = new IntRange(_myStats.stats.damage.m_Min, _myStats.stats.damage.m_Max);
        accuracy = new IntRange(_myStats.stats.accuracy.m_Min, _myStats.stats.accuracy.m_Max);
        evasion = new IntRange(_myStats.stats.evasion.m_Min, _myStats.stats.evasion.m_Max);

        attackDirection = Vector3.left;
        disableSkipIfFirst = true;

        myAbilities = new List<Ability>();

        foreach (Ability ab in _myStats.myAbility)
        {
            myAbilities.Add(new Ability(ab.moveType, ab.buff, ab.buffTarget, ab.abilityValue, ab.abilityMessage));
        }

        myEquip = _myStats.myEquip;

        CheckEquips();

        DecideAbility(myAbilities[0]);
        myMove.AddListener(ability.Invoke);
        if (myAbilities.Count > 1)
        {
            myMove.AddListener(NextAbility);
        }

        //targetHolder = oppositePartyHolder;

        GetComponent<Image>().sprite = _sprite;

        RefreshHPBar();
    }

    public void SetStats(Transform oppositePartyHolder, PlayerStats _myStats, Sprite _sprite)
    {
        maxHP = _myStats.stats.maxHP;
        currentHP = maxHP;
        speed = _myStats.stats.speed;
        
        defense = new IntRange(_myStats.stats.defense.m_Min, _myStats.stats.defense.m_Max);
        damage = new IntRange(_myStats.stats.damage.m_Min, _myStats.stats.damage.m_Max);
        accuracy = new IntRange(_myStats.stats.accuracy.m_Min, _myStats.stats.accuracy.m_Max);
        evasion = new IntRange(_myStats.stats.evasion.m_Min, _myStats.stats.evasion.m_Max);

        attackDirection = Vector3.right;

        myAbilities = new List<Ability>();

        foreach(Ability ab in _myStats.myAbility)
        {
            myAbilities.Add(new Ability(ab.moveType, ab.buff, ab.buffTarget, ab.abilityValue, ab.abilityMessage));
        }

        CheckEquips();

        DecideAbility(myAbilities[0]);
        myMove.AddListener(ability.Invoke);
        if (myAbilities.Count > 1)
        {
            myMove.AddListener(NextAbility);
        }

        //targetHolder = oppositePartyHolder;

        GetComponent<Image>().sprite = _sprite;

        RefreshHPBar();
    }

    void RefreshTargets()
    {
        target = new List<Battler>();
    }

    bool missed;
    public void Damage(int dam, int _accuracy, bool ignoreDef)
    {
        if (currentHP <= 0)
            return;

        if (numberOfBlocks > 0)
        {
            numberOfBlocks--;
            BattleManager.instance.ShowText(this.transform, "BLOCKED", Color.white);
            return;
        }

        if ((_accuracy - evasion.Random) / 100f > Random.value)
        {
            if (!ignoreDef)
            {
                int defValue = defense.Random;
                if (dam - defValue > 0)
                {
                    int realDamage = dam - defValue;
                    currentHP -= realDamage;
                    tookDamage.Invoke();

                    BattleManager.instance.ShowNumbers(this.transform, realDamage, Color.red);

                    if (currentHP <= 0)
                        TestKill();
                    else
                        StartCoroutine(DamageFlash(Color.red));
                }
            }
            else
            {
                currentHP -= dam;
                tookDamage.Invoke();

                BattleManager.instance.ShowNumbers(this.transform, dam, Color.red);

                if (currentHP <= 0)
                    TestKill();
                else
                    StartCoroutine(DamageFlash(Color.red));
            }

            RefreshHPBar();
        }
        else
        {
            missed = true;
            BattleManager.instance.ShowText(this.transform, "MISSED", Color.white);
        }
    }

    public void DamageOverTimeDamage(int _damage)
    {
        if (currentHP <= 0)
            return;

        int dam = -_damage;

        currentHP -= dam;
        BattleManager.instance.ShowNumbers(this.transform, dam, Color.yellow);
        tookDamage.Invoke();

        if (currentHP <= 0)
            TestKill();
        else
            StartCoroutine(DamageFlash(Color.yellow));

        RefreshHPBar();
    }

    public void Heal(int amount)
    {
        if (currentHP <= 0)
            return;

        currentHP += amount;

        BattleManager.instance.ShowNumbers(this.transform, amount, Color.green);

        if (currentHP > maxHP)
            currentHP = maxHP;

        StartCoroutine(DamageFlash(Color.green));
        RefreshHPBar();
    }

    public void AttemptStun()
    {
        if (!missed)
        {
            //50% chance
            //if (Random.value > 0.5f)
                stunned = true;
            //else
                //print("Stun failed on " + name + ".");
        }

        missed = false;
    }

    void RefreshHPBar()
    {
        hpBar.fillAmount = currentHP / maxHP;
    }

    public void TestKill()
    {
        if (dying)
            return;
        //BattleManager.instance.SomeoneHasDied(this);
        myMove.RemoveAllListeners();
        StartCoroutine(Death());
        //Destroy(this.gameObject);
    }

    [HideInInspector]
    public bool animating;
    GameObject placeholder;
    public IEnumerator Animation(Vector3 direction)
    {
        animating = true;
        yield return null;

        MakePlaceholder();

        transform.SetParent(BattleManager.instance.transform.parent);
        //Move the player!

        direction = new Vector3(direction.x * transform.localScale.x, direction.y);
        Vector3 endPos = transform.position + (direction * 10);

        float timeElapsed = 0;
        float timeNeeded = BattleManager.instance.timeBetweenBattlers / 4;
        float finalTime = BattleManager.instance.timeBetweenBattlers - 0.075f;

        while (timeElapsed < timeNeeded)
        {
            transform.position = Vector3.MoveTowards(transform.position, endPos, Time.deltaTime * 100);
            timeElapsed += Time.deltaTime;
            yield return null;
        }
        while(timeElapsed < finalTime)
        {
            transform.position = Vector3.MoveTowards(transform.position, placeholder.transform.position, Time.deltaTime * 100);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        transform.SetParent(placeholder.transform.parent);
        this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());

        animating = false;
        Destroy(placeholder);
        placeholder = null;
    }

    void MakePlaceholder()
    {
        //Make the placeholder
        placeholder = new GameObject();
        placeholder.name = this.name + "'s placeholder";
        placeholder.transform.SetParent(this.transform.parent);
        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;
        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());
    }

    bool dying = false;
    public IEnumerator Death()
    {
        dying = true;
        BattleManager.instance.SomeoneHasDied(this);

        MakePlaceholder();

        this.transform.SetParent(BattleManager.instance.transform);
        float timePassed = 0;

        if (placeholder != null)
            Destroy(placeholder, BattleManager.instance.deathAnimLength - 0.1f);

        while (timePassed < BattleManager.instance.deathAnimLength - 0.05f)
        {
            transform.Rotate(Vector3.forward, 1080 * Time.deltaTime);
            transform.Translate(Vector3.up * 5, Space.World);
            timePassed += Time.deltaTime;
            yield return null;
        }
        Destroy(this.gameObject);
    }

    IEnumerator DamageFlash(Color flashColor)
    {
        Image i = GetComponent<Image>();

        float elapsedTime = 0f;
        float totalTime = 0.15f;
        while(elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            i.color = Color.Lerp(Color.white, flashColor, (elapsedTime / totalTime));
            yield return null;
        }

        elapsedTime = 0;
        while (elapsedTime < totalTime)
        {
            elapsedTime += Time.deltaTime;
            i.color = Color.Lerp(flashColor, Color.white, (elapsedTime / totalTime));
            yield return null;
        }

        RefreshHPBar();
    }

    void CheckEquips()
    {
        //print("checked equip: " + myEquip.ToString());
        if(myEquip == Equipment.ATKBOOST)
        {
            damage.Add(EquipStats.instance.damageBoostValue);
        }
        else if (myEquip == Equipment.DEFBOOST)
        {
            defense.Add(EquipStats.instance.defenseBoostValue);
        }
        else if (myEquip == Equipment.EVABOOST)
        {
            evasion.Add(EquipStats.instance.evasionBoostValue);
            accuracy.Add(EquipStats.instance.evasionBoostValue);
        }
        else if (myEquip == Equipment.CURSEONHIT)
        {
            List<Ability> applicables = new List<Ability>();
            foreach(Ability ab in myAbilities)
            {
                if (ab.buff != null)
                {
                    print("no way jose");
                    continue;
                }
                else
                    applicables.Add(ab);
            }
            foreach(Ability ab in applicables)
            {
                ab.buff = EquipStats.instance.curseOnHitCurse;
                ab.buffTarget = BuffTarget.TARGET;
            }
        }
    }

    void DecideAbility(Ability ab)
    {
        if (ab.moveType == MoveType.Regular)
        {
            ability.AddListener(Attack);
        }
        else if (ab.moveType == MoveType.Healing)
        {
            ability.AddListener(HealOthers);
        }
        else if (ab.moveType == MoveType.HealMyself)
        {
            ability.AddListener(HealMyself);
        }
        else if (ab.moveType == MoveType.Block)
        {
            ability.AddListener(Block);
        }
        else if (ab.moveType == MoveType.Guard)
        {
            ability.AddListener(Guard);
        }
        else if (ab.moveType == MoveType.HitAll)
        {
            ability.AddListener(HitAll);
        }
        else if (ab.moveType == MoveType.Stun)
        {
            ability.AddListener(Stun);
        }
        else if (ab.moveType == MoveType.Double_Hit)
        {
            ability.AddListener(DoubleHit);
        }
        else if (ab.moveType == MoveType.IgnoreDef)
        {
            ability.AddListener(IgnoreDef);
        }
        else if (ab.moveType == MoveType.SpawnAds)
        {
            ability.AddListener(SpawnAds);
        }
        else if (ab.moveType == MoveType.BuffThenAttack)
        {
            ability.AddListener(Attack);
        }

        if (ab.buff != null && ab.moveType != MoveType.BuffThenAttack)
        {
            currentBuff = ab.buff;
            if (ab.buffTarget == BuffTarget.SELF)
            {
                ability.AddListener(BuffSelf);
            }
            else if (ab.buffTarget == BuffTarget.TARGET)
            {
                ability.AddListener(BuffOther);
            }
        }

        //This is put here in order to ensure every turn has the regen
        if (myEquip == Equipment.REGEN)
        {
            ability.AddListener(delegate { Regen(EquipStats.instance.regenAmountValue); });
        }

        ability.AddListener(SayAbilityThing);
    }

    void NextAbility()
    {
        StartCoroutine(FindNextAbility());
    }

    void SayAbilityThing()
    {
        string message = "";

        string color = this.transform.parent == BattleManager.instance.playerPartyHolder ? "<color=green>" : "<color=red>";

        for (int i = 0; i < myAbilities[currentAbility].abilityMessage.Length; i++)
        {
            if (myAbilities[currentAbility].abilityMessage[i] == '!')
                message += color + this.name + "</color>";

            else if(myAbilities[currentAbility].abilityMessage[i] == '?')
            {
                string opposing = target[0].transform.parent == BattleManager.instance.playerPartyHolder ? "<color=green>" : "<color=red>";

                message += opposing + target[0].name + "</color>";

                if (target.Count > 1)
                {
                    for (int j = 1; j < target.Count; j++)
                    {
                        opposing = target[j].transform.parent == BattleManager.instance.playerPartyHolder ? "<color=green>" : "<color=red>";
                        if (j == target.Count - 1)
                            message += " and " + opposing + target[j].name + "</color>";
                        else
                            message += opposing + target[j].name + "</color>" + ", ";
                    }
                }
            }

            else
                message += myAbilities[currentAbility].abilityMessage[i];
        }
        
        BattleManager.instance.AbilityFired(message);
    }

    IEnumerator FindNextAbility()
    {
        yield return null;
        //Removing any old ability
        ability.RemoveAllListeners();

        //Getting the next one
        currentAbility++;
        if (currentAbility > myAbilities.Count - 1)
            currentAbility = 0;

        DecideAbility(myAbilities[currentAbility]);
    }

    public void YouWereStunned()
    {
        StartCoroutine(DamageFlash(Color.yellow));
        BattleManager.instance.ShowText(this.transform, "STUNNED", Color.white);
        iconBar.SpawnIcon(IconNeeded.STUN);
    }

    void Attack()
    {
        //FirstInTargetGroup().GetComponent<Battler>()
        target = BattleManager.instance.GiveMeTarget(this, myEquip, false);
        foreach(Battler b in target)
        {
            b.Damage(this.damage.Random, this.accuracy.Random, false);
        }
        StartCoroutine(Animation(attackDirection));
    }

    void HealOthers()
    {
        target = BattleManager.instance.GiveMeTarget(this, myEquip, true);
        foreach (Battler b in target)
        {
            b.Heal(this.damage.Random);
        }
        StartCoroutine(Animation(Vector3.up));
    }

    void HealMyself()
    {
        this.Heal(myAbilities[currentAbility].abilityValue);
        StartCoroutine(Animation(Vector3.up));
    }

    void BuffSelf()
    {
        if (currentBuff.GetComponent<Buff>() != null)
        {
            iconBar.SpawnIcon(IconNeeded.BUFF);
        }

        if (currentBuff.GetComponent<RegenBuff>() != null)
        {
            iconBar.SpawnIcon(IconNeeded.REGEN);
        }
        Instantiate(currentBuff, this.transform);
    }

    void BuffOther()
    {
        if(target.Count == 0)
            target = BattleManager.instance.GiveMeTarget(this, myEquip, true);

        foreach (Battler b in target)
        {
            BattleManager.instance.ShowParticle(b.transform, ParticleType.BUFF);
            Instantiate(currentBuff, b.transform);

            if (currentBuff.GetComponent<Buff>() != null)
            {
                if (currentBuff.GetComponent<Buff>().GoodBuff())
                    b.iconBar.SpawnIcon(IconNeeded.BUFF);
                else
                    b.iconBar.SpawnIcon(IconNeeded.DEBUFF);
            }

            if(currentBuff.GetComponent<RegenBuff> () != null)
            {
                if (currentBuff.GetComponent<RegenBuff>().hpRegenPerTurn > 0)
                    b.iconBar.SpawnIcon(IconNeeded.REGEN);
                else
                    b.iconBar.SpawnIcon(IconNeeded.DOT);
            }
        }

        if (myAbilities[currentAbility].moveType == MoveType.None)
            StartCoroutine(Animation(Vector3.up));
    }

    void Block()
    {
        BattleManager.instance.ShowParticle(this.transform, ParticleType.BUFF);

        numberOfBlocks += myAbilities[currentAbility].abilityValue;
        if(numberOfBlocks > myAbilities[currentAbility].abilityValue)
            numberOfBlocks = myAbilities[currentAbility].abilityValue;

        iconBar.SpawnIcon(IconNeeded.BLOCK);

        StartCoroutine(Animation(Vector3.up));
    }

    void Guard()
    {
        target = BattleManager.instance.GiveMeTarget(this, myEquip, true);
        foreach (Battler b in target)
        {
            BattleManager.instance.ShowParticle(b.transform, ParticleType.BUFF);
            for (int i = 0; i < myAbilities[currentAbility].abilityValue; i++)
            {
                b.guards.Add(this);
            }
            b.iconBar.SpawnIcon(IconNeeded.GUARD);
        }
        StartCoroutine(Animation(Vector3.up));
    }

    void HitAll()
    {
        target = BattleManager.instance.GiveMeTarget(this, myEquip, false, MoveType.HitAll);
        foreach(Battler b in target)
        {
            b.Damage(this.damage.Random, this.accuracy.Random, false);
        }
        StartCoroutine(Animation(attackDirection));
    }

    void DoubleHit()
    {
        target = BattleManager.instance.GiveMeTarget(this, myEquip, false);
        foreach (Battler b in target)
        {
            b.Damage(this.damage.Random, this.accuracy.Random, false);
        }

        foreach (Battler b in target)
        {
            b.Damage(this.damage.Random, this.accuracy.Random, false);
        }

        StartCoroutine(Animation(attackDirection));
    }

    void IgnoreDef()
    {
        target = BattleManager.instance.GiveMeTarget(this, myEquip, false);
        foreach (Battler b in target)
        {
            b.Damage(this.damage.Random, this.accuracy.Random, true);
        }
        StartCoroutine(Animation(attackDirection));
    }

    void Stun()
    {
        Attack();
        foreach (Battler b in target)
        {
            b.AttemptStun();
        }
    }

    void SpawnAds()
    {
        BattleManager.instance.RespawnAds();
        StartCoroutine(Animation(Vector3.up));
    }

    void BuffThenAttack()
    {
        if (myAbilities[currentAbility].buffTarget == BuffTarget.SELF)
        {
            BuffSelf();
        }
        else if (myAbilities[currentAbility].buffTarget == BuffTarget.TARGET)
        {
            BuffOther();
        }

        Attack();
    }

    void Regen(int amount)
    {
        this.Heal(amount);
    }
}
