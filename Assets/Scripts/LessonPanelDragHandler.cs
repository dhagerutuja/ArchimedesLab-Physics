using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Header-only, CanvasScaler-safe drag behaviour for the existing lesson card.</summary>
public class LessonPanelDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler
{
    [SerializeField] private RectTransform panel;
    private RectTransform canvasRect;
    private Vector2 dragStartPointer;
    private Vector2 dragStartPosition;

    public void Configure(RectTransform panelRect)
    {
        panel = panelRect;
        canvasRect = panel != null ? panel.parent as RectTransform : null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (canvasRect == null || panel == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out dragStartPointer);
        dragStartPosition = panel.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (panel == null || canvasRect == null) return;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint)) return;

        Vector2 desired = dragStartPosition + (localPoint - dragStartPointer);
        Vector2 size = panel.rect.size;
        Rect bounds = canvasRect.rect;
        const float visibleMargin = 80f;
        Vector2 anchor = new Vector2(
            Mathf.Lerp(bounds.xMin, bounds.xMax, panel.anchorMin.x),
            Mathf.Lerp(bounds.yMin, bounds.yMax, panel.anchorMin.y));
        desired.x = Mathf.Clamp(desired.x,
            bounds.xMin + visibleMargin + panel.pivot.x * size.x - anchor.x,
            bounds.xMax - visibleMargin - (1f - panel.pivot.x) * size.x - anchor.x);
        desired.y = Mathf.Clamp(desired.y,
            bounds.yMin + visibleMargin + panel.pivot.y * size.y - anchor.y,
            bounds.yMax - visibleMargin - (1f - panel.pivot.y) * size.y - anchor.y);
        panel.anchoredPosition = desired;
    }
}
