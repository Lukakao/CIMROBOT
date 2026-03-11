using UnityEngine;
using UnityEngine.EventSystems;

public class DragPanel : MonoBehaviour, IDragHandler
{
    private RectTransform rectTransform;
    [SerializeField] RectTransform canvasRect;
    private Canvas canvas;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }


    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
        ClampToCanvas();
    }
    void ClampToCanvas()
    {
        Vector2 pos = rectTransform.anchoredPosition;

        Vector2 panelSize = rectTransform.rect.size;
        Vector2 canvasSize = canvasRect.rect.size;

        Vector2 pivot = rectTransform.pivot;

        float minX = -canvasSize.x * 0.7f + panelSize.x * pivot.x;
        float maxX =  canvasSize.x * 0.7f - panelSize.x * (1 - pivot.x);

        float minY = -canvasSize.y * 0.7f + panelSize.y * pivot.y;
        float maxY =  canvasSize.y * 0.7f - panelSize.y * (1 - pivot.y);

        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        rectTransform.anchoredPosition = pos;
    }
}