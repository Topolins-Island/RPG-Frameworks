using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public int numberOfShownBattlers = 10;
    public float timeBetweenBattlers = 0.25f;
    public float slowTime = 1.0f;
    public float fastTime = 0.25f;
    public float deathAnimLength = 0.3f;

    public Image dungeonBackground;
    public RectTransform playerPartyHolder;
    public RectTransform enemyPartyHolder;
    public Button leftDungeonSelection;
    public Button rightDungeonSelection;
    public Button climbTierButton;
    public Image transition;
    public TMP_Text abilityMessageDisplay;
    public Image pauseButton;
    public Image fastForwardButton;

    [Header("Prefabs")]
    public GameObject playerBattlerPrefab;
    public GameObject enemyBattlerPrefab;
    public GameObject bossBattlerPrefab;
    public GameObject damageText;
    public GameObject fadeText;
    public GameObject particleObject;
    public List<ParticleHolder> particles;

    [Header("End Dialogues")]
    public Dialogue firstTime;
    public Dialogue secondTime;

    [Header("Public Battling stuff")]
    public List<PlayerStats> playerParty;

    public Dungeon currentDungeon;
    int currentBattle;

    public List<Battler> battlers;

    //This covers what index in the speed list we are at currently, this is not the last added, that is this minus one. 
    public int speedIndex;
    public List<Battler> speedSorted;

    public List<Battler> line;

    public int numOfRounds = 0;
    Battler firstBattler;
    bool someoneDied = false;

    List<Dungeon> visitedDungeons;
    List<PlayerStats> lostMonsters;

    private void OnEnable()
    {
        if (fastForwardButton.color == Color.black)
        {
            timeBetweenBattlers = slowTime;
            fastForwardButton.color = Color.white;
            fastForwardButton.gameObject.GetComponentInChildren<Text>().color = Color.black;
        }

        if (pauseButton.color == Color.black)
        {
            timeBetweenBattlers = slowTime;
            pauseButton.color = Color.white;
            pauseButton.gameObject.GetComponentInChildren<Text>().color = Color.black;
        }

        if (enemyStartPos == Vector2.zero)
            enemyStartPos = enemyPartyHolder.anchoredPosition;
    }

    public void StartExpedition(Dungeon entrance, List<PlayerStats> party, List<Equipment> equips)
    {
        if (battlers.Count > 0)
            return;

        instance = this;

        youNeedToStop = false;
        someoneDied = false;
        pause = false;
        toggleTime = false;
        playerParty = party;
        numOfRounds = 0;

        visitedDungeons = new List<Dungeon>();
        lostMonsters = new List<PlayerStats>();

        currentDungeon = null;

        SpawnPlayers(equips);
        StartDungeon(entrance);
    }

    void StartDungeon(Dungeon dungeon)
    {
        if (leftDungeonSelection.gameObject.activeInHierarchy)
        {
            leftDungeonSelection.gameObject.SetActive(false);
            rightDungeonSelection.gameObject.SetActive(false);
            climbTierButton.gameObject.SetActive(false);
            transition.gameObject.SetActive(false);
        }

        if (currentDungeon == null)
        {
            currentDungeon = dungeon;

            abilityMessageDisplay.text = "";

            dungeonBackground.sprite = currentDungeon.background;

            currentBattle = 0;

            StartCoroutine(StartBattle());
        }
        else
        {
            bool right = currentDungeon.right == dungeon ? true : false;

            currentDungeon = dungeon;

            abilityMessageDisplay.text = "";

            StartCoroutine(DungeonTransition(right));
        }
    }

    IEnumerator DungeonTransition(bool choseRight)
    {
        leftDungeonSelection.gameObject.SetActive(false);
        rightDungeonSelection.gameObject.SetActive(false);
        climbTierButton.gameObject.SetActive(false);
        transition.gameObject.SetActive(true);

        Color startingAlpha = transition.color;

        Color iHaveTo = startingAlpha;
        while (iHaveTo.a < 1)
        {
            iHaveTo.a += Time.deltaTime / 0.15f;
            transition.color = iHaveTo;
            yield return null;
        }

        //Set everything up while the curtain's down
        dungeonBackground.sprite = currentDungeon.background;

        currentBattle = 0;

        yield return new WaitForSeconds(1f);

        while (iHaveTo.a > 0)
        {
            iHaveTo.a -= Time.deltaTime / 0.25f;
            transition.color = iHaveTo;
            yield return null;
        }

        transition.gameObject.SetActive(false);
        transition.color = startingAlpha;

        StartCoroutine(StartBattle());
    }

    IEnumerator ChooseNextDungeon()
    {
        ClearEnemies();

        yield return new WaitForSeconds(timeBetweenBattlers);
        
        visitedDungeons.Add(currentDungeon);

        GameManager.instance.DungeonCompleted(currentDungeon);

        yield return null;

        if (GameManager.instance.rewardSprite != null)
            yield break;

        if (currentDungeon.left == null && currentDungeon.right == null)
        {
            ClearPlayers();
            GameManager.instance.ExpeditionEnded(visitedDungeons, lostMonsters, numOfRounds);

            StopAllCoroutines();
            battlers = new List<Battler>();

            currentDungeon = null;

            yield break;
        }

        //Set up the buttons
        leftDungeonSelection.gameObject.SetActive(true);
        leftDungeonSelection.onClick.RemoveAllListeners();
        //leftDungeonSelection.onClick.AddListener(delegate { StartDungeon(currentDungeon.left); });
        leftDungeonSelection.transform.GetChild(0).GetComponent<Text>().text = currentDungeon.left.name;

        rightDungeonSelection.gameObject.SetActive(true);
        rightDungeonSelection.onClick.RemoveAllListeners();
        //rightDungeonSelection.onClick.AddListener(delegate { StartDungeon(currentDungeon.right); });
        rightDungeonSelection.transform.GetChild(0).GetComponent<Text>().text = currentDungeon.right.name;

        //Moved these down here on account of the tutorial
        leftDungeonSelection.onClick.AddListener(delegate { StartDungeon(currentDungeon.left); });
        rightDungeonSelection.onClick.AddListener(delegate { StartDungeon(currentDungeon.right); });


        transition.gameObject.SetActive(true);

        if (currentDungeon.higherTier == null)
            yield break;

        climbTierButton.gameObject.SetActive(true);
        climbTierButton.onClick.RemoveAllListeners();
        climbTierButton.onClick.AddListener(delegate { StartDungeon(currentDungeon.higherTier); });
        climbTierButton.transform.GetChild(0).GetComponent<Text>().text = currentDungeon.higherTier.name;
    }

    Vector2 startPos;
    public Vector2 enemyStartPos = Vector2.zero;
    IEnumerator StartBattle()
    {
        if (currentDungeon.name == "END")
        {
            SpawnEndBosses();
        }
        else
        {
            SpawnEnemies();
        }

        startPos = Vector2.right * -230;
        Vector2 endPos = startPos + Vector2.up * 10;

        playerPartyHolder.anchoredPosition = startPos;
        enemyPartyHolder.anchoredPosition = enemyStartPos + Vector2.right * 360;

        //If there is no boss then we can just bounce
        if (enemyPartyHolder.childCount == 0)
            yield break;

        //Walking animation
        for (int i = 0; i < 3; i++)
        {
            while (playerPartyHolder.anchoredPosition != endPos)
            {
                playerPartyHolder.anchoredPosition = Vector3.MoveTowards(playerPartyHolder.anchoredPosition, endPos, Time.deltaTime * 100);
                enemyPartyHolder.anchoredPosition = Vector3.MoveTowards(enemyPartyHolder.anchoredPosition, enemyStartPos, Time.deltaTime * 600);

                while (pause)
                {
                    if (youNeedToStop)
                        yield break;
                    yield return null;
                }

                yield return null;
            }

            while (playerPartyHolder.anchoredPosition != startPos)
            {
                playerPartyHolder.anchoredPosition = Vector3.MoveTowards(playerPartyHolder.anchoredPosition, startPos, Time.deltaTime * 100);
                enemyPartyHolder.anchoredPosition = Vector3.MoveTowards(enemyPartyHolder.anchoredPosition, enemyStartPos, Time.deltaTime * 600);

                while (pause)
                {
                    if (youNeedToStop)
                        yield break;
                    yield return null;
                }

                yield return null;
            }
        }

        yield return new WaitForSeconds(timeBetweenBattlers);

        while (pause)
        {
            if (youNeedToStop)
                yield break;
            yield return null;
        }

        line = new List<Battler>();
        speedIndex = 0;

        RefreshBattlers();
        SortBattlers();
        FillBattle();

        firstBattler = line[0];

        StartCoroutine(LineProcessing());
    }

    void NextBattle()
    {
        currentBattle++;

        //If the dungeon we just cleared was END, fill it!
        if(currentDungeon.name == "END")
        {
            PartyAbsorbed();
        }

        if(currentBattle > currentDungeon.parties.Count)
        {
            StartCoroutine(ChooseNextDungeon());
        }
        else
        {
            StartCoroutine(StartBattle());
        }
    }

    bool youNeedToStop = false;
    bool lineGoing = false;
    bool pause = false;
    bool toggleTime = false;
    IEnumerator LineProcessing()
    {
        lineGoing = true;
        yield return null;

        while (DialogueController.instance.gameObject.activeInHierarchy)
            yield return null;

        //print("started battle");
        while (true)
        {
            yield return new WaitForSeconds(timeBetweenBattlers);

            //The time needs to be done here, to prevent confusion
            if (toggleTime)
            {
                if (timeBetweenBattlers == slowTime)
                {
                    timeBetweenBattlers = fastTime;
                }
                else
                {
                    timeBetweenBattlers = slowTime;
                }
                toggleTime = false;
            }

            //Battling canceled
            if (youNeedToStop)
            {
                BattlingCanceled();
                lineGoing = false;
                yield break;
            }

            while (pause)
            {
                if (youNeedToStop)
                    yield break;
                yield return null;
            }

            //DoT's are handled here! because cool!
            if (line[0].gameObject.transform.GetComponentInChildren<RegenBuff>() != null)
            {
                foreach(Transform child in line[0].gameObject.transform)
                {
                    if (child.GetComponent<RegenBuff>() != null)
                        child.GetComponent<RegenBuff>().TargetWent();
                }
            }

            while (someoneDied)
                yield return null;
            
            bool enemiesExist = false;
            foreach (Battler b in battlers)
            {
                if ((b.gameObject.GetComponent<Battler>() != null && b.currentHP > 0) && b.transform.parent == enemyPartyHolder)
                    enemiesExist = true;
            }

            if (!enemiesExist)
            {
                //print("enmey ded");
                NextBattle();
                lineGoing = false;
                yield break;
            }

            if(playerPartyHolder.childCount == 0)
            {
                BattlingCanceled();
                lineGoing = false;
                yield break;
            }

            while (pause)
            {
                if (youNeedToStop)
                    yield break;
                yield return null;
            }

            if (line[0].currentHP > 0)
            {
                if (line[0] == firstBattler)
                    numOfRounds++;

                //Check if it's stunned!
                if (line[0].stunned)
                    line[0].YouWereStunned();
                else //If its not, then let it go!
                    line[0].myMove.Invoke();

                //Make sure they aren't stunned anymore
                line[0].stunned = false;
            }

            while (line[0].animating)
                yield return null;

            line.RemoveAt(0);
            FillBattle();
        }
    }

    public void PauseBattle()
    {
        pause = !pause;

        if (pause)
        {
            pauseButton.color = Color.black;
            pauseButton.gameObject.GetComponentInChildren<Text>().color = Color.white;
            lineGoing = false;
        }
        else
        {
            pauseButton.color = Color.white;
            pauseButton.gameObject.GetComponentInChildren<Text>().color = Color.black;
            lineGoing = true;
        }
    }
    
    public void ToggleFastForward()
    {
        if (toggleTime)
            return;

        if (timeBetweenBattlers == slowTime)
        {
            toggleTime = true;
            fastForwardButton.color = Color.black;
            fastForwardButton.gameObject.GetComponentInChildren<Text>().color = Color.white;
        }
        else
        {
            toggleTime = true;
            fastForwardButton.color = Color.white;
            fastForwardButton.gameObject.GetComponentInChildren<Text>().color = Color.black;
        }
    }

    void SpawnPlayers(List<Equipment> equips)
    {
        //Spawn players, goes in reverse because of the 200 IQ layout group coding
        for (int i = playerParty.Count - 1; i >= 0; i--)
        {
            GameObject go = Instantiate(playerBattlerPrefab, playerPartyHolder);
            go.name = playerParty[i].name;
            Battler b = go.GetComponent<Battler>();
            b.myEquip = equips[i];
            b.SetStats(enemyPartyHolder, playerParty[i], playerParty[i].sprite);
        }
    }

    void SpawnEndBosses()
    {
        List<PlayerStats> endParty = GameManager.instance.endParty;
        List<Equipment> endEquips = GameManager.instance.endEquips;

        if(endParty.Count == 0)
        {
            DialogueController.instance.gameObject.SetActive(true);
            DialogueController.instance.dialogueFinished.RemoveAllListeners();
            DialogueController.instance.dialogueFinished.AddListener(PartyAbsorbed);
            DialogueController.instance.DisplayDialogue(firstTime);
            return;
        }
        else
        {
            if (!GameManager.instance.showedSecondTime)
            {
                DialogueController.instance.gameObject.SetActive(true);
                DialogueController.instance.dialogueFinished.RemoveAllListeners();
                DialogueController.instance.DisplayDialogue(secondTime);
                GameManager.instance.showedSecondTime = true;
            }
        }

        for (int i = endParty.Count - 1; i >= 0; i--)
        {
            GameObject go = Instantiate(playerBattlerPrefab, enemyPartyHolder);
            go.name = endParty[i].name;
            go.GetComponent<RectTransform>().localScale = new Vector3(-1, 1f, 1f);

            Battler b = go.GetComponent<Battler>();
            b.SetStats(playerPartyHolder, endParty[i], endParty[i].sprite);
            b.myEquip = endEquips[i];
        }
    }

    void PartyAbsorbed()
    {
        lostMonsters = playerParty;

        GameManager.instance.FillEnd();
        BattlingCanceled();
    }

    void SpawnEnemies()
    {
        if (currentBattle < currentDungeon.parties.Count)
        {
            //Spawn regular battle
            for (int i = 0; i < currentDungeon.parties[currentBattle].party.Count; i++)
            {
                GameObject go = Instantiate(enemyBattlerPrefab, enemyPartyHolder);
                go.name = currentDungeon.parties[currentBattle].party[i].name;
                Battler b = go.GetComponent<Battler>();
                b.SetStats(playerPartyHolder, currentDungeon.parties[currentBattle].party[i], currentDungeon.parties[currentBattle].party[i].sprite);
            }
        }
        else
        {
            if (currentDungeon.boss != null)
            {
                //Spawn boss battle
                for (int i = 0; i < currentDungeon.ads.Count; i++)
                {
                    GameObject go = Instantiate(enemyBattlerPrefab, enemyPartyHolder);
                    go.name = currentDungeon.ads[i].name;
                    Battler b = go.GetComponent<Battler>();
                    b.SetStats(playerPartyHolder, currentDungeon.ads[i], currentDungeon.ads[i].sprite);
                }

                GameObject boss = Instantiate(bossBattlerPrefab, enemyPartyHolder);
                boss.name = currentDungeon.boss.name;
                Battler goob = boss.GetComponent<Battler>();
                goob.SetStats(playerPartyHolder, currentDungeon.boss, currentDungeon.boss.sprite);

                boss.transform.SetSiblingIndex(currentDungeon.bossPosition);
            }
            else
            {
                NextBattle();
            }
        }
    }

    void GetBattlers()
    {
        /*
        battlers = new List<Battler>();

        foreach (Transform child in playerPartyHolder)
        {
            PlayerBattler b = child.GetComponent<PlayerBattler>();
            battlers.Add(b);
            b.SetStats(enemyPartyHolder, testP);
        }

        foreach (Transform child in enemyPartyHolder)
        {
            EnemyBattler b = child.GetComponent<EnemyBattler>();
            battlers.Add(b);
            b.SetStats(playerPartyHolder, testE);
        }
        */
    }

    void RefreshBattlers()
    {
        battlers = new List<Battler>();

        foreach(Transform child in playerPartyHolder)
        {
            if (child.GetComponent<Battler>() != null)
                battlers.Add(child.GetComponent<Battler>());
            else
                Destroy(child.gameObject);
        }

        foreach (Transform child in enemyPartyHolder)
        {
            if (child.GetComponent<Battler>() != null)
                battlers.Add(child.GetComponent<Battler>());
            else
                Destroy(child.gameObject);
        }

        foreach(Battler b in battlers)
        {
            if (b.guards.Count > 0)
                b.guards = new List<Battler>();
        }
    }

    void SortBattlers()
    {
        //Just for testing, we is randomizing battlers
        /*for (int i = 0; i < battlers.Count; i++)
        {
            Battler temp = battlers[i];
            int randomIndex = Random.Range(0, battlers.Count);
            battlers[i] = battlers[randomIndex];
            battlers[randomIndex] = temp;
        }*/

        //Set up the list
        speedSorted = new List<Battler>();

        foreach(Battler b in battlers)
        {
            speedSorted.Add(b);
        }

        //Sort them
        bool swapped = false;
        do
        {
            swapped = false;
            for (int i = 0; i < speedSorted.Count - 1; i++)
            {
                if(speedSorted[i].speed < speedSorted[i + 1].speed)
                {
                    Battler temp = speedSorted[i];
                    speedSorted[i] = speedSorted[i + 1];
                    speedSorted[i + 1] = temp;
                    swapped = true;
                }
            }
        } while (swapped);
    }

    void FillBattle()
    {
        while(line.Count < numberOfShownBattlers)
        {
            line.Add(speedSorted[speedIndex]);
            speedIndex++;
            if (speedIndex > speedSorted.Count - 1)
            {
                speedIndex = 0;
            }
        }
    }

    public void SomeoneHasDied(Battler me)
    {
        someoneDied = true;

        BattlerDied(speedSorted.IndexOf(me));

        PlayerStats playerStats = GameManager.instance.polyPanel.FindMonster(me.name);

        if(playerStats != null)
        {
            lostMonsters.Add(playerStats);
            GameManager.instance.playerMonsterInventory.Remove(playerStats);
        }

        StartCoroutine(WaitForDeath());
    }

    IEnumerator WaitForDeath()
    {
        yield return new WaitForSeconds(deathAnimLength);

        FillBattle();

        yield return null;
        
        someoneDied = false;
    }

    void BattlerDied(int index)
    {
        Battler b = speedSorted[index];

        foreach(Battler ba in battlers)
        {
            if (ba.guards.Contains(b))
                ba.guards.Remove(b);
        }

        battlers.Remove(b);
        speedSorted.Remove(b);

        while (line.Contains(b))
            line.Remove(b);

        speedIndex = speedSorted.IndexOf(line[0]);

        line = new List<Battler>();
        FillBattle();
    }

    void ClearPlayers()
    {
        foreach (Transform child in playerPartyHolder)
        {
            if(child.GetComponent<Battler>() != null)
                child.GetComponent<Battler>().StopAllCoroutines();
            Destroy(child.gameObject);
        }
    }

    void ClearEnemies()
    {
        foreach (Transform child in enemyPartyHolder)
        {
            if(child.GetComponent<Battler>() != null)
                child.GetComponent<Battler>().StopAllCoroutines();
            Destroy(child.gameObject);
        }
    }

    public void ExitBattle()
    {
        if (lineGoing)
            youNeedToStop = true;
        else
            BattlingCanceled();
    }

    void BattlingCanceled()
    {
        ClearPlayers();
        ClearEnemies();
        
        GameManager.instance.ExpeditionEnded(visitedDungeons, lostMonsters, numOfRounds);

        StopAllCoroutines();

        playerPartyHolder.position = startPos;
        enemyPartyHolder.position = enemyStartPos;

        battlers = new List<Battler>();
    }

    public void TestRandomBattlerSpeedUp()
    {
        line = new List<Battler>();
        FillBattle();
    }

    //This is the basic one, just gives them the one in front of them
    public Battler GiveMeTarget(Battler me)
    {
        int index = battlers.IndexOf(me);

        int direction;
        //Check if it's a player or enemy
        if(me.transform.parent == playerPartyHolder)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }

        Battler b = battlers[index + direction];

        if (b.guards.Count > 0)
        {
            Battler gangganggang = b;
            b = b.guards[0];
            gangganggang.guards.RemoveAt(0);
        }
        
        return b;
    }

    public List<Battler> GiveMeTarget(Battler me, Equipment equipment, bool ignoreGuards)
    {
        int index = battlers.IndexOf(me);

        int direction;
        //Check if it's a player or enemy
        if (me.transform.parent == playerPartyHolder)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }

        List<Battler> targets = new List<Battler>();

        if(index + direction > battlers.Count - 1 || index + direction < 0)
        {
            //print("no one exists in that direction " + direction + " " + me.name + " " + me.transform.parent.name);
            return targets;
        }

        if (equipment == Equipment.ADD)
        {
            targets.Add(battlers[index + direction]);
            int newIndex = index + (direction * 2);
            if (newIndex < battlers.Count && newIndex >= 0)
                targets.Add(battlers[newIndex]);
        }
        else if (equipment == Equipment.SKIP)
        {
            int newIndex = index + (direction * 2);
            if (newIndex < battlers.Count && newIndex >= 0)
                targets.Add(battlers[newIndex]);
            else
                targets.Add(battlers[index + direction]);
        }
        else
        {
            targets.Add(battlers[index + direction]);
        }

        //Check for guards
        if (!ignoreGuards)
        {
            for (int i = 0; i < targets.Count; i++)
            {
                if (targets[i].guards.Count > 0)
                {
                    Battler bb = targets[i];
                    targets[i] = targets[i].guards[0];
                    bb.guards.RemoveAt(0);
                }
            }
        }

        List<int> toBeRemoved = new List<int>();

        //Ensure they don't target a dead battler
        for (int i = 0; i < targets.Count; i++)
        {
            if(targets[i].currentHP == 0)
            {
                toBeRemoved.Add(i);
            }
        }

        foreach(int i in toBeRemoved)
        {
            targets.RemoveAt(i);
        }

        //Check for duplicates
        List<Battler> check = new List<Battler>();

        foreach(Battler t in targets)
        {
            if (!check.Contains(t))
                check.Add(t);
        }

        targets = check;

        return targets;
    }

    public List<Battler> GiveMeTarget(Battler me, Equipment equipment, bool ignoreGuards, MoveType move)
    {
        int index = battlers.IndexOf(me);

        int direction;
        //Check if it's a player or enemy
        if (me.transform.parent == playerPartyHolder)
        {
            direction = 1;
        }
        else
        {
            direction = -1;
        }

        List<Battler> targets = new List<Battler>();

        if(move == MoveType.HitAll)
        {
            if (direction > 0)
            {
                for (int i = index + 1; i < battlers.Count; i++)
                {
                    targets.Add(battlers[i]);
                }
            }
            else
            {
                for (int i = battlers.Count - 1; i > index; i--)
                {
                    targets.Add(battlers[i]);
                }
            }
        }

        if(equipment == Equipment.SKIP)
        {
            targets.RemoveAt(0);
        }

        //Check for duplicates
        List<Battler> check = new List<Battler>();

        foreach (Battler t in targets)
        {
            if (!check.Contains(t))
                check.Add(t);
        }

        targets = check;

        return targets;
    }

    public void ShowNumbers(Transform me, int number, Color color)
    {
        GameObject go = Instantiate(damageText, me);
        go.GetComponent<Text>().text = number.ToString();
        go.GetComponent<Text>().color = color;
        go.GetComponent<RectTransform>().localPosition = new Vector3(Random.Range(-25f, 25f),-90, 0);

        if (color == Color.red)
            ShowParticle(me, ParticleType.HIT);
        else if (color == Color.green)
            ShowParticle(me, ParticleType.HEAL);
    }

    public void ShowText(Transform me, string text, Color color)
    {
        GameObject go = Instantiate(fadeText, me);
        go.GetComponent<Text>().text = text;
        go.GetComponent<Text>().color = color;
        go.GetComponent<RectTransform>().localPosition = new Vector3(Random.Range(-25f, 25f), 0, 0);
    }

    public void ShowParticle(Transform place, ParticleType type)
    {
        GameObject go = Instantiate(particleObject, place);
        Particle p = go.GetComponent<Particle>();
        List<Sprite> animation = new List<Sprite>();

        foreach(ParticleHolder pH in particles)
        {
            if (pH.type == type)
                animation = pH.particle;
        }

        p.PlayThisInThisAmountOfTime(animation, timeBetweenBattlers - 0.1f);
    }

    public void AbilityFired(string abilityMessage)
    {
        abilityMessageDisplay.text += "\n" + abilityMessage + "\n";
    }

    public void RespawnAds()
    {
        for (int i = 0; i < currentDungeon.ads.Count; i++)
        {
            GameObject go = Instantiate(enemyBattlerPrefab, enemyPartyHolder);
            go.name = currentDungeon.ads[i].name;
            go.transform.SetAsFirstSibling();
            Battler b = go.GetComponent<Battler>();
            b.SetStats(playerPartyHolder, currentDungeon.ads[i], currentDungeon.ads[i].sprite);
        }

        RefreshBattlers();
        SortBattlers();
        FillBattle();
    }
}

[System.Serializable]
public class ParticleHolder
{
    public ParticleType type;
    public List<Sprite> particle;
}

public enum ParticleType
{
    HIT,
    HEAL,
    BUFF
}