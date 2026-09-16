using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Lightweight visual-only dressing for MainLab. It never creates colliders or touches buoyancy.</summary>
public class LabVisualPresentation : MonoBehaviour
{
    private WaterVolume fluid;
    private Renderer fluidRenderer;
    private Material surfaceMaterial;
    private float movementSpeed = .025f;
    private ReflectionProbe reflectionProbe;
    private float nextProbeRender;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void CreateForMainLab()
    {
        if (SceneManager.GetActiveScene().name == "MainLab" && FindAnyObjectByType<LabVisualPresentation>() == null)
            new GameObject("Stage 2 Lab Visual Presentation").AddComponent<LabVisualPresentation>();
    }

    private void Start()
    {
        fluid = FindAnyObjectByType<WaterVolume>();
        if (fluid == null) { enabled = false; return; }
        fluidRenderer = fluid.GetComponent<Renderer>();
        BuildTankFrame();
        BuildFluidSurface();
        BuildLighting();
        BuildReflectionProbe();
    }

    private void Update()
    {
        if (surfaceMaterial == null || fluid == null) return;
        float speed = fluid.currentFluid == fluid.oil ? .35f : fluid.currentFluid == fluid.alcohol ? 1.25f : 1f;
        movementSpeed = .018f * speed;
        Vector2 offset = new Vector2(Time.time * movementSpeed, Time.time * movementSpeed * .57f);
        surfaceMaterial.SetTextureOffset("_BaseMap", offset);
        Color liquidColor = fluid.currentFluid == fluid.oil ? new Color(.95f, .52f, .08f, .62f) : fluid.currentFluid == fluid.alcohol ? new Color(.72f, .58f, 1f, .38f) : fluid.currentFluid == fluid.seawater ? new Color(.03f, .52f, .63f, .52f) : new Color(.18f, .75f, 1f, .48f);
        surfaceMaterial.SetColor("_BaseColor", liquidColor); surfaceMaterial.SetColor("_Color", liquidColor);
        if (fluidRenderer != null && fluidRenderer.sharedMaterial != null && fluidRenderer.sharedMaterial.HasProperty("_BaseMap"))
            fluidRenderer.material.SetTextureOffset("_BaseMap", offset * .35f);
        if (reflectionProbe != null && Time.time >= nextProbeRender)
        {
            reflectionProbe.RenderProbe();
            nextProbeRender = Time.time + .75f;
        }
    }

    private void BuildFluidSurface()
    {
        Bounds b = fluid.GetComponent<Collider>().bounds;
        GameObject surface = GameObject.CreatePrimitive(PrimitiveType.Plane);
        surface.name = "Animated Liquid Surface (Visual Only)";
        surface.transform.SetParent(fluid.transform, true);
        surface.transform.position = new Vector3(b.center.x, b.max.y + .006f, b.center.z);
        surface.transform.localScale = new Vector3(b.size.x / 10f, 1f, b.size.z / 10f);
        Destroy(surface.GetComponent<Collider>());
        surfaceMaterial = MakeLiquidSurfaceMaterial();
        surface.GetComponent<Renderer>().material = surfaceMaterial;
        fluidRenderer.material.renderQueue = 3000;
    }

