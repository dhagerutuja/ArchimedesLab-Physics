using UnityEngine;
using TMPro;

public class BuoyancyObject : MonoBehaviour
{
    [Header("Physics")]
    public float fluidDensity = 1000f;
    public float volume = 1f;
    public float damping = 5f;

    [Header("UI")]
    public TMP_Text massText;
    public TMP_Text volumeText;
    public TMP_Text densityText;
    public TMP_Text fluidDensityText;
    public TMP_Text buoyantForceText;
    public TMP_Text weightText;
    public TMP_Text displacedVolumeText;

    private Rigidbody rb;
    private bool inWater;
    private float submergedRatio;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        float buoyantForce = 0f;
        float displacedVolume = 0f;

        if (inWater)
        {
            displacedVolume = volume * submergedRatio;

            buoyantForce =
                fluidDensity *
                Physics.gravity.magnitude *
                displacedVolume;

            // Buoyant force
            rb.AddForce(Vector3.up * buoyantForce);

            // Damping while moving through water
            rb.AddForce(
                -rb.linearVelocity * damping
            );
        }

        UpdateUI(displacedVolume, buoyantForce);
    }

    void OnTriggerStay(Collider other)
    {
        if (other.gameObject.name == "Water")
        {
            inWater = true;

            float waterTop =
                other.bounds.max.y;

            float objectBottom =
                GetComponent<Collider>().bounds.min.y;

            float objectHeight =
                GetComponent<Collider>().bounds.size.y;

            float submergedHeight =
                Mathf.Clamp(
                    waterTop - objectBottom,
                    0f,
                    objectHeight
                );

            submergedRatio =
                submergedHeight / objectHeight;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "Water")
        {
            inWater = false;
            submergedRatio = 0f;
        }
    }

    void UpdateUI(
        float displacedVolume,
        float buoyantForce)
    {
        float mass = rb.mass;
        float weight = mass * Physics.gravity.magnitude;
        float density = mass / volume;

        if (massText != null)
            massText.text = $"Mass: {mass:F2} kg";

        if (volumeText != null)
            volumeText.text = $"Volume: {volume:F2} m³";

        if (densityText != null)
            densityText.text = $"Density: {density:F0} kg/m³";

        if (fluidDensityText != null)
            fluidDensityText.text =
                $"Fluid Density: {fluidDensity:F0} kg/m³";

        if (buoyantForceText != null)
            buoyantForceText.text =
                $"Buoyant Force: {buoyantForce:F2} N";

        if (weightText != null)
            weightText.text =
                $"Weight: {weight:F2} N";

        if (displacedVolumeText != null)
            displacedVolumeText.text =
                $"Displaced Volume: {displacedVolume:F2} m³";
    }
}