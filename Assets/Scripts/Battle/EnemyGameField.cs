using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGameField : PlayerGameField
{

    public EnemyGameField()
    {
        originObjName = "EnemyFieldOrigin";
        //toDiscloseFloorCells = false; закомментировано на время тестирования
    }

    // Start is called before the first frame update
    protected override void Start()
    {

        base.Start();
    }

    protected override CellState GetCellValue(int x, int y)
    {
        return Settings.EnemyField[x, y];
    }
}
