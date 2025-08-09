using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using OscJack;

namespace Trilho
{
    [System.Serializable]
    public class ActivationZone
    {
        [Header("Zona (interno – legado)")]
        [Tooltip("Nome da zona (uso interno)")]
        public string zoneName = "New Zone";
        [Tooltip("Início da zona em cm (uso interno)")]
        public float startPositionCm = 0f;
        [Tooltip("Fim da zona em cm (uso interno)")]
        public float endPositionCm = 100f;
        
        [Header("Conteúdo (interno – legado)")]
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
        [Header("OSC (Entrada Externa)")]
        [Tooltip("Asset com porta OSC. Deixe vazio para não usar OSC.")]
        [SerializeField] private OscConnection oscConnection;
        [Tooltip("Endereço OSC ouvido (ex.: /unity)")]
        [SerializeField] private string oscAddress = "/unity";
        private OscServer oscServer;

        [Header("Mapeamento de Posição")]
        [Tooltip("Posição física mínima em cm (início do trilho)")]
        [SerializeField] private float physicalMinCm = 0f;
        [Tooltip("Posição física máxima em cm (fim do trilho)")]
        [SerializeField] private float physicalMaxCm = 600f;
        [Tooltip("Posição mínima em Unity no eixo X que corresponde ao físico mínimo")]
        [SerializeField] private float unityMinPosition = 0f;
        [Tooltip("Posição máxima em Unity no eixo X que corresponde ao físico máximo")]
        [SerializeField] private float unityMaxPosition = 8520f;

        [Header("Câmera")]
        [Tooltip("Transform da câmera que se move no eixo X")]
        [SerializeField] private Transform cameraTransform;
        [Tooltip("Suavizar o movimento da câmera")]
        [SerializeField] private bool smoothCameraMovement = true;
        [Tooltip("Fator de suavização (maior = mais suave)")]
        [SerializeField] private float cameraSmoothing = 5f;
        
        [Header("Largura da TV")]
        [Tooltip("Largura atual da TV em centímetros. Vem do TVBorderConfigurator.")]
        [SerializeField] private float screenWidthInCm = 60f;

        [Header("Zonas Internas (legado)")]
        [Tooltip("Lista de zonas usada apenas se 'Habilitar Zonas Internas' estiver ligada")]
        [SerializeField, HideInInspector] private List<ActivationZone> activationZones = new List<ActivationZone>();
        [Tooltip("Habilitar o sistema interno de zonas (legado). Mantenha DESLIGADO quando usar o TrilhoZoneActivator.")]
        [SerializeField] private bool enableInternalZones = false;
        
        [Header("Depuração / Simulação")]
        [Tooltip("Mostrar logs detalhados no Console")]
        [SerializeField] private bool showDebugInfo = true;
        [Tooltip("Mover a posição via teclado para testes (setas)")]
        [SerializeField] private bool simulatePosition = false;
        [Tooltip("Posição simulada atual em cm")]
        [SerializeField] private float simulatedPositionCm = 0f;
        [Tooltip("Passo de movimento (cm por tick) quando simulando pelo teclado")]
        [SerializeField] private float movementSensitivity = 0.5f;

        // Private
        private float currentPositionCm = 0f;
        private float currentUnityPosition = 0f;
        private TrilhoState currentState = TrilhoState.Background;
        private ActivationZone currentActiveZone = null;
        private Coroutine fadeCoroutine;

        #region Ciclo de Vida Unity

        private void Awake()
        {
            // Vincular câmera automaticamente
            if (cameraTransform == null)
                cameraTransform = Camera.main?.transform;

            // Inicializar OSC com proteção de erro
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
            // Entrada por teclado (simulação)
            HandleKeyboardInput();
            
            // Atualiza posição e câmera
            UpdatePosition();
            if (enableInternalZones)
                UpdateZones();
            UpdateCameraPosition();
        }

        private void OnDestroy()
        {
            if (oscServer != null)
            {
                oscServer.Dispose();
            }
        }

        #endregion

        #region Entrada

        private void HandleKeyboardInput()
        {
            if (!simulatePosition) return;

            if (UnityEngine.InputSystem.Keyboard.current?.leftArrowKey.isPressed == true)
            {
                simulatedPositionCm = Mathf.Max(physicalMinCm, simulatedPositionCm - movementSensitivity);
                if (showDebugInfo) Debug.Log($"[TRILHO] Movimento: {simulatedPositionCm}cm (←)");
            }
            if (UnityEngine.InputSystem.Keyboard.current?.rightArrowKey.isPressed == true)
            {
                simulatedPositionCm = Mathf.Min(physicalMaxCm, simulatedPositionCm + movementSensitivity);
                if (showDebugInfo) Debug.Log($"[TRILHO] Movimento: {simulatedPositionCm}cm (→)");
            }
        }

        #endregion

        #region OSC

