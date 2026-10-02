using System.Collections;
using UnityEngine;

namespace LightNShadows
{
    public class CameraShake : MonoBehaviour
    {
        public static CameraShake Instance { get; private set; }

        private Vector3 originalLocalPos;
        private Coroutine shakeCoroutine;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            originalLocalPos = transform.localPosition;
        }

        public void Shake(float duration = 0.25f, float magnitude = 0.3f)
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                transform.localPosition = originalLocalPos;
            }
            shakeCoroutine = StartCoroutine(DoShake(duration, magnitude));
        }

        private IEnumerator DoShake(float duration, float magnitude)
        {
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float x = (Random.value * 2f - 1f) * magnitude;
                float y = (Random.value * 2f - 1f) * magnitude;

                transform.localPosition = originalLocalPos + new Vector3(x, y, 0f);

                elapsed += Time.deltaTime;
                // Damp magnitude over time
                magnitude = Mathf.Lerp(magnitude, 0f, elapsed / duration);
                yield return null;
            }

            transform.localPosition = originalLocalPos;
            shakeCoroutine = null;
        }

        public void StopShake()
        {
            if (shakeCoroutine != null)
            {
                StopCoroutine(shakeCoroutine);
                shakeCoroutine = null;
            }
            transform.localPosition = originalLocalPos;
        }
    }
}
