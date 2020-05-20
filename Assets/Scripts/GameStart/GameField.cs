using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameField : MonoBehaviour
{
    public enum CellState
    {
        Empty, Misdelivered, Occupied, Hit
    }

    public GameObject cellPrefab;
    public Vector2 originBottomLeft;
    public bool toAdjustOrigin = false;
    public static float cellHeightToCameraHeightY { get; protected set; } = 1 / 12f;

    protected static CellState[,] body = new CellState[10, 10];
    protected static CellState cellStateToSet;
    protected static Bounds[,] boundsOfCells;
    static Vector2 bottomLeftCellMinCorner;
    protected static float cellSize;
    protected string originObjName = "GameFieldOrigin";
    GameObject origin;

    protected virtual void Start()
    {
        origin = GameObject.Find(originObjName); // Получаем объект, положение которого
        // диктует положение игрового поля
        origin.transform.position = originBottomLeft; // Ставим его на заданную позицию

        var sprRenderer = cellPrefab.GetComponent<SpriteRenderer>();
        Settings.ScaleSpriteByY(sprRenderer, cellHeightToCameraHeightY, out cellSize);
        GenerateField(); // Генерируем поле, зная размер ячеек и их начальную позицию
        bottomLeftCellMinCorner = boundsOfCells[0, 0].min; // Запоминаем его минимальную точку
    }

    void Update()
    {
        origin.transform.position = originBottomLeft; // Позволяет двигать игровое поле из
        // инспектора (только для начальной регулировки координат)
    }

    void GenerateField()
    {
        boundsOfCells = new Bounds[Width(), Height()]; // Создаем массив игрового поля
        for (int x = 0; x < Width(); x++) GenerateColumn(x); // Проходим по каждой колонке
    }

    void GenerateColumn(int x)
    {
        for (int y = 0; y < Height(); y++) GenerateCell(x, y); // Проходим по каждой клетке в колонке
    }

    void GenerateCell(int x, int y)
    {
        var cellPos = new Vector2(originBottomLeft.x + x * cellSize, originBottomLeft.y + y * cellSize);
        // Вычисляем позицию этой конкретной клетки
        var cell = Instantiate(cellPrefab, cellPos, Quaternion.identity); // Генерируем ее там
        // Привязываем к общему родителю (только для того, чтобы можно было настраивать положение)
        OnCellGenerated(cell, x, y);
    }

    protected virtual void OnCellGenerated(GameObject cell, int x, int y)
    {
        cell.transform.SetParent(origin.transform); 
        // Запоминаем границы периметра этой клетки (в координатах сцены)
        boundsOfCells[x, y] = new Bounds(cell.transform.position, new Vector3(cellSize, cellSize));
        body[x, y] = CellState.Empty; // Делаем эту клетку в массиве незанятой по умолчанию
    }

    public static int Width()
    {
        return body.GetLength(0);
    }

    public static int Height()
    {
        return body.GetLength(1);
    }

    public void CheckLocationOverField(Ship ship, Vector3 mousePos)
    {
        // Получаем максимальную точку игрового поля на сцене (можно было так же запомнить ее
        // и в методе Start вместе с минимальной)
        var upperRightBounds = boundsOfCells[Width() - 1, Height() - 1].max;
        // Смотрим, попадает ли в итоге указатель в поле между этими точками 
        var isShipOverField = mousePos.x > bottomLeftCellMinCorner.x && 
            mousePos.x < upperRightBounds.x &&
            mousePos.y > bottomLeftCellMinCorner.y && mousePos.y < upperRightBounds.y;
        if (!isShipOverField) // Указатель не попадает - кораблик не попадает тоже
        {
            ship.isPositionCorrect = false;
            ship.isWithinCell = false;
            return;
        }

        // Указатель попадает - значит обязательно над одной из клеток
        var cellMatrixPos = GetCellMatrixPos(mousePos); //  Смотрим, которая это по счету
        int x = (int)cellMatrixPos.x, y = (int)cellMatrixPos.y;
        ship.isWithinCell = true; // Указываем кораблику, что он над клеткой
        ship.cellCenterPosition = boundsOfCells[x, y].center; // Указываем, где ее центр
        ship.isPositionCorrect = IsShipPositionAppropriate(ship, x, y); // Проверяем,
        // целиком ли он над полем и на расстоянии ли от остальных
    }

    static bool IsShipPositionAppropriate(Ship ship, int x, int y)
    {
        for (int i = 0; i < ship.floorsNum; i++) // Проверяем каждую палубу
            if (!IsCellLocationAppropriate(ship, ref x, ref y))
                return false;
        return true;
    }

    static bool IsCellLocationAppropriate(Ship ship, ref int x, ref int y)
    {
        if (!AreSurroundingCellsEmpty(ship, x, y)) // Проверяем, пуста ли клетка под палубой            
            return false; // и окружающие ее
        ShiftCoordinate(ship, ref x, ref y); // Смещаем координату следующей палубы 
        return true; // в зависимости от ориентации кораблика
    }

    static void ShiftCoordinate(Ship ship, ref int x, ref int y)
    {
        if (ship.orientation == Ship.Orientation.Horizontal) x++;
        else y--;
    }

    static bool AreSurroundingCellsEmpty(Ship ship, int x, int y)
    {
        if (!IsPointWithinMatrix(x, y)) return false; // to check surrounding cells
        var dx = new int[] { 1, 1, 0, -1, -1, -1, 0, 1, 0 }; 
        var dy = new int[] { 0, -1, -1, -1, 0, 1, 1, 1, 0 };
        for (int j = 0; j < 9; j++)
            if (!IsSurroundingCellEmpty(x + dx[j], y + dy[j]))
                return false;
        return true;
    }

    static bool IsSurroundingCellEmpty(int shiftX, int shiftY)
    {
        var isPosAppropr = !IsPointWithinMatrix(shiftX, shiftY) || // Окружающая клетка либо
            body[shiftX, shiftY] != CellState.Occupied; //  свободна, либо ее нет с той стороны
        if (!isPosAppropr) return false;
        else return true;
    }

    public static bool IsPointWithinMatrix(int x, int y)
    {
        return Settings.IsPointWithinMatrix(x, y, body);
    }

    public static void RegisterShip(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Occupied);
    }

    public static void TakeShipOff(Ship ship)
    {
        SetCellsStateUnderneathShip(ship, CellState.Empty);
    }

    static void SetCellsStateUnderneathShip(Ship ship, CellState cellState)
    {
        cellStateToSet = cellState;
        var cellNormalPos = GetCellMatrixPos(ship.transform.position);
        int x = (int)cellNormalPos.x, y = (int)cellNormalPos.y;
        for (int i = 0; i < ship.floorsNum; i++) SetCellState(ship, ref x, ref y);

        //PrintField(body);
    }

    static void SetCellState(Ship ship, ref int x, ref int y)
    {
        body[x, y] = cellStateToSet;
        ShiftCoordinate(ship, ref x, ref y);
    }

    public static void PrintField(CellState[,] field)
    {
        for (int i = Height() - 1; i >= 0; i--) // Displaying field matrix
        {
            var line = "";
            for (int j = 0; j < Width(); j++)
            {
                var cellVal = (int)field[j, i];
                line += (cellVal == 0 ? "_" : "x") + "  ";
            }
            Debug.Log(line);
        }
        Debug.Log("");
    }

    protected static Vector2 GetCellMatrixPos(Vector2 pointInField)
    {
        // Зная, где геометрически начинается игровое поле и какого размера клетки
        // определяем номер клетки в заданной точке сцены (или экрана)
        var dx = pointInField.x - bottomLeftCellMinCorner.x;
        var dy = pointInField.y - bottomLeftCellMinCorner.y;
        int nx = (int)(dx / cellSize), ny = (int)(dy / cellSize);
        return new Vector2(nx, ny);
    }
}