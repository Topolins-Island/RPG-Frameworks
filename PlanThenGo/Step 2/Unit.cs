using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    public float speed = 0;
    
    public virtual void Act()
    {
        print(gameObject.name);
    }
}
