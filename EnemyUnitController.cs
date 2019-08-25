using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyUnitController : MonoBehaviour
{
    public Transform myBoy;
    Battler _battler;
    Vector3[] path;
    public int moveDist;
    public int atkDist;
    List<PlayerPositions> playerPositions;
    PlayerPositions currentTarget;

    [Header("Settings")]
    public float moveSpeed;
    public float waitTimeBetweenNodes = 0.1f;
    public float attackTime = 0.1f;

    public void HeresYourKid(Transform current)
    {
        myBoy = current;
        _battler = myBoy.GetComponent<Battler>();
        Vector3Int boisPosition = BattleManager.instance.grid.walkable.WorldToCell(myBoy.position);

        //Find out what my boy wants to do
        //For test room purposes he wants to attack
        HeWantsToAttack();
    }

    void HeWantsToAttack()
    {
        //Find attack positions off all players at the current unit's attack range
        //atkDist is the unit's max range + min range
        playerPositions = BattleManager.instance.GivePlayerPositions(atkDist);

        int positionsChecked = 0;
        path = new Vector3[moveDist + atkDist];
        foreach (PlayerPositions player in playerPositions)
        {
            foreach (Vector3Int good in player.attackPositions)
            {
                PathRequestManager.RequestPath(myBoy.transform.position, (Vector3)good, player.unit, OnPathFound);
                positionsChecked++;
            }
        }
        
        temp = 0;
        StartCoroutine(WaitingForPaths(positionsChecked));
    }

    void Idle()
    {
        //This is where the stuff for him to idle will be.
        //For now he just ends.
        UnitDone();
    }
    
    public void OnPathFound(Vector3[] newPath, bool pathSuccessful, Battler _target)
    {
        if (pathSuccessful)
        {
            if (path == null)
            {
                path = newPath;

                foreach (PlayerPositions p in playerPositions)
                    if (p.unit == _target)
                        currentTarget = p;
            }
            else
            {
                if (newPath.Length < path.Length)
                {
                    path = newPath;
                    foreach (PlayerPositions p in playerPositions)
                        if (p.unit == _target)
                            currentTarget = p;
                }
            }
        }
        temp++;
    }

    int temp;
    IEnumerator WaitingForPaths(int numOfPaths)
    {
        while (numOfPaths > temp)
            yield return null;

        if(path.Length <= moveDist)
            StartCoroutine(MoveUnit());
        else
        {
            //print("Didn't get a path to the player, so he will idle.");
            Idle();
        }
    }

    int targetIndex;

    IEnumerator MoveUnit()
    {
        targetIndex = 0;

        if (path.Length < 0)
        {
            print("Uh.. chief?");
            yield break;
        }

        while(targetIndex < path.Length)
        {
            myBoy.transform.position = Vector3.MoveTowards(myBoy.transform.position, path[targetIndex], Time.deltaTime * moveSpeed);
            Vector3 dir = path[targetIndex] - myBoy.position;
            dir.Normalize();

            if(myBoy.transform.position == path[targetIndex])
            {
                targetIndex++;
                _battler.animator.AnimateInDirection(dir, AnimType.IDLE);
                yield return new WaitForSeconds(waitTimeBetweenNodes);
            }
            else
                _battler.animator.AnimateInDirection(dir, AnimType.MOVING);

            yield return null;
        }

        print("Path completed, I will now check what this enemy unit wants to do.");
        //Again, he wants to attack for test room purposes
        StartCoroutine(Attack());
    }

    IEnumerator Attack()
    {
        //Attacking stuff goes here

        currentTarget.unit.Attacked(_battler.damage);
        float rate = Vector3.Distance(currentTarget.unit.transform.position, myBoy.position) / attackTime;
        _battler.animator.AttackAnim(currentTarget.unit.transform.position, rate);
        yield return new WaitForSeconds(attackTime);

        UnitDone();
    }

    //Making one exit point so that I don't have a bunch of escapes.
    //Everyone comes through here.
    void UnitDone()
    {
        path = null;
        BattleManager.instance.UnitsDone();
    }

    /*
    public void OnDrawGizmos()
    {
        if (path != null)
        {
            for (int i = targetIndex; i < path.Length; i++)
            {
                Gizmos.color = Color.black;
                Gizmos.DrawCube(path[i], Vector3.one);

                if (i == targetIndex)
                {
                    Gizmos.DrawLine(transform.position, path[i]);
                }
                else
                {
                    Gizmos.DrawLine(path[i - 1], path[i]);
                }
            }
        }
    }
    */
}
