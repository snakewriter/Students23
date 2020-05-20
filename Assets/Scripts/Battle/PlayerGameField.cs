using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameField : GameField
{
    public enum AttackResult
    {
        Misdelivered, Hit, Sunk, Error
    }

    protected Animator[,] cellsAnimators;
    protected bool toDiscloseFloorCells = true;

    public PlayerGameField()
    {
        originObjName = "PlayerFieldOrigin";
        cellsAnimators = new Animator[Width(), Height()];
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        if (Camera.main.aspect < 2)
            cellHeightToCameraHeightY = Camera.main.aspect / 22f;
        base.Start();
    }

    protected override void OnCellGenerated(GameObject cell, int x, int y)
    {
        base.OnCellGenerated(cell, x, y);
        var cellAnimator = cell.GetComponent<Animator>();
        cellsAnimators[x, y] = cellAnimator;
        body[x, y] = GetCellValue(x, y);
        if (toDiscloseFloorCells) cellAnimator.SetInteger("CellState", (int)body[x, y]);
    }

    protected virtual CellState GetCellValue(int x, int y)
    {
        return Settings.PlayerField[x, y];
    }

    public AttackResult Attack(int x, int y)
    {
        var result = AttackResult.Error;
        if (!CanReceiveAttack(x, y)) return result;
        if (body[x, y] == CellState.Empty) body[x, y] = CellState.Misdelivered;
        else body[x, y] = CellState.Hit;

        return result;
    }

    protected bool CanReceiveAttack(int x, int y)
    {
        return true;
    }
}
