using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Trilho
{
    [System.Serializable]
    public class ContentMarker
    {
        [Header("Marker Settings")]
        public string markerName = "New Marker";
        public float worldPositionX = 0f; // Posição no mundo Unity
        public float activationRadius = 100f; // Raio de ativação em unidades Unity
        
        [Header("Content")]
        public GameObject contentToActivate;
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 0.5f;
        
        [Header("Visual Debug")]
        public bool showDebugSphere = true;
        public Color debugColor = Color.yellow;
        
        [Header("Runtime")]
        public bool isActive = false;
        public bool isInRange = false;
    }

    public class MarkerSystem : MonoBehaviour
    {
        [Header("Camera Reference")]
        [SerializeField] private Transform cameraTransform;
        
        [Header("Markers")]
        [SerializeField] private List<ContentMarker> markers = new List<ContentMarker>();
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool showMarkerSpheres = true;
        [SerializeField] private bool showActivationRanges = true;
        
        [Header("Settings")]
        [SerializeField] private float updateInterval = 0.1f; // Intervalo de atualização em segundos
        
        // Private variables
        private Dictionary<ContentMarker, GameObject> debugSpheres = new Dictionary<ContentMarker, GameObject>();
        private Coroutine updateCoroutine;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Auto-assign camera if not set
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;
                
            if (cameraTransform == null)
            {
                Debug.LogError("[MARKER SYSTEM] Camera não encontrada!");
                return;
            }
            
            Debug.Log($"[MARKER SYSTEM] Sistema inicializado com {markers.Count} marcadores");
        }
        
        private void Start()
        {
            // Start update coroutine
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);
            updateCoroutine = StartCoroutine(UpdateMarkersCoroutine());
            
            // Create debug spheres
            if (showMarkerSpheres)
                CreateDebugSpheres();
        }
        
        private void OnDestroy()
        {
            if (updateCoroutine != null)
                StopCoroutine(updateCoroutine);
                
            // Clean up debug spheres
            CleanupDebugSpheres();
        }
        
        #endregion
        
        #region Marker Management
        
        private IEnumerator UpdateMarkersCoroutine()
        {
            while (true)
            {
                UpdateAllMarkers();
                yield return new WaitForSeconds(updateInterval);
            }
        }
        
        private void UpdateAllMarkers()
        {
            if (cameraTransform == null) return;
            
            Vector3 cameraPosition = cameraTransform.position;
            
            foreach (var marker in markers)
            {
                if (marker == null) continue;
                
                // Calculate distance to marker
                Vector3 markerPosition = new Vector3(marker.worldPositionX, cameraPosition.y, cameraPosition.z);
                float distance = Vector3.Distance(cameraPosition, markerPosition);
                
                // Check if camera is in activation range
                bool wasInRange = marker.isInRange;
                marker.isInRange = distance <= marker.activationRadius;
                
                // Handle activation/deactivation
                if (marker.isInRange && !marker.isActive)
                {
                    ActivateMarker(marker);
                }
                else if (!marker.isInRange && marker.isActive)
                {
                    DeactivateMarker(marker);
                }
                
                // Update debug sphere position and color
                if (debugSpheres.ContainsKey(marker))
                {
                    var sphere = debugSpheres[marker];
                    sphere.transform.position = markerPosition;
                    
                    var renderer = sphere.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color sphereColor = marker.isInRange ? Color.green : marker.debugColor;
                        sphereColor.a = 0.5f;
                        renderer.material.color = sphereColor;
                    }
                }
                
                // Debug info
                if (showDebugInfo && wasInRange != marker.isInRange)
                {
                    string status = marker.isInRange ? "ATIVADO" : "DESATIVADO";
                    Debug.Log($"[MARKER SYSTEM] {marker.markerName}: {status} (distância: {distance:F1})");
                }
            }
        }
        
        private void ActivateMarker(ContentMarker marker)
        {
            if (marker.contentToActivate == null) return;
            
            Debug.Log($"[MARKER SYSTEM] Ativando conteúdo: {marker.markerName}");
            
            // Activate content
            marker.contentToActivate.SetActive(true);
            
            // Fade in
            var canvasGroup = marker.contentToActivate.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, marker.fadeInDuration));
            }
            else
            {
                // If no CanvasGroup, activate immediately
                marker.contentToActivate.SetActive(true);
            }
            
            // Handle video content
            var videoPlayer = marker.contentToActivate.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                if (!videoPlayer.isPrepared)
                {
                    videoPlayer.Prepare();
                }
                videoPlayer.Play();
            }
            
            marker.isActive = true;
        }
        
        private void DeactivateMarker(ContentMarker marker)
        {
            if (marker.contentToActivate == null) return;
            
            Debug.Log($"[MARKER SYSTEM] Desativando conteúdo: {marker.markerName}");
            
            // Handle video content
            var videoPlayer = marker.contentToActivate.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
            
            // Fade out
            var canvasGroup = marker.contentToActivate.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, marker.fadeOutDuration, () =>
                {
                    marker.contentToActivate.SetActive(false);
                }));
            }
            else
            {
                marker.contentToActivate.SetActive(false);
            }
            
            marker.isActive = false;
        }
        
        #endregion
        
        #region Debug Visualization
        
        private void CreateDebugSpheres()
        {
            CleanupDebugSpheres();
            
            foreach (var marker in markers)
            {
                if (marker == null) continue;
                
                // Create sphere for marker position
                var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphere.name = $"Debug_Sphere_{marker.markerName}";
                sphere.transform.position = new Vector3(marker.worldPositionX, 0, 0);
                sphere.transform.localScale = Vector3.one * 50f; // 50 units radius
                
                // Set material
                var renderer = sphere.GetComponent<Renderer>();
                var material = new Material(Shader.Find("Standard"));
                material.color = marker.debugColor;
                material.SetFloat("_Mode", 3); // Transparent mode
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = 3000;
                renderer.material = material;
                
                // Create activation range sphere
                var rangeSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                rangeSphere.name = $"Debug_Range_{marker.markerName}";
                rangeSphere.transform.position = new Vector3(marker.worldPositionX, 0, 0);
                rangeSphere.transform.localScale = Vector3.one * marker.activationRadius * 2f;
                
                var rangeRenderer = rangeSphere.GetComponent<Renderer>();
                var rangeMaterial = new Material(Shader.Find("Standard"));
                rangeMaterial.color = new Color(marker.debugColor.r, marker.debugColor.g, marker.debugColor.b, 0.1f);
                rangeMaterial.SetFloat("_Mode", 3);
                rangeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                rangeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                rangeMaterial.SetInt("_ZWrite", 0);
                rangeMaterial.DisableKeyword("_ALPHATEST_ON");
                rangeMaterial.EnableKeyword("_ALPHABLEND_ON");
                rangeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                rangeMaterial.renderQueue = 3000;
                rangeRenderer.material = rangeMaterial;
                
                // Store reference
                debugSpheres[marker] = sphere;
            }
            
            Debug.Log($"[MARKER SYSTEM] Esferas de debug criadas para {debugSpheres.Count} marcadores");
        }
        
        private void CleanupDebugSpheres()
        {
            foreach (var sphere in debugSpheres.Values)
            {
                if (sphere != null)
                {
                    DestroyImmediate(sphere);
                }
            }
            debugSpheres.Clear();
        }
        
        #endregion
        
        #region Transitions
        
        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration)
        {
            if (canvasGroup == null) yield break;
            
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
        }
        
        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float targetAlpha, float duration, System.Action onComplete)
        {
            if (canvasGroup == null) yield break;
            
            float startAlpha = canvasGroup.alpha;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Public API
        
        public void AddMarker(ContentMarker marker)
        {
            if (marker != null && !markers.Contains(marker))
            {
                markers.Add(marker);
                Debug.Log($"[MARKER SYSTEM] Marcador adicionado: {marker.markerName}");
            }
        }
        
        public void RemoveMarker(ContentMarker marker)
        {
            if (marker != null && markers.Contains(marker))
            {
                // Deactivate if active
                if (marker.isActive)
                {
                    DeactivateMarker(marker);
                }
                
                markers.Remove(marker);
                Debug.Log($"[MARKER SYSTEM] Marcador removido: {marker.markerName}");
            }
        }
        
        public List<ContentMarker> GetMarkers() => markers;
        
        public ContentMarker GetMarkerByName(string name)
        {
            return markers.Find(m => m.markerName == name);
        }
        
        public void SetCameraTransform(Transform camera)
        {
            cameraTransform = camera;
        }
        
        public void ToggleDebugVisualization()
        {
            showMarkerSpheres = !showMarkerSpheres;
            
            if (showMarkerSpheres)
            {
                CreateDebugSpheres();
            }
            else
            {
                CleanupDebugSpheres();
            }
        }
        
        // NOVO: Configurar opções de debug de forma programática
        public void SetDebugOptions(bool showInfo, bool showSpheres, bool showRanges)
        {
            bool spheresChanged = showMarkerSpheres != showSpheres;
            
            showDebugInfo = showInfo;
            showMarkerSpheres = showSpheres;
            showActivationRanges = showRanges;
            
            if (spheresChanged)
            {
                if (showMarkerSpheres)
                {
                    CreateDebugSpheres();
                }
                else
                {
                    CleanupDebugSpheres();
                }
            }
        }
        
        #endregion
        
        #region Context Menu Methods
        
        [ContextMenu("1. Criar Marcadores de Exemplo")]
        public void CriarMarcadoresDeExemplo()
        {
            Debug.Log("=== CRIANDO MARCADORES DE EXEMPLO ===");
            
            // Clear existing markers
            markers.Clear();
            
            // Find Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            // Marker 1 - Green content
            var marker1 = new ContentMarker
            {
                markerName = "Marcador Verde",
                worldPositionX = 1000f,
                activationRadius = 200f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f,
                debugColor = Color.green
            };
            
            var conteudo1 = new GameObject("Conteúdo Verde");
            conteudo1.transform.SetParent(canvas.transform, false);
            var image1 = conteudo1.AddComponent<Image>();
            image1.color = Color.green;
            
            var rect1 = conteudo1.GetComponent<RectTransform>();
            rect1.anchorMin = Vector2.zero;
            rect1.anchorMax = Vector2.one;
            rect1.sizeDelta = Vector2.zero;
            rect1.anchoredPosition = Vector2.zero;
            
            var canvasGroup1 = conteudo1.AddComponent<CanvasGroup>();
            canvasGroup1.alpha = 0f;
            conteudo1.SetActive(false);
            marker1.contentToActivate = conteudo1;
            
            // Marker 2 - Blue content
            var marker2 = new ContentMarker
            {
                markerName = "Marcador Azul",
                worldPositionX = 3000f,
                activationRadius = 200f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f,
                debugColor = Color.blue
            };
            
            var conteudo2 = new GameObject("Conteúdo Azul");
            conteudo2.transform.SetParent(canvas.transform, false);
            var image2 = conteudo2.AddComponent<Image>();
            image2.color = Color.blue;
            
            var rect2 = conteudo2.GetComponent<RectTransform>();
            rect2.anchorMin = Vector2.zero;
            rect2.anchorMax = Vector2.one;
            rect2.sizeDelta = Vector2.zero;
            rect2.anchoredPosition = Vector2.zero;
            
            var canvasGroup2 = conteudo2.AddComponent<CanvasGroup>();
            canvasGroup2.alpha = 0f;
            conteudo2.SetActive(false);
            marker2.contentToActivate = conteudo2;
            
            // Marker 3 - Video content
            var marker3 = new ContentMarker
            {
                markerName = "Marcador Vídeo",
                worldPositionX = 5000f,
                activationRadius = 200f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f,
                debugColor = Color.red
            };
            
            var conteudo3 = new GameObject("Conteúdo Vídeo");
            conteudo3.transform.SetParent(canvas.transform, false);
            
            var videoPlayer = conteudo3.AddComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            
            var renderTexture = new RenderTexture(1080, 1920, 24);
            videoPlayer.targetTexture = renderTexture;
            
            var rawImage = conteudo3.AddComponent<RawImage>();
            rawImage.texture = renderTexture;
            
            var rect3 = conteudo3.GetComponent<RectTransform>();
            rect3.anchorMin = Vector2.zero;
            rect3.anchorMax = Vector2.one;
            rect3.sizeDelta = Vector2.zero;
            rect3.anchoredPosition = Vector2.zero;
            
            var canvasGroup3 = conteudo3.AddComponent<CanvasGroup>();
            canvasGroup3.alpha = 0f;
            conteudo3.SetActive(false);
            marker3.contentToActivate = conteudo3;
            
            // Add markers
            markers.Add(marker1);
            markers.Add(marker2);
            markers.Add(marker3);
            
            Debug.Log("Marcadores criados:");
            Debug.Log("1. Marcador Verde - Posição: 1000, Raio: 200");
            Debug.Log("2. Marcador Azul - Posição: 3000, Raio: 200");
            Debug.Log("3. Marcador Vídeo - Posição: 5000, Raio: 200");
            Debug.Log("=== MARCADORES CRIADOS ===");
        }
        
        [ContextMenu("2. Mostrar Info dos Marcadores")]
        public void MostrarInfoDosMarcadores()
        {
            Debug.Log("=== INFORMAÇÕES DOS MARCADORES ===");
            for (int i = 0; i < markers.Count; i++)
            {
                var marker = markers[i];
                Debug.Log($"Marcador {i + 1}: {marker.markerName}");
                Debug.Log($"  Posição: {marker.worldPositionX} Unity");
                Debug.Log($"  Raio: {marker.activationRadius} Unity");
                Debug.Log($"  Conteúdo: {(marker.contentToActivate != null ? marker.contentToActivate.name : "Nenhum")}");
                Debug.Log($"  Ativo: {marker.isActive}");
                Debug.Log($"  No Range: {marker.isInRange}");
                Debug.Log("---");
            }
            Debug.Log("===============================");
        }
        
        [ContextMenu("3. Limpar Marcadores")]
        public void LimparMarcadores()
        {
            Debug.Log("=== LIMPANDO MARCADORES ===");
            
            // Deactivate all active markers
            foreach (var marker in markers)
            {
                if (marker.isActive)
                {
                    DeactivateMarker(marker);
                }
            }
            
            // Clear list
            markers.Clear();
            
            // Clean up debug spheres
            CleanupDebugSpheres();
            
            Debug.Log("Marcadores limpos!");
        }
        
        #endregion
    }
}
