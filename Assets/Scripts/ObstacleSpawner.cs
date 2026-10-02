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

        [Header("Clear Vertical Scatter Layout")]
        [SerializeField] private float spawnX = 12f;
        [SerializeField] private float groundY = -2.15f;       // DOWN BELOW: Floor track spikes (must jump)
        [SerializeField] private float ceilingY = 1.95f;       // HIGH ABOVE: Hanging ceiling spires (run under)
        [SerializeField] private float laserGateY = -0.65f;    // TALL GATE: Stands firmly on track from -2.5f up to +1.2f
        [SerializeField] private float airborneY = -1.15f;     // MID-AIR: Reachable floating diamond (in jump arc)

        [Header("Difficulty & Speed Scaling")]
        [SerializeField] private float initialSpawnInterval = 2.2f;
        [SerializeField] private float minSpawnInterval = 1.0f;
        [SerializeField] private float initialSpeed = 6.8f;
        [SerializeField] private float maxSpeed = 13.2f;
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

            // Spawn reward orbs cleanly at reachable heights
            if (Random.value < 0.35f && prismOrbPrefab != null)
            {
                float orbY = (Random.value > 0.5f) ? -2.3f : airborneY;
                SpawnOrb(spawnX - 1.8f, orbY, speed);
            }

            if (progress < 0.25f)
            {
                // Early game: Distinct below ground spikes & tall gates standing on track
                if (roll < 0.50f)
                {
                    // BELOW: Floor spike (must jump)
                    SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
                }
                else
                {
                    // TALL GATE: Stands on track (phase through with realm shift)
                    SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
                }
            }
            else if (progress < 0.65f)
            {
                // Mid game: Clear vertical separation (Spikes below, Spires high above, Tall Gates, Combos)
                if (roll < 0.35f)
                {
                    // BELOW: Floor spike
                    SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
                }
                else if (roll < 0.60f)
                {
                    // TALL GATE: Stands on track
                    SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
                }
                else if (roll < 0.80f)
                {
                    // ABOVE: Ceiling spire (run under safely, jump hits it)
                    SpawnObstacle(ceilingSpirePrefab, spawnX, ceilingY, DimensionType.Neutral, speed, ObstacleVisualType.CeilingSpire);
                }
                else
                {
                    // Combo: Jump ground spike then phase tall gate
                    StartCoroutine(SpawnJumpThenPhaseCombo(speed));
                }
            }
            else
            {
                // Intense late game: Squeezes, alternating tall gates, and multi-hazard gauntlets
                if (roll < 0.30f)
                {
                    // Precision Squeeze: Floor spike below + Ceiling spire high above
                    StartCoroutine(SpawnCeilingFloorSqueeze(speed));
                }
                else if (roll < 0.65f)
                {
                    // Double tall laser gates
                    StartCoroutine(SpawnDoubleGateRoutine(speed));
                }
                else
                {
                    // Triple Gauntlet: Spike below -> Tall Gate -> Mid-Air Reachable Diamond
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

        private void SpawnOrb(float x, float y, float speed)
        {
            if (prismOrbPrefab == null) return;
            GameObject orb = Instantiate(prismOrbPrefab, new Vector3(x, y, 0f), Quaternion.identity);
            PrismOrb po = orb.GetComponent<PrismOrb>();
            if (po != null) po.Setup(speed);
        }

        // Combo 1: Red Spike below -> Tall Realm Gate
        private IEnumerator SpawnJumpThenPhaseCombo(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.62f);
            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
        }

        // Combo 2: Floor Spike Below + Ceiling Spire Above (Corridor squeeze)
        private IEnumerator SpawnCeilingFloorSqueeze(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.48f);
            SpawnObstacle(ceilingSpirePrefab, spawnX, ceilingY, DimensionType.Neutral, speed, ObstacleVisualType.CeilingSpire);
        }

        // Combo 3: Double Tall Realm Gate (Alternating Light & Shadow)
        private IEnumerator SpawnDoubleGateRoutine(float speed)
        {
            DimensionType first = RandomRealm();
            DimensionType second = (first == DimensionType.Light) ? DimensionType.Shadow : DimensionType.Light;

            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, first, speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.65f);
            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, second, speed, ObstacleVisualType.LaserGate);
        }

        // Combo 4: Triple Gauntlet (Spike Below -> Tall Gate -> Mid-Air Reachable Diamond)
        private IEnumerator SpawnTripleGauntlet(float speed)
        {
            SpawnObstacle(crystalSpikePrefab, spawnX, groundY, DimensionType.Neutral, speed, ObstacleVisualType.CrystalSpike);
            yield return new WaitForSeconds(0.55f);
            SpawnObstacle(laserGatePrefab, spawnX, laserGateY, RandomRealm(), speed, ObstacleVisualType.LaserGate);
            yield return new WaitForSeconds(0.55f);
            SpawnObstacle(floatingDiamondPrefab, spawnX, airborneY, RandomRealm(), speed, ObstacleVisualType.FloatingDiamond);
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
