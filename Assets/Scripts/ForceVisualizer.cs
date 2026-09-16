using UnityEngine;

public class ForceVisualizer : MonoBehaviour
{
    public Rigidbody rb;
    public Collider objectCollider;
    public WaterVolume water;

    public float forceToLength = 0.00025f;
    public float lineWidth = 0.05f;

    private LineRenderer weightLine;
    private LineRenderer buoyancyLine;

    private LineRenderer weightHead1;
    private LineRenderer weightHead2;
    private LineRenderer buoyancyHead1;
    private LineRenderer buoyancyHead2;

    void Start()
    {
        weightLine = CreateLine("Weight Arrow", Color.red);
        buoyancyLine = CreateLine("Buoyant Arrow", Color.blue);

        weightHead1 = CreateLine("Weight Head 1", Color.red);
        weightHead2 = CreateLine("Weight Head 2", Color.red);

        buoyancyHead1 = CreateLine("Buoyant Head 1", Color.blue);
        buoyancyHead2 = CreateLine("Buoyant Head 2", Color.blue);
    }

    void Update()
    {
        if (rb == null || objectCollider == null || water == null)
            return;

        Vector3 center = objectCollider.bounds.center;

        BuoyancyPhysics.Snapshot snapshot = water.GetPhysicsSnapshot(rb, objectCollider);
        float weightLength = snapshot.Weight * forceToLength;
        float buoyantLength = snapshot.BuoyantForce * forceToLength;

        DrawArrow(
            weightLine,
            weightHead1,
            weightHead2,
            center,
            center + Vector3.down * weightLength,
            Vector3.down
        );

        DrawArrow(
            buoyancyLine,
            buoyancyHead1,
            buoyancyHead2,
            center,
            center + Vector3.up * buoyantLength,
            Vector3.up
        );
    }

    LineRenderer CreateLine(string objectName, Color color)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(transform);

        LineRenderer line = obj.AddComponent<LineRenderer>();

        line.positionCount = 2;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth * .72f;

        line.material = new Material(Shader.Find("Sprites/Default"));

        line.startColor = color;
        line.endColor = Color.Lerp(color, Color.white, .25f);
        line.numCapVertices = 4;
        line.numCornerVertices = 3;
        line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return line;
    }

    void DrawArrow(
        LineRenderer shaft,
        LineRenderer head1,
        LineRenderer head2,
        Vector3 start,
        Vector3 end,
        Vector3 direction)
    {
        shaft.SetPosition(0, start);
        shaft.SetPosition(1, end);

        float headSize = 0.2f;

        Vector3 side = Vector3.right;

        if (Mathf.Abs(Vector3.Dot(direction, side)) > 0.9f)
            side = Vector3.forward;

        Vector3 headBase = end - direction * headSize;

        head1.SetPosition(0, end);
        head1.SetPosition(1, headBase + side * headSize);

        head2.SetPosition(0, end);
        head2.SetPosition(1, headBase - side * headSize);
    }
}
