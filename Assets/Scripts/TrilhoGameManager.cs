using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OscJack;

namespace Trilho
{
    [System.Serializable]
    public class ActivationZone
    {
        [Header("Zone Settings")]
        public string zoneName = "New Zone";
        public float startPositionCm = 0f;
        public float endPositionCm = 100f;
        
        [Header("Content")]
        public GameObject contentToActivate;
        public float fadeInDuration = 0.5f;
        public float fadeOutDuration = 0.5f;
        
        [Header("Debug")]
        public bool isActive = false;
    }

    public enum TrilhoState
    {
        Background,
        InZone
    }

    public class TrilhoGameManager : MonoBehaviour
    {
        [Header("OSC Settings")]
        [SerializeField] private OscConnection oscConnection;
        [SerializeField] private string oscAddress = "/unity";
        private OscServer oscServer;

        [Header("Position Mapping")]
        [SerializeField] private float physicalMinCm = 0f;
        [SerializeField] private float physicalMaxCm = 600f;
        [SerializeField] private float unityMinPosition = 0f;
        [SerializeField] private float unityMaxPosition = 8520f;

        [Header("Camera Control")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private bool smoothCameraMovement = true;
        [SerializeField] private float cameraSmoothing = 5f;
        [SerializeField] private float screenWidthInCm = 60f; // Largura da tela em cm

        [Header("Zones")]
        [SerializeField] private List<ActivationZone> activationZones = new List<ActivationZone>();
        [SerializeField] private bool enableInternalZones = false; // desliga o sistema interno de zonas quando usar TrilhoZoneActivator
        
        [Header("Background")]
        [SerializeField] private CanvasGroup backgroundCanvasGroup;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private bool simulatePosition = false;
        [SerializeField] private float simulatedPositionCm = 0f;
        [SerializeField] private float movementSensitivity = 0.5f; // Sensibilidade do movimento

        // Private variables
        private float currentPositionCm = 0f;
        private float currentUnityPosition = 0f;
        private TrilhoState currentState = TrilhoState.Background;
        private ActivationZone currentActiveZone = null;
        private Coroutine fadeCoroutine;

        #region Unity Lifecycle

        private void Awake()
        {
            // Auto-assign camera if not set
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;

            // Auto-assign background canvas group if not set
            if (backgroundCanvasGroup == null)
            {
                var canvas = FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    var image = canvas.GetComponentInChildren<UnityEngine.UI.Image>();
                    if (image != null)
                        backgroundCanvasGroup = image.GetComponent<CanvasGroup>();
                }
            }

            // Initialize OSC with error handling
            if (oscConnection != null)
            {
                try
                {
                    oscServer = new OscServer(oscConnection.port);
                    oscServer.MessageDispatcher.AddCallback(oscAddress, OnOscMessageReceived);
                    if (showDebugInfo)
                        Debug.Log($"[TRILHO] OSC Server iniciado na porta {oscConnection.port}");
                }
                catch (System.Net.Sockets.SocketException e)
                {
                    Debug.LogWarning($"[TRILHO] Erro ao iniciar OSC Server: {e.Message}");
                    Debug.LogWarning("[TRILHO] A porta pode estar em uso. Use simulação para testar.");
                }
            }
        }

        private void Update()
        {
            // Handle keyboard input for testing
            HandleKeyboardInput();
            
            // Update position and zones
            UpdatePosition();
            if (enableInternalZones)
                UpdateZones();
            
            // Update camera
            UpdateCameraPosition();
            
            // Update position display on screen
            UpdatePositionDisplay();
        }

        private void OnDestroy()
        {
            if (oscServer != null)
            {
                oscServer.Dispose();
            }
        }

        #endregion

        #region Input Handling

