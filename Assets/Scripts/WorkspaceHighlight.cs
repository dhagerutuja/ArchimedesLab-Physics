using UnityEngine;
using UnityEngine.UI;

/// <summary>Reusable, non-interactive highlight for the short workspace orientation.</summary>
public class WorkspaceHighlight : MonoBehaviour
{
    public enum WorldFocus { Tank, TestObject }

    private Outline activeOutline;
    private float pulseOffset;
    private readonly System.Collections.Generic.List<RendererFocusState> worldFocusStates = new System.Collections.Generic.List<RendererFocusState>();

    private sealed class RendererFocusState
    {
        public Renderer Renderer;
        public MaterialPropertyBlock[] OriginalBlocks;
    }

    private void Awake()
    {
        // Clean up a pointer canvas if this component is reloaded during an in-editor play session.
        Transform legacyPointer = transform.Find("Workspace Pointer");
        if (legacyPointer != null) Destroy(legacyPointer.gameObject);
    }

    public void ShowUI(GameObject target)
    {
        Clear();
        if (target == null) return;
        activeOutline = target.GetComponent<Outline>() ?? target.AddComponent<Outline>();
        activeOutline.effectColor = new Color(.18f, .78f, 1f, .95f);
        activeOutline.effectDistance = new Vector2(4f, -4f);
        activeOutline.enabled = true;
        pulseOffset = Time.unscaledTime;
    }

    public void ShowWorld(Renderer target, WorldFocus focus)
    {
        Clear();
        if (target == null) return;
        pulseOffset = Time.unscaledTime;
        ApplyWorldFocus(target, focus);
    }

    public void Clear()
    {
        if (activeOutline != null) activeOutline.enabled = false;
        activeOutline = null;
        RestoreWorldFocus();
    }

    private void Update()
    {
        if (activeOutline != null)
        {
            float pulse = 3f + Mathf.Sin((Time.unscaledTime - pulseOffset) * 3f) * .8f;
            activeOutline.effectDistance = new Vector2(pulse, -pulse);
        }
    }

    private void ApplyWorldFocus(Renderer root, WorldFocus focus)
    {
        foreach (Renderer renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            if (renderer == null || renderer.sharedMaterials == null) continue;
            MaterialPropertyBlock[] originals = new MaterialPropertyBlock[renderer.sharedMaterials.Length];
            for (int materialIndex = 0; materialIndex < originals.Length; materialIndex++)
            {
                MaterialPropertyBlock original = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(original, materialIndex);
                originals[materialIndex] = original;

                Material material = renderer.sharedMaterials[materialIndex];
                if (material == null) continue;
                Color source = ReadColor(material);
                Color focusColor = focus == WorldFocus.Tank
                    ? Color.Lerp(source, new Color(.20f, .92f, 1f, source.a), .72f)
                    : Color.Lerp(source, new Color(1f, 1f, 1f, source.a), .88f);
                MaterialPropertyBlock focused = new MaterialPropertyBlock();
                renderer.GetPropertyBlock(focused, materialIndex);
                SetColor(focused, material, focusColor);
                SetEmission(focused, material, focus == WorldFocus.Tank
                    ? new Color(.05f, .42f, .55f, 1f)
                    : new Color(.55f, .70f, .76f, 1f));
                renderer.SetPropertyBlock(focused, materialIndex);
            }
            worldFocusStates.Add(new RendererFocusState { Renderer = renderer, OriginalBlocks = originals });
        }
    }

    private void RestoreWorldFocus()
    {
        foreach (RendererFocusState state in worldFocusStates)
        {
            if (state.Renderer == null) continue;
            for (int materialIndex = 0; materialIndex < state.OriginalBlocks.Length; materialIndex++)
                state.Renderer.SetPropertyBlock(state.OriginalBlocks[materialIndex], materialIndex);
        }
        worldFocusStates.Clear();
    }

    private static Color ReadColor(Material material)
    {
        if (material.HasProperty("_BaseColor")) return material.GetColor("_BaseColor");
        return material.HasProperty("_Color") ? material.GetColor("_Color") : Color.white;
    }

    private static void SetColor(MaterialPropertyBlock block, Material material, Color color)
    {
        if (material.HasProperty("_BaseColor")) block.SetColor("_BaseColor", color);
        if (material.HasProperty("_Color")) block.SetColor("_Color", color);
    }

    private static void SetEmission(MaterialPropertyBlock block, Material material, Color color)
    {
        if (material.HasProperty("_EmissionColor")) block.SetColor("_EmissionColor", color);
    }

    private void OnDestroy() => Clear();
}
