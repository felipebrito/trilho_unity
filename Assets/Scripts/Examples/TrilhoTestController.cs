using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using Trilho;

namespace Trilho.Examples
{
    /// <summary>
    /// Script de teste para simular e testar o sistema Trilho
    /// </summary>
    public class TrilhoTestController : MonoBehaviour
    {
        [Header("Test Settings")]
        [SerializeField] private TrilhoGameManager trilhoManager;
        [SerializeField] private float testSpeed = 10f;
        [SerializeField] private bool autoTest = false;
        [SerializeField] private float autoTestDuration = 30f;
        
        [Header("Keyboard Control")]
        [SerializeField] private bool enableKeyboardControl = true;
        [SerializeField] private float keyboardSpeed = 50f; // cm/s
        [SerializeField] private bool showKeyboardHelp = true;
        
        [Header("UI References")]
        [SerializeField] private Slider positionSlider;
        [SerializeField] private Text positionText;
        [SerializeField] private Text statusText;
        [SerializeField] private Button playTestButton;
        [SerializeField] private Button stopTestButton;
        
        private bool isAutoTesting = false;
        private float autoTestTimer = 0f;
        private float autoTestDirection = 1f;
        
        private void Start()
        {
            SetupUI();
            FindTrilhoManager();
        }
        
        private void SetupUI()
        {
            if (positionSlider != null)
            {
                positionSlider.minValue = 0f;
                positionSlider.maxValue = 600f;
                positionSlider.value = 0f;
                positionSlider.onValueChanged.AddListener(OnPositionSliderChanged);
            }
            
            if (playTestButton != null)
            {
                playTestButton.onClick.AddListener(StartAutoTest);
            }
            
            if (stopTestButton != null)
            {
                stopTestButton.onClick.AddListener(StopAutoTest);
            }
            
            UpdateUI();
        }
        
        private void FindTrilhoManager()
        {
            if (trilhoManager == null)
            {
                trilhoManager = FindFirstObjectByType<TrilhoGameManager>();
            }
            
            if (trilhoManager == null)
            {
                Debug.LogWarning("TrilhoGameManager not found! Please assign it in the inspector.");
            }
        }
        
        private void Update()
        {
            if (autoTest && isAutoTesting)
            {
                PerformAutoTest();
            }
            else if (enableKeyboardControl)
            {
                HandleKeyboardInput();
            }
            
            UpdateUI();
        }
        
        private void PerformAutoTest()
        {
            autoTestTimer += Time.deltaTime;
            
            if (autoTestTimer >= autoTestDuration)
            {
                StopAutoTest();
                return;
            }
            
            // Simulate movement back and forth
            float currentPos = trilhoManager != null ? trilhoManager.GetCurrentPositionCm() : 0f;
            
            currentPos += autoTestDirection * testSpeed * Time.deltaTime;
            
            if (currentPos >= 600f)
            {
                currentPos = 600f;
                autoTestDirection = -1f;
            }
            else if (currentPos <= 0f)
            {
                currentPos = 0f;
                autoTestDirection = 1f;
            }
            
            SetPosition(currentPos);
        }
        
        private void HandleKeyboardInput()
        {
            if (trilhoManager == null) return;
            
            var keyboard = Keyboard.current;
            if (keyboard == null) return;
            
            float currentPos = trilhoManager.GetCurrentPositionCm();
            float deltaPos = 0f;
            
            // Movement with arrow keys
            if (keyboard.leftArrowKey.isPressed)
            {
                deltaPos = -keyboardSpeed * Time.deltaTime;
            }
            else if (keyboard.rightArrowKey.isPressed)
            {
                deltaPos = keyboardSpeed * Time.deltaTime;
            }
            
            // Direct position jumps with number keys
            if (keyboard.digit1Key.wasPressedThisFrame)
            {
                SetPosition(65f); // Zone 1 center (30-100cm)
                Debug.Log("Jumped to Zone 1 (65cm)");
                return;
            }
            else if (keyboard.digit2Key.wasPressedThisFrame)
            {
                SetPosition(185f); // Zone 2 center (150-220cm)
                Debug.Log("Jumped to Zone 2 (185cm)");
                return;
            }
            else if (keyboard.digit3Key.wasPressedThisFrame)
            {
                SetPosition(340f); // Zone 3 center (300-380cm)
                Debug.Log("Jumped to Zone 3 (340cm)");
                return;
            }
            else if (keyboard.digit4Key.wasPressedThisFrame)
            {
                SetPosition(485f); // Zone 4 center (450-520cm)
                Debug.Log("Jumped to Zone 4 (485cm)");
                return;
            }
            else if (keyboard.digit5Key.wasPressedThisFrame)
            {
                SetPosition(550f); // Background area
                Debug.Log("Jumped to Background (550cm)");
                return;
            }
            
            // Additional controls
            if (keyboard.digit0Key.wasPressedThisFrame)
            {
                SetPosition(0f); // Start position
                Debug.Log("Jumped to Start (0cm)");
                return;
            }
            else if (keyboard.spaceKey.wasPressedThisFrame)
            {
                if (isAutoTesting)
                    StopAutoTest();
                else
                    StartAutoTest();
                return;
            }
            else if (keyboard.rKey.wasPressedThisFrame)
            {
                SetPosition(0f); // Reset to start
                Debug.Log("Reset to start position");
                return;
            }
            else if (keyboard.eKey.wasPressedThisFrame)
            {
                SetPosition(600f); // End position
                Debug.Log("Jumped to End (600cm)");
                return;
            }
            
            // Apply gradual movement
            if (deltaPos != 0f)
            {
                float newPos = Mathf.Clamp(currentPos + deltaPos, 0f, 600f);
                SetPosition(newPos);
            }
        }
        
