using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using ArrowMaze.Data;
using ArrowMaze.Gameplay.Snakes;

namespace ArrowMaze
{
    public class Arrow : MonoBehaviour
    {
        public MoveDirection direction;
        public List<Vector2Int> bodyPositions;
        public float maxSpeed = 10f;
        public float startSpeed = 0.1f;
        public float acceleration = 5f;
        public float returnSpeed = 25f;

        [Header("Visuals")]
        public GameObject bodySegmentPrefab; 
        public GameObject tailSegmentPrefab;
        public GameObject eraserPrefab;
        
        [Header("Appear Animation")]
        public float headAppearDuration = 0.12f;
        public float segmentSpawnDelay = 0.015f;
        public float segmentAppearDuration = 0.08f;
        public Vector3 headTargetScale = new Vector3(0.3f, 0.3f, 0.3f);
        public Vector3 bodyTargetScale = new Vector3(0.2f, 0.2f, 0.2f);

        [Header("Movement Feel")]
        public float blockedBumpCells = 0.35f;
        public float blockedBumpDuration = 0.07f;
        public float blockedHoldDuration = 0.05f;
        public float escapeScalePunch = 1.08f;
        public float escapeScalePunchDuration = 0.08f;
        
        private bool isMoving = false;
        private GameManager gameManager;
        private List<GameObject> activeVisuals = new List<GameObject>();
        private float segmentSpacing = 0.333f; // 3 pieces per grid unit
        private float pathLength;
        private bool hasFailed = false;
        public bool IsAppearing { get; private set; } = false;
        public bool IsErasing { get; private set; } = false;
        private int lastClickFrame = -1;
        private Color baseColor;
        private bool colorInitialized = false;
        private SpriteRenderer headSr;
        private GameManager.SnakeVisualSet assignedVisualSet;
        private int baseSortingOrder;

        public void Initialize(ArrowData data, GameManager manager, int sortingOrder, bool animate = false)
        {
            this.direction = data.direction;
            this.bodyPositions = new List<Vector2Int>(data.bodyPositions);
            this.gameManager = manager;
            this.baseSortingOrder = sortingOrder;
            this.pathLength = (bodyPositions.Count - 1) * gameManager.gridStep;
            this.hasFailed = false; // Reset failure tracking on level start
            
            if (headSr == null) headSr = GetComponent<SpriteRenderer>();
            if (headSr != null) headSr.sortingOrder = baseSortingOrder;

            if (!colorInitialized)
            {
                assignedVisualSet = gameManager.GetRandomVisualSet();
                baseColor = Color.white; // No tint by default since we use sprites
                colorInitialized = true;
            }

            if (assignedVisualSet != null && headSr != null)
            {
                headSr.sprite = assignedVisualSet.headSprite;
            }

            UpdateVisualRotation();
            SetColor(baseColor); 

            if (animate) StartCoroutine(AppearRoutine());
            else SetupInitialVisuals();
        }

        public IEnumerator AppearRoutine()
        {
            IsAppearing = true;
            transform.localScale = Vector3.zero;
            float elapsed = 0;
            while (elapsed < headAppearDuration)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(Vector3.zero, headTargetScale, elapsed / headAppearDuration);
                yield return null;
            }
            transform.localScale = headTargetScale;

            foreach (var v in activeVisuals) if (v != null) Destroy(v);
            activeVisuals.Clear();

            int totalPieces = Mathf.RoundToInt(pathLength / (segmentSpacing * gameManager.gridStep));
            float pieceStep = segmentSpacing * gameManager.gridStep;
            List<Vector2Int> gridPath = new List<Vector2Int>();
            for (int i = bodyPositions.Count - 1; i >= 0; i--) gridPath.Add(bodyPositions[i]);

            for (int i = 1; i <= totalPieces; i++)
            {
                float d = pathLength - (i * pieceStep);
                Vector3 pos = GetPositionOnGridPath(gridPath, d);
                
                GameObject prefabToSpawn = (i == totalPieces && tailSegmentPrefab != null) ? tailSegmentPrefab : bodySegmentPrefab;
                GameObject seg = Instantiate(prefabToSpawn, pos, Quaternion.identity, transform);
                
                SpriteRenderer sr = seg.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (assignedVisualSet != null) sr.sprite = (i == totalPieces) ? assignedVisualSet.tailSprite : assignedVisualSet.bodySprite;
                    sr.color = baseColor;
                    sr.sortingOrder = baseSortingOrder - i;
                }
                
