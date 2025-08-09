using UnityEngine;
using Trilho;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace Trilho.Examples
{
    /// <summary>
    /// Controlador universal que funciona tanto com Input System quanto com Input Legacy
    /// Detecta automaticamente qual sistema está ativo
    /// </summary>
    public class TrilhoUniversalInputController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private TrilhoGameManager trilhoManager;
        [SerializeField] private float movementSpeed = 100f; // cm/s
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("Zone Positions (cm)")]
        [SerializeField] private float zone1Position = 65f;
        [SerializeField] private float zone2Position = 185f;
        [SerializeField] private float zone3Position = 340f;
        [SerializeField] private float zone4Position = 485f;
        [SerializeField] private float backgroundPosition = 550f;
        
        private bool useInputSystem = false;
        
        private void Start()
        {
            // Find TrilhoGameManager if not assigned
            if (trilhoManager == null)
            {
                trilhoManager = FindFirstObjectByType<TrilhoGameManager>();
            }
            
            if (trilhoManager == null)
            {
                Debug.LogError("TrilhoGameManager not found! Please assign it or add TrilhoGameManager to the scene.");
                enabled = false;
                return;
            }
            
            // Detect which input system is being used
            DetectInputSystem();
            
            if (showDebugInfo)
            {
                PrintKeyboardHelp();
            }
        }
        
        private void DetectInputSystem()
        {
#if ENABLE_INPUT_SYSTEM
            useInputSystem = true;
            Debug.Log("Using Input System package");
#else
            useInputSystem = false;
            Debug.Log("Using Legacy Input");
#endif
        }
        
        private void Update()
        {
            if (useInputSystem)
            {
                HandleInputSystem();
            }
            else
            {
                HandleLegacyInput();
            }
        }
        
#if ENABLE_INPUT_SYSTEM
        private void HandleInputSystem()
        {
            if (trilhoManager == null) return;
            
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            
            float currentPos = trilhoManager.GetCurrentPositionCm();
            
            // Continuous movement with arrow keys
            if (keyboard.leftArrowKey.isPressed)
            {
                float newPos = Mathf.Max(0f, currentPos - movementSpeed * Time.deltaTime);
                trilhoManager.SimulatePosition(newPos);
            }
            else if (keyboard.rightArrowKey.isPressed)
            {
                float newPos = Mathf.Min(600f, currentPos + movementSpeed * Time.deltaTime);
                trilhoManager.SimulatePosition(newPos);
            }
            
            // Direct jumps with number keys
            if (keyboard.digit0Key.wasPressedThisFrame)
                JumpToPosition(0f, "Start");
            else if (keyboard.digit1Key.wasPressedThisFrame)
                JumpToPosition(zone1Position, "Zone 1");
            else if (keyboard.digit2Key.wasPressedThisFrame)
                JumpToPosition(zone2Position, "Zone 2");
            else if (keyboard.digit3Key.wasPressedThisFrame)
                JumpToPosition(zone3Position, "Zone 3");
            else if (keyboard.digit4Key.wasPressedThisFrame)
                JumpToPosition(zone4Position, "Zone 4");
            else if (keyboard.digit5Key.wasPressedThisFrame)
                JumpToPosition(backgroundPosition, "Background");
            
            // Additional controls
            if (keyboard.rKey.wasPressedThisFrame)
                JumpToPosition(0f, "Reset to Start");
            else if (keyboard.eKey.wasPressedThisFrame)
                JumpToPosition(600f, "End");
            else if (keyboard.hKey.wasPressedThisFrame)
                PrintKeyboardHelp();
            else if (keyboard.iKey.wasPressedThisFrame)
                PrintCurrentInfo();
            else if (keyboard.qKey.wasPressedThisFrame)
                StopSimulation();
        }
