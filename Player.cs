using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Player : MonoBehaviour
{
    /*
    public Camera myCam;
    public Tilemap floor;
    public bool drawGizmos = true;
    public Interactable target;
    public float speed = 20;
    public bool stopBefore = false;

    Vector3 currentWaypoint;
    Vector3[] path;
    int targetIndex;
    bool changedPath = false;
    bool going = false;

    public ShadowCalcs shadowGuy;

    void Start()
    {
        //Set up stats

        //PathRequestManager.RequestPath(transform.position, target.position, OnPathFound);
    }

    private void Update()
    {
        if(going)
            transform.position = Vector3.MoveTowards(transform.position, currentWaypoint, speed * Time.deltaTime);

        if (Input.GetMouseButtonDown(0) && LevelManager.instance.mouseOverViewport(myCam, Camera.main))
        {
            //Reset stats
            target = null;
            stopBefore = false;

            //Figure out where to go
            Vector3 pos = myCam.ScreenToWorldPoint(Input.mousePosition);
            Vector3Int cellPosition = floor.WorldToCell(pos);
            if (floor.HasTile(cellPosition))
            {
                //print("yep");
                PathRequestManager.RequestPath(transform.position, floor.GetCellCenterWorld(cellPosition), OnPathFound);
            }
        }
    }

    public void OnPathFound(Vector3[] newPath, bool pathSuccessful)
    {
        if (pathSuccessful)
        {
            path = newPath;
            targetIndex = 0;
            changedPath = true;
            StartCoroutine(NecessaryEvil());
        }
        else
        {
            print("nop");
        }
    }

    IEnumerator NecessaryEvil()
    {
        yield return null;
        if (path.Length > 0)
        {
            changedPath = false;
            StartCoroutine(FollowPath());
        }
        else
        {
            print("you clicked urself u silly billy");
        }
    }

    IEnumerator FollowPath()
    {
        if (stopBefore && path.Length <= 1)
        {
            if (target != null)
                target.Interact(this);

            yield break;
        }

        currentWaypoint = path[0];
        going = true;
        while (!changedPath)
        {
            if(Vector3.Distance(transform.position, currentWaypoint) < 0.1f)
            {
                targetIndex++;
                
                if (targetIndex >= (path.Length + (stopBefore ? -1 : 0)))
                {
                    //print("path done");
                    if(target != null)
                    {
                        target.Interact(this);
                    }
                    yield break;
                }

                currentWaypoint = path[targetIndex];
                shadowGuy.ClearShadows(currentWaypoint, 3);
            }

            yield return null;
        }

        going = false;
    }

    public void OnDrawGizmos()
    {
        if (path != null && drawGizmos)
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
