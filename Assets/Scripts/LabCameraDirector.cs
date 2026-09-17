using System.Collections;
using UnityEngine;

/// <summary>Two restrained camera states for MainLab; does not drive physics.</summary>
public class LabCameraDirector : MonoBehaviour
{
    private Camera labCamera;
    private Transform tank;
    private Vector3 normalPosition;
    private Quaternion normalRotation;
    private Vector3 explanationPosition;
    private Quaternion explanationRotation;
    private Vector3 focusPosition;
    private Quaternion focusRotation;
    private Coroutine transition;
    private Light[] sceneLights;
    private float[] normalLightIntensities;
    private float normalAmbientIntensity;

    private void Awake()
    {
        labCamera = Camera.main;
        WaterVolume water = FindAnyObjectByType<WaterVolume>();
        if (labCamera == null || water == null) { enabled = false; return; }
        tank = water.transform;
        normalPosition = labCamera.transform.position;
        Vector3 center = water.GetComponent<Collider>().bounds.center + Vector3.up * .2f;
        normalRotation = Quaternion.LookRotation(center - normalPosition);
        Vector3 direction = (normalPosition - center).normalized;
        focusPosition = center + direction * 8.2f + Vector3.up * .65f;
        focusRotation = Quaternion.LookRotation(center - focusPosition);
        explanationPosition = normalPosition + direction * 8f + Vector3.up * 1.25f;
        explanationRotation = Quaternion.LookRotation(center - explanationPosition);
        sceneLights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        normalLightIntensities = new float[sceneLights.Length];
        for (int i = 0; i < sceneLights.Length; i++) normalLightIntensities[i] = sceneLights[i].intensity;
        normalAmbientIntensity = RenderSettings.ambientIntensity;
        labCamera.transform.rotation = normalRotation;
    }

    public void FocusExperiment()
    {
        SetExperimentLighting();
        MoveTo(focusPosition, focusRotation, 1.15f);
    }

    public void FocusExplanation()
    {
        SetExplanationLighting();
        MoveTo(explanationPosition, explanationRotation, 1.15f);
    }

    public void ReturnToLab() => FocusExplanation();

    public void RestoreLabImmediately()
    {
        if (transition != null) StopCoroutine(transition);
        transition = null;
        SetExperimentLighting();
        if (labCamera != null) labCamera.transform.SetPositionAndRotation(normalPosition, normalRotation);
    }

    public void ReturnToLabImmediately()
    {
        RestoreLabImmediately();
    }

    private void SetExplanationLighting()
    {
        RenderSettings.ambientIntensity = normalAmbientIntensity * .38f;
        for (int i = 0; i < sceneLights.Length; i++)
            if (sceneLights[i] != null) sceneLights[i].intensity = normalLightIntensities[i] * .42f;
    }

    private void SetExperimentLighting()
    {
        RenderSettings.ambientIntensity = normalAmbientIntensity;
        for (int i = 0; i < sceneLights.Length; i++)
            if (sceneLights[i] != null) sceneLights[i].intensity = normalLightIntensities[i];
    }

    private void MoveTo(Vector3 position, Quaternion rotation, float duration)
    {
        if (transition != null) StopCoroutine(transition);
        transition = StartCoroutine(Transition(position, rotation, duration));
    }

    private IEnumerator Transition(Vector3 position, Quaternion rotation, float duration)
    {
        Vector3 start = labCamera.transform.position; Quaternion startRotation = labCamera.transform.rotation;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
        {
            float eased = t * t * (3f - 2f * t);
            labCamera.transform.SetPositionAndRotation(Vector3.Lerp(start, position, eased), Quaternion.Slerp(startRotation, rotation, eased));
            yield return null;
        }
        labCamera.transform.SetPositionAndRotation(position, rotation);
    }
}
