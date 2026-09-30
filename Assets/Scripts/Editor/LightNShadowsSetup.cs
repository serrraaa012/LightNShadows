#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.IO;

namespace LightNShadows.Editor
{
    public static class LightNShadowsSetup
    {
        [MenuItem("Tools/LightNShadows/Auto-Setup Scene")]
        public static void SetupScene()
        {
            // 1. Generate all custom stylized vector sprites
            SpriteArtGenerator.GenerateAllSprites();

            Sprite spikeSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CrystalSpike.png");
            Sprite gateSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/LaserGate.png");
            Sprite diamondSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/FloatingDiamond.png");
            Sprite playerCoreSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerCore.png");
            Sprite playerHaloSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/PlayerHalo.png");
            Sprite trackSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/CyberTrack.png");
            Sprite monolithSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Monolith.png");
            Sprite squareSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/SolidSquare.png");

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
            cam.backgroundColor = new Color(0.96f, 0.94f, 0.90f); // Warm Solar Dawn
            cam.transform.position = new Vector3(0f, 0f, -10f);

            UniversalAdditionalCameraData camData = cam.GetUniversalAdditionalCameraData();
            if (camData != null) camData.renderPostProcessing = true;

            if (cam.GetComponent<CameraShake>() == null)
            {
                cam.gameObject.AddComponent<CameraShake>();
            }

            // 3. Setup Global Volume & URP Bloom
            SetupGlobalPostProcessing();

            // 4. Setup Managers
            GameObject dimensionMgrObj = GameObject.Find("DimensionManager");
            if (dimensionMgrObj == null) dimensionMgrObj = new GameObject("DimensionManager");
            DimensionManager dimMgr = dimensionMgrObj.GetComponent<DimensionManager>();
            if (dimMgr == null) dimMgr = dimensionMgrObj.AddComponent<DimensionManager>();

            SerializedObject dimSo = new SerializedObject(dimMgr);
            dimSo.FindProperty("lightBackgroundColor").colorValue = new Color(0.96f, 0.94f, 0.90f); // Warm Solar Dawn
            dimSo.FindProperty("shadowBackgroundColor").colorValue = new Color(0.035f, 0.04f, 0.08f); // Deep Cosmic Void
            dimSo.ApplyModifiedProperties();

            GameObject gameMgrObj = GameObject.Find("GameManager");
            if (gameMgrObj == null) gameMgrObj = new GameObject("GameManager");
            if (gameMgrObj.GetComponent<GameManager>() == null) gameMgrObj.AddComponent<GameManager>();

            GameObject soundMgrObj = GameObject.Find("SoundManager");
            if (soundMgrObj == null) soundMgrObj = new GameObject("SoundManager");
            if (soundMgrObj.GetComponent<SoundManager>() == null) soundMgrObj.AddComponent<SoundManager>();

            // 5. Parallax Backdrop (Distant Monoliths)
            GameObject parallaxObj = GameObject.Find("ParallaxBackdrop");
            if (parallaxObj == null) parallaxObj = new GameObject("ParallaxBackdrop");
            ParallaxBackdrop pb = parallaxObj.GetComponent<ParallaxBackdrop>();
            if (pb == null) pb = parallaxObj.AddComponent<ParallaxBackdrop>();
            SerializedObject pbSo = new SerializedObject(pb);
            pbSo.FindProperty("monolithSprite").objectReferenceValue = monolithSprite;
            pbSo.ApplyModifiedProperties();

            // 6. Ambient Speed Particles
            GameObject bgObj = GameObject.Find("BackgroundParticles");
            if (bgObj == null) bgObj = new GameObject("BackgroundParticles");
            BackgroundParticles bg = bgObj.GetComponent<BackgroundParticles>();
            if (bg == null) bg = bgObj.AddComponent<BackgroundParticles>();
            SerializedObject bgSo = new SerializedObject(bg);
            bgSo.FindProperty("particleSprite").objectReferenceValue = squareSprite;
            bgSo.ApplyModifiedProperties();

            // 7. Setup Cyber Runway Track
            GameObject groundObj = GameObject.Find("Ground");
            if (groundObj == null) groundObj = new GameObject("Ground");
            groundObj.transform.position = new Vector3(0f, -3.1f, 0f);
            groundObj.transform.localScale = new Vector3(35f, 1.2f, 1f);

            SpriteRenderer groundSr = groundObj.GetComponent<SpriteRenderer>();
            if (groundSr == null) groundSr = groundObj.AddComponent<SpriteRenderer>();
            groundSr.sprite = trackSprite;
            groundSr.sortingOrder = 2;

            // 8. Create Styled Obstacle Prefabs
            string prefabsDir = "Assets/Prefabs";
            if (!Directory.Exists(prefabsDir)) Directory.CreateDirectory(prefabsDir);

            // A) Crystal Spike Prefab (Ground)
            GameObject spikePrefab = CreateOrUpdatePrefab("Assets/Prefabs/CrystalSpike.prefab", spikeSprite, new Vector3(0.9f, 1.8f, 1f), ObstacleVisualType.CrystalSpike);

            // B) Laser Gate Prefab (Tall Barrier)
            GameObject gatePrefab = CreateOrUpdatePrefab("Assets/Prefabs/LaserGate.prefab", gateSprite, new Vector3(0.9f, 3.8f, 1f), ObstacleVisualType.LaserGate);

            // C) Floating Diamond Drone Prefab (Airborne)
            GameObject diamondPrefab = CreateOrUpdatePrefab("Assets/Prefabs/FloatingDiamond.prefab", diamondSprite, new Vector3(1.1f, 1.1f, 1f), ObstacleVisualType.FloatingDiamond);

            // 9. Setup Spawner
            GameObject spawnerObj = GameObject.Find("ObstacleSpawner");
            if (spawnerObj == null) spawnerObj = new GameObject("ObstacleSpawner");
            spawnerObj.transform.position = new Vector3(12f, 0f, 0f);

            ObstacleSpawner spawner = spawnerObj.GetComponent<ObstacleSpawner>();
            if (spawner == null) spawner = spawnerObj.AddComponent<ObstacleSpawner>();

            SerializedObject spawnerSo = new SerializedObject(spawner);
            spawnerSo.FindProperty("crystalSpikePrefab").objectReferenceValue = spikePrefab;
            spawnerSo.FindProperty("laserGatePrefab").objectReferenceValue = gatePrefab;
            spawnerSo.FindProperty("floatingDiamondPrefab").objectReferenceValue = diamondPrefab;
            spawnerSo.FindProperty("spawnX").floatValue = 12f;
            spawnerSo.FindProperty("groundY").floatValue = -2.15f;
            spawnerSo.FindProperty("airborneY").floatValue = 0.35f;
            spawnerSo.ApplyModifiedProperties();

            // 10. Setup Styled Player
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj == null) playerObj = new GameObject("Player");
            playerObj.SetActive(true);
            playerObj.transform.position = new Vector3(-5f, -2.4f, 0f);
            playerObj.transform.localScale = new Vector3(0.85f, 0.85f, 1f);

