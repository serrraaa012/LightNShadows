#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace LightNShadows.Editor
{
    public static class SpriteArtGenerator
    {
        private const string SpritesDir = "Assets/Sprites";

        public static void GenerateAllSprites()
        {
            if (!Directory.Exists(SpritesDir))
            {
                Directory.CreateDirectory(SpritesDir);
                AssetDatabase.Refresh();
            }

            GenerateCrystalSpike();
            GenerateLaserGate();
            GenerateFloatingDiamond();
            GeneratePlayerHalo();
            GeneratePlayerCore();
            GenerateCyberTrack();
            GenerateMonolith();

            AssetDatabase.Refresh();
        }

        // 1. Sharp Crystal Spike (Ground hazard)
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

            // Left facet
            DrawTriangle(tex, tip, baseL, facetMid, new Color(0.85f, 0.85f, 0.9f, 1f));
            // Right facet (slightly shaded for 3D crystal depth)
            DrawTriangle(tex, tip, facetMid, baseR, new Color(1f, 1f, 1f, 1f));

            // Sharp edge line
            DrawLine(tex, tip, baseL, Color.white, 3);
            DrawLine(tex, tip, baseR, Color.white, 3);
            DrawLine(tex, tip, facetMid, new Color(0.7f, 0.7f, 0.8f, 0.9f), 2);

            SaveAndImport(tex, path, 128);
        }

        // 2. High-Tech Laser Energy Gate (Tall barrier)
        private static void GenerateLaserGate()
        {
            string path = $"{SpritesDir}/LaserGate.png";
            int w = 128, h = 512;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            // Top and bottom emitter caps
            FillRect(tex, 14, h - 40, w - 28, 30, Color.white);
            FillRect(tex, 14, 10, w - 28, 30, Color.white);

            // Outer emitter antennas
            FillRect(tex, 24, h - 55, w - 48, 15, new Color(0.8f, 0.8f, 0.85f));
            FillRect(tex, 24, 40, w - 48, 15, new Color(0.8f, 0.8f, 0.85f));

            // Vertical electric laser beam in center
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

        // 3. Floating Diamond Drone (Mid-air hazard)
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

            // Outer diamond
            DrawTriangle(tex, top, left, bot, new Color(0.85f, 0.85f, 0.9f));
            DrawTriangle(tex, top, bot, right, Color.white);

            // Inner eye / core
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            FillCircle(tex, center, 14, new Color(0.1f, 0.1f, 0.15f));
            FillCircle(tex, center, 8, Color.white);

            DrawLine(tex, top, left, Color.white, 2);
            DrawLine(tex, left, bot, Color.white, 2);
            DrawLine(tex, bot, right, Color.white, 2);
            DrawLine(tex, right, top, Color.white, 2);

            SaveAndImport(tex, path, 128);
        }

        // 4. Player Orbit Halo Ring
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
                        // Add 4 notches/breaks for a futuristic reticle look
                        float angle = Mathf.Atan2(y - center.y, x - center.x) * Mathf.Rad2Deg;
                        if (angle < 0) angle += 360f;
                        bool isNotch = (angle > 40 && angle < 50) || (angle > 130 && angle < 140) ||
                                       (angle > 220 && angle < 230) || (angle > 310 && angle < 320);

                        if (!isNotch)
                        {
                            tex.SetPixel(x, y, Color.white);
                        }
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        // 5. Player Core Sphere
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
                        // Slight radial bevel
                        float rim = d / r;
                        float brightness = Mathf.Lerp(1f, 0.85f, rim * rim);
                        tex.SetPixel(x, y, new Color(brightness, brightness, brightness, 1f));
                    }
                }
            }

            SaveAndImport(tex, path, 128);
        }

        // 6. Cyber Track (Runway with glowing neon rail)
        private static void GenerateCyberTrack()
        {
            string path = $"{SpritesDir}/CyberTrack.png";
            int w = 256, h = 64;
            Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            ClearTexture(tex);

            // Dark solid base
            FillRect(tex, 0, 0, w, h - 8, new Color(0.12f, 0.13f, 0.18f, 1f));

            // Top glowing neon guide rail
            FillRect(tex, 0, h - 8, w, 8, Color.white);

            SaveAndImport(tex, path, 128);
        }

        // 7. Parallax Monolith (Distant futuristic spires)
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

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
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
