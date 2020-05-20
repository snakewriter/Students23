using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoAllocator : GameField
{
    public event System.Action onAutoAllocationCompleted;

    List<Bounds> spawnAreas = new List<Bounds>();
    Bounds selectedArea;
    Dispatcher[] playDispatchers;


    public void OnAutoLocateClick(bool allignAtField = false)
    {
        playDispatchers = Dispatcher.CreateAllShips();
        ClearGameField();
        // Изначально доступно все игровое поле. Добавляем в список его площадь
        spawnAreas.Add(new Bounds(new Vector3((float)Width() / 2, (float)Height() / 2),
            new Vector3(Width(), Height())));
        StartCoroutine(AutoLocateAllShips(allignAtField));
    }
      

    void ClearGameField()
    {
        // Обходим все клетки игрового поля развернуто
        for (int i = 0; i < body.Length; i++) ClearFieldCell(i);
    }

    /// <summary>
    /// Запись в каждую ячейку игрового поля статуса "Свободна"
    /// </summary>
    /// <param name="i">Линейная координата ячейки</param>
    void ClearFieldCell(int i)
    {
        // Получаем пару декартовых координат от "хелпера"
        var decartCoords = Settings.ConvertLinearCoordinateToDecart(i, Width());
        // Очищаем игровое поле
        body[(int)decartCoords.x, (int)decartCoords.y] = CellState.Empty;
    }

    IEnumerator AutoLocateAllShips(bool allignAtField)
    {
        // Дожидаемся инициализации всех кораблей (пока выполнятся скрипты)
        while (!AreAllShipsInitialized()) yield return null;
        foreach (Ship ship in playDispatchers)
        {
            // Расставляем каждый отдельно

            AutoLocateShip(ship);
            // Выравниваем визуально если нужно
            if (allignAtField) ship.SetAutolocated();
            RegisterShip(ship); 
        }
        spawnAreas.Clear(); // Области разбиения больше не нужны - удаляем их все
        onAutoAllocationCompleted?.Invoke(); // Вызываем событие "Корабли расставлены"
    }
       
    bool AreAllShipsInitialized()
    {
        foreach (Ship ship in playDispatchers)
            if (ship.floorsNum == 0) return false;
        return true;
    }

    void AutoLocateShip(Ship ship)
    {
        SelectArea(ship); // Готовим область для генерации координат
        // Генерируем координаты, которые всегда подойдут
        var x = Random.Range((int)selectedArea.min.x, (int)selectedArea.max.x);
        var y = Random.Range((int)selectedArea.min.y, (int)selectedArea.max.y);
        ship.cellCenterPosition = boundsOfCells[x, y].center; // Зафиксировали положение кораблика
        MarkupSpawnAreas(ship, x, y); // Размечаем оставшуюся область
    }

    void SelectArea(Ship ship)
    {
        var areasWorkingList = CopyList(spawnAreas); // Дублируем список для выбора
        for (int i = 0; i < body.Length; i++)
        {
            var areaNum = Random.Range(0, areasWorkingList.Count);
            var randomArea = areasWorkingList[areaNum]; // Взяли произвольно выбранную область
            selectedArea = new Bounds(randomArea.center, randomArea.size);
            var isAreaAppropriate = CheckAndAdjustSpawnArea(ship, ref selectedArea);
            // Область не подошла - удаляем из списка, чтобы не перевыбрать позже
            if (!isAreaAppropriate) areasWorkingList.Remove(randomArea);
            else break; // Область подошла - работа выполнена
        }
    }

    List<T> CopyList<T>(List<T> list)
    {
        return new List<T>(list);
    }

    bool CheckAndAdjustSpawnArea(Ship ship, ref Bounds area)
    {
        var canStandHorizontally = area.size.x >= ship.floorsNum; // Подойдет ли по ширине
        var canStandVertically = area.size.y >= ship.floorsNum; // Подойдет ли по высоте
        float adjSize = ship.floorsNum - 1; // На эту величину область уменьшится
        if (!canStandHorizontally && !canStandVertically)
        {
            return false; // Область не подходит вообще
        }
        else if (canStandVertically && canStandHorizontally)
        {
            ship.orientation = (Ship.Orientation)Random.Range(0, 2);
            // Область подходит для любого положения
        }
        else if (canStandHorizontally)
        {
            ship.orientation = Ship.Orientation.Horizontal;
            // Подходит только для горизонтального размещения
        }
        else
        {
            ship.orientation = Ship.Orientation.Vertical;
            // Подходит только для вертикального размещения
        }

        if (ship.orientation == Ship.Orientation.Horizontal)
        {
            area.center = new Vector3(area.center.x - adjSize / 2, area.center.y);
            area.Expand(new Vector3(-adjSize, 0)); // Левый верхний угол области для
            // генерации координат остается на прежнем месте, уменьшается чтобы вместить
            // кораблик целиком
        }
        else
        {
            area.center = new Vector3(area.center.x, area.center.y + adjSize / 2);
            area.Expand(new Vector3(0, -adjSize)); // То же самое, но для вертикального расположения
        }
        return true;
    }

    void MarkupSpawnAreas(Ship ship, float x, float y)
    {
        var shipExtension = ((float)ship.floorsNum) / 2; // Половина длины кораблика
        float centerX = x + shipExtension, centerY = y + 0.5f; // "Переход" из левого нижнего
        // угла ячейки в СЕРЕДИНУ ИЛИ НА СТЫК ячеек под серединой кораблика
        float areaWidth = ship.floorsNum + 2, areaHeight = 3; // Размеры стерильной области

        // Рассчитали середину и размеры для "горизонтального" кораблика. Если вертикальный - пересчитываем
        if (ship.orientation == Ship.Orientation.Vertical)
        {
            centerY = y + 1 - shipExtension; // Поднялись на одну клетку и спустились на половину длины
            centerX = x + 0.5f;
            areaHeight = areaWidth;
            areaWidth = 3;
        }
        var occupiedArea = new Bounds(new Vector3(centerX, centerY),
            new Vector3(areaWidth, areaHeight)); // Область, занятая корабликом
                
        var spawnAreasCopy = CopyList(spawnAreas);
        // Обходим копию списка, чтобы ЗАМЕНИТЬ затронутые области их остатками
        for (int i = 0; i < spawnAreasCopy.Count; i++) 
        {
            var area = spawnAreasCopy[i]; // Проверяем, пересекаются ли
            if (!AreBoundsOverlap(area, occupiedArea)) continue;
            spawnAreas.Remove(area); // Раз пересекаются - из списка убираем
            SplitSpawnArea(area, occupiedArea); // и заменяем оставшимися вокруг подобластями
        }
    }

    bool AreBoundsOverlap(Bounds initial, Bounds occup)
    {
        if (initial == selectedArea) return true;
        var minMaxX = Mathf.Min(initial.max.x, occup.max.x); // Ближняя координата Х из дальних
        var maxMinX = Mathf.Max(initial.min.x, occup.min.x); // Дальняя координата Х из ближних
        var minMaxY = Mathf.Min(initial.max.y, occup.max.y); // Ближняя координата Y из дальних
        var maxMinY = Mathf.Max(initial.min.y, occup.min.y); // Дальняя координата Y из ближних
        var overlap = minMaxX > maxMinX && minMaxY > maxMinY;
        return overlap;
    }

    void SplitSpawnArea(Bounds initialArea, Bounds occupiedArea)
    {
        CreateSubarea(initialArea, initialArea.max.y, occupiedArea.max.y, false); // Сверху
        CreateSubarea(initialArea, initialArea.max.x, occupiedArea.max.x, true); // Справа
        CreateSubarea(initialArea, occupiedArea.min.y, initialArea.min.y, false); // Снизу
        CreateSubarea(initialArea, occupiedArea.min.x, initialArea.min.x, true); // Слева
    }

    void CreateSubarea(Bounds initArea, float max, float min, bool isVertical)
    {
        if (max - min < 1) return; // Стерильная зона кораблика оказалась вплотную 
        // к стороне участка или же вовсе дальше нее - подобласти с этой стороны не осталось
        var subArea = new Bounds();
        // Середина подобласти - среднее арифметическое ее границ, 
        // а ширина или высота - их разность
        if (isVertical) // Вертикальная подобласть (слева или справа)
        {
            subArea.center = new Vector3((max + min) / 2, initArea.center.y);
            subArea.size = new Vector3(max - min, initArea.size.y);
        }
        else // Горизонтальная подобласть (сверху или снизу)
        {
            subArea.center = new Vector3(initArea.center.x, (max + min) / 2);
            subArea.size = new Vector3(initArea.size.x, max - min);
        }
        spawnAreas.Add(subArea);
    }
}
