using UnityEngine;
using System.Collections;

namespace ArrowMaze
{
    public class CameraController : MonoBehaviour
    {
        [Header("Zoom Settings")]
        public float zoomSpeed = 5f;
        public float minZoom = 2f;
        public float maxZoom = 20f;
        public float zoomSmoothing = 0.35f; // Increased for a more cinematic, smooth zoom-in

        [Header("Pan Settings")]
        public float panSpeed = 1f;
        public float mousePanSpeed = 1.5f;
        public float panSmoothing = 0.25f;  // Increased for fluid camera movement

        [Header("Limits")]
        public Vector2 minBounds = new Vector2(-15, -15);
        public Vector2 maxBounds = new Vector2(15, 15);
        public float padding = 7f;          // More breathing room for the grid visuals
        public float dragDeadzone = 0.05f; // Small movement threshold

        private Camera cam;
        private Vector3 touchStart;
        private Vector3 targetPosition;
        private float targetZoom;
        private Vector3 currentVelocity;
        private float zoomVelocity;
        private bool isDragging = false;
        private Coroutine shakeRoutine;
        private Vector3 shakeOffset;

        void Start()
        {
            cam = GetComponent<Camera>();
            targetPosition = transform.position;
            targetZoom = cam.orthographic ? cam.orthographicSize : transform.position.z;
        }

        void Update()
        {
            if (SettingsUI.Instance != null && SettingsUI.Instance.IsOpen) return;
            
            HandleMouseControls();
            HandleTouchControls();

            // Apply Smoothing and Clamping
            ApplyMovement();
        }

        private void HandleMouseControls()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0)
            {
                targetZoom += cam.orthographic ? -scroll * zoomSpeed : scroll * zoomSpeed * 5f;
            }

