using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using ArrowMaze.Data;
using ArrowMaze.Gameplay.Board;
using ArrowMaze.Gameplay.Snakes;
using ArrowMaze.LevelContent;

namespace ArrowMaze
{
    public class GameManager : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int gridWidth = 10;
        public int gridHeight = 10;
        public float gridStep = 1.0f;

        [Header("Level Data")]
        public List<LevelData> allLevels;
        public List<TextAsset> jsonLevels;
        public string jsonResourcesPath = "Levels";
        public int currentLevelIndex = 0;
        public TextAsset levelJson;

        [Header("Gameplay Model Migration")]
        public bool validateGameplayModelOnLoad = true;

        [Header("Prefabs")]
        public GameObject arrowPrefab;
        public GameObject gridSquarePrefab;
        public GameObject clickParticlePrefab;
        public float arrowSpawnDelay = 0.05f;

        [Header("Movement Feedback")]
        public bool enableCameraShake = true;
        public bool enableHaptics = true;
        public float blockedShakeDuration = 0.08f;
        public float blockedShakeStrength = 0.08f;
        public float escapeShakeDuration = 0.04f;
        public float escapeShakeStrength = 0.035f;

        [Header("UI References")]
        public GameObject[] hearts;
        public GameObject gameOverPanel;
        public GameObject levelCompletePanel;
        public CameraController cameraController;
        public UnityEngine.UI.Text arrowCountText;
        public UnityEngine.UI.Image damageOverlay; // Red vignette/image from inspector
        public CanvasGroup topBarGroup;
        public GameObject mainMenuPanel;
        public UnityEngine.UI.Text playLevelButtonText;
        public CanvasGroup gameTitleGroup;
        public CanvasGroup playButtonGroup;
        public CanvasGroup directionalPanelGroup;
        public CanvasGroup erasePanelGroup;
        
        [Header("Powerup Unlocks")]
        public PowerupUI hintPowerup;
        public PowerupUI erasePowerup;
        public PowerupUI forceEscapePowerup;

        [System.Serializable]
        public struct PowerupUI
        {
            public UnityEngine.UI.Button button;
            public GameObject mainIcon; // The actual power-up graphic (e.g., Lightbulb)
            public GameObject lockIcon; // The lock image
            public UnityEngine.UI.Text levelText;
            public int unlockLevel; // The level number (e.g., 6)
        }

        [System.Serializable]
        public class SnakeVisualSet
        {
            public Sprite headSprite;
            public Sprite bodySprite;
            public Sprite tailSprite;
        }

        [Header("Snake Visuals")]
        public List<SnakeVisualSet> snakeVisualSets;

        public SnakeVisualSet GetRandomVisualSet()
        {
            if (snakeVisualSets == null || snakeVisualSets.Count == 0) return null;
            return snakeVisualSets[Random.Range(0, snakeVisualSets.Count)];
        }

        private int lives = 3;
        private bool isGameOver = false;
        private bool isLevelCompleteTriggered = false;
        private int activeMovingSnakes = 0;
        public bool IsBusy => activeMovingSnakes > 0 || IsAppearing; 
        private bool _isBusyInternal = true; // For initialization/loading
        public bool IsAppearing { get; set; } = true;
        public bool IsEraseModeActive { get; private set; } = false;
        
        private Dictionary<Vector2Int, Arrow> gridContent = new Dictionary<Vector2Int, Arrow>();
        private int remainingArrows;
        private int totalArrows;
        private List<GameObject> activeGridDots = new List<GameObject>();
        private List<GameObject> activeBoardObjectViews = new List<GameObject>();
        private Dictionary<string, GameObject> boardObjectViewById = new Dictionary<string, GameObject>();
        private Dictionary<string, List<GameObject>> boardObjectViewsById = new Dictionary<string, List<GameObject>>();
        private Dictionary<string, TextMesh> boardObjectTextById = new Dictionary<string, TextMesh>();
        private List<LevelData> runtimeJsonLevels = new List<LevelData>();
        private Vector3 gridOffset;

        private int activeLevelIndex = 0;
        private int lastWidth, lastHeight;
        private BoardModel currentBoardModel;
        private Dictionary<Arrow, string> modelSnakeIdByArrow = new Dictionary<Arrow, string>();

        public bool IsGameplayInputBlocked => isGameOver || isLevelCompleteTriggered || IsAppearing;

        void Start()
        {
            Application.targetFrameRate = 90;
            QualitySettings.vSyncCount = 0;
            Application.runInBackground = true;
            // Load progress
            LoadJsonLevels();
            currentLevelIndex = PlayerPrefs.GetInt("CurrentLevelIndex", 0);
            ClampCurrentLevelIndex();

            lastWidth = gridWidth;
            lastHeight = gridHeight;
            CalculateOffset();

            if (TutorialManager.Instance == null)
            {
                GameObject tutGo = new GameObject("TutorialManager");
                tutGo.AddComponent<TutorialManager>();
            }

            ShowMainMenu();
        }

        private void ShowMainMenu()
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
            if (topBarGroup != null) topBarGroup.alpha = 0; // Hide top bar during menu
            
            UpdateMenuUI();
            StartCoroutine(AnimateMainMenu());
            