        private void HandleKeyboardInput()
        {
            if (!simulatePosition) return;

            // Continuous movement with arrow keys
            if (UnityEngine.InputSystem.Keyboard.current?.leftArrowKey.isPressed == true)
            {
                simulatedPositionCm = Mathf.Max(physicalMinCm, simulatedPositionCm - movementSensitivity);
                Debug.Log($"[TRILHO] Movimento: {simulatedPositionCm}cm (seta esquerda)");
            }
            if (UnityEngine.InputSystem.Keyboard.current?.rightArrowKey.isPressed == true)
            {
                simulatedPositionCm = Mathf.Min(physicalMaxCm, simulatedPositionCm + movementSensitivity);
                Debug.Log($"[TRILHO] Movimento: {simulatedPositionCm}cm (seta direita)");
            }

            // Fine control with Shift + arrows
            if (UnityEngine.InputSystem.Keyboard.current?.leftShiftKey.isPressed == true)
            {
                if (UnityEngine.InputSystem.Keyboard.current?.leftArrowKey.isPressed == true)
                {
                    simulatedPositionCm = Mathf.Max(physicalMinCm, simulatedPositionCm - movementSensitivity / 10f);
                    Debug.Log($"[TRILHO] Movimento fino: {simulatedPositionCm}cm (Shift + seta esquerda)");
                }
                if (UnityEngine.InputSystem.Keyboard.current?.rightArrowKey.isPressed == true)
                {
                    simulatedPositionCm = Mathf.Min(physicalMaxCm, simulatedPositionCm + movementSensitivity / 10f);
                    Debug.Log($"[TRILHO] Movimento fino: {simulatedPositionCm}cm (Shift + seta direita)");
                }
            }

            // Number keys for direct jumps to specific zones
            if (UnityEngine.InputSystem.Keyboard.current?.digit1Key.wasPressedThisFrame == true) 
            {
                simulatedPositionCm = 130f; // Centro da Zona 01 (80-148cm)
                Debug.Log("[TRILHO] Movido para Zona 01 (Verde) - 130cm");
            }
            if (UnityEngine.InputSystem.Keyboard.current?.digit2Key.wasPressedThisFrame == true) 
            {
                simulatedPositionCm = 240f; // Centro da Zona 02 (190-290cm)
                Debug.Log("[TRILHO] Movido para Zona 02 (Azul) - 240cm");
            }
            if (UnityEngine.InputSystem.Keyboard.current?.digit3Key.wasPressedThisFrame == true) 
            {
                simulatedPositionCm = 370f; // Centro da Zona 03 (320-420cm)
                Debug.Log("[TRILHO] Movido para Zona 03 (Vídeo) - 370cm");
            }
            if (UnityEngine.InputSystem.Keyboard.current?.digit4Key.wasPressedThisFrame == true) 
            {
                simulatedPositionCm = 50f; // Fora das zonas
                Debug.Log("[TRILHO] Movido para fora das zonas - 50cm");
            }
            if (UnityEngine.InputSystem.Keyboard.current?.digit5Key.wasPressedThisFrame == true) 
            {
                simulatedPositionCm = 0f; // Início
                Debug.Log("[TRILHO] Movido para início - 0cm");
            }
        }

        #endregion

        #region OSC Handling

        private void OnOscMessageReceived(string address, OscDataHandle data)
        {
            if (simulatePosition) return; // Skip OSC when simulating

            try
            {
                // Try to read as float first
                float position = data.GetElementAsFloat(0);
                if (position == 0f)
                {
                    // If float is 0, try as int
                    int positionInt = data.GetElementAsInt(0);
                    position = positionInt;
                }

                UpdatePositionFromOsc(position);
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[TRILHO] Error reading OSC data: {e.Message}");
            }
        }

        private void UpdatePositionFromOsc(float positionCm)
        {
            currentPositionCm = Mathf.Clamp(positionCm, physicalMinCm, physicalMaxCm);
            currentUnityPosition = MapPositionToUnity(currentPositionCm);
            
            if (showDebugInfo)
                Debug.Log($"[TRILHO] Position: {currentPositionCm:F1}cm -> {currentUnityPosition:F2} Unity");
        }

        #endregion

        #region Position Management

        private void UpdatePosition()
        {
            // Update current position based on simulation or OSC
            if (simulatePosition)
            {
                currentPositionCm = simulatedPositionCm;
            }
            
            // Map to Unity position
            currentUnityPosition = MapPositionToUnity(currentPositionCm);
            
            // Debug position reading
            if (showDebugInfo)
            {
                Debug.Log($"[TRILHO] Posição lida: {currentPositionCm}cm -> Unity: {currentUnityPosition}");
            }
            
            // Update camera position
            UpdateCameraPosition();
            
            // Update zones
            UpdateZones();
        }

