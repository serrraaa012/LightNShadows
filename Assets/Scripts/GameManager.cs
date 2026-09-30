using UnityEngine;
using UnityEngine.SceneManagement;

namespace LightNShadows
{
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public enum GameState
        {
            Title,
            Playing,
            GameOver
        }

        [Header("State")]
        [SerializeField] private GameState currentState = GameState.Title;
        public GameState State => currentState;
        public bool IsPlaying => currentState == GameState.Playing;
        public bool IsGameOver => currentState == GameState.GameOver;

        [Header("Score & Combos")]
        public int CurrentScore { get; private set; } = 0;
        public int HighScore { get; private set; } = 0;
        public int ComboCount { get; private set; } = 0;
        public int MaxCombo { get; private set; } = 0;

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
        }

        private void Update()
        {
            if (currentState == GameState.Title)
            {
                // Press Space or Left Click to start
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    StartGame();
                }
                return;
            }

            if (currentState == GameState.GameOver)
            {
                // Press Space or R to restart
                if (Input.GetKeyDown(KeyCode.R) || Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                {
                    RestartGame();
                }
                return;
            }

            // Passive time score
            scoreAccumulator += scorePerSecond * Time.deltaTime;
            if (scoreAccumulator >= 1f)
            {
                int pointsToAdd = Mathf.FloorToInt(scoreAccumulator);
                scoreAccumulator -= pointsToAdd;
                AddScore(pointsToAdd);
            }
        }

        public void StartGame()
        {
            currentState = GameState.Playing;
            CurrentScore = 0;
            ComboCount = 0;
        }

        public void AddScore(int amount)
        {
            if (!IsPlaying) return;

            CurrentScore += amount;
            if (CurrentScore > HighScore)
            {
                HighScore = CurrentScore;
                PlayerPrefs.SetInt(HighScoreKey, HighScore);
            }
        }

        public void RegisterPhaseSuccess()
        {
            if (!IsPlaying) return;

            ComboCount++;
            if (ComboCount > MaxCombo) MaxCombo = ComboCount;

            int bonus = 50 * Mathf.Min(ComboCount, 5); // 50, 100, 150... up to 250
            AddScore(bonus);
        }

        public void TriggerGameOver()
        {
            if (currentState == GameState.GameOver) return;

            currentState = GameState.GameOver;
            PlayerPrefs.Save();
        }

        public void RestartGame()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void OnGUI()
        {
            bool isLight = DimensionManager.Instance != null && DimensionManager.Instance.CurrentDimension == DimensionType.Light;
            Color primaryColor = isLight ? new Color(0.1f, 0.1f, 0.15f) : new Color(0.95f, 0.95f, 1f);
            Color accentColor = isLight ? new Color(0.85f, 0.35f, 0.1f) : new Color(0.2f, 0.85f, 0.95f);

            GUIStyle titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 46,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = primaryColor;

            GUIStyle hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 24,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            hudStyle.normal.textColor = primaryColor;

            GUIStyle subStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };
            subStyle.normal.textColor = primaryColor;

            GUIStyle comboStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };
            comboStyle.normal.textColor = accentColor;

            // 1. TITLE SCREEN
            if (currentState == GameState.Title)
            {
                float midY = Screen.height * 0.30f;
                GUI.Label(new Rect(0, midY, Screen.width, 60), "LIGHT  N  SHADOWS", titleStyle);

                subStyle.fontSize = 22;
                GUI.Label(new Rect(0, midY + 70, Screen.width, 35), "Phase through realms. Leap over crimson hazards.", subStyle);

                subStyle.fontSize = 20;
                subStyle.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(0, midY + 115, Screen.width, 35), "[ SPACEBAR / CLICK ] to Begin", subStyle);

                subStyle.fontStyle = FontStyle.Normal;
                subStyle.fontSize = 17;
                GUI.Label(new Rect(0, midY + 165, Screen.width, 30), "W / UP ARROW = Jump over RED Spikes (Cannot phase!)", subStyle);
                GUI.Label(new Rect(0, midY + 195, Screen.width, 30), "SPACE / CLICK = Swap Realm to phase through TALL GATES", subStyle);
                return;
            }

            // 2. PLAYING HUD
            if (currentState == GameState.Playing)
            {
                // Score & Best
                GUI.Label(new Rect(30, 20, 300, 35), $"SCORE: {CurrentScore}", hudStyle);
                hudStyle.fontSize = 17;
                GUI.Label(new Rect(30, 55, 300, 25), $"BEST:  {HighScore}", hudStyle);

                // Combo streak
                if (ComboCount > 1)
                {
                    GUI.Label(new Rect(30, 85, 300, 35), $"COMBO x{ComboCount}!", comboStyle);
                }

                // Active Realm
                hudStyle.fontSize = 22;
                string realmText = isLight ? "REALM: LIGHT" : "REALM: SHADOW";
                GUI.Label(new Rect(Screen.width - 240, 20, 210, 35), realmText, hudStyle);
            }

            // 3. GAME OVER SCREEN
            if (currentState == GameState.GameOver)
            {
                float midY = Screen.height * 0.3f;
                titleStyle.fontSize = 44;
                GUI.Label(new Rect(0, midY, Screen.width, 55), "SHATTERED", titleStyle);

                subStyle.fontSize = 24;
                GUI.Label(new Rect(0, midY + 65, Screen.width, 35), $"Score: {CurrentScore}   |   Best: {HighScore}", subStyle);

                if (MaxCombo > 1)
                {
                    subStyle.fontSize = 20;
                    GUI.Label(new Rect(0, midY + 105, Screen.width, 30), $"Highest Combo: x{MaxCombo}", subStyle);
                }

                subStyle.fontSize = 22;
                subStyle.fontStyle = FontStyle.Bold;
                GUI.Label(new Rect(0, midY + 160, Screen.width, 40), "Press [ SPACEBAR / CLICK ] to Try Again", subStyle);
            }
        }
    }
}