            SpriteRenderer playerSr = playerObj.GetComponent<SpriteRenderer>();
            if (playerSr == null) playerSr = playerObj.AddComponent<SpriteRenderer>();
            playerSr.sprite = playerCoreSprite;
            playerSr.sortingOrder = 10;

            CircleCollider2D playerCol = playerObj.GetComponent<CircleCollider2D>();
            if (playerCol == null) playerCol = playerObj.AddComponent<CircleCollider2D>();
            playerCol.radius = 0.45f;
            playerCol.isTrigger = true;

            PlayerController pc = playerObj.GetComponent<PlayerController>();
            if (pc == null) pc = playerObj.AddComponent<PlayerController>();

            TrailRenderer trail = playerObj.GetComponent<TrailRenderer>();
            if (trail == null) trail = playerObj.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.4f;
            trail.endWidth = 0.0f;
            trail.sortingOrder = 9;
            trail.material = new Material(Shader.Find("Sprites/Default"));

            // Child Orbit Halo Ring
            Transform haloTrans = playerObj.transform.Find("OrbitHalo");
            GameObject haloObj = (haloTrans != null) ? haloTrans.gameObject : new GameObject("OrbitHalo");
            haloObj.transform.SetParent(playerObj.transform);
            haloObj.transform.localPosition = Vector3.zero;
            haloObj.transform.localScale = new Vector3(1.35f, 1.35f, 1f);

