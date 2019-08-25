using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;

public class PlayerUnitController : MonoBehaviour
{
    public Transform myBoy;
    public Battler battler;
    List<Vector3Int> possiblePositions;
    public Tilemap particlesMap;
    public AnimatedTile posMoveTile;
    public Tile selectedEnemyTile;
    public AnimatedTile notSelectedEnemyTile;


    [Header("Settings")]
    public float attackTime;
    public float moveSpeed;

    [Header("Selection Stuff")]
    public Image actionImage;
    public Sprite attackSelection;
    public Sprite staySelection;

    public void HeresYourKid(Transform current)
    {
        myBoy = current;
        battler = myBoy.GetComponent<Battler>();
        //Get all the possible positions
        possiblePositions = new List<Vector3Int>();
        possiblePositions = BattleManager.instance.GiveAllWithinDistanceOf(myBoy.transform.position, 4, 0);
        StartCoroutine(Movement());
    }

    IEnumerator Movement()
    {
        yield return null;

        //Highlight them
        foreach (Vector3Int n in possiblePositions)
        {
            particlesMap.SetTile(n, posMoveTile);
        }

        //Start the movement script
        IEnumerator _movePlayerChar = MovePlayerChar();
        StartCoroutine(_movePlayerChar);

        while (!Input.GetKeyDown(KeyCode.Joystick1Button0) && !Input.GetKeyDown(KeyCode.Z))
        {
            if (Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.X))
            {
                print("go to looking at the field");
            }

            yield return null;
        }

        StopCoroutine(_movePlayerChar);

        //Clear the highlights
        ClearParticles();

