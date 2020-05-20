using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ship : Dispatcher
{
    public enum Orientation { Horizontal, Vertical }

    public Orientation orientation = Orientation.Horizontal;
    public bool isPositionCorrect { get; set; } = false;
    public bool isWithinCell { get; set; } = false;
    public bool wasAllocatedOnce { get; private set; } = false;
    public int floorsNum { get; private set; }
    public GameField gameField { get; set; }
    public Vector2 cellCenterPosition { get; set; }
    public Vector2 lastPosition;
    public GameObject floorButtonPref;

    bool toMove = false;
    float rotAngle;
    Canvas canvas;
    Animator[] animators;
    Orientation lastOrientation;

    protected override void Start()
    {
        base.Start();
        if (orientation == Orientation.Horizontal) rotAngle = 90f;
        else rotAngle = -90f;

        lastOrientation = orientation;
        canvas = GetComponentInParent<Canvas>();
        gameField = canvas.GetComponent<GameField>();

        floorsNum = transform.childCount;
        float floorSize = 0;
        animators = new Animator[floorsNum];
        for (int i = 0; i < floorsNum; i++)
        {
            var floor = transform.GetChild(i);
            var floorPos = transform.position;
            var sprRenderer = floor.GetComponent<SpriteRenderer>();
            Settings.ScaleSpriteByY(sprRenderer, GameField.cellHeightToCameraHeightY,
                out floorSize);

            if (orientation == Orientation.Horizontal) floorPos.x += i * floorSize;
            else if (orientation == Orientation.Vertical) floorPos.y -= i * floorSize;
            floor.transform.position = floorPos;

            var floorButtonObj = Instantiate(floorButtonPref, floor.transform);
            floorButtonObj.transform.position = floorPos;
            var buttonRectTransf = floorButtonObj.GetComponent<RectTransform>();
            buttonRectTransf.sizeDelta = new Vector2(floorSize, floorSize);
            var buttonScript = floorButtonObj.GetComponent<Button>();
            buttonScript.onClick.AddListener(OnFloorClick);

            var animator = floor.GetComponent<Animator>();
            animators[i] = animator;
        }
    }

    void Update()
    {
        toMove = Equals(currentShip);
        if (!toMove) return;

        var mousePos = Input.mousePosition;
        var canvasRect = canvas.transform as RectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, 
            mousePos, Camera.main, out Vector2 result);
        result = canvas.transform.TransformPoint(result);

        gameField.CheckLocationOverField(this, result);
        if (isWithinCell) transform.position = cellCenterPosition;
        else transform.position = result;

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            currentShip = null;
            if (wasAllocatedOnce)
            {
                isPositionCorrect = isWithinCell = true;
                transform.position = lastPosition;
                if (lastOrientation != orientation) Rotate();
                GameField.RegisterShip(this);
            }
            else Destroy(gameObject);
        }

        else if (Input.GetKeyUp(KeyCode.Space)) Rotate();
        SwitchErrorAnimation();
    }

    public void SetAutolocated()
    {
        if (orientation != lastOrientation)
        {
            orientation = lastOrientation;
            Rotate();
        }
        transform.position = cellCenterPosition;
        RememberPositionAndRotation();
        isPositionCorrect = isWithinCell = true;
    }

    void OnFloorClick()
    {
        if (!Input.GetMouseButtonUp(0)) return; // Проверяем, что клик был именно мышкой
        else if (toMove && isPositionCorrect) // В данный момент двигаем - ставим, если можем
        {
            GameField.RegisterShip(this);
            RememberPositionAndRotation();
        }
        else if (!toMove && wasAllocatedOnce) GameField.TakeShipOff(this);
        // В данный момент не двигали и уже был выставлен - значит
        OnShipClick(); // переставляем. Очищаем игровое поле от него
        if (isPositionCorrect) wasAllocatedOnce = true;
    }

    void RememberPositionAndRotation()
    {
        lastPosition = transform.position;
        lastOrientation = orientation;
    }

    void SwitchErrorAnimation()
    {
        foreach (var anm in animators) anm.SetBool("IsMisplaced", !isPositionCorrect);
    }

    void Rotate()
    {
        if (orientation == Orientation.Horizontal) orientation = Orientation.Vertical;
        else orientation = Orientation.Horizontal;
        rotAngle = -rotAngle;
        transform.Rotate(new Vector3(0, 0, rotAngle), Space.Self);        
    }
}