        private float MapPositionToUnity(float positionCm)
        {
            // Clamp position to physical limits
            positionCm = Mathf.Clamp(positionCm, physicalMinCm, physicalMaxCm);
            
            // Map from physical range to Unity range
            float normalizedPosition = (positionCm - physicalMinCm) / (physicalMaxCm - physicalMinCm);
            float unityPosition = Mathf.Lerp(unityMinPosition, unityMaxPosition, normalizedPosition);
            
            if (showDebugInfo)
            {
                Debug.Log($"[TRILHO] Mapeamento: {positionCm}cm -> Normalizado: {normalizedPosition:F3} -> Unity: {unityPosition:F1}");
            }
            
            return unityPosition;
        }

        private void UpdateCameraPosition()
        {
            if (cameraTransform == null) return;

            Vector3 targetPosition = cameraTransform.position;
            targetPosition.x = currentUnityPosition;

            if (smoothCameraMovement)
            {
                cameraTransform.position = Vector3.Lerp(cameraTransform.position, targetPosition, cameraSmoothing * Time.deltaTime);
            }
            else
            {
                cameraTransform.position = targetPosition;
            }
        }

        #endregion

        #region Zone Management

        private void UpdateZones()
        {
            if (!enableInternalZones) return;
            // Calculate visible area based on camera position and screen width
            float visibleStartCm = currentPositionCm - (screenWidthInCm / 2f);
            float visibleEndCm = currentPositionCm + (screenWidthInCm / 2f);
            
            Debug.Log($"[TRILHO] Área visível: {visibleStartCm:F1}cm - {visibleEndCm:F1}cm (câmera: {currentPositionCm:F1}cm)");
            
            // Find which zone is visible in the screen area
            ActivationZone newActiveZone = null;
            
            foreach (var zone in activationZones)
            {
                // Check if zone is visible in the screen area
                bool zoneVisible = (zone.startPositionCm <= visibleEndCm && zone.endPositionCm >= visibleStartCm);
                
                if (zoneVisible)
                {
                    newActiveZone = zone;
                    Debug.Log($"[TRILHO] Zona visível: {zone.zoneName} (range: {zone.startPositionCm}-{zone.endPositionCm}cm)");
                    break;
                }
            }
            
            // Handle zone changes
            if (newActiveZone != currentActiveZone)
            {
                // Exit current zone
                if (currentActiveZone != null)
                {
                    Debug.Log($"[TRILHO] Saindo da zona: {currentActiveZone.zoneName}");
                    ExitZone(currentActiveZone);
                }
                
                // Enter new zone
                if (newActiveZone != null)
                {
                    Debug.Log($"[TRILHO] Entrando na zona: {newActiveZone.zoneName} (área visível: {visibleStartCm:F1}-{visibleEndCm:F1}cm)");
                    EnterZone(newActiveZone);
                }
                else
                {
                    // No active zone - return to background
                    Debug.Log("[TRILHO] Nenhuma zona ativa - retornando ao background");
                    ReturnToBackground();
                }
            }
        }

        private ActivationZone GetActiveZone()
        {
            foreach (var zone in activationZones)
            {
                if (currentPositionCm >= zone.startPositionCm && currentPositionCm <= zone.endPositionCm)
                {
                    return zone;
                }
            }
            return null;
        }