            IsAppearing = true;
        }

        private IEnumerator AnimateMainMenu()
        {
            float duration = 0.8f;
            float elapsed = 0;

            // Reset states
            if (gameTitleGroup != null) { gameTitleGroup.alpha = 0; gameTitleGroup.transform.localScale = Vector3.one * 0.8f; }
            if (playButtonGroup != null) { playButtonGroup.alpha = 0; playButtonGroup.transform.localScale = Vector3.one * 0.8f; }

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float easeOut = Mathf.Sin(t * Mathf.PI * 0.5f);
                float overshoot = 1f + Mathf.Sin(t * Mathf.PI) * 0.1f * (1f - t); // Subtle pop effect

                if (gameTitleGroup != null)
                {
                    gameTitleGroup.alpha = t;
                    gameTitleGroup.transform.localScale = Vector3.one * (0.8f + (overshoot - 0.8f));
                }

                // Stagger play button slightly
                if (elapsed > 0.2f && playButtonGroup != null)
                {
                    float t2 = (elapsed - 0.2f) / (duration - 0.2f);
                    playButtonGroup.alpha = t2;
                    playButtonGroup.transform.localScale = Vector3.one * (0.8f + (t2 * 0.25f)); // Scale from 0.8 to 1.05ish
                }

                yield return null;
            }

            if (gameTitleGroup != null) { gameTitleGroup.alpha = 1; gameTitleGroup.transform.localScale = Vector3.one; }
            if (playButtonGroup != null) { playButtonGroup.alpha = 1; playButtonGroup.transform.localScale = Vector3.one; }
        }

        private void UpdateMenuUI()
        {
            if (playLevelButtonText != null)
            {
                playLevelButtonText.text = "LEVEL " + (currentLevelIndex + 1);
            }
        }

        private void LoadJsonLevels()
        {
            runtimeJsonLevels.Clear();

            IEnumerable<TextAsset> sources;
            if (jsonLevels != null && jsonLevels.Count > 0)
            {
                sources = jsonLevels.Where(level => level != null);
            }
            else
            {
                string resourcesPath = string.IsNullOrWhiteSpace(jsonResourcesPath) ? "Levels" : jsonResourcesPath;
                sources = Resources
                    .LoadAll<TextAsset>(resourcesPath)
                    .Where(level => level != null)
                    .OrderBy(GetLevelSortKey)
                    .ThenBy(level => level.name);
            }

            foreach (var source in sources)
            {
                LevelData level = LevelImporter.ImportFromJson(source.text);
                if (level == null) continue;

                if (string.IsNullOrEmpty(level.name))
                {
                    level.name = source.name;
                }

                runtimeJsonLevels.Add(level);
            }

            if (runtimeJsonLevels.Count > 0)
            {
                Debug.Log($"Loaded {runtimeJsonLevels.Count} JSON levels.");
            }
        }

        private int GetLevelSortKey(TextAsset level)
        {
            if (level == null || string.IsNullOrEmpty(level.name)) return int.MaxValue;

            const string prefix = "Level_";
            if (level.name.StartsWith(prefix) &&
                int.TryParse(level.name.Substring(prefix.Length), out int levelNumber))
            {
                return levelNumber;
            }

            return int.MaxValue;
        }

        private int GetLevelCount()
        {
            if (runtimeJsonLevels.Count > 0) return runtimeJsonLevels.Count;
            if (allLevels != null && allLevels.Count > 0) return allLevels.Count;
            return levelJson != null ? 1 : 0;
        }

        private void ClampCurrentLevelIndex()
        {
            int levelCount = GetLevelCount();
            if (levelCount <= 0)
            {
                currentLevelIndex = 0;
                return;
            }

            if (currentLevelIndex < 0 || currentLevelIndex >= levelCount)
            {
                currentLevelIndex = 0;
            }
        }

        private void SaveCurrentLevelIndex()
        {
            PlayerPrefs.SetInt("CurrentLevelIndex", currentLevelIndex);
            PlayerPrefs.Save();
        }

        private void SaveNextLevelAfterCompletion()
        {
            int levelCount = GetLevelCount();
            if (levelCount <= 0) return;

            int nextLevelIndex = activeLevelIndex + 1;
            if (nextLevelIndex >= levelCount)
            {
                Debug.Log("All Levels Completed!");
                nextLevelIndex = 0;
            }

            currentLevelIndex = nextLevelIndex;
            SaveCurrentLevelIndex();
            UpdateMenuUI();
        }

        private LevelData GetLevelAt(int index)
        {
            if (index < 0) return null;

            if (runtimeJsonLevels.Count > index)
            {
                return runtimeJsonLevels[index];
            }

            if (allLevels != null && allLevels.Count > index)
            {
                return allLevels[index];
            }

            if (levelJson != null && index == 0)
            {
                return LevelImporter.ImportFromJson(levelJson.text);
            }

            return null;
        }

        private void LoadCurrentLevel()
        {
            ClampCurrentLevelIndex();
            LevelData level = GetLevelAt(currentLevelIndex);
            if (level == null)
            {
                Debug.LogError("No level data available to load.");
                return;
            }

            LoadLevel(level, currentLevelIndex);
        }

        public void StartGame()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            if (mainMenuPanel != null) mainMenuPanel.SetActive(false);

            LoadCurrentLevel();
        }

        void Update()
        {
            // Detect real-time updates from Inspector
            if (gridWidth != lastWidth || gridHeight != lastHeight)
            {
                OnGridSizeChanged();
            }

            // Logic-based Grid Clicking
            if (Input.GetMouseButtonDown(0))
            {
                HandleInput();
            }
        }

        private void HandleInput()
        {
            if (IsGameplayInputBlocked) return;
            if (SettingsUI.Instance != null && SettingsUI.Instance.IsOpen) return;

            Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0;
            
            Vector2Int gridPos = WorldToGridPos(mouseWorld);
            
            // Check if there is a snake at this grid coordinate
            if (gridContent.TryGetValue(gridPos, out Arrow arrow) && arrow != null)
            {
                arrow.HandleChildClick(mouseWorld);
            }
        }

        void OnValidate()
        {
            // This runs in the editor when you change values in the inspector
            if (Application.isPlaying) return;
            CalculateOffset();
        }

        private void OnGridSizeChanged()
        {
            lastWidth = gridWidth;
            lastHeight = gridHeight;
            CalculateOffset();
            
            // Re-create background (clean up old first)
            ClearGridVisuals();
            CreateGridBackground();
            
            Debug.Log($"Grid resized to {gridWidth}x{gridHeight}. Offset: {gridOffset}");
        }

        private void CalculateOffset()
        {
            // Calculate offset to move (0,0) from bottom-left to centered around (0,0)
            gridOffset = new Vector3(
                -(gridWidth - 1) * gridStep / 2f,
                -(gridHeight - 1) * gridStep / 2f,
                0
            );

            if (cameraController != null && Application.isPlaying)
            {
                cameraController.UpdateBounds(gridWidth, gridHeight, gridStep);
                cameraController.CenterCamera(snap: true, introZoom: true);
            }
        }

        public void ClearLevel()
        {
            // Find and destroy ALL arrows in the scene, including those that are moving
            Arrow[] allArrows = FindObjectsOfType<Arrow>();
            foreach (var arrow in allArrows)
            {
                if (arrow != null) Destroy(arrow.gameObject);
            }

            gridContent.Clear();
            modelSnakeIdByArrow.Clear();
            boardObjectViewById.Clear();
            boardObjectViewsById.Clear();
            boardObjectTextById.Clear();
            currentBoardModel = null;
            remainingArrows = 0;
            totalArrows = 0;
            isLevelCompleteTriggered = false;
            
            ClearGridVisuals();
            ClearBoardObjectVisuals();
        }

        private void ClearGridVisuals()
        {
            // Find and destroy the container
            Transform container = transform.Find("GridVisuals_Container");
            if (container != null) Destroy(container.gameObject);

            // Backup check for loose dots
            foreach (Transform child in transform) {
                if (child.name.Contains("GridSquare") || child.name.Contains("GridDot")) Destroy(child.gameObject);
            }

            activeGridDots.Clear();
            UpdateArrowCountUI();
        }

        private void ClearBoardObjectVisuals()
        {
            Transform container = transform.Find("BoardObjects_Container");
            if (container != null) Destroy(container.gameObject);

            foreach (GameObject view in activeBoardObjectViews)
            {
                if (view != null) Destroy(view);
            }

            activeBoardObjectViews.Clear();
            boardObjectViewById.Clear();
            boardObjectViewsById.Clear();
            boardObjectTextById.Clear();
        }

        private IEnumerator AnimateTopBar()
        {
            if (topBarGroup == null) yield break;

            RectTransform rt = topBarGroup.GetComponent<RectTransform>();
            Vector2 targetPos = rt.anchoredPosition;
            Vector2 startPos = targetPos + new Vector2(0, 100); // Start 100 units higher

            topBarGroup.alpha = 0;
            rt.anchoredPosition = startPos;

            float elapsed = 0;
            float duration = 0.6f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                t = Mathf.Sin(t * Mathf.PI * 0.5f); // Smooth ease out

                topBarGroup.alpha = t;
                rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                yield return null;
            }

            topBarGroup.alpha = 1;
            rt.anchoredPosition = targetPos;
        }

        private void UpdateArrowCountUI()
        {
            if (arrowCountText != null)
            {
                arrowCountText.text = $"{remainingArrows}/{totalArrows}";
                StopCoroutine("AnimateArrowCount");
                StartCoroutine(AnimateArrowCount());
            }
        }

        private IEnumerator AnimateArrowCount()
        {
            RectTransform rt = arrowCountText.GetComponent<RectTransform>();
            Vector3 originalScale = Vector3.one;
            Vector3 targetScale = originalScale * 1.3f;
            
            float elapsed = 0;
            float duration = 0.1f;
            
            // Pop out
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rt.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / duration);
                yield return null;
            }
            
            // Back to normal
            elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                rt.localScale = Vector3.Lerp(targetScale, originalScale, elapsed / duration);
                yield return null;
            }
            rt.localScale = originalScale;
        }

        public void LoadLevel(LevelData level)
        {
            int levelIndex = runtimeJsonLevels.IndexOf(level);
            if (levelIndex < 0 && allLevels != null)
            {
                levelIndex = allLevels.IndexOf(level);
            }

            LoadLevel(level, levelIndex);
        }

        private void LoadLevel(LevelData level, int levelIndex)
        {
            if (level == null)
            {
                Debug.LogError("Cannot load a null level.");
                return;
            }

            if (topBarGroup != null) StartCoroutine(AnimateTopBar());
            
            gridWidth = level.width;
            gridHeight = level.height;
            lastWidth = gridWidth;
            lastHeight = gridHeight;
            
            CalculateOffset();
            ClearLevel();

            BuildGameplayModelForLoadedLevel(level);

            // Calculate which dots should be visible
            HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();
            if (level.arrows != null)
            {
                foreach (var arrowData in level.arrows)
                {
                    if (arrowData.bodyPositions == null) continue;
                    foreach (var pos in arrowData.bodyPositions)
                    {
                        occupiedPositions.Add(pos);
                    }
                }
            }

            if (level.objects != null)
            {
                foreach (BoardObjectData boardObject in level.objects)
                {
                    if (boardObject == null || boardObject.cells == null) continue;
                    foreach (var pos in boardObject.cells)
                    {
                        occupiedPositions.Add(pos);
                    }
                }
            }

            CreateGridBackground(occupiedPositions);
            SpawnBoardObjects(level);

            if (levelIndex >= 0)
            {
                currentLevelIndex = levelIndex;
            }

            activeLevelIndex = currentLevelIndex;
            SaveCurrentLevelIndex();
            
            remainingArrows = 0;
            totalArrows = 0;
            isLevelCompleteTriggered = false;

            UpdatePowerupUI();
            StartCoroutine(SpawnArrowsRoutine(level));
        }

        private void BuildGameplayModelForLoadedLevel(LevelData level)
        {
            currentBoardModel = null;
            if (!validateGameplayModelOnLoad || level == null) return;

            LevelDefinition definition = LegacyLevelAdapter.FromLevelData(level, level.name);
            LevelValidationResult validation = LevelValidator.Validate(definition, level.name);
            if (!validation.IsValid)
            {
                Debug.LogWarning($"Gameplay model validation failed for {level.name}: {validation.ErrorCount} error(s).");
                return;
            }

            var boardResult = BoardFactory.Create(definition);
            if (!boardResult.IsSuccess)
            {
                Debug.LogWarning($"Gameplay board model could not be created for {level.name}: {boardResult.Error}");
                return;
            }

            currentBoardModel = boardResult.Value;

            int legalFirstMoves = 0;
            foreach (SnakeModel snake in currentBoardModel.Snakes)
            {
                if (MoveValidator.CanEscape(currentBoardModel, snake))
                {
                    legalFirstMoves++;
                }
            }

            int objectCount = currentBoardModel.Objects != null ? currentBoardModel.Objects.Count : 0;
            Debug.Log($"Gameplay model ready for {level.name}: {currentBoardModel.Snakes.Count} snakes, {objectCount} board object(s), {legalFirstMoves} legal first move(s).");
        }

        private IEnumerator SpawnArrowsRoutine(LevelData level)
        {
            IsAppearing = true;
            activeMovingSnakes = 0; // Reset counter for new level
            List<Arrow> spawnedArrows = new List<Arrow>();
            if (level.arrows == null)
            {
                IsAppearing = false;
                yield break;
            }

            for (int sourceIndex = 0; sourceIndex < level.arrows.Count; sourceIndex++)
            {
                var arrowData = level.arrows[sourceIndex];
                if (arrowData.bodyPositions == null || arrowData.bodyPositions.Count == 0) continue;

                remainingArrows++; // Only count valid arrows
                totalArrows++;
                UpdateArrowCountUI();
                Vector3 worldPos = GetWorldPos(arrowData.bodyPositions[0]);
                GameObject go = Instantiate(arrowPrefab, worldPos, Quaternion.identity, transform);
                Arrow arrow = go.GetComponent<Arrow>();
                
                // Initialize with unique sorting order and animation enabled
                arrow.Initialize(arrowData, this, 1000 - (sourceIndex * 50), true);
                
                BindArrowToGameplayModel(arrow, sourceIndex);
                RegisterArrowInGrid(arrow);
                spawnedArrows.Add(arrow);
                
                // Stagger the arrows slightly for a "making" effect
                yield return new WaitForSeconds(arrowSpawnDelay);
            }

            // Wait for all arrows to finish their construction animation
            bool anyAppearing = true;
            while (anyAppearing)
            {
                anyAppearing = false;
                foreach (var a in spawnedArrows) 
                {
                    if (a != null && a.IsAppearing) { anyAppearing = true; break; }
                }
                if (anyAppearing) yield return null;
            }

            // Once all arrows are constructed, play the satisfying zoom-in
            if (cameraController != null) cameraController.ZoomToPlayView();

            // Wait for the camera to settle
            yield return new WaitForSeconds(0.6f);
            IsAppearing = false;

            if (TutorialManager.Instance != null)
            {
                TutorialManager.Instance.OnLevelReady(currentLevelIndex);
            }
        }

        private void BindArrowToGameplayModel(Arrow arrow, int sourceIndex)
        {
            if (arrow == null || currentBoardModel == null) return;

            foreach (SnakeModel snake in currentBoardModel.Snakes)
            {
                if (snake.SourceIndex == sourceIndex)
                {
                    modelSnakeIdByArrow[arrow] = snake.Id;
                    return;
                }
            }
        }

        private void CreateGridBackground(HashSet<Vector2Int> filter = null)
        {
            if (gridSquarePrefab == null) return;

            GameObject gridContainer = new GameObject("GridVisuals_Container");
            gridContainer.transform.SetParent(transform);
            gridContainer.transform.localPosition = Vector3.zero;

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2Int gridPos = new Vector2Int(x, y);
                    
                    // If we have a filter and this position isn't in it, skip it
                    if (filter != null && !filter.Contains(gridPos)) continue;

                    Vector3 pos = GetWorldPos(gridPos);
                    pos.z = 1f; // Place dots slightly behind
                    GameObject dot = Instantiate(gridSquarePrefab, pos, Quaternion.identity, gridContainer.transform);
                    dot.name = "GridDot_" + x + "_" + y;
                    activeGridDots.Add(dot);
                }
            }
        }

        private void OnDrawGizmos()
        {
            CalculateOffset();
            Gizmos.color = new Color(1, 1, 1, 0.3f);
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Gizmos.DrawSphere(GetWorldPos(new Vector2Int(x, y)), 0.05f);
                }
            }

            Gizmos.color = Color.yellow;
            Vector3 size = new Vector3(gridWidth * gridStep, gridHeight * gridStep, 0.1f);
            Gizmos.DrawWireCube(Vector3.zero, size);
        }

        public Vector3 GetWorldPos(Vector2Int gridPos)
        {
            return new Vector3(gridPos.x * gridStep, gridPos.y * gridStep, 0) + gridOffset;
        }

        public Vector2Int WorldToGridPos(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - gridOffset;
            return new Vector2Int(
                Mathf.RoundToInt(localPos.x / gridStep),
                Mathf.RoundToInt(localPos.y / gridStep)
            );
        }

        public bool IsOutsideBounds(Vector3 position)
        {
            Vector2Int gridPos = WorldToGridPos(position);
            return gridPos.x < 0 || gridPos.x >= gridWidth || gridPos.y < 0 || gridPos.y >= gridHeight;
        }

        public void ClearArrowFromGrid(Arrow arrow)
        {
            // Unregister all its positions
            List<Vector2Int> keysToRemove = new List<Vector2Int>();
            foreach (var kvp in gridContent)
            {
                if (kvp.Value == arrow) keysToRemove.Add(kvp.Key);
            }
            foreach (var key in keysToRemove) gridContent.Remove(key);
        }

        public void RegisterArrowInGrid(Arrow arrow)
        {
            foreach (var pos in arrow.bodyPositions)
            {
                gridContent[pos] = arrow;
            }
        }

        private void SpawnBoardObjects(LevelData level)
        {
            ClearBoardObjectVisuals();
            if (level == null || level.objects == null || gridSquarePrefab == null) return;

            GameObject objectContainer = new GameObject("BoardObjects_Container");
            objectContainer.transform.SetParent(transform);
            objectContainer.transform.localPosition = Vector3.zero;

            int unnamedIndex = 0;
            foreach (BoardObjectData boardObject in level.objects)
            {
                if (boardObject == null || boardObject.cells == null || boardObject.cells.Count == 0) continue;

                string objectId = !string.IsNullOrEmpty(boardObject.id) ? boardObject.id : "object_" + unnamedIndex.ToString("000");
                unnamedIndex++;

                for (int i = 0; i < boardObject.cells.Count; i++)
                {
                    Vector3 pos = GetWorldPos(boardObject.cells[i]);
                    pos.z = 0.75f;

                    GameObject view = Instantiate(gridSquarePrefab, pos, Quaternion.identity, objectContainer.transform);
                    view.name = "BoardObject_" + objectId + "_" + boardObject.cells[i].x + "_" + boardObject.cells[i].y;
                    view.transform.localScale *= 1.18f;
                    ApplyBoardObjectColor(view, boardObject.objectType);
                    activeBoardObjectViews.Add(view);

                    if (!boardObjectViewById.ContainsKey(objectId))
                    {
                        boardObjectViewById[objectId] = view;
                    }

                    if (!boardObjectViewsById.TryGetValue(objectId, out List<GameObject> views))
                    {
                        views = new List<GameObject>();
                        boardObjectViewsById[objectId] = views;
                    }

                    views.Add(view);

                    if (i == 0 && boardObject.objectType == BoardObjectTypes.WayBlocker)
                    {
                        TextMesh lockText = CreateBoardObjectText(view.transform, GetBoardObjectLockCount(boardObject).ToString());
                        boardObjectTextById[objectId] = lockText;
                    }
                }
            }
        }

        private void ApplyBoardObjectColor(GameObject view, string objectType)
        {
            if (view == null) return;

            Color color;
            if (objectType == BoardObjectTypes.Barrier)
            {
                color = new Color(0.34f, 0.24f, 0.14f, 1f);
            }
            else if (objectType == BoardObjectTypes.WayBlocker)
            {
                color = new Color(0.42f, 0.18f, 0.68f, 1f);
            }
            else
            {
                color = new Color(0.18f, 0.19f, 0.22f, 1f);
            }

            SpriteRenderer spriteRenderer = view.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = color;
                return;
            }

            UnityEngine.UI.Image image = view.GetComponent<UnityEngine.UI.Image>();
            if (image != null)
            {
                image.color = color;
            }
        }

        private int GetBoardObjectLockCount(BoardObjectData boardObject)
        {
            if (boardObject == null) return 0;
            return boardObject.lockCount > 0 ? boardObject.lockCount : boardObject.lockTime;
        }

        private TextMesh CreateBoardObjectText(Transform parent, string text)
        {
            GameObject textObject = new GameObject("LockCountText");
            textObject.transform.SetParent(parent);
            textObject.transform.localPosition = new Vector3(0f, 0f, -0.25f);
            textObject.transform.localRotation = Quaternion.identity;
            textObject.transform.localScale = Vector3.one;

            TextMesh textMesh = textObject.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.anchor = TextAnchor.MiddleCenter;
            textMesh.alignment = TextAlignment.Center;
            textMesh.fontSize = 36;
            textMesh.characterSize = 0.08f;
            textMesh.color = Color.white;

            MeshRenderer renderer = textObject.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sortingOrder = 30;
            }

            return textMesh;
        }

        public Arrow GetArrowAtPos(Vector2Int gridPos)
        {
            if (gridContent.ContainsKey(gridPos))
            {
                return gridContent[gridPos];
            }

            return null;
        }

        public void TryMoveArrowWithGameplayModel(Arrow arrow, Vector3 clickWorldPos)
        {
            if (arrow == null || IsGameplayInputBlocked) return;

            SnakeMoveResult result = ValidateMoveForArrow(arrow);
            if (result.Status == SnakeMoveStatus.CanEscape)
            {
                SnakeMoveResult escapeResult = SnakeEscapeService.TryEscape(currentBoardModel, result.Snake);
                if (escapeResult.Status != SnakeMoveStatus.Escaped)
                {
                    Debug.LogError($"MoveValidator authority failed to execute escape for {result.Snake.Id}: {escapeResult.Message}");
                    return;
                }

                PlayClickParticle(clickWorldPos);
                Debug.Log($"MoveValidator authority: {result.Snake.Id} can escape.");
                arrow.PlayValidatedEscape(escapeResult);

                if (TutorialManager.Instance != null)
                {
                    TutorialManager.Instance.OnArrowTapped(arrow);
                }

                // If this is the last snake, trigger the victory animation immediately on click
                if (remainingArrows == 1 && !isLevelCompleteTriggered)
                {
                    isLevelCompleteTriggered = true;
                    SaveNextLevelAfterCompletion();
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySuccess();
                    StartCoroutine(VictoryWaveRoutine());
                    Debug.Log("Last snake clicked! Starting Victory Wave immediately.");
                }
                return;
            }

            if (result.Status == SnakeMoveStatus.Blocked)
            {
                PlayClickParticle(clickWorldPos);

                Arrow blockerArrow = GetArrowForSnake(result.Blocker);
                if (blockerArrow != null)
                {
                    blockerArrow.PunchScale(1.1f, 0.18f); // Physical feedback instead of color
                }
                else if (!string.IsNullOrEmpty(result.BlockerId))
                {
                    StartCoroutine(FlashBoardObjectTemporarily(result.BlockerId, Color.red, 0.5f));
                }

                string snakeId = result.Snake != null ? result.Snake.Id : "unknown";
                string blockerLabel = !string.IsNullOrEmpty(result.BlockerType) ? result.BlockerType : "blocker";
                Debug.Log($"MoveValidator authority: {snakeId} blocked by {blockerLabel} at {result.BlockedCell}.");
                arrow.PlayBlockedFeedback(result);
                return;
            }

            Debug.LogWarning($"MoveValidator authority refused move: {result.Message}");
        }

        private SnakeMoveResult ValidateMoveForArrow(Arrow arrow, bool forceMove = false)
        {
            if (arrow == null)
            {
                return SnakeMoveResult.Invalid(null, "Arrow is null.");
            }

            SnakeModel snake = GetSnakeModelForArrow(arrow);
            if (snake == null)
            {
                return SnakeMoveResult.Invalid(null, "No snake model is bound to this arrow.");
            }

            return MoveValidator.Validate(currentBoardModel, snake, forceMove);
        }

        private SnakeModel GetSnakeModelForArrow(Arrow arrow)
        {
            if (arrow == null || currentBoardModel == null) return null;

            if (modelSnakeIdByArrow.TryGetValue(arrow, out string snakeId))
            {
                return currentBoardModel.FindSnake(snakeId);
            }

            return null;
        }

        private Arrow GetArrowForSnake(SnakeModel snake)
        {
            if (snake == null) return null;

            foreach (KeyValuePair<Arrow, string> entry in modelSnakeIdByArrow)
            {
                if (entry.Key != null && entry.Value == snake.Id)
                {
                    return entry.Key;
                }
            }

            return null;
        }

        private IEnumerator FlashBoardObjectTemporarily(string objectId, Color flashColor, float duration)
        {
            if (string.IsNullOrEmpty(objectId)) yield break;
            if (!boardObjectViewsById.TryGetValue(objectId, out List<GameObject> views) || views == null || views.Count == 0) yield break;

            var spriteRenderers = new List<SpriteRenderer>();
            var spriteColors = new List<Color>();
            var images = new List<UnityEngine.UI.Image>();
            var imageColors = new List<Color>();

            foreach (GameObject view in views)
            {
                if (view == null) continue;
                SpriteRenderer spriteRenderer = view.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderers.Add(spriteRenderer);
                    spriteColors.Add(spriteRenderer.color);
                    spriteRenderer.color = flashColor;
                    continue;
                }

                UnityEngine.UI.Image image = view.GetComponent<UnityEngine.UI.Image>();
                if (image != null)
                {
                    images.Add(image);
                    imageColors.Add(image.color);
                    image.color = flashColor;
                }
            }

            if (spriteRenderers.Count == 0 && images.Count == 0) yield break;

            yield return new WaitForSeconds(duration);

            for (int i = 0; i < spriteRenderers.Count; i++)
            {
                if (spriteRenderers[i] != null)
                {
                    spriteRenderers[i].color = spriteColors[i];
                }
            }

            for (int i = 0; i < images.Count; i++)
            {
                if (images[i] != null)
                {
                    images[i].color = imageColors[i];
                }
            }
        }

        private bool MarkArrowEscapedInGameplayModel(Arrow arrow)
        {
            SnakeModel snake = GetSnakeModelForArrow(arrow);
            if (snake == null || currentBoardModel == null) return false;

            bool wasAlreadyEscaped = snake.HasEscaped;
            currentBoardModel.MarkEscaped(snake);
            modelSnakeIdByArrow.Remove(arrow);

            string action = wasAlreadyEscaped ? "confirmed escaped" : "marked escaped";
            Debug.Log($"Gameplay model {action}: {snake.Id}. Remaining model snakes: {currentBoardModel.RemainingSnakeCount()}.");
            return !wasAlreadyEscaped;
        }

        public void OnArrowEscaped()
        {
            OnArrowEscaped(null);
        }

        public void OnArrowEscaped(Arrow arrow)
        {
            bool countedModelEscape = false;
            if (arrow != null)
            {
                countedModelEscape = MarkArrowEscapedInGameplayModel(arrow);
            }

            if (countedModelEscape)
            {
                ResolveWayBlockersAfterSnakeEscape();
            }

            remainingArrows--;
            UpdateArrowCountUI();
            Debug.Log($"Arrow Escaped! {remainingArrows} arrows remaining.");

            if (remainingArrows <= 0 && !isLevelCompleteTriggered && activeMovingSnakes == 0)
            {
                isLevelCompleteTriggered = true;
                SaveNextLevelAfterCompletion();
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySuccess();
                StartCoroutine(VictoryWaveRoutine());
                Debug.Log("Board Clear! Waiting for animation to show Level Complete.");
            }
        }

        private IEnumerator VictoryWaveRoutine()
        {
            // Group dots by Y position and sort top to bottom
            var dotsByY = activeGridDots
                .FindAll(d => d != null)
                .GroupBy(d => Mathf.RoundToInt(d.transform.position.y * 100))
                .OrderByDescending(g => g.Key);

            float lastWaveTime = 0;
            foreach (var group in dotsByY)
            {
                foreach (var dot in group)
                {
                    StartCoroutine(FlashDot(dot));
                }
                lastWaveTime = 0.12f;
                yield return new WaitForSeconds(0.12f);
            }

            // Wait for the final dots to finish their pulse (pulse duration is 0.8s)
            yield return new WaitForSeconds(0.85f);
            
            ShowLevelComplete();
        }

        private void ShowLevelComplete()
        {
            if (levelCompletePanel != null)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayLevelComplete();
                levelCompletePanel.SetActive(true);
            }
        }

        private IEnumerator FlashDot(GameObject dot)
        {
            SpriteRenderer sr = dot.GetComponent<SpriteRenderer>();
            if (sr == null) yield break;

            Color originalColor = sr.color;
            Vector3 originalScale = dot.transform.localScale;
            Vector3 targetScale = originalScale * 1.8f; // Zoom in by 80%
            Color flashColor = new Color(0.2f, 0.6f, 1.0f); // Professional Blue
            
            float elapsed = 0;
            float duration = 0.8f; // Slower individual pulse (was 0.5)
            while (elapsed < duration)
            {
                if (dot == null) yield break;
                float t = elapsed / duration;

                // Color fade back
                sr.color = Color.Lerp(flashColor, originalColor, t);
                
                // Smooth scale pulse (Zoom in and out)
                float pulse = Mathf.Sin(t * Mathf.PI); 
                dot.transform.localScale = Vector3.Lerp(originalScale, targetScale, pulse);

                elapsed += Time.deltaTime;
                yield return null;
            }
            
            if (dot != null) 
            {
                sr.color = originalColor;
                dot.transform.localScale = originalScale;
            }
        }



        public void TakeLife()
        {
            if (isGameOver) return;

            lives--;
            
            // Update UI hearts
            if (hearts != null && lives >= 0 && lives < hearts.Length)
            {
                StartCoroutine(AnimateHeartLoss(hearts[lives]));
            }

            if (lives <= 0)
            {
                OnGameOver();
            }
            else
            {
                // Physical feedback when hit
                StartCoroutine(CameraShake(0.15f, 0.12f));
                StartCoroutine(DamageFlash(0.4f));
            }
        }

        private IEnumerator AnimateHeartLoss(GameObject heart)
        {
            if (heart == null) yield break;
            
            UnityEngine.UI.Image img = heart.GetComponent<UnityEngine.UI.Image>();
            RectTransform rt = heart.GetComponent<RectTransform>();
            if (rt == null) yield break;

            float duration = 0.4f;
            float elapsed = 0;
            Vector3 startScale = rt.localScale;
            Vector3 targetScale = startScale * 1.6f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Scale up
                rt.localScale = Vector3.Lerp(startScale, targetScale, t);
                
                // Fade out
                if (img != null)
                {
                    Color c = img.color;
                    c.a = Mathf.Lerp(1, 0, t);
                    img.color = c;
                }
                yield return null;
            }
            heart.SetActive(false);
        }

        private IEnumerator CameraShake(float duration, float magnitude)
        {
            Vector3 originalPos = Camera.main.transform.localPosition;
            float elapsed = 0.0f;
            while (elapsed < duration)
            {
                float x = Random.Range(-1f, 1f) * magnitude;
                float y = Random.Range(-1f, 1f) * magnitude;
                Camera.main.transform.localPosition = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);
                elapsed += Time.deltaTime;
                yield return null;
            }
            Camera.main.transform.localPosition = originalPos;
        }

        private IEnumerator DamageFlash(float duration)
        {
            if (damageOverlay == null) yield break;
            
            damageOverlay.gameObject.SetActive(true);
            damageOverlay.transform.localScale = Vector3.one;
            
            float elapsed = 0;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Scale up from 1.0 to 1.4
                damageOverlay.transform.localScale = Vector3.one * Mathf.Lerp(1.0f, 1.4f, t);
                
                // Fade out from 0.7 to 0
                Color c = damageOverlay.color;
                c.a = Mathf.Lerp(0.7f, 0, t);
                damageOverlay.color = c;
                
                yield return null;
            }
            damageOverlay.gameObject.SetActive(false);
        }

        public void PlayClickParticle(Vector3 worldPos)
        {
            if (clickParticlePrefab != null)
            {
                GameObject particle = Instantiate(clickParticlePrefab, worldPos, Quaternion.identity);
                // Automatically destroy the particle after 2 seconds to keep the hierarchy clean
                Destroy(particle, 2.0f);
            }
        }

        private void ResolveWayBlockersAfterSnakeEscape()
        {
            if (currentBoardModel == null) return;

            List<BoardObjectDefinition> changed = currentBoardModel.TickWayBlockersAfterSnakeEscape();
            if (changed == null || changed.Count == 0) return;

            foreach (BoardObjectDefinition boardObject in changed)
            {
                if (boardObject == null || boardObject.ObjectType != BoardObjectTypes.WayBlocker) continue;

                if (boardObject.LockCount > 0)
                {
                    UpdateWayBlockerText(boardObject.Id, boardObject.LockCount);
                    StartCoroutine(FlashBoardObjectTemporarily(boardObject.Id, new Color(1f, 0.85f, 0.2f, 1f), 0.18f));
                }
                else
                {
                    UpdateWayBlockerText(boardObject.Id, 0);
                    StartCoroutine(RemoveBoardObjectVisual(boardObject.Id));
                    Debug.Log("WayBlocker unlocked and removed: " + boardObject.Id);
                }
            }
        }

        private void UpdateWayBlockerText(string objectId, int lockCount)
        {
            if (string.IsNullOrEmpty(objectId)) return;
            if (!boardObjectTextById.TryGetValue(objectId, out TextMesh textMesh) || textMesh == null) return;

            textMesh.text = Mathf.Max(0, lockCount).ToString();
        }

        private IEnumerator RemoveBoardObjectVisual(string objectId)
        {
            if (string.IsNullOrEmpty(objectId)) yield break;
            if (!boardObjectViewsById.TryGetValue(objectId, out List<GameObject> views) || views == null) yield break;

            float duration = 0.18f;
            float elapsed = 0f;
            var startScales = new Dictionary<GameObject, Vector3>();
            foreach (GameObject view in views)
            {
                if (view != null)
                {
                    startScales[view] = view.transform.localScale;
                }
            }

            while (elapsed < duration)
            {
                float t = Mathf.Clamp01(elapsed / duration);
                float scale = 1f - t;
                foreach (KeyValuePair<GameObject, Vector3> entry in startScales)
                {
                    if (entry.Key != null)
                    {
                        entry.Key.transform.localScale = entry.Value * scale;
                    }
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            foreach (GameObject view in views)
            {
                if (view != null)
                {
                    activeBoardObjectViews.Remove(view);
                    Destroy(view);
                }
            }

            boardObjectViewById.Remove(objectId);
            boardObjectViewsById.Remove(objectId);
            boardObjectTextById.Remove(objectId);
        }

        public void PlayBlockedMoveFeedback(Vector3 worldPos)
        {
            PlayClickParticle(worldPos);
            PlayCameraShake(blockedShakeDuration, blockedShakeStrength);
            PlayHaptic(35, 140);
        }

        public void PlayEscapeMoveFeedback(Vector3 worldPos)
        {
            PlayClickParticle(worldPos);
            PlayCameraShake(escapeShakeDuration, escapeShakeStrength);
            PlayHaptic(18, 80);
        }

        private void PlayCameraShake(float duration, float strength)
        {
            if (!enableCameraShake || cameraController == null) return;
            cameraController.Shake(duration, strength);
        }

        private void PlayHaptic(long durationMs, int amplitude)
        {
            if (!enableHaptics || !Application.isMobilePlatform) return;

#if UNITY_ANDROID && !UNITY_EDITOR
            try
            {
                using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
                using (AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
                using (AndroidJavaObject vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator"))
                using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
                {
                    int sdkInt = version.GetStatic<int>("SDK_INT");
                    if (sdkInt >= 26)
                    {
                        using (AndroidJavaClass vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect"))
                        using (AndroidJavaObject effect = vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", durationMs, amplitude))
                        {
                            vibrator.Call("vibrate", effect);
                        }
                    }
                    else
                    {
                        vibrator.Call("vibrate", durationMs);
                    }
                }
            }
            catch
            {
                Handheld.Vibrate();
            }
#elif UNITY_IOS && !UNITY_EDITOR
            Handheld.Vibrate();
#endif
        }

        private void OnGameOver()
        {
            isGameOver = true;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayGameOver();
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }
            Debug.Log("Game Over!");
        }

        public void RestartLevel()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            StopAllCoroutines(); // Kill any pending "Level Complete" delays
            isGameOver = false;
            lives = 3;
            // Reset hearts
            if (hearts != null)
            {
                foreach (var h in hearts)
                {
                    h.SetActive(true);
                    h.transform.localScale = Vector3.one;
                    var img = h.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) { Color c = img.color; c.a = 1f; img.color = c; }
                }
            }

            if (gameOverPanel != null) gameOverPanel.SetActive(false);
            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            
            LoadCurrentLevel();
        }

        private void UpdatePowerupUI()
        {
            // Note: activeLevelIndex is 0-based, so Level 6 is index 5.
            // But we use the "unlockLevel" from the inspector (e.g., 6) for clarity.
            int currentLevelNum = activeLevelIndex + 1;

            UpdateSinglePowerup(hintPowerup, currentLevelNum);
            UpdateSinglePowerup(erasePowerup, currentLevelNum);
            UpdateSinglePowerup(forceEscapePowerup, currentLevelNum);
        }

        private void UpdateSinglePowerup(PowerupUI powerup, int currentLevel)
        {
            if (powerup.button == null) return;

            bool isUnlocked = currentLevel >= powerup.unlockLevel;

            powerup.button.interactable = isUnlocked;
            
            if (powerup.mainIcon != null) powerup.mainIcon.SetActive(isUnlocked);
            if (powerup.lockIcon != null) powerup.lockIcon.SetActive(!isUnlocked);
            if (powerup.levelText != null)
            {
                powerup.levelText.gameObject.SetActive(!isUnlocked);
                powerup.levelText.text = "Lv." + powerup.unlockLevel;
            }
        }

        public void NextLevel()
        {
            if (SoundManager.Instance != null) SoundManager.Instance.PlayClick();
            
            isGameOver = false;
            lives = 3;
            if (hearts != null)
            {
                foreach (var h in hearts)
                {
                    h.SetActive(true);
                    h.transform.localScale = Vector3.one;
                    var img = h.GetComponent<UnityEngine.UI.Image>();
                    if (img != null) { Color c = img.color; c.a = 1f; img.color = c; }
                }
            }

            if (levelCompletePanel != null) levelCompletePanel.SetActive(false);
            LoadCurrentLevel();
        }

        public void UseHint()
        {
            if (IsBusy || isGameOver) return;

            Arrow solvable = FindSolvableArrow();
            if (solvable != null)
            {
                if (SoundManager.Instance != null) SoundManager.Instance.PlayHint();
                // Highlight the arrow in Light Blue (Hint Color)
                Debug.Log("Hint: Showing solvable arrow at " + solvable.bodyPositions[0]);
            }
            else
            {
                Debug.Log("No arrow can escape right now.");
            }
        }

        private Arrow FindSolvableArrow()
        {
            Arrow[] allArrows = FindObjectsOfType<Arrow>();
            foreach (var arrow in allArrows)
            {
                SnakeMoveResult result = ValidateMoveForArrow(arrow);
                if (result.Status == SnakeMoveStatus.CanEscape) return arrow;
            }
            return null;
        }

        public void ActivateEraseMode()
        {
            if (IsBusy || isGameOver) return;
            if (SoundManager.Instance != null) SoundManager.Instance.PlayErase();
            IsEraseModeActive = true;
            ToggleErasePanel(true);
            Debug.Log("Erase Mode Active: Select an arrow to delete.");
        }

        public void DeactivateEraseMode()
        {
            IsEraseModeActive = false;
            ToggleErasePanel(false);
        }

        public void ToggleErasePanel(bool show)
        {
            if (erasePanelGroup == null) return;
            StopCoroutine("AnimateErasePanel");
            StartCoroutine(AnimateErasePanel(show));
        }

        private IEnumerator AnimateErasePanel(bool show)
        {
            float duration = 0.3f;
            float elapsed = 0;
            
            RectTransform rt = erasePanelGroup.GetComponent<RectTransform>();
            float startAlpha = erasePanelGroup.alpha;
            float targetAlpha = show ? 1 : 0;
            Vector3 targetScale = show ? Vector3.one : Vector3.one * 0.8f;

            if (show) erasePanelGroup.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = Mathf.Sin(t * Mathf.PI * 0.5f);
                erasePanelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, ease);
                rt.localScale = Vector3.Lerp(rt.localScale, targetScale, ease);
                yield return null;
            }

            erasePanelGroup.alpha = targetAlpha;
            rt.localScale = targetScale;
            if (!show) erasePanelGroup.gameObject.SetActive(false);
        }


        public void ClearArrowsInDirection(MoveDirection dir)
        {
            if (IsBusy || isGameOver) return;
            
            Arrow[] allArrows = FindObjectsOfType<Arrow>();
            bool foundAny = false;
            foreach (var arrow in allArrows)
            {
                if (arrow != null && arrow.direction == dir)
                {
                    arrow.ForceEscape();
                    foundAny = true;
                }
            }

            if (foundAny && SoundManager.Instance != null) SoundManager.Instance.PlayDirectional();
        }

        // Public helper methods for UI button links
        public void ClearArrowsUp() { ClearArrowsInDirection(MoveDirection.Up); ToggleDirectionalPanel(false); }
        public void ClearArrowsDown() { ClearArrowsInDirection(MoveDirection.Down); ToggleDirectionalPanel(false); }
        public void ClearArrowsLeft() { ClearArrowsInDirection(MoveDirection.Left); ToggleDirectionalPanel(false); }
        public void ClearArrowsRight() { ClearArrowsInDirection(MoveDirection.Right); ToggleDirectionalPanel(false); }

        public void SetBusy(bool busy)
        {
            if (busy) activeMovingSnakes++;
            else activeMovingSnakes = Mathf.Max(0, activeMovingSnakes - 1);
            
            // Check for level completion when all snakes finish moving
            if (activeMovingSnakes == 0 && remainingArrows <= 0 && !isLevelCompleteTriggered)
            {
                isLevelCompleteTriggered = true;
                SaveNextLevelAfterCompletion();
                if (SoundManager.Instance != null) SoundManager.Instance.PlaySuccess();
                StartCoroutine(VictoryWaveRoutine());
            }
        }

        public void ToggleDirectionalPanel(bool show)
        {
            if (show && (IsBusy || isGameOver)) return;
            if (directionalPanelGroup == null) return;

            StopCoroutine("AnimateDirectionalPanel");
            StartCoroutine(AnimateDirectionalPanel(show));
            
            if (show && SoundManager.Instance != null) SoundManager.Instance.PlayClick();
        }

        private IEnumerator AnimateDirectionalPanel(bool show)
        {
            float duration = 0.3f;
            float elapsed = 0;
            
            RectTransform rt = directionalPanelGroup.GetComponent<RectTransform>();
            float startAlpha = directionalPanelGroup.alpha;
            float targetAlpha = show ? 1 : 0;
            
            Vector3 startScale = rt.localScale;
            Vector3 targetScale = show ? Vector3.one : Vector3.one * 0.8f;

            if (show) directionalPanelGroup.gameObject.SetActive(true);

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float ease = Mathf.Sin(t * Mathf.PI * 0.5f);

                directionalPanelGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, ease);
                rt.localScale = Vector3.Lerp(startScale, targetScale, ease);
                yield return null;
            }

            directionalPanelGroup.alpha = targetAlpha;
            rt.localScale = targetScale;
            
            if (!show) directionalPanelGroup.gameObject.SetActive(false);
        }
    }
}
