using System.Collections;
using UnityEngine;

namespace LightNShadows
{
    public class ShardExplosion : MonoBehaviour
    {
        public static void Create(Vector3 position, Color shardColor, Sprite sprite)
        {
            GameObject container = new GameObject("ShardExplosion");
            container.transform.position = position;

            int shardCount = 16;
            for (int i = 0; i < shardCount; i++)
            {
                GameObject shard = new GameObject($"Shard_{i}");
                shard.transform.SetParent(container.transform);
                shard.transform.position = position;
                shard.transform.localScale = Vector3.one * Random.Range(0.12f, 0.25f);

                SpriteRenderer sr = shard.AddComponent<SpriteRenderer>();
                sr.sprite = sprite;
                sr.color = shardColor;
                sr.sortingOrder = 15;

                float angle = (360f / shardCount) * i + Random.Range(-15f, 15f);
                Vector2 dir = Quaternion.Euler(0, 0, angle) * Vector2.right;
                float speed = Random.Range(4f, 10f);

                ShardMovement movement = shard.AddComponent<ShardMovement>();
                movement.Initialize(dir * speed, Random.Range(-360f, 360f));
            }

            Destroy(container, 1.2f);
        }
    }

    public class ShardMovement : MonoBehaviour
    {
        private Vector2 velocity;
        private float rotSpeed;
        private SpriteRenderer sr;
        private float lifetime = 0.7f;
        private float timer = 0f;
        private Vector3 startScale;

        public void Initialize(Vector2 vel, float rot)
        {
            velocity = vel;
            rotSpeed = rot;
            sr = GetComponent<SpriteRenderer>();
            startScale = transform.localScale;
        }

        private void Update()
        {
            timer += Time.deltaTime;
            float t = timer / lifetime;

            transform.position += (Vector3)(velocity * Time.deltaTime);
            velocity = Vector2.Lerp(velocity, Vector2.zero, Time.deltaTime * 3.5f);
            transform.Rotate(0, 0, rotSpeed * Time.deltaTime);

            transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);

            if (sr != null)
            {
                Color c = sr.color;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;
            }
        }
    }
}
