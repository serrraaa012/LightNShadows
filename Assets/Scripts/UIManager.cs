using UnityEngine;

namespace LightNShadows
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        private Texture2D buttonTex;
        private Texture2D buttonHoverTex;
        private Texture2D iconJumpTex;
        private Texture2D iconPhaseTex;

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle headerStyle;
        private GUIStyle cardTitleStyle;
        private GUIStyle cardBodyStyle;
        private GUIStyle buttonStyle;
        private GUIStyle hudStyle;
        private GUIStyle comboStyle;

        private bool stylesInitialized = false;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            LoadTextures();
        }

        private void LoadTextures()
        {
            buttonTex = LoadFromPath("Assets/Sprites/UIButton.png");
            buttonHoverTex = LoadFromPath("Assets/Sprites/UIButtonHover.png");
            iconJumpTex = LoadFromPath("Assets/Sprites/IconJump.png");
            iconPhaseTex = LoadFromPath("Assets/Sprites/IconPhase.png");
        }

        private Texture2D LoadFromPath(string path)
        {
#if UNITY_EDITOR
            return UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(path);
#else
            return null;
#endif
        }

        private void InitStyles()
        {
            if (stylesInitialized && titleStyle != null) return;

            titleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 54,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            titleStyle.normal.textColor = Color.white;

            subtitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.MiddleCenter
            };
            subtitleStyle.normal.textColor = new Color(0.7f, 0.85f, 1f);

            headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 32,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            headerStyle.normal.textColor = Color.white;

            cardTitleStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 20,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleLeft
            };

            cardBodyStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 15,
                fontStyle = FontStyle.Normal,
                alignment = TextAnchor.UpperLeft,
                wordWrap = true
            };
            cardBodyStyle.normal.textColor = new Color(0.85f, 0.9f, 0.98f);

            buttonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 21,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            buttonStyle.normal.textColor = Color.white;
            buttonStyle.hover.textColor = new Color(0.1f, 0.95f, 1.4f);

            if (buttonTex != null)
            {
                buttonStyle.normal.background = buttonTex;
                buttonStyle.active.background = buttonHoverTex ?? buttonTex;
                buttonStyle.hover.background = buttonHoverTex ?? buttonTex;
            }

            hudStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            comboStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 26,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            stylesInitialized = true;
        }

        private void OnGUI()
        {
            InitStyles();

            GameManager.GameState state = (GameManager.Instance != null) ? GameManager.Instance.State : GameManager.GameState.Title;

            switch (state)
            {
                case GameManager.GameState.Title:
                    DrawFullMenu();
                    break;
                case GameManager.GameState.Instructions:
                    DrawFullInstructions();
                    break;
                case GameManager.GameState.Playing:
                    DrawHUD();
                    break;
                case GameManager.GameState.GameOver:
                    DrawFullGameOver();
                    break;
            }
        }

        // ==========================================
        // 1. FULL-SCREEN IMMERSIVE START MENU
        // ==========================================
        private void DrawFullMenu()
        {
            // Full-screen backdrop overlay
            DrawFullScreenBackdrop();

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            // Header Banner Line
            DrawNeonLine(new Rect(midX - 250, midY - 170, 500, 2), new Color(0.15f, 0.85f, 1.2f));

            // Title with dramatic neon drop-shadow
            DrawTextWithShadow(new Rect(0, midY - 150, Screen.width, 65), "LIGHT   N   SHADOWS", titleStyle, new Color(0.2f, 0.9f, 1.4f));

            // Subtitle
            GUI.Label(new Rect(0, midY - 80, Screen.width, 30), "// DUAL-SPECTRUM RHYTHM RUNNER //", subtitleStyle);

            // Record Badge
            int best = (GameManager.Instance != null) ? GameManager.Instance.HighScore : 0;
            GUIStyle badge = new GUIStyle(subtitleStyle) { fontSize = 18, fontStyle = FontStyle.Bold };
            badge.normal.textColor = new Color(1.2f, 0.85f, 0.2f); // Solar Gold
            GUI.Label(new Rect(0, midY - 35, Screen.width, 30), $"RECORD: {best:N0} PTS", badge);

            // Interactive Buttons
            float btnW = 340f;
            float btnH = 58f;
            float btnX = midX - btnW * 0.5f;

            if (GUI.Button(new Rect(btnX, midY + 25, btnW, btnH), "PLAY", buttonStyle))
            {
                GameManager.Instance.StartGame();
            }

            if (GUI.Button(new Rect(btnX, midY + 100, btnW, btnH), "HOW TO PLAY", buttonStyle))
            {
                GameManager.Instance.OpenInstructions();
            }

            // Footer Banner Line
            DrawNeonLine(new Rect(midX - 250, midY + 185, 500, 2), new Color(0.15f, 0.85f, 1.2f, 0.5f));

            // Controls Hint
            GUIStyle hint = new GUIStyle(subtitleStyle) { fontSize = 15 };
            hint.normal.textColor = new Color(0.6f, 0.7f, 0.85f);
            GUI.Label(new Rect(0, midY + 200, Screen.width, 30), "W / UP = Jump   |   SPACE / CLICK = Swap Dimension", hint);
        }

        // ==========================================
        // 2. FULL-SCREEN INSTRUCTIONS (HOW TO PLAY)
        // ==========================================
        private void DrawFullInstructions()
        {
            DrawFullScreenBackdrop();

            float midX = Screen.width * 0.5f;

            // Header
            DrawNeonLine(new Rect(midX - 320, 45, 640, 2), new Color(0.2f, 0.9f, 1.4f));
            DrawTextWithShadow(new Rect(0, 60, Screen.width, 45), "// MISSION DIRECTIVES //", headerStyle, new Color(0.2f, 0.9f, 1.4f));
            GUI.Label(new Rect(0, 110, Screen.width, 25), "Two simple rules to master the spectrum.", subtitleStyle);

            float cardW = Mathf.Min(780f, Screen.width * 0.92f);
            float cardX = (Screen.width - cardW) * 0.5f;
            float startY = 155f;

            // CARD 1: CRIMSON SPIKES (MUST JUMP)
            DrawDirectiveCard(
                new Rect(cardX, startY, cardW, 85),
                iconJumpTex,
                new Color(1.4f, 0.25f, 0.35f), // Crimson Neon
                "1. CRIMSON SPIKES // MUST JUMP",
                "Red spikes are SOLID in both Light and Shadow realms. You CANNOT phase through them!\n" +
                "ACTION: Press [ W ] or [ UP ARROW ] to leap over them.",
                "[ W / UP ]"
            );

            // CARD 2: REALM GATES (MUST PHASE)
            DrawDirectiveCard(
                new Rect(cardX, startY + 105, cardW, 85),
                iconPhaseTex,
                new Color(0.2f, 0.95f, 1.4f), // Neon Cyan
                "2. TALL GATES // MUST PHASE",
                "Massive 3.8m energy barriers are too high to jump. Shift to the OPPOSITE realm to phase through safely!\n" +
                "ACTION: Press [ SPACEBAR ] or [ CLICK ] to shift dimension.",
                "[ SPACE ]"
            );

            // CARD 3: COMBOS (SCORE BOOST)
            DrawDirectiveCard(
                new Rect(cardX, startY + 210, cardW, 85),
                null,
                new Color(1.3f, 0.85f, 0.2f), // Solar Amber
                "3. RHYTHM COMBOS // MULTIPLIERS",
                "Every consecutive gate you ghost through builds your COMBO multiplier up to x5!\n" +
                "TIP: Stay focused during rapid Jump-then-Phase combo sequences.",
                "[ x5 MAX ]"
            );

            // Action Buttons
            float btnW = 260f;
            float btnH = 52f;
            float bY = startY + 325f;

            if (GUI.Button(new Rect(midX - btnW - 15, bY, btnW, btnH), "PLAY", buttonStyle))
            {
                GameManager.Instance.StartGame();
            }

            if (GUI.Button(new Rect(midX + 15, bY, btnW, btnH), "BACK", buttonStyle))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        private void DrawDirectiveCard(Rect r, Texture2D icon, Color accent, string title, string body, string keyTag)
        {
            // Card background box
            GUI.color = new Color(0.08f, 0.1f, 0.16f, 0.92f);
            GUI.DrawTexture(r, Texture2D.whiteTexture);

            // Left accent border strip
            GUI.color = accent;
            GUI.DrawTexture(new Rect(r.x, r.y, 5, r.height), Texture2D.whiteTexture);
            GUI.color = Color.white;

            float textLeft = r.x + 20;

            if (icon != null)
            {
                GUI.color = accent;
                GUI.DrawTexture(new Rect(r.x + 18, r.y + 18, 48, 48), icon);
                GUI.color = Color.white;
                textLeft = r.x + 80;
            }

            // Title
            cardTitleStyle.normal.textColor = accent;
            GUI.Label(new Rect(textLeft, r.y + 12, r.width - textLeft - 110, 25), title, cardTitleStyle);

            // Body
            GUI.Label(new Rect(textLeft, r.y + 38, r.width - textLeft - 110, 42), body, cardBodyStyle);

            // Key tag on right
            GUIStyle tagStyle = new GUIStyle(cardTitleStyle) { alignment = TextAnchor.MiddleCenter, fontSize = 16 };
            tagStyle.normal.textColor = Color.white;

            GUI.color = new Color(accent.r * 0.3f, accent.g * 0.3f, accent.b * 0.3f, 0.9f);
            GUI.DrawTexture(new Rect(r.x + r.width - 105, r.y + 24, 90, 36), Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUI.Label(new Rect(r.x + r.width - 105, r.y + 24, 90, 36), keyTag, tagStyle);
        }

        // ==========================================
        // 3. IN-GAME HUD
        // ==========================================
        private void DrawHUD()
        {
            if (GameManager.Instance == null) return;

            bool isLight = DimensionManager.Instance != null && DimensionManager.Instance.CurrentDimension == DimensionType.Light;
            Color primaryCol = isLight ? new Color(0.08f, 0.09f, 0.14f) : Color.white;

            // Score Badge
            hudStyle.normal.textColor = primaryCol;
            GUI.Label(new Rect(35, 25, 300, 35), $"SCORE: {GameManager.Instance.CurrentScore:N0}", hudStyle);

            GUIStyle subHud = new GUIStyle(hudStyle) { fontSize = 16 };
            subHud.normal.textColor = new Color(primaryCol.r, primaryCol.g, primaryCol.b, 0.75f);
            GUI.Label(new Rect(35, 60, 300, 25), $"BEST:  {GameManager.Instance.HighScore:N0}", subHud);

            // Combo Streak
            if (GameManager.Instance.ComboCount > 1)
            {
                comboStyle.normal.textColor = isLight ? new Color(0.9f, 0.5f, 0.05f) : new Color(0.2f, 0.95f, 1.4f);
                GUI.Label(new Rect(35, 90, 300, 35), $"COMBO x{GameManager.Instance.ComboCount}!", comboStyle);
            }

            // Top-Right Active Realm Indicator
            string realmText = isLight ? "REALM // SOLAR LIGHT" : "REALM // ELECTRIC VOID";
            GUIStyle rightStyle = new GUIStyle(hudStyle) { alignment = TextAnchor.UpperRight };
            rightStyle.normal.textColor = isLight ? new Color(0.85f, 0.45f, 0.05f) : new Color(0.2f, 0.95f, 1.4f);
            GUI.Label(new Rect(Screen.width - 370, 25, 335, 35), realmText, rightStyle);
        }

        // ==========================================
        // 4. FULL-SCREEN GAME OVER (RUN TERMINATED)
        // ==========================================
        private void DrawFullGameOver()
        {
            if (GameManager.Instance == null) return;

            DrawFullScreenBackdrop();

            float midX = Screen.width * 0.5f;
            float midY = Screen.height * 0.5f;

            // Crimson top accent
            DrawNeonLine(new Rect(midX - 260, midY - 180, 520, 2), new Color(1.4f, 0.25f, 0.35f));

            // Title
            GUIStyle overTitle = new GUIStyle(titleStyle) { fontSize = 50 };
            overTitle.normal.textColor = new Color(1.4f, 0.25f, 0.35f);
            DrawTextWithShadow(new Rect(0, midY - 165, Screen.width, 60), "RUN TERMINATED", overTitle, new Color(1f, 0.1f, 0.2f, 0.6f));

            // Record celebration
            if (GameManager.Instance.IsNewHighScore)
            {
                GUIStyle recordStyle = new GUIStyle(subtitleStyle) { fontSize = 20, fontStyle = FontStyle.Bold };
                recordStyle.normal.textColor = new Color(1.3f, 0.85f, 0.2f);
                GUI.Label(new Rect(0, midY - 100, Screen.width, 30), "★ NEW PERSONAL RECORD! ★", recordStyle);
            }
            else
            {
                GUI.Label(new Rect(0, midY - 100, Screen.width, 30), "// SYSTEM CORE SHATTERED //", subtitleStyle);
            }

            // Score Card Box
            float cardW = 420f;
            float cardH = 120f;
            Rect scoreCard = new Rect(midX - cardW * 0.5f, midY - 55, cardW, cardH);

            GUI.color = new Color(0.08f, 0.1f, 0.16f, 0.92f);
            GUI.DrawTexture(scoreCard, Texture2D.whiteTexture);
            GUI.color = Color.white;

            GUIStyle scoreValStyle = new GUIStyle(titleStyle) { fontSize = 34 };
            scoreValStyle.normal.textColor = Color.white;
            GUI.Label(new Rect(scoreCard.x, scoreCard.y + 15, cardW, 40), $"{GameManager.Instance.CurrentScore:N0} PTS", scoreValStyle);

            GUIStyle statLine = new GUIStyle(subtitleStyle) { fontSize = 16 };
            statLine.normal.textColor = new Color(0.7f, 0.8f, 0.95f);
            string comboStat = (GameManager.Instance.MaxCombo > 1) ? $" | MAX COMBO: x{GameManager.Instance.MaxCombo}" : "";
            GUI.Label(new Rect(scoreCard.x, scoreCard.y + 65, cardW, 30), $"RECORD: {GameManager.Instance.HighScore:N0} PTS{comboStat}", statLine);

            // Buttons
            float btnW = 320f;
            float btnH = 54f;
            float btnX = midX - btnW * 0.5f;

            if (GUI.Button(new Rect(btnX, midY + 95, btnW, btnH), "PLAY AGAIN", buttonStyle))
            {
                GameManager.Instance.RestartGame();
            }

            if (GUI.Button(new Rect(btnX, midY + 165, btnW, btnH), "MAIN MENU", buttonStyle))
            {
                GameManager.Instance.OpenMainMenu();
            }
        }

        // ==========================================
        // GRAPHICS HELPERS
        // ==========================================
        private void DrawFullScreenBackdrop()
        {
            // Dark immersive cosmic background covering the ENTIRE viewport
            GUI.color = new Color(0.04f, 0.05f, 0.09f, 0.96f);
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawNeonLine(Rect r, Color c)
        {
            GUI.color = c;
            GUI.DrawTexture(r, Texture2D.whiteTexture);
            GUI.color = Color.white;
        }

        private void DrawTextWithShadow(Rect r, string text, GUIStyle style, Color glowColor)
        {
            Color origColor = style.normal.textColor;

            // Glow / Shadow behind
            style.normal.textColor = glowColor;
            GUI.Label(new Rect(r.x, r.y + 3, r.width, r.height), text, style);
            GUI.Label(new Rect(r.x + 2, r.y, r.width, r.height), text, style);

            // Crisp front text
            style.normal.textColor = Color.white;
            GUI.Label(r, text, style);

            style.normal.textColor = origColor;
        }
    }
}
