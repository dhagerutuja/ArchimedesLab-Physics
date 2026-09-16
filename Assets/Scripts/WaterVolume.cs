using UnityEngine;

public class WaterVolume : MonoBehaviour
{
    public float fluidDensity = 1000f;
    public float damping = 1000f;

    public FluidData currentFluid;

    public FluidData water;
    public FluidData seawater;
    public FluidData oil;
    public FluidData alcohol;

    public Material waterMaterial;
    public Material seawaterMaterial;
    public Material oilMaterial;
    public Material alcoholMaterial;

    private Renderer waterRenderer;
    private Collider waterCollider;

    void Start()
    {
        waterRenderer = GetComponent<Renderer>();
        waterCollider = GetComponent<Collider>();

        if (currentFluid != null)
        {
            ApplyFluid(currentFluid);
        }
    }

    public BuoyancyPhysics.Snapshot GetPhysicsSnapshot(Rigidbody body, Collider objectCollider)
    {
        return BuoyancyPhysics.Calculate(body, objectCollider, waterCollider, fluidDensity);
    }

    public void ApplyFluid(FluidData fluid)
    {
        if (fluid == null)
            return;

        currentFluid = fluid;
        fluidDensity = fluid.density;

        if (waterRenderer == null)
            return;

        if (fluid == water)
            waterRenderer.material = waterMaterial;
        else if (fluid == seawater)
            waterRenderer.material = seawaterMaterial;
        else if (fluid == oil)
            waterRenderer.material = oilMaterial;
        else if (fluid == alcohol)
            waterRenderer.material = alcoholMaterial;
    }

    public void SelectWater()
    {
        ApplyFluid(water);
    }

    public void SelectSeawater()
    {
        ApplyFluid(seawater);
    }

    public void SelectOil()
    {
        ApplyFluid(oil);
    }

    public void SelectAlcohol()
    {
        ApplyFluid(alcohol);
    }

    private void OnTriggerStay(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;

        if (rb == null)
            return;

        BuoyancyPhysics.Snapshot snapshot = GetPhysicsSnapshot(rb, other);

        rb.AddForce(
            Vector3.up * snapshot.BuoyantForce,
            ForceMode.Force
        );

        rb.AddForce(
            -rb.linearVelocity * damping,
            ForceMode.Force
        );
    }
}