    private Material MakeLiquidSurfaceMaterial()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit"); if (shader == null) shader = Shader.Find("Standard");
        Material m = new Material(shader) { name = "Animated Laboratory Liquid Surface" };
        m.SetFloat("_Surface", 1); m.SetFloat("_Blend", 0); m.SetFloat("_SrcBlend", 5); m.SetFloat("_DstBlend", 10); m.SetFloat("_ZWrite", 0);
        m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
        m.SetColor("_BaseColor", new Color(.18f, .75f, 1f, .52f)); m.SetColor("_Color", new Color(.18f, .75f, 1f, .52f));
        m.SetFloat("_Metallic", .15f); m.SetFloat("_Smoothness", .9f);
        m.SetTexture("_BaseMap", MakeRippleTexture()); m.renderQueue = 3001;
        return m;
    }

    private static Texture2D MakeRippleTexture()
    {
        const int size = 128; Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = "Subtle Moving Ripple Pattern", wrapMode = TextureWrapMode.Repeat };
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float wave = .5f + .5f * Mathf.Sin(x * .20f + Mathf.Sin(y * .13f) * 2f);
            texture.SetPixel(x, y, new Color(.75f, .95f, 1f, .18f + wave * .18f));
        }
        texture.Apply(); return texture;
    }

    private void BuildTankFrame()
    {
        Bounds b = fluid.GetComponent<Collider>().bounds;
        Transform root = new GameObject("Laboratory Tank Frame (Visual Only)").transform;
        root.SetParent(fluid.transform.parent, true);
        Material metal = MakeLitMaterial("Anodized Laboratory Frame", new Color(.07f,.12f,.16f), .85f, .55f);
        Material accent = MakeLitMaterial("Measurement Accent", new Color(.03f,.55f,.82f), .25f, .6f);
        float y = b.center.y;
        foreach (float x in new[] { b.min.x, b.max.x }) foreach (float z in new[] { b.min.z, b.max.z })
            Cube("Frame Corner", root, new Vector3(x, y, z), new Vector3(.09f, b.size.y + .38f, .09f), metal);
        Cube("Front Top Rail", root, new Vector3(b.center.x, b.max.y + .1f, b.min.z), new Vector3(b.size.x + .18f, .09f, .09f), metal);
        Cube("Front Base Rail", root, new Vector3(b.center.x, b.min.y - .14f, b.min.z), new Vector3(b.size.x + .25f, .12f, .12f), metal);
        Cube("Instrument Plinth", root, new Vector3(b.center.x, b.min.y - .34f, b.center.z), new Vector3(b.size.x + .75f, .3f, b.size.z + .75f), metal);
        for (int i = -3; i <= 3; i++) Cube("Scale Tick", root, new Vector3(b.min.x - .055f, b.center.y + i * .45f, b.min.z - .02f), new Vector3(.04f, .025f, .22f), accent);
    }

    private void BuildLighting()
    {
        CreateLight("Cool Tank Key", new Vector3(-3.2f, 4.2f, -3.5f), new Color(.55f,.82f,1f), 3.2f, 9f);
        CreateLight("Warm Fill", new Vector3(3.5f, 2.3f, -2.0f), new Color(1f,.68f,.42f), 1.5f, 7f);
    }

    private void BuildReflectionProbe()
    {
        GameObject go = new GameObject("Tank Reflection Probe (Visual Only)");
        go.transform.position = fluid.GetComponent<Collider>().bounds.center + Vector3.up * .5f;
        reflectionProbe = go.AddComponent<ReflectionProbe>();
        reflectionProbe.mode = UnityEngine.Rendering.ReflectionProbeMode.Realtime;
        reflectionProbe.refreshMode = UnityEngine.Rendering.ReflectionProbeRefreshMode.ViaScripting;
        reflectionProbe.timeSlicingMode = UnityEngine.Rendering.ReflectionProbeTimeSlicingMode.IndividualFaces;
        reflectionProbe.resolution = 128;
        reflectionProbe.intensity = .7f;
        reflectionProbe.size = fluid.GetComponent<Collider>().bounds.size + new Vector3(3f, 3f, 3f);
    }

    private static void CreateLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        GameObject go = new GameObject(name); Light light = go.AddComponent<Light>(); light.type = LightType.Point; light.color = color; light.intensity = intensity; light.range = range; light.shadows = LightShadows.None; go.transform.position = position;
    }

    private static void Cube(string name, Transform parent, Vector3 position, Vector3 scale, Material material)
    {
        GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube); go.name = name; go.transform.SetParent(parent, true); go.transform.position = position; go.transform.localScale = scale; Destroy(go.GetComponent<Collider>()); go.GetComponent<Renderer>().material = material;
    }

    private static Material MakeLitMaterial(string name, Color color, float metallic, float smoothness)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit"); if (shader == null) shader = Shader.Find("Standard"); Material m = new Material(shader) { name = name };
        m.SetColor("_BaseColor", color); m.SetColor("_Color", color); m.SetFloat("_Metallic", metallic); m.SetFloat("_Smoothness", smoothness); return m;
    }
}
