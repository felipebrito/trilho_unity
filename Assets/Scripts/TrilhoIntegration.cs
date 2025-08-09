using UnityEngine;
using System.Collections.Generic;

namespace Trilho
{
    public class TrilhoIntegration : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private TrilhoGameManager gameManager;
        [SerializeField] private MarkerSystem markerSystem;
        
        [Header("Integration Settings")]
        [SerializeField] private bool enableLegacySystem = true;
        [SerializeField] private bool enableMarkerSystem = true;
        [SerializeField] private bool syncCameraMovement = true;
        
        [Header("Position Mapping")]
        [SerializeField] private float physicalMinCm = 0f;
        [SerializeField] private float physicalMaxCm = 600f;
        [SerializeField] private float unityMinPosition = 0f;
        [SerializeField] private float unityMaxPosition = 8520f;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        private void Awake()
        {
            // Auto-assign references
            if (gameManager == null)
                gameManager = FindObjectOfType<TrilhoGameManager>();
                
            if (markerSystem == null)
                markerSystem = FindObjectOfType<MarkerSystem>();
        }
        
        private void Start()
        {
            if (enableMarkerSystem && markerSystem == null)
            {
                CreateMarkerSystem();
            }
            
            if (syncCameraMovement)
            {
                SyncCameraReferences();
            }
        }
        
        #region Setup Methods
        
        [ContextMenu("Setup Integration")]
        public void SetupIntegration()
        {
            Debug.Log("=== CONFIGURANDO INTEGRAÇÃO ===");
            
            // Ensure both systems exist
            if (gameManager == null)
            {
                Debug.LogError("TrilhoGameManager não encontrado!");
                return;
            }
            
            if (enableMarkerSystem && markerSystem == null)
            {
                CreateMarkerSystem();
            }
            
            // Sync camera references
            if (syncCameraMovement)
            {
                SyncCameraReferences();
            }
            
            Debug.Log("=== INTEGRAÇÃO CONFIGURADA ===");
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
            
            var markerSystemObj = new GameObject("MarkerSystem");
            markerSystem = markerSystemObj.AddComponent<MarkerSystem>();
            Debug.Log("MarkerSystem criado");
        }
        
        private void SyncCameraReferences()
        {
            if (gameManager != null && markerSystem != null)
            {
                // Get camera from TrilhoGameManager
                var camera = gameManager.GetCameraTransform();
                if (camera != null)
                {
                    markerSystem.SetCameraTransform(camera);
                    Debug.Log("Referência da câmera sincronizada");
                }
            }
        }
        
        #endregion
        
        #region Position Conversion
        
        public float ConvertCmToUnityPosition(float positionCm)
        {
            positionCm = Mathf.Clamp(positionCm, physicalMinCm, physicalMaxCm);
            float normalizedPosition = (positionCm - physicalMinCm) / (physicalMaxCm - physicalMinCm);
            float unityPosition = Mathf.Lerp(unityMinPosition, unityMaxPosition, normalizedPosition);
            return unityPosition;
        }
        
        public float ConvertUnityPositionToCm(float unityPosition)
        {
            float normalizedPosition = (unityPosition - unityMinPosition) / (unityMaxPosition - unityMinPosition);
            float physicalPosition = Mathf.Lerp(physicalMinCm, physicalMaxCm, normalizedPosition);
            return physicalPosition;
        }
        
        #endregion
        
        #region Zone to Marker Conversion
        
        [ContextMenu("Convert Zones to Markers")]
        public void ConvertZonesToMarkers()
        {
            if (gameManager == null || markerSystem == null)
            {
                Debug.LogError("Sistemas não encontrados!");
                return;
            }
            
            Debug.Log("=== CONVERTENDO ZONAS PARA MARCADORES ===");
            
            // Get zones from TrilhoGameManager (this would need to be implemented)
            var zones = GetZonesFromGameManager();
            
            foreach (var zone in zones)
            {
                var marker = ConvertZoneToMarker(zone);
                if (marker != null)
                {
                    markerSystem.AddMarker(marker);
                    Debug.Log($"Zona convertida: {marker.markerName}");
                }
            }
            
            Debug.Log($"Conversão concluída: {zones.Count} zonas convertidas");
        }
        