        //Here would be where the player chooses different actions to perform
        //But for now they just attack
        StartCoroutine(FindTargets());
    }

    IEnumerator FindTargets()
    {
        //All the enemies in the fight currently
        List<Battler> enemies = BattleManager.instance.GiveEnemies();

        //All the current unit's attack positions, this method is special as it ignores the units in the pathfinding grid
        List<Vector3Int> atkPoints = BattleManager.instance.grid.GiveAllWithinDistanceOf(myBoy.position,
            battler.attackDistance.outer, battler.attackDistance.inner, false);

        //Storing all the applicable targets (battlers)
        List<Battler> targets = new List<Battler>();

        //And their locations
        List<Vector3Int> targetPoses = new List<Vector3Int>();

        //Highlight the possible positions to attack
        foreach (Vector3Int n in atkPoints)
        {
            particlesMap.SetTile(n, posMoveTile);
        }

        //This checks enemy positions to our possible attack position
        foreach (Battler enem in enemies)
        {
            //Get the vector3int for multiple uses
            Vector3Int enemPos = BattleManager.instance.grid.walkable.WorldToCell(enem.transform.position);
            //If they are in a position attackable
            if (atkPoints.Contains(enemPos))
            {
                //If they is add them to the lists
                targets.Add(enem);
                targetPoses.Add(enemPos);
            }

            yield return null;
        }

        StartCoroutine(ChoosingAction(targetPoses, targets));
    }

    IEnumerator ChoosingAction(List<Vector3Int> enemyPos, List<Battler> targets)
    {
        actionImage.gameObject.transform.parent.gameObject.SetActive(true);

        //Draw all the choosable actions
        //For now, we will have stay and attack.

        Vector2 input = Vector2.zero;
        Vector2 oldInput = Vector2.zero;

        //If there's no enemies within range, default to staying
        if (enemyPos.Count == 0)
            input = Vector2.down;
        else
            input = Vector2.up;

        //Set the sprites
        if (input == Vector2.up)
            actionImage.sprite = attackSelection;
        if (input == Vector2.down)
            actionImage.sprite = staySelection;

        while (true)
        {
            //Regular input validation
            while (input == oldInput)
            {
                input = new Vector2(Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

                if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Z))
                {
                    if(actionImage.sprite == attackSelection)
                    {
                        if (enemyPos.Count > 0)
                        {
                            StartCoroutine(ChoosingTargets(enemyPos, targets));
                            yield break;
                        }
                        else
                            print("No enemies in range, should make a noise for this eventually");
                    }
                    if (actionImage.sprite == staySelection)
                    {
                        TurnFinished();
                        yield break;
                    }
                }

                if (Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.X))
                {
                    print("go back to movement");
                    actionImage.gameObject.transform.parent.gameObject.SetActive(false);
                    ClearParticles();
                    StartCoroutine(Movement());
                    yield break;
                }

                yield return null;
            }

            oldInput = input;

            //This is to prevent superfluous checks, if it's zero nothing happens 
            if (input == Vector2.zero)
                continue;

            //Set the sprites
            if (input == Vector2.up)
                actionImage.sprite = attackSelection;
            if (input == Vector2.down)
                actionImage.sprite = staySelection;

            yield return null;
        }
    }

    IEnumerator ChoosingTargets(List<Vector3Int> enemPos, List<Battler> targets)
    {
        yield return null;

        //This int is what controls which index in the enemPos list the player is looking to attack
        int selection = 0;
        /*
        foreach (Vector3Int pos in enemPos)
        {
            if (pos == enemPos[selection])
                particlesMap.SetTile(pos, selectedEnemyTile);
            else
                particlesMap.SetTile(pos, notSelectedEnemyTile);
        }*/

        Vector2 input = Vector2.zero;
        Vector2 oldInput = Vector2.zero;

        while (true)
        {
            foreach (Vector3Int pos in enemPos)
            {
                if (pos == enemPos[selection])
                    particlesMap.SetTile(pos, selectedEnemyTile);
                else
                    particlesMap.SetTile(pos, notSelectedEnemyTile);
            }

            //The input validation loop we've been using
            while (input == oldInput)
            {
                input = new Vector2(Input.GetAxisRaw("Horizontal"), -Input.GetAxisRaw("Vertical"));

                //This is the only place we exit this coroutine

                //Choosing a target
                if (Input.GetKeyDown(KeyCode.Joystick1Button0) || Input.GetKeyDown(KeyCode.Z))
                {
                    StartCoroutine(Attacking(targets[selection]));
                    yield break;
                }

                //Going backwards
                if (Input.GetKeyDown(KeyCode.Joystick1Button1) || Input.GetKeyDown(KeyCode.X))
                {
                    print("go back to selecting");
                    ClearParticles();
                    StartCoroutine(ChoosingAction(enemPos, targets));
                    yield break;
                }

                yield return null;
            }

            oldInput = input;

            if (input == Vector2.zero)
                continue;

            //Choosing a different enemy is here
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Abs(Input.GetAxis("Vertical")))
            {
                selection += Input.GetAxis("Horizontal") > 0 ? 1 : -1;
            }
            if (Mathf.Abs(Input.GetAxis("Vertical")) > Mathf.Abs(Input.GetAxis("Horizontal")))
            {
                selection += -Input.GetAxis("Vertical") > 0 ? 1 : -1;
            }

            if (selection < 0)
                selection = targets.Count - 1;
            if (selection >= targets.Count)
                selection = 0;
            /*
            foreach(Vector3Int pos in enemPos)
            {
                if (pos == enemPos[selection])
                    particlesMap.SetTile(pos, selectedEnemyTile);
                else
                    particlesMap.SetTile(pos, posMoveTile);
            }
            */
            yield return null;
        }
    }

    //Dealing the damage
    IEnumerator Attacking(Battler target)
    {
        //This coroutine takes care of animations
        //But for now it's instantaneous

        ClearParticles();
        yield return null;

        float rate = Vector3.Distance(target.transform.position, myBoy.position) / attackTime;
        
        battler.animator.AttackAnim(target.transform.position, rate);

        yield return new WaitForSeconds(attackTime);

        target.Attacked(battler.damage);

        yield return null;
        TurnFinished();
    }

    IEnumerator MovePlayerChar()
    {
        Vector3 startPos = myBoy.position;
        Vector2 input = Vector2.zero;
        Vector2 oldInput = Vector2.zero;

        while (true)
        {
            //Input verification
            while (input == oldInput)
            {
                input = new Vector2(Mathf.RoundToInt(Input.GetAxisRaw("Horizontal")), Mathf.RoundToInt(Input.GetAxisRaw("Vertical")));
                yield return null;
            }
            oldInput = input;

            if (input == Vector2.zero)
                continue;

            //Direction verification
            if (Mathf.Abs(Input.GetAxis("Horizontal")) > Mathf.Abs(Input.GetAxis("Vertical")) && Input.GetAxis("Horizontal") != 0)
            {
                Vector2 dir = new Vector2(Input.GetAxisRaw("Horizontal") > 0 ? 1 : -1, 0);
                StartCoroutine(MoveChar(myBoy.position + (Vector3)dir, dir));
            }
            if (Mathf.Abs(Input.GetAxis("Vertical")) > Mathf.Abs(Input.GetAxis("Horizontal")) && Input.GetAxis("Vertical") != 0)
            {
                Vector2 dir = new Vector2(0, Input.GetAxisRaw("Vertical") > 0 ? -1 : 1);
                StartCoroutine(MoveChar(myBoy.position + (Vector3)dir, dir));
            }
            while (moving)
                yield return null;

            yield return null;
        }
    }

    bool moving = false;
    IEnumerator MoveChar(Vector3 finalPos, Vector2 dir)
    {
        moving = true;

        if (!possiblePositions.Contains(BattleManager.instance.grid.walkable.WorldToCell(finalPos)))
        {
            moving = false;
            battler.animator.AnimateInDirection(dir, AnimType.IDLE);
            yield break;
        }

        battler.animator.AnimateInDirection(dir, AnimType.MOVING);

        while (myBoy.position != finalPos)
        {
            myBoy.position = Vector3.MoveTowards(myBoy.position, finalPos, Time.deltaTime * moveSpeed);
            yield return null;
        }

        battler.animator.AnimateInDirection(dir, AnimType.IDLE);
        moving = false;
    }

    void TurnFinished()
    {
        ClearParticles();
        actionImage.gameObject.transform.parent.gameObject.SetActive(false);
        BattleManager.instance.UnitsDone();
    }

    //Apparently there's something for this already...
    void ClearParticles()
    {
        //Clear the highlights
        particlesMap.ClearAllTiles();
    }
}
