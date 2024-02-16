using System.Collections;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public IEnumerator Shake(float duration, float magnitude, Transform cameraContainerTransform)
    {
        Vector3 originalPosition = cameraContainerTransform.localPosition;
        float elapsed = 0f;
        float taper = 1f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude * taper;
            float y = Random.Range(-1f, 1f) * magnitude * taper;

            cameraContainerTransform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);
            taper = Mathf.Lerp(1f, 0f, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        cameraContainerTransform.localPosition = originalPosition;
    }
}