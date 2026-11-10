using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace ArrowMaze
{
    public class TutorialManager : MonoBehaviour
    {
        private static TutorialManager _instance;
        public static TutorialManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<TutorialManager>();
                    if (_instance == null)
                    {
                        GameObject go = new GameObject("TutorialManager");
                        _instance = go.AddComponent<TutorialManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [Header("Tutorial Content")]
        [SerializeField] private string tutorialMessage = "Tap to exit";
        [Tooltip("If true, keeps whatever text you type into the Text component in the hierarchy. If false, overrides with tutorialMessage.")]
        [SerializeField] private bool useHierarchyText = true;
        [SerializeField] private Sprite customHandSprite;

        [Header("Hand Customization & Positioning")]
        [Tooltip("If true, the hand tracks the target snake head. If false, the hand stays at the exact position you place it in the hierarchy.")]
        [SerializeField] private bool followTargetSnake = true;
        [Tooltip("Pixel offset added to the hand position when followTargetSnake is true.")]
        [SerializeField] private Vector2 handOffset = Vector2.zero;

        [Header("Animation Settings")]
        [SerializeField] private float tapCycleDuration = 1.1f;
        [SerializeField] private float tapMovementDistance = 24f;
        [SerializeField] private float tapScalePunch = 0.88f;
        [SerializeField] private float fadeInDuration = 0.35f;
        [SerializeField] private float fadeOutDuration = 0.25f;

        [Header("UI References (Assigned in Hierarchy)")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private RectTransform overlayRoot;
        [SerializeField] private RectTransform tooltipBanner;
        [SerializeField] private Text tooltipText;
        [SerializeField] private RectTransform handCursor;
        [SerializeField] private Image handImage;
        [SerializeField] private CanvasGroup overlayCanvasGroup;

        private GameManager gameManager;
        private Arrow trackedArrow;
        private Coroutine tapRoutine;
        private Coroutine fadeRoutine;
        private Coroutine showRoutine;
        private bool isTutorialActive = false;
        private bool isDismissed = false;
        private float currentTapOffset = 0f;
        private Vector2 initialHandPosition = Vector2.zero;
        private Vector3 initialHandScale = Vector3.one;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            CacheInitialHandTransform();
        }

        private void CacheInitialHandTransform()
        {
            if (handCursor != null)
            {
                initialHandPosition = handCursor.anchoredPosition;
                initialHandScale = handCursor.localScale;
            }
        }

        private void Start()
        {
            gameManager = FindObjectOfType<GameManager>();
            EnsureUIHierarchy();

            // If game is already running at Level 1, start tutorial
            if (gameManager != null && gameManager.currentLevelIndex == 0)
            {
                OnLevelReady(0);
            }
        }

        /// <summary>
        /// Builds or ensures the tutorial UI hierarchy exists under Canvas.
        /// </summary>
        public void EnsureUIHierarchy()
        {
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
            }

            if (targetCanvas == null)
            {
                Debug.LogWarning("[TutorialManager] No Canvas found in scene.");
                return;
            }

            if (overlayRoot != null && tooltipBanner != null && handCursor != null)
            {
                CacheInitialHandTransform();
                return;
            }

            // Check if existing hierarchy is complete
            Transform existing = targetCanvas.transform.Find("TutorialOverlay");
            if (existing != null)
            {
                Transform bannerTr = existing.Find("TooltipBanner");
                Transform handTr = existing.Find("HandCursor");

                if (bannerTr != null && handTr != null)
                {
                    overlayRoot = existing.GetComponent<RectTransform>();
                    overlayCanvasGroup = existing.GetComponent<CanvasGroup>();
                    tooltipBanner = bannerTr.GetComponent<RectTransform>();
                    tooltipText = bannerTr.GetComponentInChildren<Text>();
                    handCursor = handTr.GetComponent<RectTransform>();
                    handImage = handTr.GetComponent<Image>();
                    CacheInitialHandTransform();
                    return;
                }
                else
                {
                    // Incomplete hierarchy from previous run, clean it up
                    Destroy(existing.gameObject);
                }
            }

            // 1. Create Root Container
            GameObject rootGo = new GameObject("TutorialOverlay", typeof(RectTransform), typeof(CanvasGroup));
            rootGo.transform.SetParent(targetCanvas.transform, false);
            overlayRoot = rootGo.GetComponent<RectTransform>();
            overlayRoot.anchorMin = Vector2.zero;
            overlayRoot.anchorMax = Vector2.one;
            overlayRoot.offsetMin = Vector2.zero;
            overlayRoot.offsetMax = Vector2.zero;

            // Make sure tutorial renders above gameplay UI but doesn't block clicks
            overlayCanvasGroup = rootGo.GetComponent<CanvasGroup>();
            overlayCanvasGroup.alpha = 0f;
            overlayCanvasGroup.blocksRaycasts = false;
            overlayCanvasGroup.interactable = false;

            // 2. Create smooth pill sprite
            Sprite pillSprite = CreatePillSprite(380, 80, 40);

            // 3. Create Tooltip Banner (positioned safely below TopBar with hearts at y = -345)
            GameObject bannerGo = new GameObject("TooltipBanner", typeof(RectTransform), typeof(Image));
            bannerGo.transform.SetParent(overlayRoot, false);
            tooltipBanner = bannerGo.GetComponent<RectTransform>();
            tooltipBanner.anchorMin = new Vector2(0.5f, 1f);
            tooltipBanner.anchorMax = new Vector2(0.5f, 1f);
            tooltipBanner.pivot = new Vector2(0.5f, 1f);
            tooltipBanner.anchoredPosition = new Vector2(0f, -345f);
            tooltipBanner.sizeDelta = new Vector2(360f, 76f);

            Image bannerImg = bannerGo.GetComponent<Image>();
            bannerImg.sprite = pillSprite;
            bannerImg.type = Image.Type.Sliced;
            bannerImg.color = new Color(0.08f, 0.12f, 0.22f, 0.92f); // Sleek modern dark navy pill
            bannerImg.raycastTarget = false;

            Outline outline = bannerGo.AddComponent<Outline>();
            outline.effectColor = new Color(1f, 1f, 1f, 0.22f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);

            // 4. Create Text inside Banner
            GameObject textGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textGo.transform.SetParent(tooltipBanner, false);
            RectTransform textRect = textGo.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20f, 0f);
            textRect.offsetMax = new Vector2(-20f, 0f);

            tooltipText = textGo.GetComponent<Text>();
            tooltipText.text = tutorialMessage;
            tooltipText.font = GetSafeFont();
            tooltipText.fontSize = 32;
            tooltipText.fontStyle = FontStyle.Bold;
            tooltipText.alignment = TextAnchor.MiddleCenter;
            tooltipText.color = Color.white;
            tooltipText.raycastTarget = false;

            // 5. Create Hand Cursor
            GameObject handGo = new GameObject("HandCursor", typeof(RectTransform), typeof(Image));
            handGo.transform.SetParent(overlayRoot, false);
            handCursor = handGo.GetComponent<RectTransform>();
            handCursor.anchorMin = new Vector2(0.5f, 0.5f);
            handCursor.anchorMax = new Vector2(0.5f, 0.5f);
            // Pivot at index fingertip: top-left area of Hand.png (approx x=0.14, y=1.0)
            handCursor.pivot = new Vector2(0.14f, 1.0f);
            handCursor.sizeDelta = new Vector2(130f, 110f);

            handImage = handGo.GetComponent<Image>();
            handImage.raycastTarget = false;

            Sprite loadedHand = customHandSprite != null ? customHandSprite : Resources.Load<Sprite>("Sprites/Hand");
            if (loadedHand == null)
            {
                loadedHand = Resources.Load<Sprite>("Hand");
            }

            if (loadedHand != null)
            {
                handImage.sprite = loadedHand;
            }

            rootGo.SetActive(false);
        }

        private Font GetSafeFont()
        {
            // Try Unity 6 LegacyRuntime font
            try
            {
                Font f = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (f != null) return f;
            }
            catch { }

            // Borrow font from any existing Text component in scene
            Text[] allTexts = FindObjectsOfType<Text>();
            foreach (var t in allTexts)
            {
                if (t != null && t.font != null) return t.font;
            }

            // Fallback to Arial
            try
            {
                Font f = Resources.GetBuiltinResource<Font>("Arial.ttf");
                if (f != null) return f;
            }
            catch { }

            return null;
        }

        /// <summary>
        /// Called when a level is ready. Activates tutorial on Level 1 (index 0).
        /// </summary>
        public void OnLevelReady(int levelIndex)
        {
            EnsureUIHierarchy();

            if (levelIndex == 0)
            {
                isDismissed = false;
                if (showRoutine != null) StopCoroutine(showRoutine);
                showRoutine = StartCoroutine(DelayedShowTutorial());
            }
            else
            {
                HideTutorial(false);
            }
        }

        private IEnumerator DelayedShowTutorial()
        {
            if (gameManager == null) gameManager = FindObjectOfType<GameManager>();

            // Wait until gameplay begins (menu is closed and arrows finished appearing)
            while (gameManager != null && (gameManager.IsAppearing || (gameManager.mainMenuPanel != null && gameManager.mainMenuPanel.activeInHierarchy)))
            {
                yield return null;
            }

            yield return new WaitForSeconds(0.2f);

            if (isDismissed) yield break;

            Arrow target = FindTutorialTargetArrow();
            if (target != null)
            {
                ShowTutorial(target);
            }
        }

        private Arrow FindTutorialTargetArrow()
        {
            Arrow[] allArrows = FindObjectsOfType<Arrow>();
            if (allArrows == null || allArrows.Length == 0) return null;

            // In Level 1, the center snake is at column 2 (X=2)
            Arrow best = null;
            float closestDist = float.MaxValue;
            foreach (var a in allArrows)
            {
                if (a == null || a.bodyPositions == null || a.bodyPositions.Count == 0) continue;
                float dist = Mathf.Abs(a.bodyPositions[0].x - 2);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    best = a;
                }
            }

            return best != null ? best : allArrows[0];
        }

        public void ShowTutorial(Arrow target)
        {
            if (overlayRoot == null) EnsureUIHierarchy();
            if (overlayRoot == null) return;

            trackedArrow = target;
            isTutorialActive = true;
            overlayRoot.gameObject.SetActive(true);

            if (tooltipText != null)
            {
                if (!useHierarchyText || string.IsNullOrEmpty(tooltipText.text))
                {
                    tooltipText.text = tutorialMessage;
                }
            }

            // Position hand immediately at target
            UpdateHandPosition(0f);

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeRoutine(1f, fadeInDuration));

            if (tapRoutine != null) StopCoroutine(tapRoutine);
            tapRoutine = StartCoroutine(TapLoopRoutine());
        }

        public void HideTutorial(bool animate = true)
        {
            isTutorialActive = false;
            isDismissed = true;
            trackedArrow = null;

            if (tapRoutine != null)
            {
                StopCoroutine(tapRoutine);
                tapRoutine = null;
            }

            if (overlayRoot == null || !overlayRoot.gameObject.activeInHierarchy) return;

            if (animate)
            {
                if (fadeRoutine != null) StopCoroutine(fadeRoutine);
                fadeRoutine = StartCoroutine(FadeRoutine(0f, fadeOutDuration, () =>
                {
                    if (overlayRoot != null) overlayRoot.gameObject.SetActive(false);
                }));
            }
            else
            {
                if (overlayCanvasGroup != null) overlayCanvasGroup.alpha = 0f;
                overlayRoot.gameObject.SetActive(false);
            }
        }

        public void OnArrowTapped(Arrow tappedArrow)
        {
            if (!isTutorialActive) return;
            StartCoroutine(TapFeedbackAndHide());
        }

        private IEnumerator TapFeedbackAndHide()
        {
            isTutorialActive = false;
            isDismissed = true;

            if (tapRoutine != null)
            {
                StopCoroutine(tapRoutine);
                tapRoutine = null;
            }

            // Quick squeeze tap feedback
            if (handCursor != null)
            {
                float elapsed = 0f;
                float dur = 0.08f;
                Vector3 startScale = handCursor.localScale;
                Vector3 targetScale = initialHandScale * 0.82f;
                while (elapsed < dur)
                {
                    elapsed += Time.deltaTime;
                    handCursor.localScale = Vector3.Lerp(startScale, targetScale, elapsed / dur);
                    yield return null;
                }
            }

            HideTutorial(true);
        }

        private void LateUpdate()
        {
            if (!isTutorialActive || trackedArrow == null) return;

            if (trackedArrow.gameObject == null || !trackedArrow.gameObject.activeInHierarchy)
            {
                HideTutorial(true);
                return;
            }

            UpdateHandPosition(currentTapOffset);
        }

        private void UpdateHandPosition(float tapOffset)
        {
            if (handCursor == null) return;

            Vector2 targetPos = initialHandPosition;

            if (followTargetSnake && trackedArrow != null && targetCanvas != null)
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    Vector3 worldPos = trackedArrow.transform.position;
                    // Point gracefully at upper body without covering the eyes
                    Vector3 targetWorld = worldPos + new Vector3(0.08f, -0.3f, 0f);
                    Vector3 screenPos = cam.WorldToScreenPoint(targetWorld);

                    RectTransform canvasRect = targetCanvas.transform as RectTransform;
                    if (canvasRect != null && RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, null, out Vector2 localPoint))
                    {
                        targetPos = localPoint + handOffset;
                    }
                }
            }

            // Subtle diagonal tap bounce: down-left and return
            Vector2 tapDir = new Vector2(-0.4f, 0.9f).normalized;
            targetPos += tapDir * tapOffset;

            handCursor.anchoredPosition = targetPos;
        }

        private IEnumerator TapLoopRoutine()
        {
            while (isTutorialActive)
            {
                float elapsed = 0f;
                float halfCycle = tapCycleDuration * 0.45f;

                // Move forward towards arrow + scale punch
                while (elapsed < halfCycle)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / halfCycle);
                    float ease = Mathf.SmoothStep(0f, 1f, t);

                    currentTapOffset = ease * tapMovementDistance;
                    if (handCursor != null)
                    {
                        handCursor.localScale = initialHandScale * Mathf.Lerp(1f, tapScalePunch, ease);
                    }
                    yield return null;
                }

                yield return new WaitForSeconds(0.08f);

                // Release back
                elapsed = 0f;
                float returnDur = tapCycleDuration * 0.35f;
                while (elapsed < returnDur)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / returnDur);
                    float ease = Mathf.SmoothStep(0f, 1f, t);

                    currentTapOffset = (1f - ease) * tapMovementDistance;
                    if (handCursor != null)
                    {
                        handCursor.localScale = initialHandScale * Mathf.Lerp(tapScalePunch, 1f, ease);
                    }
                    yield return null;
                }

                currentTapOffset = 0f;
                if (handCursor != null) handCursor.localScale = initialHandScale;

                yield return new WaitForSeconds(tapCycleDuration * 0.2f);
            }
        }

        private IEnumerator FadeRoutine(float targetAlpha, float duration, System.Action onComplete = null)
        {
            if (overlayCanvasGroup == null) yield break;

            float startAlpha = overlayCanvasGroup.alpha;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                overlayCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
                yield return null;
            }

            overlayCanvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }

        private static Sprite CreatePillSprite(int width, int height, int radius)
        {
            Texture2D tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            Color[] colors = new Color[width * height];

            float r = radius;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float cx = x < r ? r : (x > width - 1 - r ? width - 1 - r : x);
                    float cy = y < r ? r : (y > height - 1 - r ? height - 1 - r : y);
                    float dist = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));

                    float alpha = 1f - Mathf.Clamp01(dist - (r - 1f));
                    colors[x + y * width] = new Color(1f, 1f, 1f, alpha);
                }
            }

            tex.SetPixels(colors);
            tex.Apply();

            Vector4 border = new Vector4(radius, radius, radius, radius);
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, border);
        }
    }
}
