using UnityEngine;
using UnityEngine.InputSystem;
using Trilho;

namespace Trilho.Examples
{
    /// <summary>
    /// Controle simples por teclado para testes do sistema Trilho
    /// Use este script para testes rápidos sem interface visual
    /// </summary>
    public class TrilhoKeyboardController : MonoBehaviour
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
        
        private void Start()
        {
            // Find TrilhoGameManager if not assigned
            if (trilhoManager == null)
            {
                trilhoManager = FindFirstObjectByType<TrilhoGameManager>();
            }
            
            if (trilhoManager == null)
            {
                UnityEngine.Debug.LogError("TrilhoGameManager not found! Please assign it or add TrilhoGameManager to the scene.");
                enabled = false;
                return;
            }
            
            if (showDebugInfo)
            {
                PrintKeyboardHelp();
            }
        }
        
        private void Update()
        {
            HandleKeyboardInput();
        }
        
        private void HandleKeyboardInput()
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
            {
                JumpToPosition(0f, "Start");
            }
            else if (keyboard.digit1Key.wasPressedThisFrame)
            {
                JumpToPosition(zone1Position, "Zone 1");
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                JumpToPosition(zone2Position, "Zone 2");
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                JumpToPosition(zone3Position, "Zone 3");
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                JumpToPosition(zone4Position, "Zone 4");
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                JumpToPosition(backgroundPosition, "Background");
            }
            
            // Additional controls
            if (keyboard.rKey.wasPressedThisFrame)
            {
                JumpToPosition(0f, "Reset to Start");
            }
            else if (keyboard.eKey.wasPressedThisFrame)
            {
                JumpToPosition(600f, "End");
            }
            else if (keyboard.hKey.wasPressedThisFrame)
            {
                PrintKeyboardHelp();
            }
            else if (keyboard.iKey.wasPressedThisFrame)
            {
                PrintCurrentInfo();
            }
            else if (keyboard.qKey.wasPressedThisFrame)
            {
                if (trilhoManager != null)
                {
                    trilhoManager.StopSimulation();
                    UnityEngine.Debug.Log("Simulation stopped");
                }
            }
        }
        
        private void JumpToPosition(float position, string label)
        {
            if (trilhoManager != null)
            {
                trilhoManager.SimulatePosition(position);
                if (showDebugInfo)
                {
                    UnityEngine.Debug.Log($"Jumped to {label} ({position}cm)");
                }
            }
        }
        
        private void PrintKeyboardHelp()
        {
            UnityEngine.Debug.Log("=== TRILHO KEYBOARD CONTROLS ===");
            UnityEngine.Debug.Log("Movement:");
            UnityEngine.Debug.Log("  ← → : Move Left/Right");
            UnityEngine.Debug.Log($"  Speed: {movementSpeed} cm/s");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("Quick Jump:");
            UnityEngine.Debug.Log($"  0 : Start (0cm)");
            UnityEngine.Debug.Log($"  1 : Zone 1 ({zone1Position}cm)");
            UnityEngine.Debug.Log($"  2 : Zone 2 ({zone2Position}cm)");
            UnityEngine.Debug.Log($"  3 : Zone 3 ({zone3Position}cm)");
            UnityEngine.Debug.Log($"  4 : Zone 4 ({zone4Position}cm)");
            UnityEngine.Debug.Log($"  5 : Background ({backgroundPosition}cm)");
            UnityEngine.Debug.Log($"  E : End (600cm)");
            UnityEngine.Debug.Log("");
            UnityEngine.Debug.Log("Other:");
            UnityEngine.Debug.Log("  R : Reset to Start");
            UnityEngine.Debug.Log("  H : Show this Help");
            UnityEngine.Debug.Log("  I : Show current Info");
            UnityEngine.Debug.Log("  Q : Stop simulation");
            UnityEngine.Debug.Log("================================");
        }
        
        private void PrintCurrentInfo()
        {
            if (trilhoManager == null) return;
            
            float currentPos = trilhoManager.GetCurrentPositionCm();
            float unityPos = trilhoManager.GetCurrentUnityPosition();
            
            UnityEngine.Debug.Log("=== INFO ATUAL DO TRILHO ===");
            UnityEngine.Debug.Log($"Posição: {currentPos:F1}cm");
            UnityEngine.Debug.Log($"Unity X: {unityPos:F3}");
            UnityEngine.Debug.Log("===========================");
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
                UnityEngine.Debug.Log("TrilhoKeyboardController enabled. Press H for help.");
            }
        }
        
        private void OnDisable()
        {
            if (trilhoManager != null)
            {
                trilhoManager.StopSimulation();
            }
        }
    }
}
