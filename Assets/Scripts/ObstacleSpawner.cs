using System.Collections;
using UnityEngine;

namespace LightNShadows
{
    public class ObstacleSpawner : MonoBehaviour
    {
        [Header("Styled Hazard Prefabs")]
        [SerializeField] private GameObject crystalSpikePrefab;
        [SerializeField] private GameObject ceilingSpirePrefab;
        [SerializeField] private GameObject laserGatePrefab;
        [SerializeField] private GameObject floatingDiamondPrefab;

        [Header("Collectibles")]
        [SerializeField] private GameObject prismOrbPrefab;

        [Header("Layout Coordinates")]
        [SerializeField] private float spawnX = 12f;
        [SerializeField] private float groundY = -2.15f;
        [SerializeField] private float ceilingY = 1.95f;
        [SerializeField] private float hoopGroundY = -0.85f;
        [SerializeField] private float hoopAirborneY = 0.45f;
        [SerializeField] private float laserGateY = -0.55f;

        [Header("Difficulty & Speed Scaling")]
        [SerializeField] private float initialSpawnInterval = 2.2f;
        [SerializeField] private float minSpawnInterval = 0.95f;
        [SerializeField] private float initialSpeed = 6.8f;
        [SerializeField] private float maxSpeed = 13.5f;
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

            // Spawn occasional reward orbs between obstacles
            if (Random.value < 0.35f && prismOrbPrefab != null)
            {
                SpawnOrb(spawnX - 1.8f, (Random.value > 0.5f) ? -0.8f : 0.8f, speed);
            }

            if (progress < 0.25f)
            {
                // Early game: Single ground spikes, neon laser gates, and floating neon hoops
                if (roll < 0.35f)
                {
                    SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
                }
                else if (roll < 0.70f)
                {
                    SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
                }
                else
                {
                    // Floating Neon Phase Hoop
                    SpawnNeonHoop(spawnX, hoopGroundY, RandomRealm(), speed);
                }
            }
            else if (progress < 0.65f)
            {
                // Mid game: Neon Laser Gates, Neon Hoops, Ceiling Spires, and Combos
                if (roll < 0.22f)
                {
                    SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
                }
                else if (roll < 0.44f)
                {
                    SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
                }
                else if (roll < 0.62f)
                {
                    // Airborne Neon Phase Hoop (jump to ring or phase through)
                    SpawnNeonHoop(spawnX, hoopAirborneY, RandomRealm(), speed);
                }
                else if (roll < 0.78f)
                {
                    SpawnObstacle(ceilingSpirePrefab, spawnX, ceilingY, DimensionType.Neutral, speed, ObstacleVisualType.CeilingSpire);
                }
                else
                {
                    StartCoroutine(SpawnSpikeThenHoopCombo(speed));
                }
            }
            else
            {
                // Intense late game: Neon Laser gauntlets, rapid phase hoops, squeezes
                if (roll < 0.25f)
                {
                    StartCoroutine(SpawnCeilingFloorSqueeze(speed));
                }
                else if (roll < 0.50f)
                {
                    StartCoroutine(SpawnDoubleLaserGateRoutine(speed));
                }
                else if (roll < 0.75f)
                {
                    StartCoroutine(SpawnLaserThenHoopWave(speed));
                }
                else
                {
                    StartCoroutine(SpawnTripleNeonGauntlet(speed));
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

        private void SpawnNeonHoop(float x, float y, DimensionType dim, float speed)
        {
            // Instantiate using laserGatePrefab as robust base template
            GameObject basePrefab = (laserGatePrefab != null) ? laserGatePrefab : crystalSpikePrefab;
            if (basePrefab == null) return;

            GameObject obj = Instantiate(basePrefab, new Vector3(x, y, 0f), Quaternion.identity);
            obj.name = "NeonHoop";
            Obstacle obs = obj.GetComponent<Obstacle>();
            if (obs != null)
            {
                obs.Setup(dim, speed, ObstacleVisualType.NeonHoop);
            }
        }

        private void SpawnOrb(float x, float y, float speed)
        {
            if (prismOrbPrefab == null) return;
            GameObject orb = Instantiate(prismOrbPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            PrismOrb po = orb.GetComponent<PrismOrb>();
            if (po != null) po.Setup(speed);
        }

        // Combo 1: Jump Spike -> Floating Neon Hoop
        private IEnumerator SpawnSpikeThenHoopCombo(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.60f);
            SpawnNeonHoop(spawnX, hoopGroundY, RandomRealm(), speed);
        }

        // Combo 2: Floor Spike + Ceiling Spire (Precision squeeze!)
        private IEnumerator SpawnCeilingFloorSqueeze(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.48f);
            SpawnObstacle(ceilingSpirePrefab, spawnX, ceilingY, DimensionType.Neutral, speed, ObstacleVisualType.CeilingSpire);
        }

        // Combo 3: Double Neon Laser Gate (Rapid realm flip)
        private IEnumerator SpawnDoubleLaserGateRoutine(float speed)
        {
            DimensionType first = RandomRealm();
            DimensionType second = (first == DimensionType.Light) ? DimensionType.Shadow : DimensionType.Light;

            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, first, speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.62f);
            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, second, speed, ObstacleVisualType.LaserGate);
        }

        // Combo 4: Neon Laser Gate -> Floating Neon Hoop
        private IEnumerator SpawnLaserThenHoopWave(float speed)
        {
            DimensionType first = RandomRealm();
            DimensionType second = (first == DimensionType.Light) ? DimensionType.Shadow : DimensionType.Light;

            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, first, speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.55f);
            SpawnNeonHoop(spawnX, hoopAirborneY, second, speed);
        }

        // Combo 5: Triple Neon Gauntlet (Ground Spike -> Laser Gate -> Floating Neon Drone)
        private IEnumerator SpawnTripleNeonGauntlet(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.52f);
            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.52f);
            SpawnObstacle(floatingDiamondPrefab, spawnX, 0.35f, RandomRealm(), speed, ObstacleVisualType.FloatingDiamond);
        }

        private DimensionType RandomRealm()
        {
            return (Random.value > 0.5f) ? DimensionType.Light : DimensionType.Shadow;
        }

        public void ResetSpawner()
        {
            StopAllCoroutines();
            gameTime = 0f;
            timer = 0f;
            isSpawningStarted = false;

            // Clear all active obstacles
            Obstacle[] obstacles = FindObjectsOfType<Obstacle>();
            for (int i = 0; i < obstacles.Length; i++)
            {
                if (obstacles[i] != null)
                {
                    Destroy(obstacles[i].gameObject);
                }
            }
            Obstacle.ActiveObstacles.Clear();

            // Clear all active prism orbs
            PrismOrb[] orbs = FindObjectsOfType<PrismOrb>();
            for (int i = 0; i < orbs.Length; i++)
            {
                if (orbs[i] != null)
                {
                    Destroy(orbs[i].gameObject);
                }
            }
        }
    }
}
