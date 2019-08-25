using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShadowCalcs : MonoBehaviour
{
    public Tilemap ground;
    public Tilemap shadowMap;
    public Tile shadow;
    public List<Transform> positionOfPlayers;
    public GridManager grid;

    private void Start()
    {
        Stopwatch cool = new Stopwatch();
        cool.Start();

        shadowMap.size = ground.size;
        shadowMap.origin = ground.origin;

        for (int x = -ground.size.x / 2; x < ground.size.x/2; x++)
        {
            for (int y = -ground.size.y / 2; y < ground.size.y/2; y++)
            {
                shadowMap.SetTile(new Vector3Int(x,y,0), shadow);
            }
        }

        foreach (Transform t in positionOfPlayers)
            StartCoroutine(Poop(t.position, 3));
            //ClearShadows(t.position, 3);

        cool.Stop();
        print(cool.Elapsed);
    }

    public void ClearShadows(Vector3 position, int distance)
    {
        //Use grid to get neighbors
        //Use breadth first search to do this

        //These are the ones to be checked
        Queue<Node> openset = new Queue<Node>();
        //These are the ones that have been checked
        Stack<Node> closedSet = new Stack<Node>();
        //These are the ones we need
        Stack<Node> goodOnes = new Stack<Node>();

        Node startNode = grid.NodeFromWorldPoint(position);
        
        openset.Enqueue(startNode);

        while(openset.Count > 0)
        {
            Node current = openset.Dequeue();

            closedSet.Push(current);

            //print(GetDistance(startNode, current));
            if (GetDistance(startNode, current) > distance)
                continue;

            goodOnes.Push(current);

            foreach(Node n in grid.GetNeighbours(current))
            {
                if (closedSet.Contains(n) || !current.walkable)
                    continue;

                openset.Enqueue(n);
            }
        }

        //print(closedSet.Count);

        foreach(Node n in goodOnes)
        {
            Vector3Int nodePos = shadowMap.WorldToCell(n.worldPosition);
            shadowMap.SetTile(nodePos, null);
        }

        //Here
        Vector3Int startPos = shadowMap.WorldToCell(position);
        shadowMap.SetTile(startPos, null);
    }

    IEnumerator Poop(Vector3 position, int distance)
    {
        //Use grid to get neighbors
        //Use breadth first search to do this

        //These are the ones to be checked
        Queue<Node> openset = new Queue<Node>();
        //These are the ones that have been checked
        Stack<Node> closedSet = new Stack<Node>();
        //These are the ones we need
        Stack<Node> goodOnes = new Stack<Node>();

        Node startNode = grid.NodeFromWorldPoint(position);

        openset.Enqueue(startNode);

        while (openset.Count > 0)
        {
            Node current = openset.Dequeue();

            closedSet.Push(current);

            //print(GetDistance(startNode, current));
            if (GetDistance(startNode, current) > distance)
                continue;

            goodOnes.Push(current);

            foreach (Node n in grid.GetNeighbours(current))
            {
                if (closedSet.Contains(n) || !current.walkable)
                    continue;

                openset.Enqueue(n);
            }
        }

        //print(closedSet.Count);

        Stack<Node> temp = new Stack<Node>();

        while(goodOnes.Count != 0)
            temp.Push(goodOnes.Pop());

        foreach (Node n in temp)
        {
            Vector3Int nodePos = shadowMap.WorldToCell(n.worldPosition);
            shadowMap.SetTile(nodePos, null);
            yield return new WaitForSeconds(0.25f);
        }

        //Here
        Vector3Int startPos = shadowMap.WorldToCell(position);
        shadowMap.SetTile(startPos, null);
    }


    float GetDistance(Node nodeA, Node nodeB)
    {
        return Mathf.Sqrt(Mathf.Pow(nodeA.gridX - nodeB.gridX, 2) + Mathf.Pow(nodeA.gridY - nodeB.gridY, 2));
    }
}