            SpriteRenderer haloSr = haloObj.GetComponent<SpriteRenderer>();
            if (haloSr == null) haloSr = haloObj.AddComponent<SpriteRenderer>();
            haloSr.sprite = playerHaloSprite;
            haloSr.sortingOrder = 11;

            if (haloObj.GetComponent<OrbitRing>() == null) haloObj.AddComponent<OrbitRing>();

            SerializedObject pcSo = new SerializedObject(pc);
            pcSo.FindProperty("spriteRenderer").objectReferenceValue = playerSr;
            pcSo.FindProperty("trailRenderer").objectReferenceValue = trail;
            pcSo.FindProperty("groundY").floatValue = -2.4f;
            pcSo.FindProperty("jumpForce").floatValue = 13.5f;
            pcSo.FindProperty("gravity").floatValue = 35f;
            pcSo.FindProperty("shadowDimensionColor").colorValue = new Color(1.3f, 1.35f, 1.5f, 1f);
            pcSo.FindProperty("lightDimensionColor").colorValue = new Color(0.08f, 0.09f, 0.14f, 1f);
            pcSo.ApplyModifiedProperties();

            Debug.Log("<color=green>[LightNShadows]</color> Complete Theme Overhaul applied! Neon Prism aesthetic active.");
            EditorUtility.DisplayDialog("LightNShadows", "THEME OVERHAUL COMPLETE!\n\n1. Obstacles: Crystal Spikes, Laser Gates & Floating Diamond Drones\n2. Player: Dual-Core Entity with rotating energy halo ring\n3. Runway: High-tech cyber track with glowing guide rail\n4. Background: Parallax distant monoliths + atmospheric speed motes\n\nPress PLAY to see the transformation!", "Let's Go!");
        }

        private static GameObject CreateOrUpdatePrefab(string path, Sprite sprite, Vector3 scale, ObstacleVisualType type)
        {
            GameObject temp = new GameObject("TempObstacle");
            temp.transform.localScale = scale;

            SpriteRenderer sr = temp.AddComponent<SpriteRenderer>();
            sr.sprite = sprite;
            sr.sortingOrder = 5;

            BoxCollider2D col = temp.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = new Vector2(0.9f, 0.95f);

            Obstacle obs = temp.AddComponent<Obstacle>();
            SerializedObject obsSo = new SerializedObject(obs);
            obsSo.FindProperty("visualType").enumValueIndex = (int)type;
            obsSo.ApplyModifiedProperties();

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(temp, path);
            GameObject.DestroyImmediate(temp);
            return prefab;
        }

        private static void SetupGlobalPostProcessing()
        {
            GameObject volumeObj = GameObject.Find("Global Volume");
            if (volumeObj == null) volumeObj = new GameObject("Global Volume");

            Volume volume = volumeObj.GetComponent<Volume>();
            if (volume == null) volume = volumeObj.AddComponent<Volume>();
            volume.isGlobal = true;

            string settingsDir = "Assets/Settings";
            if (!Directory.Exists(settingsDir)) Directory.CreateDirectory(settingsDir);

            string profilePath = "Assets/Settings/LightNShadows_PostProcessProfile.asset";
            VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>(profilePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<VolumeProfile>();
                AssetDatabase.CreateAsset(profile, profilePath);
                AssetDatabase.SaveAssets();
            }

            volume.profile = profile;

            Bloom bloom;
            if (!profile.TryGet(out bloom)) bloom = profile.Add<Bloom>(true);
            bloom.intensity.Override(1.4f);
            bloom.threshold.Override(0.82f);
            bloom.scatter.Override(0.7f);

            ChromaticAberration ca;
            if (!profile.TryGet(out ca)) ca = profile.Add<ChromaticAberration>(true);
            ca.intensity.Override(0.06f);

            Vignette vig;
            if (!profile.TryGet(out vig)) vig = profile.Add<Vignette>(true);
            vig.intensity.Override(0.24f);
            vig.smoothness.Override(0.42f);

            if (volumeObj.GetComponent<PostProcessEffects>() == null)
            {
                volumeObj.AddComponent<PostProcessEffects>();
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
