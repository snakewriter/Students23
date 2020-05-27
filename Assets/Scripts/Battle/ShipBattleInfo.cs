using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipBattleInfo 
{
    List<Vector2> floors = new List<Vector2>();

    public int leftFloorsCount { get; protected set; } = 1;

    public ShipBattleInfo(int x, int y)
    {
        var newFloorPos = new Vector2(x, y);
        floors.Add(newFloorPos);
    }

    public void MergeAnotherShipInfo(ShipBattleInfo another)
    {
        floors.AddRange(another.floors);
        leftFloorsCount += another.leftFloorsCount;

    }

    public Vector2[] GetAllFloors()
    {
        var result = new Vector2[floors.Count];
        floors.CopyTo(result);
        return result;
    }

    public void HitFloor()
    {
        leftFloorsCount--;
    }
}
