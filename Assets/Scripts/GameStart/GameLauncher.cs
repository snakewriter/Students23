using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLauncher : AutoAllocator
{
    GameObject errorMessagePanel;
    bool toStartGame;


    public void OnGameStartButtonClick()
    {
        if (Dispatcher.AreAllShipsAllocated()) StartGame();
        else errorMessagePanel.SetActive(true);
    }

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        onAutoAllocationCompleted += OnAutoAllocationCompleted;
        errorMessagePanel = transform.Find("ErrorMessagePanel").gameObject;
        toStartGame = false;
    }

    void StartGame()
    {
        Settings.PlayerField = (CellState[,])body.Clone();
        toStartGame = true;
        OnAutoLocateClick();
    }

    void OnAutoAllocationCompleted()
    {
        if (!toStartGame) return;

        Settings.EnemyField = (CellState[,])body.Clone();
        Settings.ChangeScene("Battle");
    }
}
