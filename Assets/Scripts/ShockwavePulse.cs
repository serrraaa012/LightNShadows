using UnityEngine;

namespace LightNShadows
{
    public class ShockwavePulse : MonoBehaviour
    {
        public static void Create(Vector3 position, Color color, Sprite sprite)
        {
            GameObject wave = new GameObject("Shockwave");
            wave.transform.position = position;
            wave.transform.localScale = Vector3.one * 0.4f;

            SpriteRenderer sr = wave.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.color = color;
            sr.sortingOrder = 8;

            ShockwaveAnim anim = wave.AddComponent<ShockwaveAnim>();
            anim.Initialize(color);
        }
    }

    public class ShockwaveAnim : MonoBehaviour
    {
        private Color baseColor;
        private SpriteRenderer sr;
        private float duration = 0.28f;
        private float timer = 0f;

        public void Initialize(Color c)
        {
            baseColor = c;
            sr = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / duration;

            float scale = Mathf.Lerp(0.4f, 3.2f, t);
            transform.localScale = new Vector3(scale, scale, 1f);

            if (sr != null)
            {
                Color c = baseColor;
                c.a = Mathf.Lerp(0.9f, 0f, t * t);
                sr.color = c;
            }

            if (timer >= duration)
            {
                Destroy(gameObject);
            }
        }
    }
}
