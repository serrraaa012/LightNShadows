using UnityEngine;

namespace LightNShadows
{
    public class CyberTrackScroller : MonoBehaviour
    {
        [SerializeField] private float scrollSpeed = 7f;
        private SpriteRenderer sr;
        private Material matInstance;

        private void Start()
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                if (sr.sharedMaterial != null)
                {
                    matInstance = new Material(sr.sharedMaterial);
                }
                else
                {
                    matInstance = new Material(Shader.Find("Sprites/Default"));
                }
                sr.material = matInstance;
            }
        }

        private void Update()
        {
            if (GameManager.Instance != null && (!GameManager.Instance.IsPlaying || GameManager.Instance.IsGameOver))
            {
                return;
            }

            if (matInstance != null)
            {
                Vector2 offset = matInstance.mainTextureOffset;
                offset.x += (scrollSpeed * Time.deltaTime) * 0.15f;
                matInstance.mainTextureOffset = offset;
            }
        }
    }
}