        private List<ActivationZone> GetZonesFromGameManager()
        {
            if (gameManager != null)
            {
                return gameManager.GetActivationZones();
            }
            
            // Fallback: Create zones based on typical Trilho setup
            var zones = new List<ActivationZone>();
            
            zones.Add(new ActivationZone
            {
                zoneName = "Zona Verde",
                startPositionCm = 80f,
                endPositionCm = 148f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f
            });
            
            zones.Add(new ActivationZone
            {
                zoneName = "Zona Azul",
                startPositionCm = 190f,
                endPositionCm = 290f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f
            });
            
            zones.Add(new ActivationZone
            {
                zoneName = "Zona Vídeo",
                startPositionCm = 320f,
                endPositionCm = 420f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f
            });
            
            return zones;
        }
        
        private ContentMarker ConvertZoneToMarker(ActivationZone zone)
        {
            if (zone == null) return null;
            
            // Calculate center position
            float centerPositionCm = zone.startPositionCm + (zone.endPositionCm - zone.startPositionCm) / 2f;
            float worldPositionX = ConvertCmToUnityPosition(centerPositionCm);
            
            var marker = new ContentMarker
            {
                markerName = zone.zoneName,
                worldPositionX = worldPositionX,
                activationRadius = 200f, // Default radius
                fadeInDuration = zone.fadeInDuration,
                fadeOutDuration = zone.fadeOutDuration,
                contentToActivate = zone.contentToActivate,
                debugColor = GetColorForZone(zone.zoneName)
            };
            
            return marker;
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
        
        [ContextMenu("Show System Status")]
        public void ShowSystemStatus()
        {
            Debug.Log("=== STATUS DOS SISTEMAS ===");
            
            if (gameManager != null)
            {
                Debug.Log($"TrilhoGameManager: ✓ Ativo");
                Debug.Log($"  Posição atual: {gameManager.GetCurrentPositionCm():F1}cm");
                Debug.Log($"  Estado: {gameManager.GetCurrentState()}");
            }
            else
            {
                Debug.Log("TrilhoGameManager: ✗ Não encontrado");
            }
            
            if (markerSystem != null)
            {
                var markers = markerSystem.GetMarkers();
                Debug.Log($"MarkerSystem: ✓ Ativo");
                Debug.Log($"  Marcadores: {markers.Count}");
                
                int activeMarkers = 0;
                foreach (var marker in markers)
                {
                    if (marker.isActive) activeMarkers++;
                }
                Debug.Log($"  Marcadores ativos: {activeMarkers}");
            }
            else
            {
                Debug.Log("MarkerSystem: ✗ Não encontrado");
            }
            
            Debug.Log("===========================");
        }
        
        [ContextMenu("Create Example Setup")]
        public void CreateExampleSetup()
        {
            Debug.Log("=== CRIANDO CONFIGURAÇÃO DE EXEMPLO ===");
            
            SetupIntegration();
            
            if (markerSystem != null)
            {
                markerSystem.CriarMarcadoresDeExemplo();
            }
            
            Debug.Log("=== CONFIGURAÇÃO DE EXEMPLO CRIADA ===");
        }
        
        [ContextMenu("Toggle Systems")]
        public void ToggleSystems()
        {
            enableLegacySystem = !enableLegacySystem;
            enableMarkerSystem = !enableMarkerSystem;
            
            Debug.Log($"Sistemas alternados:");
            Debug.Log($"  Legacy System: {(enableLegacySystem ? "✓" : "✗")}");
            Debug.Log($"  Marker System: {(enableMarkerSystem ? "✓" : "✗")}");
        }
        
        #endregion
        
        #region Runtime Methods
        
        private void Update()
        {
            if (showDebugInfo && Time.frameCount % 60 == 0) // Update every 60 frames
            {
                UpdateDebugInfo();
            }
        }
        
        private void UpdateDebugInfo()
        {
            if (gameManager != null)
            {
                float positionCm = gameManager.GetCurrentPositionCm();
                float unityPosition = gameManager.GetCurrentUnityPosition();
                
                Debug.Log($"[INTEGRATION] Posição: {positionCm:F1}cm -> {unityPosition:F0} Unity");
            }
        }
        
        #endregion
        
        #region Public API
        
        public TrilhoGameManager GetGameManager() => gameManager;
        public MarkerSystem GetMarkerSystem() => markerSystem;
        
        public bool IsLegacySystemEnabled() => enableLegacySystem;
        public bool IsMarkerSystemEnabled() => enableMarkerSystem;
        
        public void EnableLegacySystem(bool enable)
        {
            enableLegacySystem = enable;
            Debug.Log($"Sistema Legacy: {(enable ? "Ativado" : "Desativado")}");
        }
        
        public void EnableMarkerSystem(bool enable)
        {
            enableMarkerSystem = enable;
            Debug.Log($"Sistema de Marcadores: {(enable ? "Ativado" : "Desativado")}");
        }
        
        #endregion
    }
}
