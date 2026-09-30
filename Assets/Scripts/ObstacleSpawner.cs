using System.Collections;
using UnityEngine;

namespace LightNShadows
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Styled Prefabs")]
        [SerializeField] private GameObject crystalSpikePrefab;
        [SerializeField] private GameObject laserGatePrefab;
        [SerializeField] private GameObject floatingDiamondPrefab;

        [Header("Spawn Layout Coordinates")]
        [SerializeField] private float spawnX = 12f;
        [SerializeField] private float groundY = -2.15f;
        [SerializeField] private float airborneY = 0.35f;

        [Header("Difficulty & Speed Scaling")]
        [SerializeField] private float initialSpawnInterval = 2.1f;
        [SerializeField] private float minSpawnInterval = 1.05f;
        [SerializeField] private float initialSpeed = 6.8f;
        [SerializeField] private float maxSpeed = 13.0f;
        [SerializeField] private float difficultyRampTime = 60f;

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
                timer = 1.0f;
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
            float roll = Random.value;

            if (progress < 0.25f)
            {
                // Early game: Introduce Red Spike (Must Jump) and Realm Gate (Must Phase)
                if (roll < 0.5f)
                {
                    // Red Spike -> MUST JUMP!
                    SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
                }
                else
                {
                    // Tall Realm Gate -> MUST PHASE!
                    SpawnObstacle(laserGatePrefab, spawnX, -0.65f, RandomRealm(), speed, ObstacleVisualType.LaserGate);
                }
            }
            else if (progress < 0.65f)
            {
                // Mid game: Combos (Jump + Phase combinations)
                if (roll < 0.35f)
                {
                    // Red Spike
                    SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
                }
                else if (roll < 0.65f)
                {
                    // Realm Gate
                    SpawnObstacle(laserGatePrefab, spawnX, -0.65f, RandomRealm(), speed, ObstacleVisualType.LaserGate);
                }
                else if (roll < 0.82f)
                {
                    // Floating Drone
                    SpawnObstacle(floatingDiamondPrefab, spawnX, airborneY, RandomRealm(), speed, ObstacleVisualType.FloatingDiamond);
                }
                else
                {
                    // Quick Combo: Red Spike (Jump) -> Realm Gate (Phase)
                    StartCoroutine(SpawnJumpThenPhaseCombo(speed));
                }
            }
            else
            {
                // Late game: fast rhythmic sequences
                if (roll < 0.35f)
                {
                    StartCoroutine(SpawnJumpThenPhaseCombo(speed));
                }
                else if (roll < 0.7f)
                {
                    StartCoroutine(SpawnDoubleGateRoutine(speed));
                }
                else
                {
                    StartCoroutine(SpawnTripleGauntlet(speed));
                }
            }
        }

        private void SpawnObstacle(GameObject prefab, float x, float y, DimensionType dim, float speed, ObstacleVisualType visualType)
        {
            if (prefab == null) return;

            GameObject obj = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity);
            Obstacle obs = obj.GetComponent<Obstacle>();
            if (obs != null)
            {
                obs.Setup(dim, speed, visualType);
            }
        }

        // Combo 1: Red Spike (Must Jump!) -> Realm Gate (Must Phase!)
        private IEnumerator SpawnJumpThenPhaseCombo(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.65f);
            SpawnObstacle(laserGatePrefab, spawnX, -0.65f, RandomRealm(), speed, ObstacleVisualType.LaserGate);
        }

        // Combo 2: Double Realm Gate (Phase -> Swap -> Phase)
        private IEnumerator SpawnDoubleGateRoutine(float speed)
        {
            DimensionType first = RandomRealm();
            DimensionType second = (first == DimensionType.Light) ? DimensionType.Shadow : DimensionType.Light;

            SpawnObstacle(laserGatePrefab, spawnX, -0.65f, first, speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.62f);
            SpawnObstacle(laserGatePrefab, spawnX, -0.65f, second, speed, ObstacleVisualType.LaserGate);
        }

        // Combo 3: Jump -> Gate -> Airborne Drone
        private IEnumerator SpawnTripleGauntlet(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.55f);
            SpawnObstacle(laserGatePrefab, spawnX, -0.65f, RandomRealm(), speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.55f);
            SpawnObstacle(floatingDiamondPrefab, spawnX, airborneY, RandomRealm(), speed, ObstacleVisualType.FloatingDiamond);
        }

        private DimensionType RandomRealm()
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