        private void OnOscMessageReceived(string address, OscDataHandle data)
        {
            if (simulatePosition) return; // Ignora OSC quando simulando

            try
            {
                float position = data.GetElementAsFloat(0);
                if (position == 0f)
                {
                    int positionInt = data.GetElementAsInt(0);
                    position = positionInt;
                }
                UpdatePositionFromOsc(position);
            }
            catch (System.Exception e)
            {
                if (showDebugInfo)
                    Debug.LogWarning($"[TRILHO] Erro ao ler OSC: {e.Message}");
            }
        }

        private void UpdatePositionFromOsc(float positionCm)
        {
            currentPositionCm = Mathf.Clamp(positionCm, physicalMinCm, physicalMaxCm);
            currentUnityPosition = MapPositionToUnity(currentPositionCm);
            if (showDebugInfo)
                Debug.Log($"[TRILHO] Posição OSC: {currentPositionCm:F1}cm -> {currentUnityPosition:F2} Unity");
        }

        #endregion

        #region Posição / Câmera

        private void UpdatePosition()
        {
            if (simulatePosition)
            {
                currentPositionCm = simulatedPositionCm;
            }
            currentUnityPosition = MapPositionToUnity(currentPositionCm);
            if (showDebugInfo)
                Debug.Log($"[TRILHO] Posição lida: {currentPositionCm}cm -> Unity: {currentUnityPosition}");
        }

        private float MapPositionToUnity(float positionCm)
        {
            positionCm = Mathf.Clamp(positionCm, physicalMinCm, physicalMaxCm);
            float normalizedPosition = (positionCm - physicalMinCm) / (physicalMaxCm - physicalMinCm);
            float unityPosition = Mathf.Lerp(unityMinPosition, unityMaxPosition, normalizedPosition);
            return unityPosition;
        }

        private float MapUnityToPosition(float unityX)
        {
            float normalized = (unityX - unityMinPosition) / (unityMaxPosition - unityMinPosition);
            float cm = Mathf.Lerp(physicalMinCm, physicalMaxCm, normalized);
            return cm;
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

        #region Zonas internas (legado)

        private void UpdateZones()
        {
            if (!enableInternalZones) return;
            float visibleStartCm = currentPositionCm - (screenWidthInCm / 2f);
            float visibleEndCm = currentPositionCm + (screenWidthInCm / 2f);
            if (showDebugInfo)
                Debug.Log($"[TRILHO] Área visível: {visibleStartCm:F1}cm - {visibleEndCm:F1}cm (câmera: {currentPositionCm:F1}cm)");

            ActivationZone newActiveZone = null;
            foreach (var zone in activationZones)
            {
                bool zoneVisible = (zone.startPositionCm <= visibleEndCm && zone.endPositionCm >= visibleStartCm);
                if (zoneVisible)
                {
                    newActiveZone = zone;
                    break;
                }
            }

            if (newActiveZone != currentActiveZone)
            {
                if (currentActiveZone != null)
                {
                    ExitZone(currentActiveZone);
                }
                if (newActiveZone != null)
                {
                    EnterZone(newActiveZone);
                }
                else
                {
                    ReturnToBackground();
                }
            }
        }

        private void EnterZone(ActivationZone zone)
        {
            if (zone.contentToActivate == null) return;
            zone.contentToActivate.SetActive(true);
            var canvasGroup = zone.contentToActivate.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, zone.fadeInDuration));
            }
            var videoPlayer = zone.contentToActivate.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null)
            {
                if (!videoPlayer.isPrepared) videoPlayer.Prepare();
                videoPlayer.Play();
            }
            zone.isActive = true;
            currentActiveZone = zone;
        }

        private void ExitZone(ActivationZone zone)
        {
            if (zone.contentToActivate == null) return;
            var videoPlayer = zone.contentToActivate.GetComponent<UnityEngine.Video.VideoPlayer>();
            if (videoPlayer != null) videoPlayer.Stop();
            var canvasGroup = zone.contentToActivate.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                StartCoroutine(FadeCanvasGroup(canvasGroup, 1f, 0f, zone.fadeOutDuration, () => { zone.contentToActivate.SetActive(false); }));
            }
            else
            {
                zone.contentToActivate.SetActive(false);
            }
            zone.isActive = false;
            currentActiveZone = null;
        }

        private void ReturnToBackground() {}

        #endregion

        #region Transições (utilitário)

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

        #endregion

        #region API Pública

        public float GetCurrentPositionCm() => currentPositionCm;
        public float GetCurrentUnityPosition() => currentUnityPosition;
        public TrilhoState GetCurrentState() => currentState;
        public ActivationZone GetCurrentZone() => currentActiveZone;
        
        public float GetScreenWidthCm() => screenWidthInCm;
        public void SetScreenWidthCm(float widthCm)
        {
            screenWidthInCm = Mathf.Max(0f, widthCm);
        }
        
        public float UnityToCm(float unityX)
        {
            return MapUnityToPosition(unityX);
        }
        
        public float CmToUnity(float cm)
        {
            return MapPositionToUnity(cm);
        }
        
        public Transform GetCameraTransform() => cameraTransform;
        public void SetCameraTransform(Transform camera) => cameraTransform = camera;
        
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
    }
}