using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is the parent of everything that could possibly act,
//since all actors will be adding themselves to the line
public class BattleActor : MonoBehaviour
{
    public virtual void Act()
    {

    }
}
