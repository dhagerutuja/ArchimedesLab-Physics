using UnityEngine;

/// <summary>Places the sample on the existing lab floor for free-play preview without affecting buoyancy.</summary>
public class FreeExperimentObjectPresenter : MonoBehaviour
{
    private Rigidbody body;
    private Collider sampleCollider;
    private Collider labFloor;
    private Collider[] tankColliders;
    private WaterVolume water;
    private MaterialController materials;
    private ObjectGrabber grabber;
    private bool previewing;
    [SerializeField] private float showcaseDegreesPerSecond = 18f;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        sampleCollider = GetComponent<Collider>();
        water = FindAnyObjectByType<WaterVolume>();
        materials = FindAnyObjectByType<MaterialController>();
        grabber = FindAnyObjectByType<ObjectGrabber>();
        GameObject floor = GameObject.Find("Labfloor");
        labFloor = floor != null ? floor.GetComponent<Collider>() : null;
        // The tank is assembled from several child colliders. Looking only at the
        // disabled "Tank" collider misses the visible side walls in this scene.
        Transform tankRoot = water != null && water.transform.parent != null ? water.transform.parent : null;
        tankColliders = tankRoot != null ? tankRoot.GetComponentsInChildren<Collider>(true) : System.Array.Empty<Collider>();
    }

    public void BeginFreeExperiment()
    {
        if (body == null || sampleCollider == null || labFloor == null || materials == null) return;
        gameObject.SetActive(true);
        sampleCollider.enabled = true;
        sampleCollider.isTrigger = false;
        body.detectCollisions = true;
        if (grabber == null) grabber = FindAnyObjectByType<ObjectGrabber>();
        if (grabber != null)
        {
            grabber.enabled = true;
            grabber.EnsureCamera();
        }
        materials.ApplyMaterial(materials.wood);
        body.isKinematic = true;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        body.rotation = Quaternion.identity;
        Physics.SyncTransforms();
        body.position = GetStableStagingPosition();
        Physics.SyncTransforms();
        body.isKinematic = false;
        body.useGravity = true;
        body.WakeUp();
        previewing = false;
    }

    private Vector3 GetStableStagingPosition()
    {
        Collider fluidCollider = water != null ? water.GetComponent<Collider>() : null;
        if (fluidCollider == null) return body.position;

        Bounds floorBounds = labFloor.bounds;
        Bounds fluidBounds = fluidCollider.bounds;
        Vector3 extents = sampleCollider.bounds.extents;
        float floorY = floorBounds.max.y + extents.y + .02f;
        const float sideClearance = 1f;

        // This is deliberately world-space and tank-relative. It is on the open left
        // side of the tank (toward the normal lab camera), never at the prior lesson's
        // settled position and never derived from the cinematic camera.
        Vector3 leftSide = new Vector3(fluidBounds.min.x - extents.x - sideClearance, floorY, fluidBounds.center.z);
        if (IsExternalFloorPosition(leftSide, extents, floorBounds, fluidCollider)) return leftSide;

        Vector3 rightSide = new Vector3(fluidBounds.max.x + extents.x + sideClearance, floorY, fluidBounds.center.z);
        if (IsExternalFloorPosition(rightSide, extents, floorBounds, fluidCollider)) return rightSide;

        Debug.LogError("Free Experiment could not find a tank-exterior staging position.", this);
        return leftSide;
    }

    private Vector3 GetExternalStagingPosition()
    {
        Bounds floorBounds = labFloor.bounds;
        Vector3 extents = sampleCollider.bounds.extents;
        float floorY = floorBounds.max.y + extents.y + .01f;
        Camera camera = Camera.main;
        Collider fluidCollider = water != null ? water.GetComponent<Collider>() : null;

        // The lab camera sees the tank at about the centre/right of the screen, so
        // .59-.76 (the previous search) is still over it. Deliberately search the
        // clear lower-right band instead. Each screen point is projected to the real
        // floor, then accepted only if its world-space bounds and camera sight line are clear.
        Vector2 desiredViewport = new Vector2(.86f, .23f);
        Vector3 best = default;
        float bestScore = float.PositiveInfinity;
        for (float x = .78f; x <= .94f; x += .02f)
        for (float y = .14f; y <= .36f; y += .02f)
        {
            Vector2 viewport = new Vector2(x, y);
            if (!TryFloorPoint(camera, viewport, floorY, out Vector3 candidate) ||
                !IsSafeStagingPosition(candidate, extents, floorBounds, fluidCollider, camera))
                continue;

            float score = (viewport - desiredViewport).sqrMagnitude;
            if (score < bestScore)
            {
                best = candidate;
                bestScore = score;
            }
        }

        if (bestScore < float.PositiveInfinity)
            return best;

        // Do not use an unvalidated world-space corner: it can be behind the tank when
        // the camera changes. A second, still camera-derived pass searches the visible
        // lower-middle/right area and retains all of the same safety requirements.
        for (float x = .60f; x <= .96f; x += .02f)
        for (float y = .10f; y <= .42f; y += .02f)
        {
            if (TryFloorPoint(camera, new Vector2(x, y), floorY, out Vector3 candidate) &&
                IsSafeStagingPosition(candidate, extents, floorBounds, fluidCollider, camera))
                return candidate;
        }

        // Never reuse the previous lesson position here: it may be a settled point inside
        // the tank.  This wider camera-derived fallback keeps the object on the lab floor
        // and outside the tank even when a decorative collider blocks the strict sight-line test.
        for (float x = .08f; x <= .94f; x += .02f)
        for (float y = .08f; y <= .46f; y += .02f)
        {
            if (TryFloorPoint(camera, new Vector2(x, y), floorY, out Vector3 candidate) &&
                IsExternalFloorPosition(candidate, extents, floorBounds, fluidCollider))
                return candidate;
        }

        Debug.LogError("Free Experiment could not find a floor staging position outside the tank and water.", this);
        return new Vector3(floorBounds.center.x, floorY, floorBounds.min.z + extents.z + .1f);
    }

    private static bool TryFloorPoint(Camera camera, Vector2 viewport, float floorY, out Vector3 point)
    {
        point = default;
        if (camera == null) return false;
        Ray ray = camera.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
        if (!new Plane(Vector3.up, new Vector3(0f, floorY, 0f)).Raycast(ray, out float distance)) return false;
        point = ray.GetPoint(distance);
        point.y = floorY;
        return true;
    }

    private bool IsSafeStagingPosition(Vector3 position, Vector3 extents, Bounds floorBounds, Collider fluidCollider, Camera camera)
    {
        const float clearance = .65f;
        if (position.x - extents.x < floorBounds.min.x || position.x + extents.x > floorBounds.max.x ||
            position.z - extents.z < floorBounds.min.z || position.z + extents.z > floorBounds.max.z)
            return false;

        Bounds sampleAtPosition = new Bounds(position, extents * 2f);
        if (fluidCollider != null && IntersectsWithClearance(sampleAtPosition, fluidCollider.bounds, clearance)) return false;
        foreach (Collider tankCollider in tankColliders)
        {
            if (tankCollider != null && tankCollider != fluidCollider &&
                IntersectsWithClearance(sampleAtPosition, tankCollider.bounds, clearance))
                return false;
        }

        Vector3 viewport = camera.WorldToViewportPoint(position);
        if (viewport.z <= 0f || viewport.x < .60f || viewport.x > .96f || viewport.y < .10f || viewport.y > .42f)
            return false;

        // Reject a point that is geometrically outside the water but visually behind a
        // tank wall or the water volume from the active Game-view camera.
        Vector3 target = position + Vector3.up * extents.y * .35f;
        Vector3 direction = target - camera.transform.position;
        float distance = direction.magnitude;
        RaycastHit[] hits = Physics.RaycastAll(camera.transform.position, direction / distance, distance, ~0, QueryTriggerInteraction.Collide);
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider == sampleCollider || hit.collider == labFloor) continue;
            if (hit.collider == fluidCollider || IsTankCollider(hit.collider)) return false;
        }
        return true;
    }

    private bool IsExternalFloorPosition(Vector3 position, Vector3 extents, Bounds floorBounds, Collider fluidCollider)
    {
        if (position.x - extents.x < floorBounds.min.x || position.x + extents.x > floorBounds.max.x ||
            position.z - extents.z < floorBounds.min.z || position.z + extents.z > floorBounds.max.z)
            return false;

        Bounds sampleAtPosition = new Bounds(position, extents * 2f);
        if (fluidCollider != null && IntersectsWithClearance(sampleAtPosition, fluidCollider.bounds, .15f)) return false;
        foreach (Collider tankCollider in tankColliders)
            if (tankCollider != null && tankCollider != fluidCollider && IntersectsWithClearance(sampleAtPosition, tankCollider.bounds, .15f)) return false;
        return true;
    }

    private bool IsTankCollider(Collider candidate)
    {
        foreach (Collider tankCollider in tankColliders)
            if (candidate == tankCollider) return true;
        return false;
    }

    private static bool IntersectsWithClearance(Bounds sample, Bounds obstacle, float clearance)
    {
        obstacle.Expand(Vector3.one * clearance * 2f);
        return sample.Intersects(obstacle);
    }

    private void Update()
    {
        if (!previewing) return;
        if (grabber != null && grabber.IsGrabbing)
        {
            previewing = false;
            return;
        }
        transform.Rotate(Vector3.up, showcaseDegreesPerSecond * Time.deltaTime, Space.World);
    }
}