            if (Input.GetMouseButtonDown(1))
            {
                touchStart = cam.ScreenToWorldPoint(Input.mousePosition);
                isDragging = false;
            }
            if (Input.GetMouseButton(1))
            {
                Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
                Vector3 direction = touchStart - currentPos;
                
                if (isDragging || direction.magnitude > dragDeadzone)
                {
                    isDragging = true;
                    targetPosition += direction * mousePanSpeed;
                    touchStart = currentPos;
                }
            }
        }

        private void HandleTouchControls()
        {
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    touchStart = cam.ScreenToWorldPoint(touch.position);
                    isDragging = false;
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    Vector3 currentPos = cam.ScreenToWorldPoint(touch.position);
                    Vector3 direction = touchStart - currentPos;
                    
                    if (isDragging || direction.magnitude > dragDeadzone)
                    {
                        isDragging = true;
                        targetPosition += direction * panSpeed;
                        touchStart = currentPos;
                    }
                }
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.GetTouch(0);
                Touch t1 = Input.GetTouch(1);

                Vector2 t0Prev = t0.position - t0.deltaPosition;
                Vector2 t1Prev = t1.position - t1.deltaPosition;

                float prevMag = (t0Prev - t1Prev).magnitude;
                float currentMag = (t0.position - t1.position).magnitude;
                float diff = currentMag - prevMag;

                targetZoom += cam.orthographic ? -diff * 0.01f * zoomSpeed : diff * 0.01f * zoomSpeed * 5f;
            }
        }

        private void ApplyMovement()
        {
            // Clamp Target Position (X and Y only)
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);

            // Smooth Position - ONLY X and Y to avoid fighting with Z-zoom
            Vector3 targetPosWithCurrentZ = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position - shakeOffset, targetPosWithCurrentZ, ref currentVelocity, panSmoothing);
            transform.position = smoothedPosition + shakeOffset;

            // Clamp and Smooth Zoom
            if (cam.orthographic)
            {
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                cam.orthographicSize = Mathf.SmoothDamp(cam.orthographicSize, targetZoom, ref zoomVelocity, zoomSmoothing);
            }
            else
            {
                targetZoom = Mathf.Clamp(targetZoom, -maxZoom, -minZoom);
                float newZ = Mathf.SmoothDamp(transform.position.z, targetZoom, ref zoomVelocity, zoomSmoothing);
                transform.position = new Vector3(transform.position.x, transform.position.y, newZ);
            }
        }

        public void UpdateBounds(int width, int height, float step)
        {
            float halfW = (width - 1) * step / 2f;
            float halfH = (height - 1) * step / 2f;

            minBounds = new Vector2(-halfW - padding, -halfH - padding);
            maxBounds = new Vector2(halfW + padding, halfH + padding);
            
            // Limit zoom based on size - increased multiplier to prevent cropping
            maxZoom = Mathf.Max(width, height) * step * 1.5f;
            
            Debug.Log($"Camera bounds updated to: {minBounds} to {maxBounds}");
        }

        public void CenterCamera(bool snap = false, bool introZoom = false)
        {
            if (cam == null) cam = GetComponent<Camera>();
            targetPosition = new Vector3(0, 0, transform.position.z);
            
            // Set a default zoom that fits the grid well
            if (cam != null)
            {
                float gridHeight = (maxBounds.y - minBounds.y) - (padding * 2);
                float gridWidth = (maxBounds.x - minBounds.x) - (padding * 2);
                
                if (cam.orthographic)
                {
                    targetZoom = Mathf.Max(gridHeight, gridWidth / cam.aspect) * 0.62f; 
                    targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
                }
                else
                {
                    targetZoom = -Mathf.Max(gridHeight, gridWidth / cam.aspect) * 1.3f;
                    targetZoom = Mathf.Clamp(targetZoom, -maxZoom, -minZoom);
                }

                if (introZoom)
                {
                    // Calculate satisfying intro zoom (50% further out)
                    float zoomedOutSize = targetZoom * (cam.orthographic ? 1.5f : 1.3f);
                    
                    if (cam.orthographic) cam.orthographicSize = zoomedOutSize;
                    else transform.position = new Vector3(transform.position.x, transform.position.y, zoomedOutSize);
                    
                    // Hold this zoom until specifically told to zoom in
                    targetZoom = zoomedOutSize;
                }
            }
            
            if (snap && !introZoom)
            {
                transform.position = new Vector3(targetPosition.x, targetPosition.y, cam.orthographic ? transform.position.z : targetZoom);
                if (cam.orthographic) cam.orthographicSize = targetZoom;
                
                currentVelocity = Vector3.zero;
                zoomVelocity = 0;
            }
        }

        public void ZoomToPlayView()
        {
            // Recalculate the perfect play view zoom
            float gridHeight = (maxBounds.y - minBounds.y) - (padding * 2);
            float gridWidth = (maxBounds.x - minBounds.x) - (padding * 2);
            
            if (cam.orthographic)
            {
                targetZoom = Mathf.Max(gridHeight, gridWidth / cam.aspect) * 0.62f;
                targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
            }
            else
            {
                targetZoom = -Mathf.Max(gridHeight, gridWidth / cam.aspect) * 1.3f;
                targetZoom = Mathf.Clamp(targetZoom, -maxZoom, -minZoom);
            }

            // Optional: Temporarily slow down smoothing for a more "satisfying" feel
            // zoomSmoothing = 0.25f; 
        }

        public void Shake(float duration, float strength)
        {
            if (!isActiveAndEnabled || duration <= 0f || strength <= 0f) return;

            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
            }

            shakeRoutine = StartCoroutine(ShakeRoutine(duration, strength));
        }

        private IEnumerator ShakeRoutine(float duration, float strength)
        {
            float elapsed = 0f;
            while (elapsed < duration)
            {
                float t = elapsed / duration;
                float decay = 1f - t;
                Vector2 random = Random.insideUnitCircle * strength * decay;
                shakeOffset = new Vector3(random.x, random.y, 0f);
                elapsed += Time.deltaTime;
                yield return null;
            }

            shakeOffset = Vector3.zero;
            shakeRoutine = null;
        }
    }
}
