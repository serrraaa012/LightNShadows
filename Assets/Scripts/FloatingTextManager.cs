using System.Collections.Generic;
using UnityEngine;

namespace LightNShadows
{
    public class FloatingTextManager : MonoBehaviour
    {
        public static FloatingTextManager Instance { get; private set; }

        private class Popup
        {
            public string text;
            public Vector3 worldPos;
            public Color color;
            public float timer;
            public float duration;
            public float velocityY;
        }

        private List<Popup> activePopups = new List<Popup>();
        private GUIStyle popupStyle;

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        public void SpawnPopup(Vector3 worldPos, string text, Color color, float duration = 0.9f)
        {
            activePopups.Add(new Popup
            {
                text = text,
                worldPos = worldPos + new Vector3(Random.Range(-0.3f, 0.3f), 0.5f, 0f),
                color = color,
                timer = 0f,
                duration = duration,
                velocityY = 2.2f
            });
        }

        private void Update()
        {
            for (int i = activePopups.Count - 1; i >= 0; i--)
            {
                Popup p = activePopups[i];
                p.timer += Time.deltaTime;
                p.worldPos.y += p.velocityY * Time.deltaTime;
                p.velocityY = Mathf.Lerp(p.velocityY, 0.5f, Time.deltaTime * 3f);

                if (p.timer >= p.duration)
                {
                    activePopups.RemoveAt(i);
                }
            }
        }

        private void OnGUI()
        {
            if (Camera.main == null || activePopups.Count == 0) return;

            if (popupStyle == null)
            {
                popupStyle = new GUIStyle(GUI.skin.label)
                {
                    fontSize = 22,
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter
                };
            }

            for (int i = 0; i < activePopups.Count; i++)
            {
                Popup p = activePopups[i];
                Vector3 screenPos = Camera.main.WorldToScreenPoint(p.worldPos);
                if (screenPos.z < 0) continue;

                float t = p.timer / p.duration;
                Color c = p.color;
                c.a = Mathf.Clamp01(1f - t * t);
                popupStyle.normal.textColor = c;

                float y = Screen.height - screenPos.y;
                GUI.Label(new Rect(screenPos.x - 100, y - 20, 200, 40), p.text, popupStyle);
            }
        }
    }
}
