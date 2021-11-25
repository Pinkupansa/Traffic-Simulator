using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct VehicleData
{
    
    public float speed;
    public float sightRadius;
    public float reactionTime;
    public float maxCarSpeed;
    public Vector2 position;
    public Vector2 target;
    public Vector2 destination;
    public int roadPointID;
    public int hasToChangePoint;
    public int moving;
    public float imprudence;
    public int crashedWithCar;
    
}