        private void UpdateUI()
        {
            if (trilhoManager == null) return;
            
            float currentPos = trilhoManager.GetCurrentPositionCm();
            float unityPos = trilhoManager.GetCurrentUnityPosition();
            TrilhoState state = trilhoManager.GetCurrentState();
            ActivationZone currentZone = trilhoManager.GetCurrentZone();
            
            if (positionText != null)
            {
                positionText.text = $"Position: {currentPos:F1}cm\nUnity: {unityPos:F3}\nState: {state}";
            }
            
            if (statusText != null)
            {
                string zoneInfo = currentZone != null ? currentZone.zoneName : "None";
                statusText.text = $"Current Zone: {zoneInfo}";
            }
            
            if (positionSlider != null && !isAutoTesting)
            {
                positionSlider.value = currentPos;
            }
        }
        
        private void OnPositionSliderChanged(float value)
        {
            if (!isAutoTesting)
            {
                SetPosition(value);
            }
        }
        
        private void SetPosition(float positionCm)
        {
            if (trilhoManager != null)
            {
                trilhoManager.SimulatePosition(positionCm);
            }
        }
        
        public void StartAutoTest()
        {
            isAutoTesting = true;
            autoTestTimer = 0f;
            autoTestDirection = 1f;
            
            if (trilhoManager != null)
            {
                trilhoManager.SimulatePosition(0f);
            }
            
            Debug.Log("Auto test started");
        }
        
        public void StopAutoTest()
        {
            isAutoTesting = false;
            autoTestTimer = 0f;
            
            if (trilhoManager != null)
            {
                trilhoManager.StopSimulation();
            }
            
            Debug.Log("Auto test stopped");
        }
        
        // Test specific zones
        [ContextMenu("Test Zone 1")]
        public void TestZone1()
        {
            SetPosition(65f); // Middle of zone 1 (30-100cm)
        }
        
        [ContextMenu("Test Zone 2")]
        public void TestZone2()
        {
            SetPosition(185f); // Middle of zone 2 (150-220cm)
        }
        
        [ContextMenu("Test Zone 3")]
        public void TestZone3()
        {
            SetPosition(340f); // Middle of zone 3 (300-380cm)
        }
        
        [ContextMenu("Test Zone 4")]
        public void TestZone4()
        {
            SetPosition(485f); // Middle of zone 4 (450-520cm)
        }
        
        [ContextMenu("Test Background")]
        public void TestBackground()
        {
            SetPosition(550f); // Outside all zones
        }
        
        private void OnGUI()
        {
            if (trilhoManager == null) return;
            
            // Main debug GUI
            GUILayout.BeginArea(new Rect(10, 10, 300, 280));
            GUILayout.Label("Trilho Test Controller", GUI.skin.box);
            
            GUILayout.Space(5);
            
            GUILayout.Label($"Position: {trilhoManager.GetCurrentPositionCm():F1}cm");
            GUILayout.Label($"State: {trilhoManager.GetCurrentState()}");
            
            var currentZone = trilhoManager.GetCurrentZone();
            GUILayout.Label($"Zone: {(currentZone?.zoneName ?? "None")}");
            
            GUILayout.Space(10);
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Zone 1"))
                TestZone1();
            if (GUILayout.Button("Zone 2"))
                TestZone2();
            if (GUILayout.Button("Zone 3"))
                TestZone3();
            if (GUILayout.Button("Zone 4"))
                TestZone4();
            GUILayout.EndHorizontal();
            
            if (GUILayout.Button("Background"))
                TestBackground();
            
            GUILayout.Space(10);
            
            if (!isAutoTesting)
            {
                if (GUILayout.Button("Start Auto Test (Space)"))
                    StartAutoTest();
            }
            else
            {
                if (GUILayout.Button("Stop Auto Test (Space)"))
                    StopAutoTest();
                
                GUILayout.Label($"Test Progress: {(autoTestTimer / autoTestDuration * 100):F0}%");
            }
            
            GUILayout.Space(10);
            
            // Keyboard control toggle
            enableKeyboardControl = GUILayout.Toggle(enableKeyboardControl, "Keyboard Control");
            
            GUILayout.EndArea();
            
            // Keyboard help panel
            if (enableKeyboardControl && showKeyboardHelp)
            {
                GUILayout.BeginArea(new Rect(320, 10, 350, 280));
                GUILayout.Label("Keyboard Controls", GUI.skin.box);
                
                GUILayout.Space(5);
                
                GUILayout.Label("Movement:");
                GUILayout.Label("← → : Move Left/Right");
                GUILayout.Label($"Speed: {keyboardSpeed} cm/s");
                
                GUILayout.Space(5);
                
                GUILayout.Label("Quick Jump:");
                GUILayout.Label("1 : Zone 1 (65cm)");
                GUILayout.Label("2 : Zone 2 (185cm)");
                GUILayout.Label("3 : Zone 3 (340cm)");
                GUILayout.Label("4 : Zone 4 (485cm)");
                GUILayout.Label("5 : Background (550cm)");
                
                GUILayout.Space(5);
                
                GUILayout.Label("Other:");
                GUILayout.Label("0 : Start (0cm)");
                GUILayout.Label("E : End (600cm)");
                GUILayout.Label("R : Reset to Start");
                GUILayout.Label("Space : Toggle Auto Test");
                
                GUILayout.Space(10);
                
                showKeyboardHelp = GUILayout.Toggle(showKeyboardHelp, "Show Help");
                
                GUILayout.EndArea();
            }
        }
        
        private void OnDestroy()
        {
            StopAutoTest();
        }
    }
}
