using UnityEngine;
using UnityEngine.EventSystems;

public class GridPosition : MonoBehaviour//IPointerDownHandler
{
    // public void OnPointerDown(PointerEventData eventData)
    // {
    //     Debug.Log("OnPointerDown");
    // }


    [SerializeField] private int x;
    [SerializeField] private int y;
    private void OnMouseDown()
    {
        Debug.Log($"Click {x} {y}");
        GameManager.Instance.ClickedOnGridPosition(x, y);
    }
}
