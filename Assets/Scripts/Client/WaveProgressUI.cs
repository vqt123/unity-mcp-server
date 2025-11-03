using UnityEngine;
using UnityEngine.UI;

namespace ArenaGame.Client
{
    /// <summary>
    /// Displays wave progress as a radial fill in top-right corner
    /// Uses existing sprite from scene - no procedural generation needed!
    /// </summary>
    public class WaveProgressUI : MonoBehaviour
    {
        private Image fillImage;
        private WaveManager waveManager;
        private float lastWaveStartTime;
        private float waveDuration = 15f; // Approximate wave duration
        
        void Start()
        {
            Debug.Log("[WaveProgress] Starting wave progress UI");
            CreateWaveProgressUI();
        }
        
        private void CreateWaveProgressUI()
        {
            // Find Canvas
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[WaveProgress] No Canvas found!");
                return;
            }
            
            // Find an existing Image with a sprite to copy from (XPBarFill)
            Image sourceImage = FindExistingUISprite();
            if (sourceImage == null || sourceImage.sprite == null)
            {
                // If no sprite found, create a simple colored sprite (this is normal behavior)
                Debug.Log("[WaveProgress] No existing sprite found, creating default sprite");
                sourceImage = CreateDefaultSprite(canvas.transform);
            }
            
            if (sourceImage != null && sourceImage.sprite != null)
            {
                Debug.Log($"[WaveProgress] Using sprite: {sourceImage.sprite.name}");
            }
            
            // Create container in top-right
            GameObject containerObj = new GameObject("WaveProgressContainer");
            containerObj.transform.SetParent(canvas.transform, false);
            
            RectTransform containerRect = containerObj.AddComponent<RectTransform>();
            containerRect.anchorMin = new Vector2(1, 1);
            containerRect.anchorMax = new Vector2(1, 1);
            containerRect.pivot = new Vector2(1, 1);
            containerRect.anchoredPosition = new Vector2(-20, -20);
            containerRect.sizeDelta = new Vector2(80, 80);
            
            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(containerObj.transform, false);
            RectTransform bgRect = bgObj.AddComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.sizeDelta = Vector2.zero;
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.sprite = sourceImage.sprite; // Reuse sprite!
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            // Create fill
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(containerObj.transform, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.sizeDelta = Vector2.zero;
            fillImage = fillObj.AddComponent<Image>();
            fillImage.sprite = sourceImage.sprite; // Reuse sprite!
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Radial360;
            fillImage.fillOrigin = (int)Image.Origin360.Top;
            fillImage.fillClockwise = true;
            fillImage.fillAmount = 0f;
            fillImage.color = new Color(1f, 0.8f, 0f, 1f); // Gold/yellow color
            
            // Add text label
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(containerObj.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 0);
            textRect.pivot = new Vector2(0.5f, 1);
            textRect.anchoredPosition = new Vector2(0, -85);
            textRect.sizeDelta = new Vector2(0, 30);
            TMPro.TextMeshProUGUI textLabel = textObj.AddComponent<TMPro.TextMeshProUGUI>();
            textLabel.text = "Wave";
            textLabel.fontSize = 16;
            textLabel.color = Color.white;
            textLabel.alignment = TMPro.TextAlignmentOptions.Center;
            
            Debug.Log("[WaveProgress] Created wave progress UI using existing sprite!");
        }
        
        private Image FindExistingUISprite()
        {
            // Find XPBarFill or any other Image component with a sprite
            Image[] allImages = FindObjectsByType<Image>(FindObjectsSortMode.None);
            foreach (Image img in allImages)
            {
                if (img.sprite != null && img.gameObject.name.Contains("XPBarFill"))
                {
                    Debug.Log($"[WaveProgress] Found XPBarFill with sprite: {img.sprite.name}");
                    return img;
                }
            }
            
            // Fallback: return any Image with a sprite
            foreach (Image img in allImages)
            {
                if (img.sprite != null)
                {
                    Debug.Log($"[WaveProgress] Using fallback sprite from: {img.gameObject.name}");
                    return img;
                }
            }
            
            return null;
        }
        
        private Image CreateDefaultSprite(Transform parent)
        {
            // Create a simple colored sprite as fallback
            // This GameObject is only used to hold the sprite - it's hidden
            GameObject spriteObj = new GameObject("DefaultSprite");
            spriteObj.transform.SetParent(parent, false);
            
            RectTransform rect = spriteObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(100, 100);
            // Position off-screen so it's not visible
            rect.anchoredPosition = new Vector2(-10000, -10000);
            
            Image img = spriteObj.AddComponent<Image>();
            // Create a simple white sprite texture
            Texture2D texture = new Texture2D(1, 1);
            texture.SetPixel(0, 0, Color.white);
            texture.Apply();
            
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 100);
            img.sprite = sprite;
            
            // Hide the GameObject so it doesn't appear on screen
            spriteObj.SetActive(false);
            
            return img;
        }
        
        void Update()
        {
            if (fillImage == null) return;
            
            // Find wave manager if we don't have it
            if (waveManager == null)
            {
                waveManager = FindFirstObjectByType<WaveManager>();
                if (waveManager == null) return;
            }
            
            // Simple time-based progress (could be improved with actual wave progress)
            float timeSinceWaveStart = Time.time - lastWaveStartTime;
            float progress = Mathf.Clamp01(timeSinceWaveStart / waveDuration);
            fillImage.fillAmount = progress;
            
            // Reset when wave completes
            if (progress >= 1f)
            {
                lastWaveStartTime = Time.time;
            }
        }
    }
}

