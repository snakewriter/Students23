using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameField;

public static class Settings 
{
    public static event Action<PlayerGameField> enemyInitialized;
    public static CellState[,] PlayerField { get; set; }
    public static CellState[,] EnemyField { get; set; }

    static List<PlayerGameField> gameFields = new List<PlayerGameField>(2);

    public static Vector2 ConvertLinearCoordinateToDecart(int i, int width)
    {
        //Линейная координата - последовательный счетчик по рядам ИЛИ колонкам
        return new Vector2(i % width, i / width); // Разворачиваем такой счетчик в пару
        // координат посредством деления
    }
         
    public static int ConvertDecartCoordinatesToLinear(int x, int y, int width)
    {
        return y * width + x; // Сворачиваем пару координат в линейный счетчик
    }

    public static void ChangeScene(string sceneName)
    {
        if (sceneName == "Battle") SetComplexityLevel();
        SceneManager.LoadScene(sceneName);
    }

    public static bool IsPointWithinMatrix<T>(int x, int y, T[,] matrix)
    {
        // Точка попадает внутрь массива, если принадлежит промежутку [0; длина массива)
        int width = matrix.GetLength(0), height = matrix.GetLength(1);
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public static void ScaleSpriteByY(SpriteRenderer sr, float yScale, out float spriteSize)
    {
        var currentSize = sr.bounds.size.x;
        var desiredSize = Camera.main.orthographicSize * 2 * yScale;
        var scaleFactor = desiredSize / currentSize;
        sr.transform.localScale *= scaleFactor;
        spriteSize = sr.bounds.size.x;
    }

    public static void RegisterGameField(PlayerGameField gameField)
    {
        gameFields.Add(gameField);
        foreach (var gField in gameFields) enemyInitialized(gField);
    }

    static void SetComplexityLevel()
    {
        // Здесь будем выставлять ИИ компьютерного противника
    }
}
