using UnityEngine;

public class CameraDrag : MonoBehaviour
{
    [Header("Drag Settings")]
    public float dragSpeed = 2f;
    private float minX = -137f, maxX = 205f;

    [Header("Auto-Pan Settings")]
    public float autoPanDuration = 3f; // Durasi perpindahan (detik)
    public AnimationCurve autoPanCurve = AnimationCurve.EaseInOut(0, 0, 1, 1); // Kurva easing

    private Vector3 dragOrigin;
    private int lastScreenWidth, lastScreenHeight;
    private CameraShake cameraShake;

    private Vector3 targetPosition;
    private float smoothTime = 0.2f;
    private Vector3 velocity = Vector3.zero;

    private bool canDrag = true;
    private bool isAutoPanning = false;
    private float autoPanProgress = 0f;
    private Vector3 autoPanStartPos;

    void Start()
    {
        cameraShake = GetComponent<CameraShake>();

        // ✅ Auto-pan hanya di portrait mode
        if (Screen.width < Screen.height)
        {
            autoPanStartPos = new Vector3(maxX, transform.position.y, transform.position.z);
            targetPosition = new Vector3(minX, transform.position.y, transform.position.z);
            transform.position = autoPanStartPos;
            isAutoPanning = true;
            autoPanProgress = 0f;

            // ✅ Nonaktifkan CameraShake sementara selama auto-pan
            if (cameraShake != null)
            {
                cameraShake.enabled = false;
            }
        }
        else
        {
            targetPosition = transform.position;
        }
    }

    void Update()
    {
        // Update screen size changes
        if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
        {
            UpdateCameraBounds();
        }

        // ✅ Nonaktifkan drag di landscape mode
        if (Screen.width > Screen.height)
        {
            canDrag = false;
            return;
        }
        else
        {
            canDrag = true;
        }

        // ✅ Auto-pan logic (lebih smooth dengan AnimationCurve)
        if (isAutoPanning)
        {
            autoPanProgress += Time.deltaTime / autoPanDuration;
            float t = autoPanCurve.Evaluate(autoPanProgress);
            transform.position = Vector3.Lerp(autoPanStartPos, targetPosition, t);

            // Selesai auto-pan
            if (autoPanProgress >= 1f)
            {
                isAutoPanning = false;
                if (cameraShake != null)
                {
                    cameraShake.enabled = true; // Aktifkan kembali CameraShake
                }
            }
            return; // Skip input drag selama auto-pan
        }

        // Input drag (hanya jika tidak auto-panning)
        if (canDrag && Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;
        }

        if (canDrag && Input.GetMouseButton(0))
        {
            Vector3 difference = Input.mousePosition - dragOrigin;
            if (difference.sqrMagnitude > 0.01f)
            {
                float moveX = -difference.x * dragSpeed * Time.deltaTime;
                targetPosition = transform.position + new Vector3(moveX, 0, 0);
                targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
                dragOrigin = Input.mousePosition;
            }
        }

        // Smoothing pergerakan kamera
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);

        // ✅ Update CameraShake reference position
        if (cameraShake != null && !isAutoPanning)
        {
            cameraShake.SetOriginalLocalPos(transform.localPosition);
        }
    }

    void UpdateCameraBounds()
    {
        lastScreenWidth = Screen.width;
        lastScreenHeight = Screen.height;
    }
}