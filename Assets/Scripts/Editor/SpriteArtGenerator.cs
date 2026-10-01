#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace LightNShadows.Editor
{
    public static class SpriteArtGenerator
    {
        private const string SpritesDir = "Assets/Sprites";
        private const string BrainDir = @"C:\Users\sarah\.gemini\antigravity\brain\dd098baa-b450-4d87-950b-5edbdecc7adb";

        public static void GenerateAllSprites()
        {
            if (!Directory.Exists(SpritesDir))
            {
                Directory.CreateDirectory(SpritesDir);
                AssetDatabase.Refresh();
            }

            // Copy generated High-Resolution Master Artwork
            ImportMasterArtwork();

            GenerateCrystalSpike();
            GenerateCeilingSpire();
            GenerateLaserGate();
            GenerateFloatingDiamond();
            GeneratePrismOrb();
            GeneratePlayerHalo();
            GeneratePlayerCore();
            GenerateCyberTrack();
            GenerateMonolith();
            GenerateShockwaveRing();

            // High-End Modern UI Sprites
            GenerateButtonPill(false);
            GenerateButtonPill(true);
            GenerateModernGlassCard();
            GenerateIconJump();
            GenerateIconPhase();

            AssetDatabase.Refresh();
        }

        private static void ImportMasterArtwork()
        {
            string bgSource = Path.Combine(BrainDir, ".user_uploaded/media_1790877488173.jpg");
            string logoSource = Path.Combine(BrainDir, "game_title_logo_1790875829034.jpg");

            string bgDest = $"{SpritesDir}/GameBackground.jpg";
            string logoDest = $"{SpritesDir}/TitleLogo.jpg";

            if (File.Exists(bgSource))
            {
                File.Copy(bgSource, bgDest, true);
                AssetDatabase.ImportAsset(bgDest);
                ConfigureTextureAsSprite(bgDest, 100);
            }

            if (File.Exists(logoSource))
            {
                File.Copy(logoSource, logoDest, true);
                AssetDatabase.ImportAsset(logoDest);
                ConfigureTextureAsSprite(logoDest, 100);
            }
        }

        // --- GAMEPLAY SPRITES ---

        private static void GenerateCrystalSpike()
        {
            string path = $"{SpritesDir}/CrystalSpike.png";
            int w = 128, h = 256;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 tip = new Vector2(w * 0.5f, h * 0.95f);
            Vector2 baseL = new Vector2(w * 0.15f, h * 0.05f);
            Vector2 baseR = new Vector2(w * 0.85f, h * 0.05f);
            Vector2 facetMid = new Vector2(w * 0.5f, h * 0.05f);

            DrawTriangle(tex, tip, baseL, facetMid, new Color(0.85f, 0.85f, 0.9f, 1f));
            DrawTriangle(tex, tip, facetMid, baseR, new Color(1f, 1f, 1f, 1f));

            DrawLine(tex, tip, baseL, Color.white, 3);
            DrawLine(tex, tip, baseR, Color.white, 3);
            DrawLine(tex, tip, facetMid, new Color(0.7f, 0.7f, 0.8f, 0.9f), 2);

            SaveAndImport(tex, path, 128);
        }

        private static void GenerateCeilingSpire()
        {
            string path = $"{SpritesDir}/CeilingSpire.png";
            int w = 128, h = 256;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 tip = new Vector2(w * 0.5f, h * 0.05f);
            Vector2 topL = new Vector2(w * 0.15f, h * 0.95f);
            Vector2 topR = new Vector2(w * 0.85f, h * 0.95f);
            Vector2 facetMid = new Vector2(w * 0.5f, h * 0.95f);

            DrawTriangle(tex, topL, tip, facetMid, new Color(0.85f, 0.85f, 0.9f, 1f));
            DrawTriangle(tex, facetMid, tip, topR, new Color(1f, 1f, 1f, 1f));

            DrawLine(tex, topL, tip, Color.white, 3);
            DrawLine(tex, topR, tip, Color.white, 3);
            DrawLine(tex, facetMid, tip, new Color(0.7f, 0.7f, 0.8f, 0.9f), 2);

            SaveAndImport(tex, path, 128);
        }

        private static void GenerateLaserGate()
        {
            string path = $"{SpritesDir}/LaserGate.png";
            int w = 128, h = 512;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            FillRect(tex, 14, h - 40, w - 28, 30, Color.white);
            FillRect(tex, 14, 10, w - 28, 30, Color.white);

            FillRect(tex, 24, h - 55, w - 48, 15, new Color(0.8f, 0.8f, 0.85f));
            FillRect(tex, 24, 40, w - 48, 15, new Color(0.8f, 0.8f, 0.85f));

            int beamCenter = w / 2;
            int beamHalfWidth = 14;
            for (int y = 55; y < h - 55; y++)
            {
                for (int x = beamCenter - beamHalfWidth; x <= beamCenter + beamHalfWidth; x++)
                {
                    float dist = Mathf.Abs(x - beamCenter) / (float)beamHalfWidth;
                    float alpha = Mathf.Clamp01(1f - dist * dist);
                    tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            SaveAndImport(tex, path, 128);
        }

        private static void GenerateFloatingDiamond()
        {
            string path = $"{SpritesDir}/FloatingDiamond.png";
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 top = new Vector2(size * 0.5f, size * 0.92f);
            Vector2 bot = new Vector2(size * 0.5f, size * 0.08f);
            Vector2 left = new Vector2(size * 0.08f, size * 0.5f);
            Vector2 right = new Vector2(size * 0.92f, size * 0.5f);

            DrawTriangle(tex, top, left, bot, new Color(0.85f, 0.85f, 0.9f));
            DrawTriangle(tex, top, bot, right, Color.white);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            FillCircle(tex, center, 14, new Color(0.1f, 0.1f, 0.15f));
            FillCircle(tex, center, 8, Color.white);

            DrawLine(tex, top, left, Color.white, 2);
            DrawLine(tex, left, bot, Color.white, 2);
            DrawLine(tex, bot, right, Color.white, 2);
            DrawLine(tex, right, top, Color.white, 2);

            SaveAndImport(tex, path, 128);
        }

        private static void GeneratePrismOrb()
        {
            string path = $"{SpritesDir}/PrismOrb.png";
            int size = 96;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float radius = size * 0.4f;

            int sides = 8;
            Vector2[] pts = new Vector2[sides];
            for (int i = 0; i < sides; i++)
            {
                float ang = (i * 360f / sides + 22.5f) * Mathf.Deg2Rad;
                pts[i] = center + new Vector2(Mathf.Cos(ang), Mathf.Sin(ang)) * radius;
            }

            for (int i = 0; i < sides; i++)
            {
                Vector2 next = pts[(i + 1) % sides];
                Color c = (i % 2 == 0) ? Color.white : new Color(0.85f, 0.92f, 1f);
                DrawTriangle(tex, center, pts[i], next, c);
                DrawLine(tex, pts[i], next, Color.white, 2);
            }

            FillCircle(tex, center, radius * 0.35f, Color.white);
            SaveAndImport(tex, path, 96);
        }

        private static void GenerateShockwaveRing()
        {
            string path = $"{SpritesDir}/ShockwaveRing.png";
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float inner = 48f;
            float outer = 60f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d >= inner && d <= outer)
                    {
                        float alpha = 1f - Mathf.Abs(d - (inner + outer) * 0.5f) / ((outer - inner) * 0.5f);
                        tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        private static void GeneratePlayerHalo()
        {
            string path = $"{SpritesDir}/PlayerHalo.png";
            int size = 160;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float innerR = 56f;
            float outerR = 64f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d >= innerR && d <= outerR)
                    {
                        float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        bool isNotch = (angle > 40 && angle < 50) || (angle > 130 && angle < 140) ||
                                       (angle > 220 && angle < 230) || (angle > 310 && angle < 320);

                        if (!isNotch) tex.SetPixel(x, y, Color.white);
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        private static void GeneratePlayerCore()
        {
            string path = $"{SpritesDir}/PlayerCore.png";
            int size = 128;
            Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            float r = 46f;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), center);
                    if (d <= r)
                    {
                        float rim = d / r;
                        float brightness = Mathf.Lerp(1f, 0.85f, rim * rim);
                        tex.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        private static void GenerateCyberTrack()
        {
            string path = $"{SpritesDir}/CyberTrack.png";
            int w = 256, h = 64;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            FillRect(tex, 0, 0, w, h - 8, new Color(0.10f, 0.11f, 0.17f, 1f));
            FillRect(tex, 0, h - 8, w, 8, new Color(1.3f, 0.85f, 0.2f, 1f));

            SaveAndImport(tex, path, 128);
        }

        private static void GenerateMonolith()
        {
            string path = $"{SpritesDir}/Monolith.png";
            int w = 128, h = 512;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 tip = new Vector2(w * 0.5f, h * 0.98f);
            Vector2 bL = new Vector2(w * 0.1f, 0);
            Vector2 bR = new Vector2(w * 0.9f, 0);

            DrawTriangle(tex, tip, bL, bR, new Color(1f, 1f, 1f, 0.85f));
            SaveAndImport(tex, path, 128);
        }

        // --- MODERN UI SPRITES ---

        private static void GenerateButtonPill(bool isHover)
        {
            string path = isHover ? $"{SpritesDir}/UIButtonHover.png" : $"{SpritesDir}/UIButton.png";
            int w = 320, h = 64;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float cornerRadius = 30f;

            Color darkGlass = isHover 
                ? new Color(0.20f, 0.25f, 0.42f, 0.96f) 
                : new Color(0.08f, 0.10f, 0.17f, 0.94f);

            Color borderGlow = isHover 
                ? new Color(0.35f, 1.0f, 1.6f, 1f)
                : new Color(0.25f, 0.8f, 1.1f, 0.75f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float distFromEdge = GetPillDistance(x, y, w, h, cornerRadius);

                    if (distFromEdge > 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float d = -distFromEdge;
                        if (d <= 2.5f)
                        {
                            tex.SetPixel(x, y, borderGlow);
                        }
                        else
                        {
                            float vertGrad = (float)y / h;
                            Color fill = Color.Lerp(darkGlass * 0.9f, darkGlass * 1.15f, vertGrad);
                            tex.SetPixel(x, y, fill);
                        }
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        private static void GenerateModernGlassCard()
        {
            string path = $"{SpritesDir}/UIPanel.png";
            int w = 512, h = 512;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            float cornerRadius = 24f;

            Color cardBg = new Color(0.05f, 0.06f, 0.11f, 0.95f);
            Color border = new Color(0.25f, 0.8f, 1.1f, 0.6f);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    float dist = GetBoxDistance(x, y, w, h, cornerRadius);
                    if (dist > 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        float d = -dist;
                        if (d <= 2.0f)
                        {
                            tex.SetPixel(x, y, border);
                        }
                        else
                        {
                            float grad = (float)y / h;
                            Color c = Color.Lerp(cardBg * 0.9f, cardBg * 1.1f, grad);
                            tex.SetPixel(x, y, c);
                        }
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        private static float GetPillDistance(float x, float y, float w, float h, float r)
        {
            float cx = Mathf.Clamp(x, r, w - r);
            float cy = h * 0.5f;
            return Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)) - r;
        }

        private static float GetBoxDistance(float x, float y, float w, float h, float r)
        {
            float dx = Mathf.Max(0f, Mathf.Max(r - x, x - (w - r)));
            float dy = Mathf.Max(0f, Mathf.Max(r - y, y - (h - r)));
            if (dx == 0f || dy == 0f) return -Mathf.Min(Mathf.Min(x, w - x), Mathf.Min(y, h - y));
            return Mathf.Sqrt(dx * dx + dy * dy) - r;
        }

        private static void GenerateIconJump()
        {
            string path = $"{SpritesDir}/IconJump.png";
            int s = 64;
            Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 top = new Vector2(s * 0.5f, s * 0.85f);
            Vector2 left = new Vector2(s * 0.2f, s * 0.45f);
            Vector2 right = new Vector2(s * 0.8f, s * 0.45f);

            DrawTriangle(tex, top, left, right, Color.white);
            FillRect(tex, (int)(s * 0.38f), (int)(s * 0.12f), (int)(s * 0.24f), (int)(s * 0.35f), Color.white);

            SaveAndImport(tex, path, 64);
        }

        private static void GenerateIconPhase()
        {
            string path = $"{SpritesDir}/IconPhase.png";
            int s = 64;
            Texture2D tex = new Texture2D(s, s, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            Vector2 center = new Vector2(s * 0.5f, s * 0.5f);
            FillCircle(tex, center, s * 0.44f, new Color(0.2f, 0.85f, 1f));
            FillCircle(tex, new Vector2(s * 0.5f, s * 0.65f), s * 0.22f, Color.white);
            FillCircle(tex, new Vector2(s * 0.5f, s * 0.35f), s * 0.22f, new Color(0.06f, 0.08f, 0.12f));

            SaveAndImport(tex, path, 64);
        }

        // Drawing Utilities
        private static void ClearTexture(Texture2D tex)
        {
            Color clear = new Color(0, 0, 0, 0);
            Color[] cols = new Color[tex.width * tex.height];
            for (int i = 0; i < cols.Length; i++) cols[i] = clear;
            tex.SetPixels(cols);
        }

        private static void FillRect(Texture2D tex, int x, int y, int width, int height, Color col)
        {
            for (int cy = y; cy < y + height && cy < tex.height; cy++)
            {
                for (int cx = x; cx < x + width && cx < tex.width; cx++)
                {
                    if (cx >= 0 && cy >= 0) tex.SetPixel(cx, cy, col);
                }
            }
        }

        private static void FillCircle(Texture2D tex, Vector2 center, float radius, Color col)
        {
            int minX = Mathf.Max(0, (int)(center.x - radius));
            int maxX = Mathf.Min(tex.width - 1, (int)(center.x + radius));
            int minY = Mathf.Max(0, (int)(center.y - radius));
            int maxY = Mathf.Min(tex.height - 1, (int)(center.y + radius));

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (Vector2.Distance(new Vector2(x, y), center) <= radius)
                    {
                        tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static void DrawTriangle(Texture2D tex, Vector2 p1, Vector2 p2, Vector2 p3, Color col)
        {
            int minX = (int)Mathf.Min(p1.x, Mathf.Min(p2.x, p3.x));
            int maxX = (int)Mathf.Max(p1.x, Mathf.Max(p2.x, p3.x));
            int minY = (int)Mathf.Min(p1.y, Mathf.Min(p2.y, p3.y));
            int maxY = (int)Mathf.Max(p1.y, Mathf.Max(p2.y, p3.y));

            minX = Mathf.Max(0, minX); maxX = Mathf.Min(tex.width - 1, maxX);
            minY = Mathf.Max(0, minY); maxY = Mathf.Min(tex.height - 1, maxY);

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    Vector2 p = new Vector2(x, y);
                    if (PointInTriangle(p, p1, p2, p3))
                    {
                        tex.SetPixel(x, y, col);
                    }
                }
            }
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float s = a.y * c.x - a.x * c.y + (c.y - a.y) * p.x + (a.x - c.x) * p.y;
            float t = a.x * b.y - a.y * b.x + (a.y - b.y) * p.x + (b.x - a.x) * p.y;
            if ((s < 0) != (t < 0) && s != 0 && t != 0) return false;
            float d = -b.y * c.x + a.y * (c.x - b.x) + a.x * (b.y - c.y) + b.x * c.y;
            return d < 0 ? (s <= 0 && s + t >= d) : (s >= 0 && s + t <= d);
        }

        private static void DrawLine(Texture2D tex, Vector2 p1, Vector2 p2, Color col, int thickness)
        {
            int steps = (int)(Vector2.Distance(p1, p2) * 2f);
            for (int i = 0; i <= steps; i++)
            {
                Vector2 p = Vector2.Lerp(p1, p2, (float)i / steps);
                FillCircle(tex, p, thickness * 0.5f, col);
            }
        }

        private static void SaveAndImport(Texture2D tex, string path, int ppu)
        {
            tex.Apply();
            File.WriteAllBytes(path, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(path);
            ConfigureTextureAsSprite(path, ppu);
        }

        private static void ConfigureTextureAsSprite(string assetPath, int ppu)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer != null)
            {
                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = ppu;
                importer.filterMode = FilterMode.Bilinear;
                importer.SaveAndReimport();
            }
        }
    }
}
#endif
