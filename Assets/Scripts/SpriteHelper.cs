using UnityEngine;
using System.IO;
using System.Collections.Generic;

namespace LightNShadows
{
    public static class SpriteHelper
    {
        private const string BrainDir = @"C:\Users\sarah\.gemini\antigravity\brain\dd098baa-b450-4d87-950b-5edbdecc7adb";

        private static Sprite cachedLaserSprite;

        public static Sprite GetLaserGateSprite()
        {
            if (cachedLaserSprite != null) return cachedLaserSprite;
            cachedLaserSprite = LoadOrProcessSprite("LaserGate.png", "neon_laser_beam_1790925034517.jpg", 320f);
            return cachedLaserSprite;
        }

        private static Sprite LoadOrProcessSprite(string pngName, string brainJpgName, float ppu)
        {
            string spritesDir = Path.Combine(Application.dataPath, "Sprites");
            if (!Directory.Exists(spritesDir)) Directory.CreateDirectory(spritesDir);

            string pngPath = Path.Combine(spritesDir, pngName);

            // 1. If PNG already exists, load directly
            if (File.Exists(pngPath))
            {
                byte[] pngBytes = File.ReadAllBytes(pngPath);
                Texture2D loaded = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                if (loaded.LoadImage(pngBytes))
                {
                    loaded.wrapMode = TextureWrapMode.Clamp;
                    loaded.filterMode = FilterMode.Bilinear;
                    return Sprite.Create(loaded, new Rect(0, 0, loaded.width, loaded.height), new Vector2(0.5f, 0.5f), ppu);
                }
            }

            // 2. Otherwise load source JPG from BrainDir, key out white background, crop and save
            string jpgPath = Path.Combine(BrainDir, brainJpgName);
            if (!File.Exists(jpgPath))
            {
                Debug.LogWarning($"[SpriteHelper] Source artwork not found at: {jpgPath}");
                return null;
            }

            byte[] jpgBytes = File.ReadAllBytes(jpgPath);
            Texture2D srcTex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            if (!srcTex.LoadImage(jpgBytes)) return null;

            int w = srcTex.width;
            int h = srcTex.height;
            Color[] pixels = srcTex.GetPixels();
            bool[] isBg = new bool[w * h];

            Queue<int> q = new Queue<int>();

            System.Func<Color, bool> isNearWhite = (col) =>
            {
                return (col.r > 0.90f && col.g > 0.90f && col.b > 0.90f);
            };

            for (int x = 0; x < w; x++)
            {
                int topIdx = (h - 1) * w + x;
                int botIdx = x;
                if (!isBg[topIdx] && isNearWhite(pixels[topIdx])) { isBg[topIdx] = true; q.Enqueue(topIdx); }
                if (!isBg[botIdx] && isNearWhite(pixels[botIdx])) { isBg[botIdx] = true; q.Enqueue(botIdx); }
            }
            for (int y = 0; y < h; y++)
            {
                int leftIdx = y * w;
                int rightIdx = y * w + (w - 1);
                if (!isBg[leftIdx] && isNearWhite(pixels[leftIdx])) { isBg[leftIdx] = true; q.Enqueue(leftIdx); }
                if (!isBg[rightIdx] && isNearWhite(pixels[rightIdx])) { isBg[rightIdx] = true; q.Enqueue(rightIdx); }
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (q.Count > 0)
            {
                int idx = q.Dequeue();
                int cy = idx / w;
                int cx = idx % w;

                for (int d = 0; d < 4; d++)
                {
                    int nx = cx + dx[d];
                    int ny = cy + dy[d];
                    if (nx >= 0 && nx < w && ny >= 0 && ny < h)
                    {
                        int nIdx = ny * w + nx;
                        if (!isBg[nIdx] && isNearWhite(pixels[nIdx]))
                        {
                            isBg[nIdx] = true;
                            q.Enqueue(nIdx);
                        }
                    }
                }
            }

            int minX = w, maxX = 0, minY = h, maxY = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int idx = y * w + x;
                    if (!isBg[idx])
                    {
                        if (x < minX) minX = x;
                        if (x > maxX) maxX = x;
                        if (y < minY) minY = y;
                        if (y > maxY) maxY = y;
                    }
                }
            }

            if (minX > maxX || minY > maxY)
            {
                return Sprite.Create(srcTex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), ppu);
            }

            int pad = 6;
            minX = Mathf.Max(0, minX - pad);
            maxX = Mathf.Min(w - 1, maxX + pad);
            minY = Mathf.Max(0, minY - pad);
            maxY = Mathf.Min(h - 1, maxY + pad);

            int cropW = maxX - minX + 1;
            int cropH = maxY - minY + 1;

            Texture2D cropped = new Texture2D(cropW, cropH, TextureFormat.RGBA32, false);

            for (int cy = 0; cy < cropH; cy++)
            {
                for (int cx = 0; cx < cropW; cx++)
                {
                    int origX = minX + cx;
                    int origY = minY + cy;
                    int idx = origY * w + origX;

                    if (isBg[idx])
                    {
                        cropped.SetPixel(cx, cy, Color.clear);
                    }
                    else
                    {
                        Color c = pixels[idx];
                        bool neighborIsBg = false;
                        for (int d = 0; d < 4; d++)
                        {
                            int nx = origX + dx[d];
                            int ny = origY + dy[d];
                            if (nx >= 0 && nx < w && ny >= 0 && ny < h && isBg[ny * w + nx])
                            {
                                neighborIsBg = true;
                                break;
                            }
                        }

                        if (neighborIsBg && c.r > 0.80f && c.g > 0.80f && c.b > 0.80f)
                        {
                            float avg = (c.r + c.g + c.b) / 3f;
                            c.a = Mathf.Clamp01(1f - (avg - 0.80f) / 0.20f);
                        }

                        cropped.SetPixel(cx, cy, c);
                    }
                }
            }

            cropped.Apply();
            cropped.wrapMode = TextureWrapMode.Clamp;
            cropped.filterMode = FilterMode.Bilinear;

            byte[] outBytes = cropped.EncodeToPNG();
            File.WriteAllBytes(pngPath, outBytes);

            return Sprite.Create(cropped, new Rect(0, 0, cropW, cropH), new Vector2(0.5f, 0.5f), ppu);
        }
    }
}
