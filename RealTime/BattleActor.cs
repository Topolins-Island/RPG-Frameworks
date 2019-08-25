using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This class is the parent of everything that could possibly act,
//since all actors will be adding themselves to the line.
//For instance, a damage over time effect would inherit from this,
//and add itself to the line whenever it needs to inflict damage.
public class BattleActor : MonoBehaviour
{
    public virtual void Act()
    {

    }
}
