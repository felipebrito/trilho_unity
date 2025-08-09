using UnityEngine;
using System.Collections.Generic;

namespace Trilho
{
    public class MarkerConfiguration : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private TrilhoGameManager gameManager;
        [SerializeField] private MarkerSystem markerSystem;
        
        [Header("Configuration")]
        [SerializeField] private bool autoSetup = true;
        [SerializeField] private bool convertExistingZones = true;
        
        [Header("Marker Settings")]
        [SerializeField] private float defaultActivationRadius = 200f;
        [SerializeField] private float defaultFadeInDuration = 0.5f;
        [SerializeField] private float defaultFadeOutDuration = 0.5f;
        
        private void Awake()
        {
            // Auto-assign references
            if (gameManager == null)
                gameManager = FindObjectOfType<TrilhoGameManager>();
                
            if (markerSystem == null)
                markerSystem = FindObjectOfType<MarkerSystem>();
                
            if (autoSetup)
            {
                SetupMarkerSystem();
            }
        }
        
        #region Setup Methods
        
        [ContextMenu("Setup Marker System")]
        public void SetupMarkerSystem()
        {
            Debug.Log("=== SETUP DO SISTEMA DE MARCADORES ===");
            
            // Ensure we have the required components
            if (gameManager == null)
            {
                Debug.LogError("TrilhoGameManager não encontrado!");
                return;
            }
            
            if (markerSystem == null)
            {
                Debug.Log("Criando MarkerSystem...");
                CreateMarkerSystem();
            }
            
            // Convert existing zones if requested
            if (convertExistingZones)
            {
                ConvertExistingZonesToMarkers();
            }
            
            // Configure camera reference
            if (markerSystem != null && gameManager != null)
            {
                var camera = gameManager.GetCameraTransform();
                if (camera != null)
                {
                    markerSystem.SetCameraTransform(camera);
                    Debug.Log("Câmera configurada no MarkerSystem");
                }
            }
            
            Debug.Log("=== SETUP CONCLUÍDO ===");
        }
        
        private void CreateMarkerSystem()
        {
            var existingMarkerSystem = FindObjectOfType<MarkerSystem>();
            if (existingMarkerSystem != null)
            {
                markerSystem = existingMarkerSystem;
                Debug.Log("MarkerSystem existente encontrado");
                return;
            }
            
            // Create new MarkerSystem GameObject
            var markerSystemObj = new GameObject("MarkerSystem");
            markerSystem = markerSystemObj.AddComponent<MarkerSystem>();
            
            Debug.Log("MarkerSystem criado");
        }
        
        private void ConvertExistingZonesToMarkers()
        {
            if (gameManager == null) return;
            
            Debug.Log("Convertendo zonas existentes para marcadores...");
            
            // Get existing zones from TrilhoGameManager
            var existingZones = GetExistingZones();
            
            foreach (var zone in existingZones)
            {
                var marker = ConvertZoneToMarker(zone);
                if (marker != null)
                {
                    markerSystem.AddMarker(marker);
                    Debug.Log($"Zona convertida: {marker.markerName}");
                }
            }
            
            Debug.Log($"Conversão concluída: {existingZones.Count} zonas convertidas");
        }
        
        private List<ActivationZone> GetExistingZones()
        {
            // This would need to be implemented based on how zones are stored in TrilhoGameManager
            // For now, we'll return an empty list and create example markers
            return new List<ActivationZone>();
        }
        
        private ContentMarker ConvertZoneToMarker(ActivationZone zone)
        {
            if (zone == null) return null;
            
            // Convert zone position from cm to Unity world position
            float worldPositionX = ConvertCmToUnityPosition(zone.startPositionCm + (zone.endPositionCm - zone.startPositionCm) / 2f);
            
            var marker = new ContentMarker
            {
                markerName = zone.zoneName,
                worldPositionX = worldPositionX,
                activationRadius = defaultActivationRadius,
                fadeInDuration = zone.fadeInDuration,
                fadeOutDuration = zone.fadeOutDuration,
                contentToActivate = zone.contentToActivate,
                debugColor = GetColorForZone(zone.zoneName)
            };
            
            return marker;
        }
        