                seg.AddComponent<ArrowSegment>().Initialize(this);
                
                // Use backward-look for corner alignment (stay like previous)
                Vector3 prevPos = GetPositionOnGridPath(gridPath, d - 0.05f);
                Vector3 moveDir = (pos - prevPos).normalized;
                float segAngle = 0;
                if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) segAngle = moveDir.x > 0 ? -90f : 90f;
                else segAngle = moveDir.y > 0 ? 0f : 180f;

                seg.transform.rotation = Quaternion.Euler(0, 0, segAngle + 180f);
                
                seg.transform.localScale = Vector3.zero;
                activeVisuals.Add(seg);
                StartCoroutine(TweenScale(seg.transform, bodyTargetScale, segmentAppearDuration));

                yield return new WaitForSeconds(segmentSpawnDelay);
            }
            IsAppearing = false;
        }

        private IEnumerator TweenScale(Transform t, Vector3 target, float time)
        {
            float e = 0;
            Vector3 start = t.localScale;
            while (e < time)
            {
                if (t == null) yield break;
                e += Time.deltaTime;
                t.localScale = Vector3.Lerp(start, target, e / time);
                yield return null;
            }
            if (t != null) t.localScale = target;
        }

        private void SetupInitialVisuals()
        {
            transform.localScale = headTargetScale;
            foreach (var v in activeVisuals) if (v != null) Destroy(v);
            activeVisuals.Clear();

            int totalPieces = Mathf.RoundToInt(pathLength / (segmentSpacing * gameManager.gridStep));
            float pieceStep = segmentSpacing * gameManager.gridStep;
            List<Vector2Int> gridPath = new List<Vector2Int>();
            for (int i = bodyPositions.Count - 1; i >= 0; i--) gridPath.Add(bodyPositions[i]);

            for (int i = 1; i <= totalPieces; i++)
            {
                float d = pathLength - (i * pieceStep);
                Vector3 pos = GetPositionOnGridPath(gridPath, d);
                
                GameObject prefabToSpawn = (i == totalPieces && tailSegmentPrefab != null) ? tailSegmentPrefab : bodySegmentPrefab;
                GameObject seg = Instantiate(prefabToSpawn, pos, Quaternion.identity, transform);
                
                SpriteRenderer sr = seg.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    if (assignedVisualSet != null) sr.sprite = (i == totalPieces) ? assignedVisualSet.tailSprite : assignedVisualSet.bodySprite;
                    sr.color = baseColor;
                    sr.sortingOrder = baseSortingOrder - i;
                }
                
                seg.AddComponent<ArrowSegment>().Initialize(this);
                
                // Use backward-look for corner alignment (stay like previous)
                Vector3 prevPos = GetPositionOnGridPath(gridPath, d - 0.05f);
                Vector3 moveDir = (pos - prevPos).normalized;
                float segAngle = 0;
                if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) segAngle = moveDir.x > 0 ? -90f : 90f;
                else segAngle = moveDir.y > 0 ? 0f : 180f;

                seg.transform.rotation = Quaternion.Euler(0, 0, segAngle + 180f);
                seg.transform.localScale = bodyTargetScale;
                activeVisuals.Add(seg);
            }
        }

        public void HandleChildClick(Vector3 clickWorldPos)
        {
            if (gameManager == null || gameManager.IsGameplayInputBlocked) return;
            if (isMoving || IsErasing || IsAppearing || Time.frameCount == lastClickFrame) return;
            if (SettingsUI.Instance != null && SettingsUI.Instance.IsOpen) return;
            
            lastClickFrame = Time.frameCount;

            if (gameManager.IsEraseModeActive)
            {
                gameManager.DeactivateEraseMode();
                StartCoroutine(EraseWithVisualRoutine());
                return;
            }

            if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
            {
                gameManager.ClearArrowFromGrid(this);
                gameManager.OnArrowEscaped(this);
                Destroy(gameObject);
                return;
            }

            gameManager.TryMoveArrowWithGameplayModel(this, clickWorldPos);
        }


        private List<Vector2Int> BuildVisualGridPath(int extensionCells = 60)
        {
            List<Vector2Int> gridPath = new List<Vector2Int>();
            for (int i = bodyPositions.Count - 1; i >= 0; i--) gridPath.Add(bodyPositions[i]);
            Vector2Int step = GetDirectionStep(direction);
            for (int i = 1; i < extensionCells; i++) gridPath.Add(gridPath[gridPath.Count - 1] + step);
            return gridPath;
        }

        public void PlayValidatedEscape(SnakeMoveResult result)
        {
            if (isMoving || IsErasing || IsAppearing) return;
            StartCoroutine(MoveRoutine());
        }

        public void PlayBlockedFeedback(SnakeMoveResult result)
        {
            if (isMoving || IsErasing || IsAppearing) return;
            isMoving = true;
            gameManager.SetBusy(true);
            StartCoroutine(FlashRedAndReturn(BuildVisualGridPath(), CalculateBlockedFeedbackDistance(result)));
        }

        private IEnumerator MoveRoutine()
        {
            isMoving = true;
            gameManager.SetBusy(true);
            bool isEscaping = false;
            List<Vector2Int> gridPath = BuildVisualGridPath();
            float totalDist = pathLength;
            float currentSpeed = startSpeed; 
            gameManager.ClearArrowFromGrid(this);

            while (true)
            {
                if (currentSpeed < maxSpeed) currentSpeed = Mathf.Min(maxSpeed, currentSpeed + acceleration * Time.deltaTime);
                float moveStep = currentSpeed * Time.deltaTime;

                if (!isEscaping && gameManager.IsOutsideBounds(GetPositionOnGridPath(gridPath, totalDist)))
                {
                    isEscaping = true;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySuccess();
                    gameManager.PlayEscapeMoveFeedback(transform.position);
                    gameManager.OnArrowEscaped(this);
                }

                totalDist += moveStep;
                transform.position = GetPositionOnGridPath(gridPath, totalDist);
                UpdateHeadRotation(gridPath, totalDist);
                UpdateBodySegments(gridPath, totalDist);

                float tailDist = totalDist - pathLength;
                if (isEscaping && gameManager.IsOutsideBounds(GetPositionOnGridPath(gridPath, tailDist)))
                {
                    if (Vector3.Distance(transform.position, Vector3.zero) > (gameManager.gridWidth + 20)) 
                    {
                        gameManager.SetBusy(false);
                        Destroy(gameObject);
                        yield break;
                    }
                }
                yield return null;
            }
        }

        private IEnumerator FlashRedAndReturn(List<Vector2Int> gridPath, float startDist)
        {
            if (!hasFailed) { hasFailed = true; gameManager.TakeLife(); }
            if (SoundManager.Instance != null) SoundManager.Instance.PlayCollision();
            gameManager.PlayBlockedMoveFeedback(transform.position);

            float impactDist = Mathf.Max(pathLength, startDist);
            float targetDist = pathLength;
            float elapsed = 0f;

            while (elapsed < blockedBumpDuration)
            {
                elapsed += Time.deltaTime;
                float currentDist = Mathf.Lerp(targetDist, impactDist, Mathf.Sin(Mathf.Clamp01(elapsed / blockedBumpDuration) * Mathf.PI * 0.5f));
                transform.position = GetPositionOnGridPath(gridPath, currentDist);
                UpdateHeadRotation(gridPath, currentDist);
                UpdateBodySegments(gridPath, currentDist);
                yield return null;
            }

            if (blockedHoldDuration > 0f) yield return new WaitForSeconds(blockedHoldDuration);

            float returnDuration = Mathf.Max(0.05f, (impactDist - targetDist) / Mathf.Max(returnSpeed, 0.05f));
            elapsed = 0f;
            while (elapsed < returnDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / returnDuration);
                float currentDist = Mathf.Lerp(impactDist, targetDist, 1f - ((1f - t) * (1f - t)));
                transform.position = GetPositionOnGridPath(gridPath, currentDist);
                UpdateHeadRotation(gridPath, currentDist);
                UpdateBodySegments(gridPath, currentDist);
                yield return null;
            }

            UpdateVisualRotation();
            UpdateBodySegments(gridPath, targetDist);
            isMoving = false;
            gameManager.RegisterArrowInGrid(this);
            gameManager.SetBusy(false);
        }

        private Color GetNormalColor() { return colorInitialized ? baseColor : new Color(63f/255f, 63f/255f, 63f/255f); }

        private void SetColor(Color c)
        {
            if (headSr != null) headSr.color = c;
            for (int i = 0; i < activeVisuals.Count; i++)
            {
                if (activeVisuals[i] != null)
                {
                    SpriteRenderer sr = activeVisuals[i].GetComponent<SpriteRenderer>();
                    if (sr != null) sr.color = c;
                }
            }
        }

        private void UpdateBodySegments(List<Vector2Int> gridPath, float totalDist)
        {
            float pieceStep = segmentSpacing * gameManager.gridStep;
            for (int i = 0; i < activeVisuals.Count; i++)
            {
                if (activeVisuals[i] == null) continue;
                float d = totalDist - ((i + 1) * pieceStep);
                float snappedD = Mathf.Round(d / pieceStep) * pieceStep;
                Vector3 segPos = GetPositionOnGridPath(gridPath, snappedD);
                activeVisuals[i].transform.position = segPos;
                
                // Use backward-look for corner alignment (stay like previous)
                Vector3 prevPos = GetPositionOnGridPath(gridPath, snappedD - 0.05f);
                Vector3 moveDir = (segPos - prevPos).normalized;
                if (moveDir.magnitude > 0.05f)
                {
                    float segAngle = 0;
                    if (Mathf.Abs(moveDir.x) > Mathf.Abs(moveDir.y)) segAngle = moveDir.x > 0 ? -90f : 90f;
                    else segAngle = moveDir.y > 0 ? 0f : 180f;
                    activeVisuals[i].transform.rotation = Quaternion.Euler(0, 0, segAngle + 180f);
                }
                SpriteRenderer sr = activeVisuals[i].GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = (snappedD >= 0);
            }
        }

        private void UpdateHeadRotation(List<Vector2Int> path, float distance)
        {
            Vector3 current = GetPositionOnGridPath(path, distance);
            // Use backward-look for head to maintain previous orientation at corners
            Vector3 prev = GetPositionOnGridPath(path, distance - 0.05f);
            Vector3 dir = (current - prev).normalized;
            if (dir.magnitude > 0.1f)
            {
                float angle = 0;
                if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y)) angle = dir.x > 0 ? -90 : 90;
                else angle = dir.y > 0 ? 0 : 180;
                transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
            }
        }

        private Vector3 GetPositionOnGridPath(List<Vector2Int> path, float distance)
        {
            float indexF = distance / gameManager.gridStep;
            int i = Mathf.FloorToInt(indexF);
            if (i < 0) return gameManager.GetWorldPos(path[0]);
            if (i >= path.Count - 1) return gameManager.GetWorldPos(path[path.Count - 1]);
            return Vector3.Lerp(gameManager.GetWorldPos(path[i]), gameManager.GetWorldPos(path[i + 1]), indexF - i);
        }

        private float CalculateBlockedFeedbackDistance(SnakeMoveResult result)
        {
            int routeIndex = 0;
            if (result != null && result.Route != null)
            {
                for (int i = 0; i < result.Route.Count; i++)
                {
                    if (result.Route[i].X == result.BlockedCell.X && result.Route[i].Y == result.BlockedCell.Y) { routeIndex = i; break; }
                }
            }
            return pathLength + ((Mathf.Max(0.1f, routeIndex + blockedBumpCells)) * gameManager.gridStep);
        }

        private IEnumerator PulseHeadScale(float multiplier, float duration)
        {
            if (duration <= 0f || multiplier <= 1f) yield break;
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * multiplier;
            float halfDuration = duration * 0.5f;
            float elapsed = 0f;
            while (elapsed < halfDuration) { elapsed += Time.deltaTime; transform.localScale = Vector3.Lerp(originalScale, targetScale, Mathf.Sin(Mathf.Clamp01(elapsed / halfDuration) * Mathf.PI * 0.5f)); yield return null; }
            elapsed = 0f;
            while (elapsed < halfDuration) { elapsed += Time.deltaTime; transform.localScale = Vector3.Lerp(targetScale, originalScale, 1f - ((1f - Mathf.Clamp01(elapsed / halfDuration)) * (1f - Mathf.Clamp01(elapsed / halfDuration)))); yield return null; }
            transform.localScale = originalScale;
        }

        private void UpdateVisualRotation()
        {
            float angle = 0;
            switch (direction) { case MoveDirection.Up: angle = 0; break; case MoveDirection.Down: angle = 180; break; case MoveDirection.Left: angle = 90; break; case MoveDirection.Right: angle = -90; break; }
            transform.rotation = Quaternion.Euler(0, 0, angle + 180f);
        }

        public IEnumerator EraseWithVisualRoutine()
        {
            IsErasing = true;
            if (gameManager != null) gameManager.SetBusy(true);
            gameManager.ClearArrowFromGrid(this);
            gameManager.OnArrowEscaped(this);
            if (eraserPrefab != null)
            {
                Vector3 startPos = activeVisuals.Count > 0 ? activeVisuals[activeVisuals.Count - 1].transform.position : transform.position;
                GameObject eraser = Instantiate(eraserPrefab, startPos, Quaternion.identity);
                for (int i = activeVisuals.Count - 1; i >= 0; i--)
                {
                    if (activeVisuals[i] == null) continue;
                    Vector3 segmentPos = activeVisuals[i].transform.position;
                    float elapsed = 0;
                    float stepDuration = 0.035f;
                    Vector3 initialEraserPos = eraser.transform.position;
                    while (elapsed < stepDuration) { elapsed += Time.deltaTime; eraser.transform.position = Vector3.Lerp(initialEraserPos, segmentPos, elapsed / stepDuration); yield return null; }
                    activeVisuals[i].SetActive(false);
                }
                float finalElapsed = 0; float finalDur = 0.05f; Vector3 postSegmentsPos = eraser.transform.position;
                while (finalElapsed < finalDur) { finalElapsed += Time.deltaTime; eraser.transform.position = Vector3.Lerp(postSegmentsPos, transform.position, finalElapsed / finalDur); yield return null; }
                GetComponent<SpriteRenderer>().enabled = false;
                yield return new WaitForSeconds(0.1f); Destroy(eraser);
            }
            yield return new WaitForSeconds(0.2f);
            if (gameManager != null) gameManager.SetBusy(false);
            Destroy(gameObject);
        }

        public void PunchScale(float multiplier, float duration)
        {
            StartCoroutine(PunchScaleRoutine(multiplier, duration));
        }

        private IEnumerator PunchScaleRoutine(float multiplier, float duration)
        {
            Vector3 originalScale = transform.localScale;
            Vector3 targetScale = originalScale * multiplier;
            
            float elapsed = 0;
            // Ease Out (Grow)
            while (elapsed < duration * 0.3f)
            {
                elapsed += Time.deltaTime;
                transform.localScale = Vector3.Lerp(originalScale, targetScale, elapsed / (duration * 0.3f));
                yield return null;
            }
            
            elapsed = 0;
            // Ease In (Back to normal with a bit of bounce feel)
            while (elapsed < duration * 0.7f)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / (duration * 0.7f);
                // Use a smooth overshoot/bounce curve
                transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
                yield return null;
            }
            transform.localScale = originalScale;
        }

        public IEnumerator FlashColorTemporarily(Color flashColor, float duration)
        {
            Color original = GetNormalColor();
            SetColor(flashColor);
            yield return new WaitForSeconds(duration);
            SetColor(original);
        }

        public void ForceEscape()
        {
            if (isMoving || IsErasing || IsAppearing) return;
            StartCoroutine(ForceMoveRoutine());
        }

        private IEnumerator ForceMoveRoutine()
        {
            isMoving = true;
            gameManager.SetBusy(true);
            bool isEscaping = false; 
            List<Vector2Int> gridPath = BuildVisualGridPath();
            float totalDist = pathLength;
            float currentSpeed = startSpeed; 
            gameManager.ClearArrowFromGrid(this);

            while (true)
            {
                if (currentSpeed < maxSpeed) currentSpeed += acceleration * Time.deltaTime;
                float moveStep = currentSpeed * Time.deltaTime;

                if (!isEscaping && gameManager.IsOutsideBounds(GetPositionOnGridPath(gridPath, totalDist)))
                {
                    isEscaping = true;
                    if (SoundManager.Instance != null) SoundManager.Instance.PlaySuccess();
                    gameManager.PlayEscapeMoveFeedback(transform.position);
                    gameManager.OnArrowEscaped(this);
                }

                totalDist += moveStep;
                transform.position = GetPositionOnGridPath(gridPath, totalDist);
                UpdateHeadRotation(gridPath, totalDist);
                UpdateBodySegments(gridPath, totalDist);

                float tailDist = totalDist - pathLength;
                if (isEscaping && gameManager.IsOutsideBounds(GetPositionOnGridPath(gridPath, tailDist)))
                {
                    if (Vector3.Distance(transform.position, Vector3.zero) > (gameManager.gridWidth + 20))
                    {
                        gameManager.SetBusy(false);
                        Destroy(gameObject);
                        yield break;
                    }
                }
                yield return null;
            }
        }

        private Vector2Int GetDirectionStep(MoveDirection dir)
        {
            switch (dir) { case MoveDirection.Up: return Vector2Int.up; case MoveDirection.Down: return Vector2Int.down; case MoveDirection.Left: return Vector2Int.left; case MoveDirection.Right: return Vector2Int.right; default: return Vector2Int.zero; }
        }
    }
}
