using UnityEngine;
using System.Collections.Generic;
using ArrowMaze.Data;
using ArrowMaze.Gameplay.Snakes;

namespace ArrowMaze
{
    public class LevelMaker : MonoBehaviour
    {
        public GameManager gameManager;
        public LineRenderer linePreview;
        public bool isEditMode = false;

        private Vector2Int? dragStartGrid = null;
        private List<Vector2Int> currentDragPath = new List<Vector2Int>();
        private Camera mainCamera;

        public void SetEditMode(bool active)
        {
            isEditMode = active;
            if (!active && linePreview != null) linePreview.positionCount = 0;
            Debug.Log("Level Maker Mode: " + (active ? "ON" : "OFF"));
        }

        public void AddArrowAt(ArrowData data)
        {
            SpawnArrow(data);
        }

        private void SpawnArrow(ArrowData data)
        {
            Vector3 worldPos = gameManager.GetWorldPos(data.bodyPositions[0]);
            GameObject go = Instantiate(gameManager.arrowPrefab, worldPos, Quaternion.identity, gameManager.transform);
            Arrow arrow = go.GetComponent<Arrow>();
            arrow.Initialize(data, gameManager, 1000);
            
            gameManager.RegisterArrowInGrid(arrow);
            Debug.Log("Spawned Arrow at " + data.bodyPositions[0]);
        }

        void Update()
        {
            if (mainCamera == null) mainCamera = Camera.main;
            if (gameManager == null || !isEditMode || mainCamera == null) return;
            if (SettingsUI.Instance != null && SettingsUI.Instance.IsOpen) return;

            // Don't draw arrows if we are zooming with two fingers
            if (Input.touchCount > 1) return;

            HandleInput();
        }

        private void HandleInput()
        {
            Vector3 mousePos = Input.mousePosition;
            
            // Validate mouse position is within screen
            if (mousePos.x < 0 || mousePos.x > Screen.width || mousePos.y < 0 || mousePos.y > Screen.height)
                return;

            // Calculate world position on the Z=0 plane
            float distanceToPlayer = Mathf.Abs(mainCamera.transform.position.z);
            mousePos.z = distanceToPlayer;
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(mousePos);
            
            // USE THE GAMEMANAGER TO CONVERT TO GRID POS (Handles Centering)
            Vector2Int mouseGridPos = gameManager.WorldToGridPos(mouseWorldPos);

            if (Input.GetMouseButtonDown(0))
            {
                // Check for Deletion (Alt + Click)
                if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
                {
                    Arrow arrowToDelete = gameManager.GetArrowAtPos(mouseGridPos);
                    if (arrowToDelete != null)
                    {
                        gameManager.ClearArrowFromGrid(arrowToDelete);
                        Destroy(arrowToDelete.gameObject);
                        Debug.Log("Deleted arrow at " + mouseGridPos);
                        return; // Exit so we don't start drawing
                    }
                }

                if (IsWithinBounds(mouseGridPos))
                {
                    dragStartGrid = mouseGridPos;
                    currentDragPath.Clear();
                    currentDragPath.Add(mouseGridPos);
                    Debug.Log("Started drawing at: " + mouseGridPos);
                }
                else
                {
                    Debug.Log("Clicked outside grid at: " + mouseGridPos);
                }
            }
            else if (Input.GetMouseButton(0) && dragStartGrid.HasValue)
            {
                UpdateDragPath(mouseGridPos);
                UpdatePreview();
            }
            else if (Input.GetMouseButtonUp(0) && dragStartGrid.HasValue)
            {
                FinalizeArrow();
                dragStartGrid = null;
                if (linePreview != null) linePreview.positionCount = 0;
            }
        }

        private bool IsWithinBounds(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < gameManager.gridWidth && 
                   pos.y >= 0 && pos.y < gameManager.gridHeight;
        }

        private void UpdateDragPath(Vector2Int currentMouseGrid)
        {
            if (!IsWithinBounds(currentMouseGrid)) return;
            
            // Only add if it's a new cell
            Vector2Int last = currentDragPath[currentDragPath.Count - 1];
            if (last == currentMouseGrid) return;

            // Check if it's adjacent (Orthogonal only)
            int dist = Mathf.Abs(currentMouseGrid.x - last.x) + Mathf.Abs(currentMouseGrid.y - last.y);

            if (dist == 1)
            {
                // Check for backtracking (if user moves mouse back to previous cell, undo last)
                if (currentDragPath.Count > 1 && currentDragPath[currentDragPath.Count - 2] == currentMouseGrid)
                {
                    currentDragPath.RemoveAt(currentDragPath.Count - 1);
                }
                else
                {
                    // Check if cell already exists in path to avoid self-overlap
                    if (!currentDragPath.Contains(currentMouseGrid))
                    {
                        currentDragPath.Add(currentMouseGrid);
                    }
                }
            }
        }

        private void UpdatePreview()
        {
            if (linePreview == null) return;

            linePreview.positionCount = currentDragPath.Count;
            for (int i = 0; i < currentDragPath.Count; i++)
            {
                Vector3 worldPos = gameManager.GetWorldPos(currentDragPath[i]);
                worldPos.z = -0.5f; // Keep line in front
                linePreview.SetPosition(i, worldPos);
            }
        }

        private void FinalizeArrow()
        {
            if (currentDragPath.Count < 2) return;

            // Determine direction based on the last two points
            Vector2Int last = currentDragPath[currentDragPath.Count - 1];
            Vector2Int secondToLast = currentDragPath[currentDragPath.Count - 2];
            Vector2Int dirVec = last - secondToLast;

            MoveDirection direction = MoveDirection.Up;
            if (dirVec.x > 0) direction = MoveDirection.Right;
            else if (dirVec.x < 0) direction = MoveDirection.Left;
            else if (dirVec.y > 0) direction = MoveDirection.Up;
            else if (dirVec.y < 0) direction = MoveDirection.Down;

            List<Vector2Int> finalizedBody = new List<Vector2Int>(currentDragPath);
            finalizedBody.Reverse(); // Index 0 is the HEAD

            ArrowData data = new ArrowData();
            data.bodyPositions = finalizedBody;
            data.direction = direction;

            SpawnArrow(data);
        }
    }
}
