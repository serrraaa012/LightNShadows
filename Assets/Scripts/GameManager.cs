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

        private const string HighScoreKey = "LightNShadows_HighScore";

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

            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);

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

        public void StartGame()
        {
            Time.timeScale = 1f;
            currentState = GameState.Playing;
            CurrentScore = 0;
            IsNewHighScore = false;
        }

        public void PauseGame()
        {
            if (currentState != GameState.Playing) return;
            currentState = GameState.Paused;
            Time.timeScale = 0f;
        }

        public void ResumeGame()
        {
            if (currentState != GameState.Paused) return;
            currentState = GameState.Playing;
            Time.timeScale = 1f;
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
            Time.timeScale = 1f;
            currentState = GameState.Title;
        }

        public void AddScore(int amount)
        {
            if (!IsPlaying) return;

            CurrentScore += amount;
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                IsNewHighScore = true;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
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
            Time.timeScale = 1f;
            startImmediately = true;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
