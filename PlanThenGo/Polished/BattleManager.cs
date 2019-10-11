using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public PlayerUnitController playerController;
    public EnemyUnitController enemyController;
    public Text messageDisplay;

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

            //Polish stuffs - Instead of just waiting for a certain amount of time,
            //we wait for these booleans to be true!
            while (printing)
                yield return null;

            animating = true;
            while (animating)
                yield return null;

            while (!Input.GetKeyDown(KeyCode.Mouse0))
                yield return null;
        }

        if (BattleOver())
        {
            BattleEnding();
            yield break;
        }
        else
            StartRound();
    }

    bool printing = false;
    public void DisplayMessage(string message)
    {
        printing = true;
        StartCoroutine(PrintMessage(message));
    }

    IEnumerator PrintMessage(string message)
    {
        string printedMessage = "";
        for (int i = 0; i < message.Length; i++)
        {
            printedMessage += message[i];
            messageDisplay.text = printedMessage;
            if (i % 5 == 0)
                yield return null;
        }
        printing = false;
    }

    bool animating = false;
    public void DoneAnimating()
    {
        animating = false;
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

    private void BattleEnding()
    {
        bool enemiesLeft = false;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i].gameObject.GetComponent<EnemyUnit>() != null)
                enemiesLeft = true;
        }

        if (enemiesLeft)
            PlayerLost();
        else
            PlayerWon();
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

    void PlayerWon()
    {
        DisplayMessage("Trans rights");
        //Here is where the rest of the stuff would go
    }

    void PlayerLost()
    {
        DisplayMessage("You lost!");
        //Here is where the rest of the stuff would go
    }
}
