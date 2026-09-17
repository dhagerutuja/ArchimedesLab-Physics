using UnityEngine;
using UnityEngine.UI;

/// <summary>Draws a single, connected, filled shaft-and-triangle workspace pointer mesh.</summary>
public sealed class FilledWorkspaceArrowGraphic : Graphic
{
    protected override void OnPopulateMesh(VertexHelper vertices)
    {
        vertices.Clear();
        Rect rect = GetPixelAdjustedRect();
        float shaftEnd = Mathf.Lerp(rect.xMin, rect.xMax, .64f);
        float shaftHalfHeight = rect.height * .16f;

        // Clockwise outline of one continuous solid arrow: shaft joins the broad triangle without a gap.
        Vector2[] points =
        {
            new Vector2(rect.xMin, -shaftHalfHeight),
            new Vector2(shaftEnd, -shaftHalfHeight),
            new Vector2(shaftEnd, rect.yMin),
            new Vector2(rect.xMax, 0f),
            new Vector2(shaftEnd, rect.yMax),
            new Vector2(shaftEnd, shaftHalfHeight),
            new Vector2(rect.xMin, shaftHalfHeight)
        };
        for (int i = 0; i < points.Length; i++)
            vertices.AddVert(points[i], color, Vector2.zero);

        vertices.AddTriangle(0, 1, 6);
        vertices.AddTriangle(1, 5, 6);
        vertices.AddTriangle(1, 2, 3);
        vertices.AddTriangle(1, 3, 5);
        vertices.AddTriangle(3, 4, 5);
    }
}