#endif
        
        private void HandleLegacyInput()
        {
#if !ENABLE_INPUT_SYSTEM || ENABLE_LEGACY_INPUT_MANAGER
            if (trilhoManager == null) return;
            
            float currentPos = trilhoManager.GetCurrentPositionCm();
            
            // Continuous movement with arrow keys
            if (Input.GetKey(KeyCode.LeftArrow))
            {
                float newPos = Mathf.Max(0f, currentPos - movementSpeed * Time.deltaTime);
                trilhoManager.SimulatePosition(newPos);
            }
            else if (Input.GetKey(KeyCode.RightArrow))
            {
                float newPos = Mathf.Min(600f, currentPos + movementSpeed * Time.deltaTime);
                trilhoManager.SimulatePosition(newPos);
            }
            
            // Direct jumps with number keys
            if (Input.GetKeyDown(KeyCode.Alpha0))
                JumpToPosition(0f, "Start");
            else if (Input.GetKeyDown(KeyCode.Alpha1))
                JumpToPosition(zone1Position, "Zone 1");
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                JumpToPosition(zone2Position, "Zone 2");
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                JumpToPosition(zone3Position, "Zone 3");
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                JumpToPosition(zone4Position, "Zone 4");
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                JumpToPosition(backgroundPosition, "Background");
            
            // Additional controls
            if (Input.GetKeyDown(KeyCode.R))
                JumpToPosition(0f, "Reset to Start");
            else if (Input.GetKeyDown(KeyCode.E))
                JumpToPosition(600f, "End");
            else if (Input.GetKeyDown(KeyCode.H))
                PrintKeyboardHelp();
            else if (Input.GetKeyDown(KeyCode.I))
                PrintCurrentInfo();
            else if (Input.GetKeyDown(KeyCode.Q))
                StopSimulation();
#else
            // Input System is active but this method shouldn't be called
            Debug.LogWarning("Legacy input called but Input System is active!");
#endif
        }
        
        private void JumpToPosition(float position, string label)
        {
            if (trilhoManager != null)
            {
                trilhoManager.SimulatePosition(position);
                if (showDebugInfo)
                {
                    Debug.Log($"[Universal Input] Jumped to {label} ({position}cm)");
                }
            }
        }
        
        private void StopSimulation()
        {
            if (trilhoManager != null)
            {
                trilhoManager.StopSimulation();
                Debug.Log("[Universal Input] Simulation stopped");
            }
        }
        
        private void PrintKeyboardHelp()
        {
            string inputType = useInputSystem ? "Input System" : "Legacy Input";
            Debug.Log($"=== TRILHO UNIVERSAL KEYBOARD CONTROLS ({inputType}) ===");
            Debug.Log("Movement:");
            Debug.Log("  ← → : Move Left/Right");
            Debug.Log($"  Speed: {movementSpeed} cm/s");
            Debug.Log("");
            Debug.Log("Quick Jump:");
            Debug.Log($"  0 : Start (0cm)");
            Debug.Log($"  1 : Zone 1 ({zone1Position}cm)");
            Debug.Log($"  2 : Zone 2 ({zone2Position}cm)");
            Debug.Log($"  3 : Zone 3 ({zone3Position}cm)");
            Debug.Log($"  4 : Zone 4 ({zone4Position}cm)");
            Debug.Log($"  5 : Background ({backgroundPosition}cm)");
            Debug.Log($"  E : End (600cm)");
            Debug.Log("");
            Debug.Log("Other:");
            Debug.Log("  R : Reset to Start");
            Debug.Log("  H : Show this Help");
            Debug.Log("  I : Show current Info");
            Debug.Log("  Q : Stop simulation");
            Debug.Log("================================================");
        }
        
        private void PrintCurrentInfo()
        {
            if (trilhoManager == null) return;
            
            float currentPos = trilhoManager.GetCurrentPositionCm();
            float unityPos = trilhoManager.GetCurrentUnityPosition();
            
            string inputType = useInputSystem ? "Input System" : "Legacy Input";
            Debug.Log($"=== TRILHO INFO ({inputType}) ===");
            Debug.Log($"Posição: {currentPos:F1}cm");
            Debug.Log($"Unity X: {unityPos:F3}");
            Debug.Log("===============================");
        }
        
        // Context menu methods for easy testing
        [ContextMenu("Jump to Zone 1")]
        private void MenuJumpZone1() => JumpToPosition(zone1Position, "Zone 1");
        
        [ContextMenu("Jump to Zone 2")]
        private void MenuJumpZone2() => JumpToPosition(zone2Position, "Zone 2");
        
        [ContextMenu("Jump to Zone 3")]
        private void MenuJumpZone3() => JumpToPosition(zone3Position, "Zone 3");
        
        [ContextMenu("Jump to Zone 4")]
        private void MenuJumpZone4() => JumpToPosition(zone4Position, "Zone 4");
        
        [ContextMenu("Jump to Background")]
        private void MenuJumpBackground() => JumpToPosition(backgroundPosition, "Background");
        
        [ContextMenu("Print Help")]
        private void MenuPrintHelp() => PrintKeyboardHelp();
        
        [ContextMenu("Print Info")]
        private void MenuPrintInfo() => PrintCurrentInfo();
        
        private void OnEnable()
        {
            if (showDebugInfo && trilhoManager != null)
            {
                string inputType = useInputSystem ? "Input System" : "Legacy Input";
                Debug.Log($"TrilhoUniversalInputController enabled ({inputType}). Press H for help.");
            }
        }
        
        private void OnDisable()
        {
            StopSimulation();
        }
    }
}
