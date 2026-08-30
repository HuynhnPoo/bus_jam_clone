using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MoveDirection
{
    Up,
    Down,
    Left,
    Right
}

[System.Serializable]
public class VehicleData
{
    public int vehicleId;
    public int capacity;
    public int length;

    public Vector3Int gridPostion;
    public MoveDirection direction;
}

[System.Serializable]
public class PersonGroupData
{
    public int groupPersonId;
  //  public Color colorPerson;
    public int Count;
    public Vector3Int slotPosition;
    public int ownerVehicleId;
}
