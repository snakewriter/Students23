using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerGameField : GameField
{
    public enum AttackResult
    {
        Error, Misdelivered = 2, Sunk, Hit
    }

    protected Animator[,] cellsAnimators;
    protected ShipsInfoStorage shipsInfoStorage = new ShipsInfoStorage();
    protected bool toDiscloseFloorCells = true, isGameOver = false;

    protected static PlayerGameField currentPlayer;
    protected PlayerGameField enemy = null;
    protected static int targetX, targetY;
    protected static bool hasBeenInput = false;


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
        Settings.enemyInitialized += OnEnemyInitialized;
        Settings.RegisterGameField(this);
        base.Start();
    }

    private void OnEnemyInitialized(PlayerGameField gameField)
    {
        if (gameField.GetType() != GetType()) RegisterEnemy(gameField);
    }

    void RegisterEnemy(PlayerGameField gameField)
    {
        enemy = gameField;
        if (this is PlayerGameField) currentPlayer = this;
    }

    protected override void OnCellGenerated(GameObject cell, int x, int y)
    {
        base.OnCellGenerated(cell, x, y);
        var cellAnimator = cell.GetComponent<Animator>();
        cellsAnimators[x, y] = cellAnimator;
        body[x, y] = GetCellValue(x, y);
        if (body[x, y] == CellState.Occupied) shipsInfoStorage.RegisterShipFloor(x, y);
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

        result = (AttackResult)Enum.Parse(typeof(AttackResult), body[x, y].ToString());
        if (result == AttackResult.Hit)
        {
            result = DamageShip(x, y);
            if (shipsInfoStorage.AreAllShipsSunk()) isGameOver = true;
        }
        else currentPlayer = this;
        cellsAnimators[x, y].SetInteger("CellState", (int)result);
        return result;
    }

    protected bool CanReceiveAttack(int x, int y)
    {
        var result = !Equals(currentPlayer) &&
            body[x, y] != CellState.Misdelivered && body[x, y] != CellState.Hit;
        return result;
    }

    AttackResult DamageShip(int x, int y)
    {
        var damagedShip = shipsInfoStorage.GetShipInfo(x, y);
        damagedShip.HitFloor();
        if (damagedShip.leftFloorsCount == 0)
        {

            return AttackResult.Sunk;
        }      
        return AttackResult.Hit;
    }
}
