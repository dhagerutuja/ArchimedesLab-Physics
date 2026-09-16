using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectGrabber : MonoBehaviour
{
    public Camera playerCamera;

    private Rigidbody grabbedObject;
    private float grabDistance;
    public bool IsGrabbing => grabbedObject != null;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Rigidbody rb = hit.rigidbody;

                if (rb != null)
                {
                    grabbedObject = rb;
                    grabDistance = hit.distance;
                    grabbedObject.isKinematic = true;
                }
            }
        }

        if (grabbedObject != null)
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

            Vector3 targetPosition =
                ray.origin + ray.direction * grabDistance;

            grabbedObject.MovePosition(targetPosition);
        }

        if (Input.GetMouseButtonUp(0))
        {
            if (grabbedObject != null)
            {
                grabbedObject.isKinematic = false;
                grabbedObject = null;
            }
        }
    }
}
