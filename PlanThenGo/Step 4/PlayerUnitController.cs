using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUnitController : MonoBehaviour
{
    public List<Button> moveButtons;
    public GameObject selectionPrefab;
    public GameObject selectedPlayerPrefab;
    public List<PlayerUnit> players;
    PlayerUnit current = null;
    Move selectedMove;

    public void BattleStarted()
    {
        players = new List<PlayerUnit>();
        current = null;
    }

    public void ShowMyStuff(PlayerUnit me)
    {
        players.Add(me);
        if (current == null)
            StartPlayer();
    }

    GameObject selectedPlayer = null;
    void StartPlayer()
    {
        current = players[0];
        players.RemoveAt(0);

        for (int i = 0; i < moveButtons.Count; i++)
        {
            if (i < current.moves.Count)
            {
                moveButtons[i].gameObject.SetActive(true);
                Move temp = current.moves[i];
                moveButtons[i].onClick.AddListener(delegate { MoveSelected(temp); });
            }
            else
                moveButtons[i].gameObject.SetActive(false);
        }

        selectedPlayer = Instantiate(selectedPlayerPrefab, current.transform);
    }

    public void MoveSelected(Move move)
    {
        selectedMove = move;
        if(move.type == MoveType.DAMAGE)
        {
            SelectEnemies();
        }

        for (int i = 0; i < moveButtons.Count; i++)
        {
            moveButtons[i].onClick.RemoveAllListeners();
        }
    }

    List<GameObject> selections;
    void SelectEnemies()
    {
        List<EnemyUnit> enemies = BattleManager.instance.GiveEnemies();
        selections = new List<GameObject>();

        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject go = Instantiate(selectionPrefab, enemies[i].transform);
            selections.Add(go);
            EnemyUnit temp = enemies[i];
            go.GetComponent<Button>().onClick.AddListener(delegate { EnemySelected(temp); });
        }
    }

    public void EnemySelected(EnemyUnit enemyUnit)
    {
        current.MoveChosen(selectedMove, enemyUnit);

        foreach (GameObject go in selections)
        {
            Destroy(go);
        }

        current = null;

        GameObject.Destroy(selectedPlayer);

        if (players.Count > 0)
            StartPlayer();
    }

    public void NeedNewTarget(PlayerUnit me, Move move)
    {
        if(move.type == MoveType.DAMAGE)
        {
            List<EnemyUnit> enemies = BattleManager.instance.GiveEnemies();
            me.target = enemies[Random.Range(0, enemies.Count)];
        }
    }
}
