using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyGameField : PlayerGameField
{

    public EnemyGameField()
    {
        originObjName = "EnemyFieldOrigin";
        //toDiscloseFloorCells = false; закомментировано на время тестирования
        BattleCell.onCellClick += OnBattleCellClick;
    }

    private void OnBattleCellClick(GameObject cell)
    {
        // проверим можно ли регистрировать клик

        var cellNormalPos = GetCellMatrixPos(cell.transform.position);
        targetX = (int)cellNormalPos.x;
        targetY = (int)cellNormalPos.y;

        if (body[targetX, targetY] == CellState.Misdelivered ||
            body[targetX, targetY] == CellState.Hit) return;
        hasBeenInput = true;


        Attack(targetX, targetY);

        hasBeenInput = false;
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
