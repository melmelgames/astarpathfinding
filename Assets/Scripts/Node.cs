using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node 
{
    public Vector3 worldPosition;
    public bool walkable;

    public Node(bool _walkable, Vector3 _worldPosition){
        walkable = _walkable;
        worldPosition = _worldPosition;
    }
}