using UnityEngine;
using UnityEngine.UI;

namespace Trilho.Examples
{
    public class MarkerSystemExample : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private MarkerSystem markerSystem;
        [SerializeField] private Canvas targetCanvas;
        
        [Header("Example Settings")]
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private float[] markerPositions = { 1000f, 3000f, 5000f };
        [SerializeField] private Color[] markerColors = { Color.green, Color.blue, Color.red };
        
        private void Start()
        {
            if (createOnStart)
            {
                SetupExample();
            }
        }
        
        [ContextMenu("Setup Example")]
        public void SetupExample()
        {
            Debug.Log("=== CONFIGURANDO EXEMPLO DO SISTEMA DE MARCADORES ===");
            
            // Ensure we have required components
            if (markerSystem == null)
            {
                markerSystem = FindObjectOfType<MarkerSystem>();
                if (markerSystem == null)
                {
                    Debug.Log("Criando MarkerSystem...");
                    var markerSystemObj = new GameObject("MarkerSystem");
                    markerSystem = markerSystemObj.AddComponent<MarkerSystem>();
                }
            }
            
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
                if (targetCanvas == null)
                {
                    Debug.LogError("Canvas não encontrado!");
                    return;
                }
            }
            
            // Clear existing markers
            markerSystem.LimparMarcadores();
            
            // Create example markers
            CreateExampleMarkers();
            
            Debug.Log("=== EXEMPLO CONFIGURADO ===");
        }
        
        private void CreateExampleMarkers()
        {
            for (int i = 0; i < markerPositions.Length; i++)
            {
                CreateMarker($"Exemplo {i + 1}", markerPositions[i], markerColors[i], i == 2);
            }
        }
        
        private void CreateMarker(string name, float positionX, Color color, bool isVideo = false)
        {
            var marker = new ContentMarker
            {
                markerName = name,
                worldPositionX = positionX,
                activationRadius = 200f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f,
                debugColor = color
            };
            
            // Create content
            var content = new GameObject($"Conteúdo {name}");
            content.transform.SetParent(targetCanvas.transform, false);
            
            if (isVideo)
            {
                CreateVideoContent(content);
            }
            else
            {
                CreateImageContent(content, color);
            }
            
            // Configure RectTransform
            var rectTransform = content.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Add CanvasGroup
            var canvasGroup = content.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            content.SetActive(false);
            
            marker.contentToActivate = content;
            markerSystem.AddMarker(marker);
            
            Debug.Log($"Marcador criado: {name} na posição {positionX}");
        }
        
        private void CreateImageContent(GameObject content, Color color)
        {
            var image = content.AddComponent<Image>();
            image.color = color;
            
            // Add text overlay
            var textObj = new GameObject("Texto");
            textObj.transform.SetParent(content.transform, false);
            
            var text = textObj.AddComponent<Text>();
            text.text = $"Conteúdo {content.name}";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 72;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        
        private void CreateVideoContent(GameObject content)
        {
            var videoPlayer = content.AddComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            
            var renderTexture = new RenderTexture(1080, 1920, 24);
            videoPlayer.targetTexture = renderTexture;
            
            var rawImage = content.AddComponent<RawImage>();
            rawImage.texture = renderTexture;
            
            // Try to set video URL
            string videoPath = "file://" + Application.dataPath + "/Videos/video.mp4";
            videoPlayer.url = videoPath;
            
            // Add text overlay
            var textObj = new GameObject("Texto Vídeo");
            textObj.transform.SetParent(content.transform, false);
            
            var text = textObj.AddComponent<Text>();
            text.text = "Conteúdo de Vídeo";
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 72;
            text.color = Color.white;
            text.alignment = TextAnchor.MiddleCenter;
            
            var textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        
        [ContextMenu("Test Movement")]
        public void TestMovement()
        {
            Debug.Log("=== TESTE DE MOVIMENTO ===");
            Debug.Log("Use as setas do teclado para mover a câmera");
            Debug.Log("Ou use as teclas 1-5 para pular para posições específicas");
            Debug.Log("Observe os marcadores sendo ativados/desativados");
        }
        
        [ContextMenu("Show Marker Info")]
        public void ShowMarkerInfo()
        {
            if (markerSystem != null)
            {
                markerSystem.MostrarInfoDosMarcadores();
            }
        }
        
        [ContextMenu("Toggle Debug Visualization")]
        public void ToggleDebugVisualization()
        {
            if (markerSystem != null)
            {
                markerSystem.ToggleDebugVisualization();
            }
        }
        
        [ContextMenu("Clear All Markers")]
        public void ClearAllMarkers()
        {
            if (markerSystem != null)
            {
                markerSystem.LimparMarcadores();
                Debug.Log("Todos os marcadores removidos");
            }
        }
        
        private void OnGUI()
        {
            // Simple UI for testing
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== MARKER SYSTEM EXAMPLE ===");
            
            if (GUILayout.Button("Setup Example"))
            {
                SetupExample();
            }
            
            if (GUILayout.Button("Show Marker Info"))
            {
                ShowMarkerInfo();
            }
            
            if (GUILayout.Button("Toggle Debug Visualization"))
            {
                ToggleDebugVisualization();
            }
            
            if (GUILayout.Button("Clear All Markers"))
            {
                ClearAllMarkers();
            }
            
            GUILayout.Label("Use arrow keys to move camera");
            GUILayout.Label("Use 1-5 keys for quick jumps");
            
            GUILayout.EndArea();
        }
    }
}
