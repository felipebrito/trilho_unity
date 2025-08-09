using UnityEngine;
using System.Collections;
using OscJack;

namespace Trilho
{
    // Zonas internas foram removidas. Ativação é responsabilidade do TrilhoZoneActivator.

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
        // Estado/Zonas internos removidos

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

        // Zonas internas e transições foram removidas; ativação é feita pelo TrilhoZoneActivator.

        #region API Pública

        public float GetCurrentPositionCm() => currentPositionCm;
        public float GetCurrentUnityPosition() => currentUnityPosition;
        
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
        
        // Sistema interno de zonas removido
        
        public void SimulatePosition(float positionCm)
        {
            simulatePosition = true;
            simulatedPositionCm = positionCm;
        }

        public void StopSimulation()
        {
            simulatePosition = false;
        }

        #endregion
    }
}