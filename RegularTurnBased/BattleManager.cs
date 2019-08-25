using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BattleManager : MonoBehaviour
{
    public static BattleManager instance;

    public float timeBetweenBattlers;
    public float cameraSpeed;
    public List<Battler> playerUnits;
    public List<Battler> enemyUnits;
    public List<Battler> line;

    public Battler currentlyGoing;
    public List<Vector3Int> unitPositions;
    public GridManager grid;
    public PlayerUnitController playerUnitController;
    public EnemyUnitController enemyUnitController;

    private void Start()
    {
        instance = this;
        StartBattle();
    }

    void StartBattle()
    {
        //Get the playerunits and enemyUnits. This is temporary until the units are instantiated
        //at battle start.

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player"))
            playerUnits.Add(g.GetComponent<Battler>());

        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Enemy"))
            enemyUnits.Add(g.GetComponent<Battler>());

        //Set up the units list
        RefreshUnits();

        currentlyGoing = line[0];
        StartCoroutine(LineHandler());
    }

    IEnumerator LineHandler()
    {
        //Replace this with a function that returns a bool
        while (!BattleOver())
        {
            Battler current = currentlyGoing;
            //Do everything we need
            RefreshTheGrid();

            //Tell the unit to do their thing
            if (playerUnits.Contains(currentlyGoing))
                playerUnitController.HeresYourKid(currentlyGoing.transform);
            else
                enemyUnitController.HeresYourKid(currentlyGoing.transform);

            while (current == currentlyGoing)
            {
                Camera.main.transform.position = currentlyGoing.transform.position + (Vector3.back * 10);
                yield return null;
            }

            while(Camera.main.transform.position != currentlyGoing.transform.position + (Vector3.back * 10))
            {
                Camera.main.transform.position = Vector3.MoveTowards(Camera.main.transform.position,
                    currentlyGoing.transform.position + (Vector3.back * 10), Time.deltaTime * cameraSpeed);
                yield return null;
            }
            
            yield return new WaitForSeconds(timeBetweenBattlers);
        }

        print("Battle's over");
    }

    bool BattleOver()
    {
        bool temp = false;

        if (playerUnits.Count == 0 || enemyUnits.Count == 0)
            temp = true;

        return temp;
    }

    public void UnitsDone()
    {
        line.RemoveAt(0);
        //print(line.Count);
        if (line.Count == 0)
        {
            //print("uh oh");
            RefreshUnits();
        }

        currentlyGoing = line[0];
    }

    public void UnitDied(Battler theDeparted)
    {
        if (playerUnits.Contains(theDeparted))
            playerUnits.Remove(theDeparted);

        if (enemyUnits.Contains(theDeparted))
            enemyUnits.Remove(theDeparted);

        RefreshUnits(currentlyGoing);
    }

    void RefreshUnits()
    {
        line = new List<Battler>();

        //Get the line list filled
        line.AddRange(playerUnits);
        line.AddRange(enemyUnits);

        //Sort the units list sorted
        bool swapped = false;
        do
        {
            swapped = false;
            for (int i = 0; i < line.Count - 1; i++)
            {
                if (line[i].speed < line[i + 1].speed)
                {
                    Battler temp = line[i];
                    line[i] = line[i + 1];
                    line[i + 1] = temp;
                    swapped = true;
                }
            }
        } while (swapped);
    }

    void RefreshUnits(Battler _curr)
    {
        RefreshUnits();

        while (line[0] != _curr)
            line.RemoveAt(0);
    }

    void RefreshTheGrid()
    {
        unitPositions = new List<Vector3Int>();

        List<Battler> units = new List<Battler>();
        units.AddRange(playerUnits);
        units.AddRange(enemyUnits);

        //Get the positions of the units and send them to the grid
        foreach (Battler b in units)
        {
            if (b == currentlyGoing)
                continue;
            
            unitPositions.Add(grid.walkable.WorldToCell(b.transform.position));
        }

        grid.CreateGridFromTilemaps(unitPositions);
    }

    public List<PlayerPositions> GivePlayerPositions(int distance)
    {
        List<PlayerPositions> positions = new List<PlayerPositions>();

        foreach (Battler b in playerUnits)
        {
            /*List<Vector3Int> playerPositions = new List<Vector3Int>();

            playerPositions.Add(grid.walkable.WorldToCell(b.transform.position));

            foreach (Vector3Int ppos in )*/
            positions.Add(new PlayerPositions(b,
                grid.GiveAllWithinDistanceOf(grid.walkable.WorldToCell(b.transform.position), distance, distance)));
        }

        return positions;
    }

    public List<Battler> GiveEnemies()
    {
        return enemyUnits;
    }

    public List<Vector3Int> GiveAllWithinDistanceOf(Vector3 pos, int outerDistance, int innerDistance)
    {
        RefreshTheGrid();
        return grid.GiveAllWithinDistanceOf(pos, outerDistance, innerDistance);
    }
}

public class PlayerPositions
{
    public Battler unit;
    public List<Vector3Int> attackPositions;

    public PlayerPositions(Battler unit, List<Vector3Int> attackPositions)
    {
        this.unit = unit;
        this.attackPositions = attackPositions;
    }
}
