using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Reusable, non-interactive highlight for the short workspace orientation.</summary>
public class WorkspaceHighlight : MonoBehaviour
{
    private Outline activeOutline;
    private readonly List<LineRenderer> worldLines = new List<LineRenderer>();
    private float pulseOffset;
    private RectTransform uiTarget;
    private Renderer worldTarget;
    private RectTransform arrowRect;

    public void ShowUI(GameObject target)
    {
        Clear();
        if (target == null) return;
        activeOutline = target.GetComponent<Outline>() ?? target.AddComponent<Outline>();
        activeOutline.effectColor = new Color(.18f, .78f, 1f, .95f);
        activeOutline.effectDistance = new Vector2(4f, -4f);
        activeOutline.enabled = true;
        uiTarget = target.GetComponent<RectTransform>();
        pulseOffset = Time.unscaledTime;
        ShowArrow();
    }

    public void ShowWorld(Renderer target)
    {
        Clear();
        if (target == null) return;
        Bounds bounds = target.bounds;
        worldTarget = target;
        Vector3 min = bounds.min;
        Vector3 max = bounds.max;
        Vector3[] points =
        {
            new Vector3(min.x, min.y, min.z), new Vector3(max.x, min.y, min.z), new Vector3(max.x, min.y, max.z), new Vector3(min.x, min.y, max.z),
            new Vector3(min.x, max.y, min.z), new Vector3(max.x, max.y, min.z), new Vector3(max.x, max.y, max.z), new Vector3(min.x, max.y, max.z)
        };
        CreateLine(new[] { points[0], points[1], points[2], points[3], points[0] });
        CreateLine(new[] { points[4], points[5], points[6], points[7], points[4] });
        for (int i = 0; i < 4; i++) CreateLine(new[] { points[i], points[i + 4] });
        pulseOffset = Time.unscaledTime;
        ShowArrow();
    }

    public void Clear()
    {
        if (activeOutline != null) activeOutline.enabled = false;
        activeOutline = null;
        uiTarget = null;
        worldTarget = null;
        if (arrowRect != null) arrowRect.gameObject.SetActive(false);
        foreach (LineRenderer line in worldLines)
            if (line != null) Destroy(line.gameObject);
        worldLines.Clear();
    }

    private void Update()
    {
        if (activeOutline != null)
        {
            float pulse = 3f + Mathf.Sin((Time.unscaledTime - pulseOffset) * 3f) * .8f;
            activeOutline.effectDistance = new Vector2(pulse, -pulse);
        }
        float width = .025f + Mathf.Sin((Time.unscaledTime - pulseOffset) * 3f) * .006f;
        foreach (LineRenderer line in worldLines)
            if (line != null) line.widthMultiplier = width;
        UpdateArrow();
    }

    private void CreateLine(Vector3[] points)
    {
        GameObject lineObject = new GameObject("Workspace Highlight", typeof(LineRenderer));
        lineObject.transform.SetParent(transform, false);
        LineRenderer line = lineObject.GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = points.Length;
        line.SetPositions(points);
        line.loop = false;
        line.material = new Material(Shader.Find("Sprites/Default"));
        line.startColor = line.endColor = new Color(.18f, .78f, 1f, .95f);
        line.widthMultiplier = .025f;
        line.numCornerVertices = 4;
        line.numCapVertices = 4;
        line.sortingOrder = 50;
        worldLines.Add(line);
    }

    private void ShowArrow()
    {
        if (arrowRect == null)
        {
            GameObject canvasObject = new GameObject("Workspace Pointer", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 150;
            canvasObject.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920f, 1080f);
            GameObject arrow = new GameObject("Broad Pointer Arrow", typeof(RectTransform));
            arrow.transform.SetParent(canvasObject.transform, false);
            arrowRect = arrow.GetComponent<RectTransform>();
            arrowRect.sizeDelta = new Vector2(240f, 100f);
            // A substantial shaft plus a broad triangular head: presentation pointer, not a cursor glyph.
            Image shaft = CreateArrowPart("Shaft", arrow.transform, new Vector2(154f, 22f), new Vector2(-28f, 0f));
            shaft.color = new Color(.50f, .91f, 1f, .98f);
            Outline shaftOutline = shaft.gameObject.AddComponent<Outline>();
            shaftOutline.effectColor = new Color(.015f, .24f, .38f, .95f);
            shaftOutline.effectDistance = new Vector2(2f, -2f);

            GameObject head = new GameObject("Broad Arrowhead", typeof(RectTransform), typeof(TextMeshProUGUI));
            head.transform.SetParent(arrow.transform, false);
            RectTransform headRect = head.GetComponent<RectTransform>();
            headRect.anchorMin = headRect.anchorMax = new Vector2(.5f, .5f);
            headRect.anchoredPosition = new Vector2(82f, 0f);
            headRect.sizeDelta = new Vector2(88f, 100f);
            TextMeshProUGUI headText = head.GetComponent<TextMeshProUGUI>();
            headText.font = TMP_Settings.defaultFontAsset;
            headText.text = "▶";
            headText.fontSize = 102f;
            headText.alignment = TextAlignmentOptions.Center;
            headText.color = new Color(.50f, .91f, 1f, .98f);
            headText.outlineWidth = .15f;
            headText.outlineColor = new Color(.015f, .24f, .38f, .95f);
            headText.raycastTarget = false;
        }
        arrowRect.gameObject.SetActive(true);
        UpdateArrow();
    }

    private void UpdateArrow()
    {
        if (arrowRect == null || !arrowRect.gameObject.activeSelf) return;
        Vector3 screenPoint;
        if (uiTarget != null)
            screenPoint = RectTransformUtility.WorldToScreenPoint(null, uiTarget.position);
        else if (worldTarget != null && Camera.main != null)
            screenPoint = Camera.main.WorldToScreenPoint(worldTarget.bounds.center);
        else
            return;

        Vector2 offset = new Vector2(-210f, 125f);
        Vector2 destination = new Vector2(screenPoint.x, screenPoint.y);
        Vector2 origin = destination + offset;
        arrowRect.position = origin;
        Vector2 direction = destination - origin;
        arrowRect.rotation = Quaternion.Euler(0f, 0f, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);
    }

    private static Image CreateArrowPart(string name, Transform parent, Vector2 size, Vector2 position)
    {
        GameObject part = new GameObject(name, typeof(RectTransform), typeof(Image));
        part.transform.SetParent(parent, false);
        RectTransform rect = part.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return part.GetComponent<Image>();
    }

    private void OnDestroy() => Clear();
}
