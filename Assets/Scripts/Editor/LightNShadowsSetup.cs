#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace LightNShadows.Editor
{
    public static class LightNShadowsSetup
    {
        [MenuItem("Tools/LightNShadows/Auto-Setup Scene")]
        public static void SetupScene()
        {
            // 1. Ensure crisp procedural sprites exist
            Sprite squareSprite = GetOrCreateSolidSquareSprite();
            Sprite circleSprite = GetOrCreateSolidCircleSprite();

            // 2. Setup Camera
            Camera cam = Camera.main;
            if (cam == null)
            {
                GameObject camObj = new GameObject("Main Camera");
                cam = camObj.AddComponent<Camera>();
                camObj.tag = "MainCamera";
            }

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.93f, 0.93f, 0.96f);
            cam.transform.position = new Vector3(0f, 0f, -10f);

            if (cam.GetComponent<CameraShake>() == null)
            {
                cam.gameObject.AddComponent<CameraShake>();
            }

            // 3. Setup Managers
            GameObject dimensionMgrObj = GameObject.Find("DimensionManager");
            if (dimensionMgrObj == null)
            {
                dimensionMgrObj = new GameObject("DimensionManager");
                dimensionMgrObj.AddComponent<DimensionManager>();
            }

            GameObject gameMgrObj = GameObject.Find("GameManager");
            if (gameMgrObj == null)
            {
                gameMgrObj = new GameObject("GameManager");
                gameMgrObj.AddComponent<GameManager>();
            }

            GameObject soundMgrObj = GameObject.Find("SoundManager");
            if (soundMgrObj == null)
            {
                soundMgrObj = new GameObject("SoundManager");
                soundMgrObj.AddComponent<SoundManager>();
            }

            GameObject bgObj = GameObject.Find("BackgroundParticles");
            if (bgObj == null)
            {
                bgObj = new GameObject("BackgroundParticles");
                BackgroundParticles bg = bgObj.AddComponent<BackgroundParticles>();
                SerializedObject bgSo = new SerializedObject(bg);
                bgSo.FindProperty("particleSprite").objectReferenceValue = squareSprite;
                bgSo.ApplyModifiedProperties();
            }

            // 4. Setup Ground (Thick platform baseline)
            GameObject groundObj = GameObject.Find("Ground");
            if (groundObj == null)
            {
                groundObj = new GameObject("Ground");
            }
            groundObj.transform.position = new Vector3(0f, -3.2f, 0f);
            groundObj.transform.localScale = new Vector3(35f, 0.8f, 1f);

            SpriteRenderer groundSr = groundObj.GetComponent<SpriteRenderer>();
            if (groundSr == null) groundSr = groundObj.AddComponent<SpriteRenderer>();
            groundSr.sprite = squareSprite;
            groundSr.color = new Color(0.25f, 0.26f, 0.32f);
            groundSr.sortingOrder = 1;

            // 5. Setup Obstacle Prefab
            string prefabsDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabsDir))
            {
                Directory.CreateDirectory(prefabsDir);
                AssetDatabase.Refresh();
            }

            string prefabPath = "Assets/Prefabs/Obstacle.prefab";
            GameObject tempObstacle = new GameObject("Obstacle");
            tempObstacle.transform.localScale = new Vector3(0.75f, 1.8f, 1f);

            SpriteRenderer obsSr = tempObstacle.AddComponent<SpriteRenderer>();
            obsSr.sprite = squareSprite;
            obsSr.sortingOrder = 5;

            BoxCollider2D obsCol = tempObstacle.AddComponent<BoxCollider2D>();
            obsCol.size = new Vector2(1f, 1f);
            obsCol.isTrigger = true;

            tempObstacle.AddComponent<Obstacle>();

            GameObject obstaclePrefab = PrefabUtility.SaveAsPrefabAsset(tempObstacle, prefabPath);
            GameObject.DestroyImmediate(tempObstacle);

            // 6. Setup Spawner
            GameObject spawnerObj = GameObject.Find("ObstacleSpawner");
            if (spawnerObj == null)
            {
                spawnerObj = new GameObject("ObstacleSpawner");
            }
            spawnerObj.transform.position = new Vector3(12f, 0f, 0f);
            ObstacleSpawner spawner = spawnerObj.GetComponent<ObstacleSpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<ObstacleSpawner>();

            SerializedObject spawnerSo = new SerializedObject(spawner);
            spawnerSo.FindProperty("obstaclePrefab").objectReferenceValue = obstaclePrefab;
            spawnerSo.FindProperty("spawnX").floatValue = 12f;
            spawnerSo.FindProperty("groundY").floatValue = -1.9f;
            spawnerSo.FindProperty("floatingY").floatValue = 0.5f;
            spawnerSo.ApplyModifiedProperties();

            // 7. Setup Player (Properly scaled, perfectly grounded)
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj == null)
            {
                playerObj = new GameObject("Player");
            }
            playerObj.SetActive(true);
            playerObj.transform.position = new Vector3(-5f, -2.4f, 0f);
            playerObj.transform.localScale = new Vector3(0.75f, 0.75f, 1f); // Sleek arcade ball size

            // Clean up any old physics components that cause falling
            Rigidbody2D oldRb = playerObj.GetComponent<Rigidbody2D>();
            if (oldRb != null)
            {
                oldRb.bodyType = RigidbodyType2D.Kinematic;
                oldRb.gravityScale = 0f;
            }

            BoxCollider2D oldBox = playerObj.GetComponent<BoxCollider2D>();
            if (oldBox != null) GameObject.DestroyImmediate(oldBox);

            SpriteRenderer playerSr = playerObj.GetComponent<SpriteRenderer>();
            if (playerSr == null) playerSr = playerObj.AddComponent<SpriteRenderer>();
            playerSr.sprite = circleSprite;
            playerSr.color = new Color(0.08f, 0.08f, 0.12f);
            playerSr.sortingOrder = 10;

            CircleCollider2D playerCol = playerObj.GetComponent<CircleCollider2D>();
            if (playerCol == null) playerCol = playerObj.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.5f;
            playerCol.isTrigger = true; // Trigger-based collision for flawless phasing and hit detection

            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc == null) pc = playerObj.AddComponent<PlayerController>();

            // Setup TrailRenderer for juicy movement trail
            TrailRenderer trail = playerObj.GetComponent<TrailRenderer>();
            if (trail == null) trail = playerObj.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.45f;
            trail.endWidth = 0.0f;
            trail.sortingOrder = 9;
            trail.material = new Material(Shader.Find("Sprites/Default"));

            SerializedObject pcSo = new SerializedObject(pc);
            pcSo.FindProperty("spriteRenderer").objectReferenceValue = playerSr;
            pcSo.FindProperty("trailRenderer").objectReferenceValue = trail;
            pcSo.FindProperty("groundY").floatValue = -2.4f;
            pcSo.FindProperty("jumpForce").floatValue = 13f;
            pcSo.FindProperty("gravity").floatValue = 35f;
            pcSo.ApplyModifiedProperties();

            Debug.Log("<color=green>[LightNShadows]</color> Setup calibrated: Compact ball size + Kinematic locked ground physics!");
            EditorUtility.DisplayDialog("LightNShadows", "Setup updated!\n\n1. Ball size is now sleek and compact (0.75x).\n2. Ball is locked to the floor—it will NEVER fall through.\n3. Jump (W / Up Arrow) and Swap (Spacebar / Click) are ready.\n\nPress PLAY to test!", "Let's Go!");
        }

        private static Sprite GetOrCreateSolidSquareSprite()
        {
            string path = "Assets/Sprites/SolidSquare.png";
            EnsureSpritesDirectory();

            if (!File.Exists(path))
            {
                Texture2D tex = new Texture2D(128, 128, TextureFormat.RGBA32, false);
                Color[] colors = new Color[128 * 128];
                for (int i = 0; i < colors.Length; i++) colors[i] = Color.white;
                tex.SetPixels(colors);
                tex.Apply();

                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
            }
            ConfigureTextureAsSprite(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static Sprite GetOrCreateSolidCircleSprite()
        {
            string path = "Assets/Sprites/SolidCircle.png";
            EnsureSpritesDirectory();

            if (!File.Exists(path))
            {
                int size = 128;
                Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
                Vector2 center = new Vector2(size / 2f, size / 2f);
                float radius = (size / 2f) - 2f;

                for (int y = 0; y < size; y++)
                {
                    for (int x = 0; x < size; x++)
                    {
                        float dist = Vector2.Distance(new Vector2(x, y), center);
                        if (dist <= radius)
                        {
                            tex.SetPixel(x, y, Color.white);
                        }
                        else
                        {
                            tex.SetPixel(x, y, Color.clear);
                        }
                    }
                }
                tex.Apply();

                File.WriteAllBytes(path, tex.EncodeToPNG());
                AssetDatabase.ImportAsset(path);
            }
            ConfigureTextureAsSprite(path);

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        private static void EnsureSpritesDirectory()
        {
            if (!Directory.Exists("Assets/Sprites"))
            {
                Directory.CreateDirectory("Assets/Sprites");
                AssetDatabase.Refresh();
            }
        }

        private static void ConfigureTextureAsSprite(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 128;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