        private float ConvertCmToUnityPosition(float positionCm)
        {
            // Use the same mapping logic as TrilhoGameManager
            float physicalMinCm = 0f;
            float physicalMaxCm = 600f;
            float unityMinPosition = 0f;
            float unityMaxPosition = 8520f;
            
            positionCm = Mathf.Clamp(positionCm, physicalMinCm, physicalMaxCm);
            float normalizedPosition = (positionCm - physicalMinCm) / (physicalMaxCm - physicalMinCm);
            float unityPosition = Mathf.Lerp(unityMinPosition, unityMaxPosition, normalizedPosition);
            
            return unityPosition;
        }
        
        private Color GetColorForZone(string zoneName)
        {
            if (zoneName.Contains("Verde") || zoneName.Contains("Green"))
                return Color.green;
            else if (zoneName.Contains("Azul") || zoneName.Contains("Blue"))
                return Color.blue;
            else if (zoneName.Contains("Vídeo") || zoneName.Contains("Video"))
                return Color.red;
            else
                return Color.yellow;
        }
        
        #endregion
        
        #region Utility Methods
        
        [ContextMenu("Create Example Markers")]
        public void CreateExampleMarkers()
        {
            if (markerSystem == null)
            {
                SetupMarkerSystem();
            }
            
            markerSystem.CriarMarcadoresDeExemplo();
        }
        
        [ContextMenu("Show Marker Info")]
        public void ShowMarkerInfo()
        {
            if (markerSystem != null)
            {
                markerSystem.MostrarInfoDosMarcadores();
            }
        }
        
        [ContextMenu("Clear All Markers")]
        public void ClearAllMarkers()
        {
            if (markerSystem != null)
            {
                markerSystem.LimparMarcadores();
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
        
        #endregion
        
        #region Migration Helpers
        
        [ContextMenu("Migrate from TrilhoGameManager")]
        public void MigrateFromTrilhoGameManager()
        {
            Debug.Log("=== MIGRAÇÃO DO TRILHO GAME MANAGER ===");
            
            if (gameManager == null)
            {
                Debug.LogError("TrilhoGameManager não encontrado!");
                return;
            }
            
            // Create marker system if needed
            SetupMarkerSystem();
            
            // Create markers based on typical Trilho setup
            CreateTypicalTrilhoMarkers();
            
            Debug.Log("=== MIGRAÇÃO CONCLUÍDA ===");
        }
        
        private void CreateTypicalTrilhoMarkers()
        {
            // Create markers based on typical Trilho zones
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            // Marker 1 - Green (around 1000 Unity units)
            CreateMarker("Marcador Verde", 1000f, Color.green, canvas);
            
            // Marker 2 - Blue (around 3000 Unity units)
            CreateMarker("Marcador Azul", 3000f, Color.blue, canvas);
            
            // Marker 3 - Video (around 5000 Unity units)
            CreateMarker("Marcador Vídeo", 5000f, Color.red, canvas, true);
            
            Debug.Log("Marcadores típicos do Trilho criados");
        }
        
        private void CreateMarker(string name, float positionX, Color color, Canvas canvas, bool isVideo = false)
        {
            var marker = new ContentMarker
            {
                markerName = name,
                worldPositionX = positionX,
                activationRadius = defaultActivationRadius,
                fadeInDuration = defaultFadeInDuration,
                fadeOutDuration = defaultFadeOutDuration,
                debugColor = color
            };
            
            // Create content
            var content = new GameObject($"Conteúdo {name}");
            content.transform.SetParent(canvas.transform, false);
            
            if (isVideo)
            {
                // Create video content
                var videoPlayer = content.AddComponent<UnityEngine.Video.VideoPlayer>();
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = true;
                videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
                
                var renderTexture = new RenderTexture(1080, 1920, 24);
                videoPlayer.targetTexture = renderTexture;
                
                var rawImage = content.AddComponent<UnityEngine.UI.RawImage>();
                rawImage.texture = renderTexture;
                
                // Set video URL (you'll need to adjust this path)
                string videoPath = "file://" + Application.dataPath + "/Videos/video.mp4";
                videoPlayer.url = videoPath;
            }
            else
            {
                // Create image content
                var image = content.AddComponent<UnityEngine.UI.Image>();
                image.color = color;
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
        }
        
        #endregion
    }
}