        private void EnterZone(ActivationZone zone)
        {
            if (zone.contentToActivate == null) return;
            
            Debug.Log($"[TRILHO] Ativando conteúdo da zona: {zone.zoneName} (posição atual: {currentPositionCm}cm)");
            
            // Activate content immediately when entering zone (at left edge)
            zone.contentToActivate.SetActive(true);
            
            // Fade in content
            var canvasGroup = zone.contentToActivate.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, zone.fadeInDuration));
            }
            
            // Handle video content specifically
            var videoPlayer = zone.contentToActivate.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                Debug.Log($"[TRILHO] Preparando vídeo para zona: {zone.zoneName}");
                if (!videoPlayer.isPrepared)
                {
                    videoPlayer.Prepare();
                }
                videoPlayer.Play();
            }
            
            zone.isActive = true;
            currentActiveZone = zone;
        }

        private void ExitZone(ActivationZone zone)
        {
            if (zone.contentToActivate == null) return;
            
            Debug.Log($"[TRILHO] Saindo da zona: {zone.zoneName}");
            
            // Handle video content specifically
            var videoPlayer = zone.contentToActivate.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                Debug.Log($"[TRILHO] Parando vídeo da zona: {zone.zoneName}");
                videoPlayer.Stop();
            }
            
            // Fade out content
            var canvasGroup = zone.contentToActivate.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, zone.fadeOutDuration, () =>
                {
                    zone.contentToActivate.SetActive(false);
                }));
            }
            else
            {
                zone.contentToActivate.SetActive(false);
            }
            
            zone.isActive = false;
            currentActiveZone = null;
        }

        private void ReturnToBackground()
        {
            Debug.Log("[TRILHO] Retornando ao background");
            // Background is always visible, no fade needed
        }

        #endregion

        #region Transitions

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float targetAlpha, float duration)
        {
            if (canvasGroup == null) yield break;

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

        private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float targetAlpha, float duration, System.Action onComplete)
        {
            if (canvasGroup == null) yield break;

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

        private IEnumerator FadeOutAndDestroy(GameObject obj, CanvasGroup canvasGroup, float duration)
        {
            yield return StartCoroutine(FadeCanvasGroup(canvasGroup, 0f, duration));
            obj.SetActive(false);
        }

        #endregion

        #region Public API

        public float GetCurrentPositionCm() => currentPositionCm;
        public float GetCurrentUnityPosition() => currentUnityPosition;
        public TrilhoState GetCurrentState() => currentState;
        public ActivationZone GetCurrentZone() => currentActiveZone;
        
        // Novo: largura da janela/TV em cm
        public float GetScreenWidthCm() => screenWidthInCm;
        public void SetScreenWidthCm(float widthCm)
        {
            screenWidthInCm = Mathf.Max(0f, widthCm);
        }
        
        // Conversão pública Unity->cm (wrapper)
        public float UnityToCm(float unityX)
        {
            return MapUnityToPosition(unityX);
        }
        
        // Conversão pública cm->Unity (para gizmos e debug)
        public float CmToUnity(float cm)
        {
            return MapPositionToUnity(cm);
        }
        
        // Camera access methods
        public Transform GetCameraTransform() => cameraTransform;
        public void SetCameraTransform(Transform camera) => cameraTransform = camera;
        
        // Zones access methods
        public List<ActivationZone> GetActivationZones() => activationZones;
        public void SetEnableInternalZones(bool enabled) { enableInternalZones = enabled; }

        public void SimulatePosition(float positionCm)
        {
            simulatePosition = true;
            simulatedPositionCm = positionCm;
        }

        public void StopSimulation()
        {
            simulatePosition = false;
        }

        public void AddZone(ActivationZone zone)
        {
            if (zone != null && !activationZones.Contains(zone))
            {
                activationZones.Add(zone);
            }
        }

        public void RemoveZone(ActivationZone zone)
        {
            if (zone != null && activationZones.Contains(zone))
            {
                activationZones.Remove(zone);
            }
        }

        #endregion

        #region Context Menu Methods

        // === 1. CONFIGURAÇÃO INICIAL ===
        [ContextMenu("1. Verificar Background")]
        public void VerificarBackground()
        {
            Debug.Log("=== VERIFICAÇÃO DO BACKGROUND ===");
            
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            Debug.Log($"Canvas encontrado: {canvas.name}");
            Debug.Log($"Render Mode: {canvas.renderMode}");
            
            // Check for background image
            var backgroundImage = canvas.GetComponentInChildren<UnityEngine.UI.Image>();
            if (backgroundImage != null)
            {
                Debug.Log($"Background Image encontrado: {backgroundImage.name}");
                Debug.Log($"Color: {backgroundImage.color}");
                
                var rectTransform = backgroundImage.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    Debug.Log($"Size: {rectTransform.sizeDelta}");
                    Debug.Log($"Anchors: Min({rectTransform.anchorMin}), Max({rectTransform.anchorMax})");
                }
                
                // Check CanvasGroup
                var canvasGroup = backgroundImage.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    Debug.Log($"CanvasGroup Alpha: {canvasGroup.alpha}");
                }
                else
                {
                    Debug.LogWarning("Background não tem CanvasGroup!");
                }
            }
            else
            {
                Debug.LogWarning("Background Image não encontrado!");
                Debug.Log("Criando background...");
                CriarBackground();
            }
            
            Debug.Log("================================");
        }

        // === 2. LIMPEZA E CRIAÇÃO ===
        [ContextMenu("2. Limpar Tudo")]
        public void LimparTudo()
        {
            Debug.Log("=== LIMPEZA COMPLETA ===");
            
            // Clear zones list
            activationZones.Clear();
            
            // Find Canvas and clean only zone content
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"Limpando Canvas: {canvas.name}");
                
                // Get all children except the Canvas itself
                var children = new List<Transform>();
                for (int i = 0; i < canvas.transform.childCount; i++)
                {
                    children.Add(canvas.transform.GetChild(i));
                }
                
                // Destroy only zone content objects, preserve background
                foreach (var child in children)
                {
                    if (child.name.Contains("Conteúdo") || 
                        child.name.Contains("Posição"))
                    {
                        Debug.Log($"Removendo: {child.name}");
                        DestroyImmediate(child.gameObject);
                    }
                    else
                    {
                        Debug.Log($"Preservando: {child.name}");
                    }
                }
                
                Debug.Log("Canvas limpo! Background preservado.");
            }
            else
            {
                Debug.LogWarning("Canvas não encontrado!");
            }
            
            // Clear any orphaned content objects
            var orphanedContent = FindObjectsOfType<GameObject>();
            foreach (var obj in orphanedContent)
            {
                if (obj.name.Contains("Conteúdo") && obj.transform.parent == null)
                {
                    Debug.Log($"Removendo objeto órfão: {obj.name}");
                    DestroyImmediate(obj);
                }
            }
            
            Debug.Log("=== LIMPEZA CONCLUÍDA ===");
        }

        [ContextMenu("3. Criar Zonas de Debug")]
        public void CriarZonasDeDebug()
        {
            Debug.Log("=== CRIANDO ZONAS DE DEBUG ===");
            
            // First, clean everything
            LimparTudo();
            
            // Wait a frame to ensure cleanup is complete
            StartCoroutine(CriarZonasAposLimpeza());
        }

        // === 4. CORREÇÃO DE VALORES ===
        [ContextMenu("4. Corrigir Valores")]
        public void CorrigirValores()
        {
            Debug.Log("=== CORRIGINDO VALORES ===");
            
            // Force correct values
            unityMaxPosition = 8520f;
            physicalMaxCm = 600f;
            physicalMinCm = 0f;
            unityMinPosition = 0f;
            movementSensitivity = 0.5f;
            showDebugInfo = true;
            screenWidthInCm = 60f; // Largura da tela em cm
            
            Debug.Log("Valores corrigidos:");
            Debug.Log($"Physical: {physicalMinCm}cm a {physicalMaxCm}cm");
            Debug.Log($"Unity: {unityMinPosition} a {unityMaxPosition}");
            Debug.Log($"Sensibilidade: {movementSensitivity}");
            Debug.Log($"Largura da tela: {screenWidthInCm}cm");
            Debug.Log($"Debug Info: {showDebugInfo}");
            
            // Force Unity to save the changes
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            
            Debug.Log("=== VALORES CORRIGIDOS ===");
        }

        // === 5. DEBUG E TESTE ===
        [ContextMenu("5. Mostrar Info das Zonas")]
        public void MostrarInfoDasZonas()
        {
            Debug.Log("=== INFORMAÇÕES DAS ZONAS ===");
            for (int i = 0; i < activationZones.Count; i++)
            {
                var zone = activationZones[i];
                Debug.Log($"Zona {i + 1}: {zone.zoneName}");
                Debug.Log($"  Posição: {zone.startPositionCm}cm a {zone.endPositionCm}cm");
                Debug.Log($"  Conteúdo: {(zone.contentToActivate != null ? zone.contentToActivate.name : "Nenhum")}");
                Debug.Log($"  Ativa: {zone.isActive}");
                Debug.Log($"  Fade In: {zone.fadeInDuration}s, Fade Out: {zone.fadeOutDuration}s");
                Debug.Log("---");
            }
            Debug.Log("=============================");
        }

        [ContextMenu("6. Testar Vídeo")]
        public void TestarVideo()
        {
            Debug.Log("=== TESTE DE VÍDEO ===");
            
            // Find video content
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            var videoContent = canvas.transform.Find("Conteúdo Zona 03");
            if (videoContent == null)
            {
                Debug.LogError("Conteúdo de vídeo não encontrado! Execute '3. Criar Zonas de Debug' primeiro.");
                return;
            }
            
            var videoPlayer = videoContent.GetComponent<UnityEngine.Video.VideoPlayer>();
            var rawImage = videoContent.GetComponent<UnityEngine.UI.RawImage>();
            
            if (videoPlayer == null)
            {
                Debug.LogError("VideoPlayer não encontrado!");
                return;
            }
            
            if (rawImage == null)
            {
                Debug.LogError("RawImage não encontrado!");
                return;
            }
            
            Debug.Log($"VideoPlayer encontrado: {videoPlayer.name}");
            Debug.Log($"URL: {videoPlayer.url}");
            Debug.Log($"Render Mode: {videoPlayer.renderMode}");
            Debug.Log($"Target Texture: {(videoPlayer.targetTexture != null ? "✓" : "✗")}");
            Debug.Log($"RawImage Texture: {(rawImage.texture != null ? "✓" : "✗")}");
            Debug.Log($"Is Playing: {videoPlayer.isPlaying}");
            Debug.Log($"Is Prepared: {videoPlayer.isPrepared}");
            
            // Test video playback
            if (!videoPlayer.isPrepared)
            {
                Debug.Log("Preparando vídeo...");
                videoPlayer.Prepare();
            }
            
            if (videoPlayer.isPrepared && !videoPlayer.isPlaying)
            {
                Debug.Log("Iniciando reprodução...");
                videoPlayer.Play();
            }
            
            // Activate content for testing
            videoContent.gameObject.SetActive(true);
            var canvasGroup = videoContent.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
            }
            
            Debug.Log("Vídeo ativado para teste. Verifique na tela.");
        }

        [ContextMenu("7. Configurar Largura da Tela")]
        public void ConfigurarLarguraDaTela()
        {
            Debug.Log("=== CONFIGURAÇÃO DA LARGURA DA TELA ===");
            
            // Remove existing indicators
            var existingIndicators = GameObject.Find("Indicadores Visuais");
            if (existingIndicators != null)
            {
                DestroyImmediate(existingIndicators);
            }
            
            // Create container for indicators
            var container = new GameObject("Indicadores Visuais");
            
            // Create left collision area (red)
            var leftArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftArea.name = "Área Colisão Esquerda";
            leftArea.transform.SetParent(container.transform);
            leftArea.transform.position = new Vector3(-500, 0, 10);
            leftArea.transform.localScale = new Vector3(100, 2000, 100);
            
            var leftRenderer = leftArea.GetComponent<Renderer>();
            leftRenderer.material.color = new Color(1, 0, 0, 0.5f);
            leftRenderer.material.SetFloat("_Mode", 3);
            leftRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            leftRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            leftRenderer.material.SetInt("_ZWrite", 0);
            leftRenderer.material.DisableKeyword("_ALPHATEST_ON");
            leftRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            leftRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            leftRenderer.material.renderQueue = 3000;
            
            // Create right collision area (red)
            var rightArea = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightArea.name = "Área Colisão Direita";
            rightArea.transform.SetParent(container.transform);
            rightArea.transform.position = new Vector3(500, 0, 10);
            rightArea.transform.localScale = new Vector3(100, 2000, 100);
            
            var rightRenderer = rightArea.GetComponent<Renderer>();
            rightRenderer.material.color = new Color(1, 0, 0, 0.5f);
            rightRenderer.material.SetFloat("_Mode", 3);
            rightRenderer.material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            rightRenderer.material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            rightRenderer.material.SetInt("_ZWrite", 0);
            rightRenderer.material.DisableKeyword("_ALPHATEST_ON");
            rightRenderer.material.EnableKeyword("_ALPHABLEND_ON");
            rightRenderer.material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            rightRenderer.material.renderQueue = 3000;
            
            // Create text indicator
            var textIndicator = new GameObject("Instruções");
            textIndicator.transform.SetParent(container.transform);
            textIndicator.transform.position = new Vector3(0, 1000, 10);
            
            var canvas = textIndicator.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = Camera.main;
            
            var text = textIndicator.AddComponent<UnityEngine.UI.Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 48;
            text.color = Color.white;
            text.alignment = TextAnchor.UpperCenter;
            text.text = "Mova as áreas vermelhas para as bordas da tela\nDepois execute '8. Salvar Largura'";
            
            var textRect = textIndicator.GetComponent<RectTransform>();
            textRect.sizeDelta = new Vector2(800, 200);
            
            Debug.Log("Áreas de colisão criadas!");
            Debug.Log("1. Mova as áreas vermelhas para as bordas da tela");
            Debug.Log("2. Execute '8. Salvar Largura' para salvar");
        }

        [ContextMenu("8. Salvar Largura")]
        public void SalvarLargura()
        {
            Debug.Log("=== SALVANDO LARGURA DA TELA ===");
            
            var leftArea = GameObject.Find("Área Colisão Esquerda");
            var rightArea = GameObject.Find("Área Colisão Direita");
            
            if (leftArea == null || rightArea == null)
            {
                Debug.LogError("Áreas de colisão não encontradas! Execute '7. Configurar Largura da Tela' primeiro.");
                return;
            }
            
            // Get positions
            float leftUnityPos = leftArea.transform.position.x;
            float rightUnityPos = rightArea.transform.position.x;
            
            // Convert to cm
            float leftCm = MapUnityToPosition(leftUnityPos);
            float rightCm = MapUnityToPosition(rightUnityPos);
            float calculatedWidth = rightCm - leftCm;
            
            Debug.Log($"Posição esquerda: {leftUnityPos:F0} Unity = {leftCm:F1}cm");
            Debug.Log($"Posição direita: {rightUnityPos:F0} Unity = {rightCm:F1}cm");
            Debug.Log($"Largura calculada: {calculatedWidth:F1}cm");
            
            // Save width
            screenWidthInCm = calculatedWidth;
            
            Debug.Log($"Largura da tela salva: {screenWidthInCm:F1}cm");
            Debug.Log("Execute '8. Remover Indicadores' para limpar");
        }

        [ContextMenu("9. Remover Indicadores")]
        public void RemoverIndicadores()
        {
            var indicators = GameObject.Find("Indicadores Visuais");
            if (indicators != null)
            {
                DestroyImmediate(indicators);
                Debug.Log("Indicadores visuais removidos!");
            }
        }

        private float MapUnityToPosition(float unityPosition)
        {
            // Reverse mapping from Unity to physical
            float normalizedPosition = (unityPosition - unityMinPosition) / (unityMaxPosition - unityMinPosition);
            float physicalPosition = Mathf.Lerp(physicalMinCm, physicalMaxCm, normalizedPosition);
            return physicalPosition;
        }

        #endregion

        private void CriarBackground()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null) return;
            
            // Create background
            var backgroundObj = new GameObject("Background Image");
            backgroundObj.transform.SetParent(canvas.transform, false);
            
            var backgroundImage = backgroundObj.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.color = Color.white; // Default white
            
            // Set RectTransform to fill canvas
            var rectTransform = backgroundImage.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Add CanvasGroup
            var canvasGroup = backgroundObj.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 1f;
            
            // Assign to GameManager
            backgroundCanvasGroup = canvasGroup;
            
            Debug.Log("Background criado e configurado!");
        }
        
        private IEnumerator CriarZonasAposLimpeza()
        {
            // Wait for cleanup to complete
            yield return null;
            
            // Clear existing zones and content
            activationZones.Clear();

            // Find or create Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("[TRILHO] Canvas não encontrado!");
                yield break;
            }

            // Verify background first
            Debug.Log("Verificando background...");
            var backgroundImage = canvas.GetComponentInChildren<UnityEngine.UI.Image>();
            if (backgroundImage == null)
            {
                Debug.Log("Background não encontrado, criando...");
                CriarBackground();
            }
            else
            {
                Debug.Log($"Background encontrado: {backgroundImage.name}");
                // Ensure it has CanvasGroup
                var canvasGroup = backgroundImage.GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = backgroundImage.gameObject.AddComponent<CanvasGroup>();
                    canvasGroup.alpha = 1f;
                    Debug.Log("CanvasGroup adicionado ao background");
                }
                backgroundCanvasGroup = canvasGroup;
            }

            Debug.Log("Criando Zona 01 (Verde)...");
            // Zone 01 - Green background (80cm to 148cm)
            var zona01 = new ActivationZone
            {
                zoneName = "Zona 01 - Verde",
                startPositionCm = 80f,
                endPositionCm = 148f, // Updated to match pattern
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f
            };

            var conteudo01 = new GameObject("Conteúdo Zona 01");
            conteudo01.transform.SetParent(canvas.transform, false);
            var image01 = conteudo01.AddComponent<UnityEngine.UI.Image>();
            image01.color = Color.green;
            
            // Configure RectTransform to fill canvas properly
            var rect01 = conteudo01.GetComponent<RectTransform>();
            rect01.anchorMin = Vector2.zero;  // Bottom-left
            rect01.anchorMax = Vector2.one;   // Top-right
            rect01.sizeDelta = Vector2.zero;  // Fill parent
            rect01.anchoredPosition = Vector2.zero; // Center in parent
            rect01.localPosition = Vector3.zero; // Ensure no offset
            rect01.localScale = Vector3.one;   // Normal scale
            
            var canvasGroup01 = conteudo01.AddComponent<CanvasGroup>();
            canvasGroup01.alpha = 0f;
            conteudo01.SetActive(false);
            zona01.contentToActivate = conteudo01;

            Debug.Log("Criando Zona 02 (Azul)...");
            // Zone 02 - Blue background (190cm to 290cm)
            var zona02 = new ActivationZone
            {
                zoneName = "Zona 02 - Azul",
                startPositionCm = 190f,
                endPositionCm = 290f, // 100cm width
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f
            };

            var conteudo02 = new GameObject("Conteúdo Zona 02");
            conteudo02.transform.SetParent(canvas.transform, false);
            var image02 = conteudo02.AddComponent<UnityEngine.UI.Image>();
            image02.color = Color.blue;
            
            // Configure RectTransform to fill canvas properly
            var rect02 = conteudo02.GetComponent<RectTransform>();
            rect02.anchorMin = Vector2.zero;  // Bottom-left
            rect02.anchorMax = Vector2.one;   // Top-right
            rect02.sizeDelta = Vector2.zero;  // Fill parent
            rect02.anchoredPosition = Vector2.zero; // Center in parent
            rect02.localPosition = Vector3.zero; // Ensure no offset
            rect02.localScale = Vector3.one;   // Normal scale
            
            var canvasGroup02 = conteudo02.AddComponent<CanvasGroup>();
            canvasGroup02.alpha = 0f;
            conteudo02.SetActive(false);
            zona02.contentToActivate = conteudo02;

            Debug.Log("Criando Zona 03 (Vídeo)...");
            // Zone 03 - Video (320cm to 420cm)
            var zona03 = new ActivationZone
            {
                zoneName = "Zona 03 - Vídeo",
                startPositionCm = 320f,
                endPositionCm = 420f, // 100cm width
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f
            };

            var conteudo03 = new GameObject("Conteúdo Zona 03");
            conteudo03.transform.SetParent(canvas.transform, false);
            
            // Add VideoPlayer
            var videoPlayer = conteudo03.AddComponent<UnityEngine.Video.VideoPlayer>();
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
            
            var renderTexture = new RenderTexture(1080, 1920, 24);
            videoPlayer.targetTexture = renderTexture;
            
            // Add RawImage to display video
            var rawImage = conteudo03.AddComponent<UnityEngine.UI.RawImage>();
            rawImage.texture = renderTexture;
            
            // Configure RectTransform to fill canvas properly
            var rect03 = conteudo03.GetComponent<RectTransform>();
            rect03.anchorMin = Vector2.zero;  // Bottom-left
            rect03.anchorMax = Vector2.one;   // Top-right
            rect03.sizeDelta = Vector2.zero;  // Fill parent
            rect03.anchoredPosition = Vector2.zero; // Center in parent
            rect03.localPosition = Vector3.zero; // Ensure no offset
            rect03.localScale = Vector3.one;   // Normal scale
            
            // Set video URL
            string videoPath = "file://" + Application.dataPath + "/Videos/video.mp4";
            videoPlayer.url = videoPath;
            
            var canvasGroup03 = conteudo03.AddComponent<CanvasGroup>();
            canvasGroup03.alpha = 0f;
            conteudo03.SetActive(false);
            zona03.contentToActivate = conteudo03;

            // Add zones to list
            activationZones.Add(zona01);
            activationZones.Add(zona02);
            activationZones.Add(zona03);

            Debug.Log("=== ZONAS CRIADAS COM SUCESSO ===");
            Debug.Log("Zona 01: 80-148cm (Verde) - Resolução 1080x1920");
            Debug.Log("Zona 02: 190-290cm (Azul) - Resolução 1080x1920");
            Debug.Log("Zona 03: 320-420cm (Vídeo) - Videos/video.mp4");
            Debug.Log("Hierarquia limpa e organizada!");
        }

        private void UpdatePositionDisplay()
        {
            var positionText = GameObject.Find("Posição Atual");
            if (positionText != null)
            {
                var textComponent = positionText.GetComponent<UnityEngine.UI.Text>();
                if (textComponent != null)
                {
                    textComponent.text = $"Posição: {currentPositionCm:F1}cm\nUnity: {currentUnityPosition:F0}\nZona: {(currentActiveZone?.zoneName ?? "Nenhuma")}";
                }
            }
        }
    }
}