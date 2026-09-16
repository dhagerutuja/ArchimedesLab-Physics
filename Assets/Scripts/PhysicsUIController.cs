using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PhysicsUIController : MonoBehaviour
{
    public Rigidbody rb;
    public Collider objectCollider;
    public WaterVolume water;
    public TMP_Text massText;
    public TMP_Text volumeText;
    public TMP_Text densityText;
    public TMP_Text fluidDensityText;
    public TMP_Text buoyantForceText;
    public TMP_Text weightText;
    public TMP_Text displacedVolumeText;
    public TMP_Text currentFluidText;
    private MaterialController materialController;

    void Start()
    {
        RectTransform panel = transform as RectTransform;
        panel.anchorMin = panel.anchorMax = new Vector2(0f, 1f); panel.pivot = new Vector2(0f, 1f);
        panel.anchoredPosition = new Vector2(26f, -522f); panel.sizeDelta = new Vector2(390f, 500f);
        Image image = GetComponent<Image>(); if (image != null) image.color = new Color(.025f, .07f, .12f, .94f);
        TMP_Text title = transform.Find("TitleText")?.GetComponent<TMP_Text>();
        if (title != null) { title.text = "PHYSICS DATA\n<size=16><color=#8FC8E8>LIVE MEASUREMENTS</color></size>"; title.fontSize = 25f; title.color = Color.white; }
        StyleValue(massText, 19f); StyleValue(volumeText, 19f); StyleValue(densityText, 19f); StyleValue(fluidDensityText, 19f); StyleValue(buoyantForceText, 19f); StyleValue(weightText, 19f); StyleValue(displacedVolumeText, 19f);
        materialController = FindAnyObjectByType<MaterialController>();
        LayoutSelectionStatus();
    }

    private static void StyleValue(TMP_Text value, float size)
    {
        if (value == null) return;
        value.fontSize = size; value.color = new Color(.9f, .96f, 1f); value.alignment = TextAlignmentOptions.Left;
    }

    void Update()
    {
        if (rb == null || objectCollider == null || water == null || water.currentFluid == null)
            return;

        BuoyancyPhysics.Snapshot snapshot = water.GetPhysicsSnapshot(rb, objectCollider);
        float density = snapshot.Volume > 0f ? rb.mass / snapshot.Volume : 0f;
        massText.text = $"Mass: {rb.mass:F2} kg";
        volumeText.text = $"Volume: {snapshot.Volume:F2} m³";
        densityText.text = $"Density: {density:F0} kg/m³";
        fluidDensityText.text = $"Fluid Density: {water.fluidDensity:F0} kg/m³";
        buoyantForceText.text = $"Buoyant Force: {snapshot.BuoyantForce:F2} N";
        weightText.text = $"Weight: {snapshot.Weight:F2} N";
        displacedVolumeText.text = $"Displaced Volume: {snapshot.DisplacedVolume:F2} m³";
        if (currentFluidText != null) currentFluidText.text = $"FLUID: {water.currentFluid.fluidName.ToUpperInvariant()}";
    }

    private void LayoutSelectionStatus()
    {
        if (materialController == null || materialController.currentMaterialText == null || currentFluidText == null) return;
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;

        Transform existing = canvas.transform.Find("Current Selection Status");
        RectTransform strip;
        if (existing != null)
        {
            strip = existing as RectTransform;
        }
        else
        {
            GameObject root = new GameObject("Current Selection Status", typeof(RectTransform));
            root.transform.SetParent(canvas.transform, false);
            strip = root.GetComponent<RectTransform>();
            strip.anchorMin = strip.anchorMax = new Vector2(.5f, 0f);
            strip.pivot = new Vector2(.5f, 0f);
            strip.anchoredPosition = new Vector2(0f, 148f);
            strip.sizeDelta = new Vector2(560f, 58f);
        }

        PlaceStatusText(materialController.currentMaterialText, strip, "Material Status", -140f);
        PlaceStatusText(currentFluidText, strip, "Fluid Status", 140f);
    }

    private static void PlaceStatusText(TMP_Text text, RectTransform strip, string cardName, float x)
    {
        Transform existing = strip.Find(cardName);
        RectTransform card;
        if (existing != null)
        {
            card = existing as RectTransform;
        }
        else
        {
            GameObject cardObject = new GameObject(cardName, typeof(RectTransform), typeof(Image));
            cardObject.transform.SetParent(strip, false);
            card = cardObject.GetComponent<RectTransform>();
            card.anchorMin = card.anchorMax = new Vector2(.5f, .5f);
            card.pivot = new Vector2(.5f, .5f);
            card.sizeDelta = new Vector2(260f, 54f);
            Image image = cardObject.GetComponent<Image>();
            image.color = new Color(.025f, .09f, .15f, .94f);
        }
        card.anchoredPosition = new Vector2(x, 0f);
        text.transform.SetParent(card, false);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(14f, 0f); textRect.offsetMax = new Vector2(-14f, 0f);
        text.alignment = TextAlignmentOptions.Center;
        text.fontSize = 18f;
        text.fontStyle = FontStyles.Bold;
        text.color = new Color(.88f, .95f, 1f);
        text.raycastTarget = false;
    }
}
