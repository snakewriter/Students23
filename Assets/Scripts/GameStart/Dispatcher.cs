using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Dispatcher : MonoBehaviour
{
    static Dictionary<string, int> shipsLeftToAllocate = new Dictionary<string, int>();
    static Dictionary<string, Text> shipsLabels = new Dictionary<string, Text>();
    static List<Dispatcher> allShips = new List<Dispatcher>();

    public GameObject shipPrefab;
    public static Ship currentShip;
    protected string dictKey;
    bool isWorkingInstance = true;
    static bool isAutoLocation = false;



    // Start is called before the first frame update
    protected virtual void Start()
    {
        isWorkingInstance = gameObject.name.Contains("(Clone)");
        dictKey = gameObject.name.Replace("(Clone)", null);
        if (!isWorkingInstance) FillLabelsDict();
        if (!isAutoLocation) allShips.Add(this);
        
        var shipKindCounter = 5 - int.Parse(dictKey.Replace("Ship-", null));
        if (!shipsLeftToAllocate.ContainsKey(dictKey))
            shipsLeftToAllocate.Add(dictKey, shipKindCounter);
        RefreshLabel();
    }

    void OnDestroy()
    {
        allShips.Remove(this);
    }

    static Dispatcher[] GetAllShips(bool templateOnes)
    {
        var result = new List<Dispatcher>();
        foreach (var disp in allShips)
            if (disp.isWorkingInstance ^ templateOnes) result.Add(disp);
        return result.ToArray();
    }

    void CreateAllClonesOfType()
    {
        for (int i = 0; i < shipsLeftToAllocate[dictKey]; i++)
        {
            var ship = Instantiate(shipPrefab, transform.parent.transform);
            allShips.Add(ship.GetComponent<Ship>());
        }
        shipsLeftToAllocate[dictKey] = 0;
    }

    public static Dispatcher[] CreateAllShips()
    {
        isAutoLocation = true; // Отключаем добавление в общий список в методе Start
        var templateShips = GetAllShips(true); // Получаем все витринные
        foreach (var tmplShip in templateShips) // От каждого из них создаем игровые
            tmplShip.CreateAllClonesOfType();
        return GetAllShips(false); // Получаем и возвращаем все игровые
    }

    public static bool AreAllShipsAllocated()
    {
        foreach (var counter in shipsLeftToAllocate.Values)
            if (counter > 0) return false;
        return true;
    }

    void FillLabelsDict()
    {
        var textBlock = GameObject.Find(dictKey + " label").GetComponent<Text>();
        var labelKey = textBlock.name.Replace(" label", null);
        if (shipsLabels.ContainsKey(labelKey)) return;
        shipsLabels.Add(labelKey, textBlock);
    }

    protected void OnShipClick()
    {
        if (isWorkingInstance) TakeShipOrChangePosition();
        else if (currentShip == null) CreateWorkingInstance();
    }

    void CreateWorkingInstance()
    {
        if (shipsLeftToAllocate[dictKey] == 0) return;
        var shipObjToPlay = Instantiate(shipPrefab, transform.parent.transform);
        currentShip = shipObjToPlay.GetComponentInChildren<Ship>();
    }

    void TakeShipOrChangePosition()
    {
        if (currentShip == null) TakeShipOff();
        else if (currentShip.isPositionCorrect) PlaceShip();
    }

    void TakeShipOff()
    {
        currentShip = GetComponentInChildren<Ship>();
    }

    void PlaceShip()
    {
        if (!currentShip.wasAllocatedOnce) shipsLeftToAllocate[dictKey]--;
        RefreshLabel();
        currentShip = null;
    }

    void RefreshLabel()
    {
        shipsLabels[dictKey].text = shipsLeftToAllocate[dictKey] + "x";
    }
}
