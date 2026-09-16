using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class MaterialController : MonoBehaviour
{
    public Rigidbody rb;
    public Collider objectCollider;

    public MaterialData currentMaterial;

    public MaterialData wood;
    public MaterialData plastic;
    public MaterialData steel;
    public MaterialData cork;
    public TMP_Text currentMaterialText;

    [Header("Stage 2 visual samples (never used for physics)")]
    public Material woodVisual;
    public Material plasticVisual;
    public Material steelVisual;
    public Material corkVisual;
    private Renderer sampleRenderer;

    void Awake()
    {
        sampleRenderer = GetComponent<Renderer>();
        CreateVisualMaterials();
        BuildSelectionPanelDragArea();
    }

    void Start()
    {
        if (currentMaterial != null)
        {
            ApplyMaterial(currentMaterial);
        }
    }

    public void ApplyMaterial(MaterialData material)
    {
        if (material == null)
            return;

        currentMaterial = material;

        if (currentMaterialText != null)
        {
            currentMaterialText.text = $"MATERIAL: {material.materialName.ToUpperInvariant()}";
        }

        

        Physics.SyncTransforms();
        float volume =
            objectCollider.bounds.size.x *
            objectCollider.bounds.size.y *
            objectCollider.bounds.size.z;

        rb.mass = material.density * volume;
        if (!rb.isKinematic) rb.WakeUp();
        ApplyVisual(material);
    }

    private void ApplyVisual(MaterialData data)
    {
        if (sampleRenderer == null) sampleRenderer = GetComponent<Renderer>();
        if (sampleRenderer == null) return;
        if (data == wood) sampleRenderer.material = woodVisual;
        else if (data == plastic) sampleRenderer.material = plasticVisual;
        else if (data == steel) sampleRenderer.material = steelVisual;
        else if (data == cork) sampleRenderer.material = corkVisual;
    }

    private void CreateVisualMaterials()
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");
        woodVisual = BuildMaterial(shader, "Wood Sample", new Color(.38f, .16f, .055f), .05f, .25f, MakeGrainTexture(new Color(.23f,.075f,.018f), new Color(.62f,.30f,.10f), false));
        plasticVisual = BuildMaterial(shader, "Plastic Sample", new Color(.055f, .32f, .55f), .0f, .7f, null);
        steelVisual = BuildMaterial(shader, "Steel Sample", new Color(.38f, .43f, .48f), .92f, .72f, null);
        corkVisual = BuildMaterial(shader, "Cork Sample", new Color(.62f, .38f, .16f), .0f, .12f, MakeGrainTexture(new Color(.34f,.17f,.055f), new Color(.78f,.54f,.27f), true));
    }

    private static Material BuildMaterial(Shader shader, string name, Color color, float metallic, float smoothness, Texture texture)
    {
        Material material = new Material(shader) { name = name };
        material.SetColor("_BaseColor", color); material.SetColor("_Color", color);
        if (material.HasProperty("_Metallic")) material.SetFloat("_Metallic", metallic);
        if (material.HasProperty("_Smoothness")) material.SetFloat("_Smoothness", smoothness);
        if (texture != null && material.HasProperty("_BaseMap")) material.SetTexture("_BaseMap", texture);
        return material;
    }

    private static Texture2D MakeGrainTexture(Color dark, Color light, bool porous)
    {
        const int size = 128; Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false) { name = porous ? "Procedural Cork" : "Procedural Wood Grain", wrapMode = TextureWrapMode.Repeat, filterMode = FilterMode.Bilinear };
        for (int y = 0; y < size; y++) for (int x = 0; x < size; x++)
        {
            float noise = Mathf.PerlinNoise(x * .12f, y * .12f);
            float grain = porous ? noise : Mathf.Abs(Mathf.Sin((x + noise * 18f) * .16f));
            Color c = Color.Lerp(dark, light, porous ? Mathf.SmoothStep(.25f, .85f, grain) : Mathf.SmoothStep(.20f, .8f, grain));
            texture.SetPixel(x, y, c);
        }
        texture.Apply(); return texture;
    }

    private void BuildSelectionPanelDragArea()
    {
        GameObject panel = GameObject.Find("MaterialPanel");
        if (panel == null || panel.GetComponentInChildren<LessonPanelDragHandler>() != null) return;
        RectTransform panelRect = panel.GetComponent<RectTransform>();
        if (panelRect == null) return;
        // Default composition is upper-left; this is only assigned once, never during selection updates.
        panelRect.anchorMin = panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(26f, -26f);
        panelRect.sizeDelta = new Vector2(340f, 470f);
        Image panelImage = panel.GetComponent<Image>(); if (panelImage != null) panelImage.color = new Color(.025f, .07f, .12f, .94f);
        GameObject header = new GameObject("Selection Drag Header", typeof(RectTransform), typeof(Image), typeof(LessonPanelDragHandler));
        header.transform.SetParent(panel.transform, false);
        RectTransform headerRect = header.GetComponent<RectTransform>();
        headerRect.anchorMin = new Vector2(0f, 1f); headerRect.anchorMax = new Vector2(1f, 1f);
        headerRect.pivot = new Vector2(.5f, 1f); headerRect.sizeDelta = new Vector2(0f, 42f); headerRect.anchoredPosition = Vector2.zero;
        header.GetComponent<Image>().color = new Color(.055f, .22f, .34f, .96f);
        header.GetComponent<LessonPanelDragHandler>().Configure(panelRect);
        RectTransform materialTitle = panel.transform.Find("MaterialTitle") as RectTransform;
        if (materialTitle != null) materialTitle.anchoredPosition = new Vector2(0f, 155f);
        GameObject label = new GameObject("Header Label", typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        label.transform.SetParent(header.transform, false);
        TMPro.TextMeshProUGUI text = label.GetComponent<TMPro.TextMeshProUGUI>();
        text.font = TMP_Settings.defaultFontAsset; text.text = "MATERIAL & FLUID   •   DRAG"; text.fontSize = 16f; text.fontStyle = TMPro.FontStyles.Bold;
        text.alignment = TMPro.TextAlignmentOptions.Center; text.color = new Color(.8f, .93f, 1f); text.raycastTarget = false;
        RectTransform labelRect = label.GetComponent<RectTransform>(); labelRect.anchorMin = Vector2.zero; labelRect.anchorMax = Vector2.one; labelRect.offsetMin = Vector2.zero; labelRect.offsetMax = Vector2.zero;
    }

    public void SelectWood()
    {
        ApplyMaterial(wood);
    }

    public void SelectPlastic()
    {
        ApplyMaterial(plastic);
    }

    public void SelectSteel()
    {
        ApplyMaterial(steel);
    }

    public void SelectCork()
    {
        ApplyMaterial(cork);
    }
}
