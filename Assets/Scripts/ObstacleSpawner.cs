using System.Collections;
using UnityEngine;

namespace LightNShadows
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject obstaclePrefab;

        [Header("Spawn Layout Coordinates")]
        [SerializeField] private float spawnX = 12f;
        [SerializeField] private float groundY = -1.9f;
        [SerializeField] private float floatingY = 0.5f;

        [Header("Difficulty & Speed Scaling")]
        [SerializeField] private float initialSpawnInterval = 2.2f;
        [SerializeField] private float minSpawnInterval = 1.1f;
        [SerializeField] private float initialSpeed = 6.5f;
        [SerializeField] private float maxSpeed = 12.5f;
        [SerializeField] private float difficultyRampTime = 60f; // Seconds to max difficulty

        private float timer = 0f;
        private float gameTime = 0f;
        private bool isSpawningStarted = false;

        private void Start()
        {
            timer = 0f;
            gameTime = 0f;
        }

        private void Update()
        {
            if (GameManager.Instance != null && (!GameManager.Instance.IsPlaying || GameManager.Instance.IsGameOver))
            {
                return;
            }

            if (!isSpawningStarted)
            {
                isSpawningStarted = true;
                timer = 1.0f; // 1s breather after pressing Start
                return;
            }

            gameTime += Time.deltaTime;
            float progress = Mathf.Clamp01(gameTime / difficultyRampTime);
            float currentInterval = Mathf.Lerp(initialSpawnInterval, minSpawnInterval, progress);
            float currentSpeed = Mathf.Lerp(initialSpeed, maxSpeed, progress);

            timer += Time.deltaTime;
            if (timer >= currentInterval)
            {
                timer = 0f;
                ChooseAndExecutePattern(currentSpeed, progress);
            }
        }

        private void ChooseAndExecutePattern(float speed, float progress)
        {
            if (obstaclePrefab == null) return;

            // Roll a pattern based on game progression
            float roll = Random.value;

            if (progress < 0.25f)
            {
                // Early game: single obstacles and occasional gates
                if (roll < 0.7f)
                {
                    SpawnSingle(speed, RandomDimension(), groundY, 1.8f);
                }
                else
                {
                    SpawnTallGate(speed, RandomDimension());
                }
            }
            else if (progress < 0.6f)
            {
                // Mid game: mix in double swaps
                if (roll < 0.45f)
                {
                    SpawnSingle(speed, RandomDimension(), (Random.value > 0.4f) ? groundY : floatingY, 1.8f);
                }
                else if (roll < 0.75f)
                {
                    SpawnTallGate(speed, RandomDimension());
                }
                else
                {
                    StartCoroutine(SpawnDoubleSwapRoutine(speed));
                }
            }
            else
            {
                // Intense late game: fast rhythm sequences and mixed gates
                if (roll < 0.35f)
                {
                    SpawnTallGate(speed, RandomDimension());
                }
                else if (roll < 0.7f)
                {
                    StartCoroutine(SpawnDoubleSwapRoutine(speed));
                }
                else
                {
                    StartCoroutine(SpawnRhythmRunRoutine(speed));
                }
            }
        }

        private void SpawnSingle(float speed, DimensionType type, float yPos, float height)
        {
            GameObject obj = Instantiate(obstaclePrefab, new Vector3(spawnX, yPos, 0f), Quaternion.identity);
            obj.transform.localScale = new Vector3(0.75f, height, 1f);

            Obstacle obs = obj.GetComponent<Obstacle>();
            if (obs != null) obs.Setup(type, speed);
        }

        private void SpawnTallGate(float speed, DimensionType type)
        {
            // A tall 3.8 unit pillar: cannot be jumped over, MUST be phased through!
            GameObject obj = Instantiate(obstaclePrefab, new Vector3(spawnX, -1.0f, 0f), Quaternion.identity);
            obj.transform.localScale = new Vector3(0.85f, 3.8f, 1f);

            Obstacle obs = obj.GetComponent<Obstacle>();
            if (obs != null) obs.Setup(type, speed);
        }

        private IEnumerator SpawnDoubleSwapRoutine(float speed)
        {
            DimensionType first = RandomDimension();
            DimensionType second = (first == DimensionType.Light) ? DimensionType.Shadow : DimensionType.Light;

            SpawnSingle(speed, first, groundY, 1.8f);
            yield return new WaitForSeconds(0.42f);
            SpawnSingle(speed, second, groundY, 1.8f);
        }

        private IEnumerator SpawnRhythmRunRoutine(float speed)
        {
            // Rapid 3-beat rhythm sequence: Light -> Shadow -> Light
            DimensionType current = RandomDimension();
            for (int i = 0; i < 3; i++)
            {
                SpawnSingle(speed, current, groundY, 1.7f);
                current = (current == DimensionType.Light) ? DimensionType.Shadow : DimensionType.Light;
                yield return new WaitForSeconds(0.48f);
            }
        }

        private DimensionType RandomDimension()
        {
            return (Random.value > 0.5f) ? DimensionType.Light : DimensionType.Shadow;
        }

        public void ResetSpawner()
        {
            gameTime = 0f;
            timer = 0f;
            isSpawningStarted = false;
        }
    }
}
