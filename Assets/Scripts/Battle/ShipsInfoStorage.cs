using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipsInfoStorage 
{
    protected Dictionary<Vector2, ShipBattleInfo> navy = new Dictionary<Vector2, ShipBattleInfo>();
    ShipBattleInfo newFloorInfo;

    public void RegisterShipFloor(int x, int y)
    {
        newFloorInfo = new ShipBattleInfo(x, y);
        var newFloorPos = new Vector2(x, y);
        navy.Add(newFloorPos, newFloorInfo);
        SearchForNearbyFloors(newFloorPos);
    }

    public ShipBattleInfo GetShipInfo(int x, int y)
    {
        return TryGetShip(new Vector2(x, y));
    }

    public bool AreAllShipsSunk()
    {
        foreach (var shipInfo in navy.Values)
            if (shipInfo.leftFloorsCount > 0) return false;
        return true;
    }

    ShipBattleInfo TryGetShip(Vector2 key)
    {
        ShipBattleInfo result = null;
        if (navy.ContainsKey(key)) result = navy[key];
        return result;
    }

    void SearchForNearbyFloors(Vector2 newFloorPos)
    {
        CheckNearbyFloor(newFloorPos + Vector2.left, newFloorPos);
        CheckNearbyFloor(newFloorPos + Vector2.up, newFloorPos);
        CheckNearbyFloor(newFloorPos + Vector2.right, newFloorPos);
        CheckNearbyFloor(newFloorPos + Vector2.down, newFloorPos);
    }

    void CheckNearbyFloor(Vector2 nearbyFloorPos, Vector2 newFloorPos)
    {
        var hasNeighbour = navy.ContainsKey(nearbyFloorPos);
        if (hasNeighbour) MergeFloors(nearbyFloorPos);
    }

    void MergeFloors(Vector2 existingFloorPos)
    {
        newFloorInfo.MergeAnotherShipInfo(navy[existingFloorPos]);
        var floorsToRebind = newFloorInfo.GetAllFloors();
        foreach (var prevFloorPos in floorsToRebind)
            navy[prevFloorPos] = newFloorInfo;
    }

}
