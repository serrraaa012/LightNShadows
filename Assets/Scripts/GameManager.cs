using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightNShadows
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            IntroSplash,
            Title,
            Instructions,
            Playing,
            Paused,
            GameOver
        }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.IntroSplash;
        public GameState State => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsPaused => currentState == GameState.Paused;
        public bool IsGameOver => currentState == GameState.GameOver;
        public bool IsNewHighScore { get; private set; } = false;

        private static bool hasShownIntro = false;
        private static bool startImmediately = false;

        private PlayerController player;
        public PlayerController Player => player;

        public void RegisterPlayer(PlayerController pc)
        {
            player = pc;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            hasShownIntro = false;
            startImmediately = false;
        }

        [Header("Score")]
        public int CurrentScore { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public int ComboCount => 0;
        public int MaxCombo => 0;

        [Header("Score Settings")]
        [SerializeField] private float scorePerSecond = 10f;
        private float scoreAccumulator = 0f;

        private const string LegacyHighScoreKey = "LightNShadows_HighScore";
        private const string PlayerNameKey = "LightNShadows_PlayerName";

        public static string GetPlayerScoreKey(string name)
        {
            string clean = (name ?? "").Trim().ToUpperInvariant();
            if (string.IsNullOrEmpty(clean)) clean = "RUNNER";
            return $"LightNShadows_Score_{clean}";
        }

        [Header("Player Identity")]
        public string PlayerName { get; private set; } = "";
        public bool HasPlayerName => !string.IsNullOrWhiteSpace(PlayerName);

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            PlayerName = PlayerPrefs.GetString(PlayerNameKey, "");
            if (!string.IsNullOrWhiteSpace(PlayerName))
            {
                HighScore = PlayerPrefs.GetInt(GetPlayerScoreKey(PlayerName), 0);
            }
            else
            {
                HighScore = 0;
            }

            player = FindObjectOfType<PlayerController>(true);

            if (startImmediately)
            {
                startImmediately = false;
                currentState = GameState.Playing;
            }
            else if (!hasShownIntro)
            {
                currentState = GameState.IntroSplash;
            }
            else
            {
                currentState = GameState.Title;
            }
        }

        private void Update()
        {
            if (currentState != GameState.Playing)
            {
                // Splash & menus are navigated cleanly via timers/buttons
                return;
            }

            // Score increases strictly when passing obstacles or collecting items
        }

        public void SetPlayerName(string newName)
        {
            if (string.IsNullOrWhiteSpace(newName))
            {
                newName = "RUNNER";
            }
            newName = newName.Trim();
            if (newName.Length > 14)
            {
                newName = newName.Substring(0, 14);
            }

            PlayerName = newName;
            PlayerPrefs.SetString(PlayerNameKey, PlayerName);

            // Load this specific player's previous record (or 0 if they are a new player!)
            HighScore = PlayerPrefs.GetInt(GetPlayerScoreKey(PlayerName), 0);
            PlayerPrefs.Save();
        }

        public void StartGame()
        {
            if (!HasPlayerName)
            {
                SetPlayerName("RUNNER");
            }
            else
            {
                // Ensure HighScore is synced to this player's record
                HighScore = PlayerPrefs.GetInt(GetPlayerScoreKey(PlayerName), 0);
            }

            ResetGameState(startGameplay: true);
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.Paused;
            Time.timeScale = 0f;
            if (SoundManager.Instance != null) SoundManager.Instance.PauseMusic();
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            currentState = GameState.Playing;
            Time.timeScale = 1f;
            if (SoundManager.Instance != null) SoundManager.Instance.ResumeMusic();
        }

        public void TogglePause()
        {
            if (currentState == GameState.Playing)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }

        public void OpenInstructions()
        {
            currentState = GameState.Instructions;
        }

        public void OpenMainMenu()
        {
            ResetGameState(startGameplay: false);
        }

        public void AddScore(int amount)
        {
            if (!IsPlaying) return;

            CurrentScore += amount;
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                IsNewHighScore = true;
                string pKey = GetPlayerScoreKey(PlayerName);
                PlayerPrefs.SetInt(pKey, HighScore);
                PlayerPrefs.SetInt(LegacyHighScoreKey, HighScore);
            }
        }

        public void RegisterPhaseSuccess()
        {
            if (!IsPlaying) return;

            AddScore(1);
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver) return;

            Time.timeScale = 1f;
            currentState = GameState.GameOver;
            PlayerPrefs.Save();
        }

        public void CompleteIntroSplash()
        {
            hasShownIntro = true;
            currentState = GameState.Title;
        }

        public void RestartGame()
        {
            ResetGameState(startGameplay: true);
        }

        private void ResetGameState(bool startGameplay)
        {
            Time.timeScale = 1f;
            CurrentScore = 0;
            IsNewHighScore = false;
            currentState = startGameplay ? GameState.Playing : GameState.Title;

            // 1. Reset Obstacle Spawner & destroy all active hazards/orbs
            ObstacleSpawner spawner = FindObjectOfType<ObstacleSpawner>();
            if (spawner != null)
            {
                spawner.ResetSpawner();
            }

            // 2. Reset Dimension to Light
            if (DimensionManager.Instance != null)
            {
                DimensionManager.Instance.ResetToLight();
            }

            // 3. Reset and Re-activate Player ball
            if (player == null)
            {
                player = FindObjectOfType<PlayerController>(true);
            }
            if (player != null)
            {
                player.ResetPlayer();
            }

            // 4. Stop lingering camera shake
            if (CameraShake.Instance != null)
            {
                CameraShake.Instance.StopShake();
            }

            // 5. Clean up any remaining shard explosions or shockwaves
            ShardExplosion[] shards = FindObjectsOfType<ShardExplosion>();
            for (int i = 0; i < shards.Length; i++)
            {
                if (shards[i] != null) Destroy(shards[i].gameObject);
            }

            ShockwavePulse[] waves = FindObjectsOfType<ShockwavePulse>();
            for (int i = 0; i < waves.Length; i++)
            {
                if (waves[i] != null) Destroy(waves[i].gameObject);
            }

            // 6. Ensure music is playing
            if (SoundManager.Instance != null)
            {
                SoundManager.Instance.ResumeMusic();
            }
        }
    }
}
