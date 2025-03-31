using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    public float dragSpeed = 2f;
    private float minX = -137f, maxX = 205f; // Set fixed movement boundaries

    private Vector3 dragOrigin;
    private int lastScreenWidth, lastScreenHeight;
    private CameraShake cameraShake;

    private Vector3 targetPosition;
    private float smoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;

    private bool canDrag = true; // Flag to enable/disable dragging

    void Start()
    {
        cameraShake = GetComponent<CameraShake>();

        // ✅ Set position to -137 when starting in portrait mode
        if (Screen.width < Screen.height) 
        {
            transform.position = new Vector3(minX, transform.position.y, transform.position.z);
        }

        targetPosition = transform.position;
    }

    void Update()
    {
        // Check for screen size changes to adjust camera bounds
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraBounds();
        }

        // ✅ Disable dragging in landscape mode
        if (Screen.width > Screen.height)
        {
            canDrag = false;
            return;
        }
        else
        {
            canDrag = true;
        }

        if (canDrag && Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }

        if (canDrag && Input.GetMouseButton(0))
        {
            Vector3 difference = Input.mousePosition - dragOrigin;

            // ✅ Fix movement so it does not snap unexpectedly
            if (difference.sqrMagnitude > 0.01f)
            {
                float moveX = -difference.x * dragSpeed * Time.deltaTime;

                // Update target position
                targetPosition = transform.position + new Vector3(moveX, 0, 0);
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);

                // ✅ Update drag origin correctly after moving
                dragOrigin = Input.mousePosition;
            }
        }

        // Smooth camera movement
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // ✅ Ensure CameraShake does not override new position
        if (cameraShake != null)
        {
            cameraShake.SetOriginalLocalPos(transform.localPosition);
        }
    }

    void UpdateCameraBounds()
    {
        // ✅ Ensure bounds remain fixed
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}
