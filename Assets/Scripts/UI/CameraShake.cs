using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraShake : MonoBehaviour
{
    private static CameraShake _instance;

    private void Awake() => _instance = this;

    public static void Shake(float duration, float magnitude = 0.05f)
    {
        if (_instance != null)
            _instance.StartCoroutine(_instance.DoShake(duration, magnitude));
    }

    private IEnumerator DoShake(float duration, float magnitude)
    {
        Vector3 origin  = transform.localPosition;
        float   elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localPosition = origin + new Vector3(
                Random.Range(-1f, 1f) * magnitude,
                Random.Range(-1f, 1f) * magnitude,
                0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = origin;
    }
}
