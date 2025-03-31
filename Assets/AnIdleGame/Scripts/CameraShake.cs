using UnityEngine;
using System.Collections;

public class CameraShake : MonoBehaviour
{
    public Transform camTransform;

    public float shakeDuration = 0f;
    public float shakeAmount = 0.7f;
    public float decreaseFactor = 1.0f;

    public Vector3 originalLocalPos; // Made public

    private Vector3 shakeOffset; // Shake offset

    void Awake()
    {
        if (camTransform == null)
        {
            camTransform = GetComponent<Transform>();
        }
    }

    void OnEnable()
    {
        originalLocalPos = camTransform.localPosition;
    }

    void Update()
    {
        if (shakeDuration > 0)
        {
            shakeOffset = Random.insideUnitSphere * shakeAmount;
            shakeDuration -= Time.deltaTime * decreaseFactor;
        }
        else
        {
            shakeDuration = 0f;
            shakeOffset = Vector3.zero;
        }

        // Apply shakeOffset on top of current camera position
        camTransform.localPosition = originalLocalPos + shakeOffset;
    }

	public void SetOriginalLocalPos(Vector3 newPos)
	{
		originalLocalPos = newPos;
	}
}
