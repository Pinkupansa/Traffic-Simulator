using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Vehicle")]
public class Vehicle : ScriptableObject
{
    public VehicleData data;
    public GameObject sprite;
}

