using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BattleCell : MonoBehaviour, IPointerClickHandler
{
    public static event System.Action<GameObject> onCellClick;

    public void OnPointerClick(PointerEventData eventData)
    {
        onCellClick?.Invoke(gameObject);
    }
}
