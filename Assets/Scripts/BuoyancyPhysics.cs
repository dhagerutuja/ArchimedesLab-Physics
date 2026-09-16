using UnityEngine;

/// <summary>Shared calculation used by buoyancy, UI, arrows, lessons, and challenges.</summary>
public static class BuoyancyPhysics
{
    public struct Snapshot
    {
        public float Volume;
        public float SubmergedRatio;
        public float DisplacedVolume;
        public float BuoyantForce;
        public float Weight;
        public bool IsInFluid;
    }

    public static Snapshot Calculate(Rigidbody body, Collider objectCollider, Collider fluidCollider, float fluidDensity)
    {
        var snapshot = new Snapshot();
        if (body == null || objectCollider == null || fluidCollider == null)
            return snapshot;

        Bounds objectBounds = objectCollider.bounds;
        snapshot.Volume = objectBounds.size.x * objectBounds.size.y * objectBounds.size.z;
        snapshot.Weight = body.mass * Physics.gravity.magnitude;
        snapshot.IsInFluid = objectBounds.Intersects(fluidCollider.bounds);
        if (!snapshot.IsInFluid || objectBounds.size.y <= Mathf.Epsilon)
            return snapshot;

        float submergedHeight = Mathf.Clamp(fluidCollider.bounds.max.y - objectBounds.min.y, 0f, objectBounds.size.y);
        snapshot.SubmergedRatio = submergedHeight / objectBounds.size.y;
        snapshot.DisplacedVolume = snapshot.Volume * snapshot.SubmergedRatio;
        snapshot.BuoyantForce = fluidDensity * Physics.gravity.magnitude * snapshot.DisplacedVolume;
        return snapshot;
    }
}
