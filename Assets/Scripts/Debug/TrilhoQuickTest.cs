using UnityEngine;
using UnityEngine.UI;

namespace Trilho.Debug
{
    /// <summary>
    /// Script de teste rápido para verificar funcionalidades da cena Trilho
    /// </summary>
    public class TrilhoQuickTest : MonoBehaviour
    {
        [Header("Teste Rápido")]
        [SerializeField] private bool showTestUI = true;
        [SerializeField] private bool autoTestOnStart = false;
        
        [Header("Referências")]
        [SerializeField] private TrilhoConfigLoader configLoader;
        [SerializeField] private TrilhoGameManager gameManager;
        [SerializeField] private TrilhoZoneActivator zoneActivator;
        
        [Header("Teste de Posição")]
        [SerializeField] private float testPositionCm = 100f;
        [SerializeField] private bool simulateMovement = false;
        [SerializeField] private float movementSpeed = 50f; // cm/s
        
        private Canvas testCanvas;
        private Text statusText;
        private Slider positionSlider;
        private Button[] zoneButtons;
        
        #region Unity Lifecycle
        
        private void Start()
        {
            FindReferences();
            
            if (autoTestOnStart)
            {
                StartAutoTest();
            }
            
            if (showTestUI)
            {
                CreateTestUI();
            }
        }
        
        private void Update()
        {
            if (simulateMovement)
            {
                SimulateMovement();
            }
            
            UpdateTestUI();
        }
        
        #endregion
        
        #region Referências
        
        private void FindReferences()
        {
            if (configLoader == null)
                configLoader = FindObjectOfType<TrilhoConfigLoader>();
            
            if (gameManager == null)
                gameManager = FindObjectOfType<TrilhoGameManager>();
            
            if (zoneActivator == null)
                zoneActivator = FindObjectOfType<TrilhoZoneActivator>();
        }
        
        #endregion
        
        #region Teste Automático
        
        /// <summary>
        /// Inicia teste automático da cena
        /// </summary>
        [ContextMenu("🚀 Iniciar Teste Automático")]
        public void StartAutoTest()
        {
            UnityEngine.Debug.Log("[QUICK TEST] 🚀 Iniciando teste automático...");
            
            try
            {
                // 1. Verificar componentes
                TestComponents();
                
                // 2. Verificar configuração
                TestConfiguration();
                
                // 3. Verificar zonas
                TestZones();
                
                // 4. Verificar movimento
                TestMovement();
                
                UnityEngine.Debug.Log("[QUICK TEST] ✅ Teste automático concluído!");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[QUICK TEST] ❌ Erro durante teste: {e.Message}");
            }
        }
        
        /// <summary>
        /// Testa se todos os componentes estão presentes
        /// </summary>
        private void TestComponents()
        {
            UnityEngine.Debug.Log("[QUICK TEST] 🔍 Testando componentes...");
            
            bool allGood = true;
            
            if (configLoader == null)
            {
                UnityEngine.Debug.LogError("[QUICK TEST] ❌ TrilhoConfigLoader não encontrado");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("[QUICK TEST] ✅ TrilhoConfigLoader encontrado");
            }
            
            if (gameManager == null)
            {
                UnityEngine.Debug.LogError("[QUICK TEST] ❌ TrilhoGameManager não encontrado");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("[QUICK TEST] ✅ TrilhoGameManager encontrado");
            }
            
            if (zoneActivator == null)
            {
                UnityEngine.Debug.LogError("[QUICK TEST] ❌ TrilhoZoneActivator não encontrado");
                allGood = false;
            }
            else
            {
                UnityEngine.Debug.Log("[QUICK TEST] ✅ TrilhoZoneActivator encontrado");
            }
            
            if (allGood)
            {
                UnityEngine.Debug.Log("[QUICK TEST] ✅ Todos os componentes estão presentes");
            }
        }
        
        /// <summary>
        /// Testa se a configuração foi carregada
        /// </summary>
        private void TestConfiguration()
        {
            UnityEngine.Debug.Log("[QUICK TEST] 📁 Testando configuração...");
            
            if (configLoader != null && configLoader.IsConfigLoaded)
            {
                var config = configLoader.CurrentConfig;
                UnityEngine.Debug.Log($"[QUICK TEST] ✅ Configuração carregada: {config.configName} v{config.version}");
                UnityEngine.Debug.Log($"[QUICK TEST] 📊 Zonas configuradas: {config.contentZones.Count}");
                
                foreach (var zone in config.contentZones)
                {
                    UnityEngine.Debug.Log($"[QUICK TEST] 🎯 Zona: {zone.name} em {zone.positionCm}cm ({zone.contentType})");
                }
            }
            else
            {
                UnityEngine.Debug.LogWarning("[QUICK TEST] ⚠️ Configuração não carregada");
            }
        }
        
        /// <summary>
        /// Testa se as zonas estão funcionando
        /// </summary>
        private void TestZones()
        {
            UnityEngine.Debug.Log("[QUICK TEST] 🎯 Testando zonas...");
            
            if (zoneActivator != null)
            {
                // Usar reflection para acessar campos privados
                var zonesField = typeof(TrilhoZoneActivator).GetField("zones", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (zonesField != null)
                {
                    var zones = zonesField.GetValue(zoneActivator) as System.Collections.Generic.List<TrilhoZoneActivator.Zone>;
                    if (zones != null)
                    {
                        UnityEngine.Debug.Log($"[QUICK TEST] ✅ Zonas ativas: {zones.Count}");
                    }
                    else
                    {
                        UnityEngine.Debug.LogWarning("[QUICK TEST] ⚠️ Lista de zonas não encontrada");
                    }
                }
            }
        }
        
        /// <summary>
        /// Testa movimento da câmera
        /// </summary>
        private void TestMovement()
        {
            UnityEngine.Debug.Log("[QUICK TEST] 📷 Testando movimento da câmera...");
            
            if (gameManager != null)
            {
                // Simular movimento para posição de teste
                SimulatePosition(testPositionCm);
                UnityEngine.Debug.Log($"[QUICK TEST] ✅ Movimento simulado para {testPositionCm}cm");
            }
        }
        
        #endregion
        
        #region Simulação
        
        /// <summary>
        /// Simula movimento para uma posição específica
        /// </summary>
        [ContextMenu("🎯 Simular Posição")]
        public void SimulatePosition()
        {
            SimulatePosition(testPositionCm);
        }
        
        /// <summary>
        /// Simula movimento para uma posição específica
        /// </summary>
        public void SimulatePosition(float positionCm)
        {
            if (gameManager != null)
            {
                // Usar reflection para acessar método privado
                var method = typeof(TrilhoGameManager).GetMethod("SetPosition", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (method != null)
                {
                    method.Invoke(gameManager, new object[] { positionCm });
                    UnityEngine.Debug.Log($"[QUICK TEST] 🎯 Posição simulada: {positionCm}cm");
                }
                else
                {
                    UnityEngine.Debug.LogWarning("[QUICK TEST] ⚠️ Método SetPosition não encontrado");
                }
            }
        }
        
        /// <summary>
        /// Simula movimento contínuo
        /// </summary>
        private void SimulateMovement()
        {
            if (gameManager != null)
            {
                testPositionCm += movementSpeed * Time.deltaTime;
                
                // Loop entre 0 e 600cm
                if (testPositionCm > 600f)
                    testPositionCm = 0f;
                else if (testPositionCm < 0f)
                    testPositionCm = 600f;
                
                SimulatePosition(testPositionCm);
            }
        }
        
        #endregion
        
        #region Interface de Teste
        
        /// <summary>
        /// Cria interface de teste
        /// </summary>
        private void CreateTestUI()
        {
            if (testCanvas != null) return;
            
            // Criar canvas de teste
            GameObject canvasGO = new GameObject("Test Canvas");
            testCanvas = canvasGO.AddComponent<Canvas>();
            testCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            testCanvas.sortingOrder = 1000;
            
            // Adicionar componentes necessários
            canvasGO.AddComponent<CanvasScaler>();
            canvasGO.AddComponent<GraphicRaycaster>();
            
            // Criar painel de fundo
            GameObject panelGO = new GameObject("Background");
            panelGO.transform.SetParent(canvasGO.transform, false);
            
            Image panelImage = panelGO.AddComponent<Image>();
            panelImage.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform panelRect = panelGO.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0, 0.7f);
            panelRect.anchorMax = new Vector2(1, 1);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            // Criar texto de status
            GameObject statusGO = new GameObject("Status Text");
            statusGO.transform.SetParent(panelGO.transform, false);
            
            statusText = statusGO.AddComponent<Text>();
            statusText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            statusText.fontSize = 16;
            statusText.color = Color.white;
            statusText.text = "Status: Inicializando...";
            
            RectTransform statusRect = statusGO.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0, 0);
            statusRect.anchorMax = new Vector2(1, 1);
            statusRect.offsetMin = new Vector2(10, 10);
            statusRect.offsetMax = new Vector2(-10, -10);
            
            // Criar slider de posição
            GameObject sliderGO = new GameObject("Position Slider");
            sliderGO.transform.SetParent(panelGO.transform, false);
            
            positionSlider = sliderGO.AddComponent<Slider>();
            positionSlider.minValue = 0f;
            positionSlider.maxValue = 600f;
            positionSlider.value = testPositionCm;
            positionSlider.onValueChanged.AddListener(OnPositionSliderChanged);
            
            RectTransform sliderRect = sliderGO.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.1f, 0.1f);
            sliderRect.anchorMax = new Vector2(0.9f, 0.3f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            
            // Criar botões de zona
            CreateZoneButtons(panelGO);
            
            UnityEngine.Debug.Log("[QUICK TEST] 🎨 Interface de teste criada");
        }
        
        /// <summary>
        /// Cria botões para cada zona
        /// </summary>
        private void CreateZoneButtons(GameObject parent)
        {
            if (configLoader == null || !configLoader.IsConfigLoaded) return;
            
            var config = configLoader.CurrentConfig;
            zoneButtons = new Button[config.contentZones.Count];
            
            for (int i = 0; i < config.contentZones.Count; i++)
            {
                var zone = config.contentZones[i];
                
                GameObject buttonGO = new GameObject($"Zone {i + 1}: {zone.name}");
                buttonGO.transform.SetParent(parent.transform, false);
                
                Button button = buttonGO.AddComponent<Button>();
                Image buttonImage = buttonGO.AddComponent<Image>();
                buttonImage.color = new Color(0.2f, 0.6f, 1f, 0.8f);
                
                Text buttonText = buttonGO.AddComponent<Text>();
                buttonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                buttonText.fontSize = 12;
                buttonText.color = Color.white;
                buttonText.text = $"{i + 1}: {zone.name}";
                buttonText.alignment = TextAnchor.MiddleCenter;
                
                RectTransform buttonRect = buttonGO.GetComponent<RectTransform>();
                float buttonWidth = 0.15f;
                float buttonHeight = 0.15f;
                float xPos = 0.1f + (i % 4) * (buttonWidth + 0.02f);
                float yPos = 0.4f + (i / 4) * (buttonHeight + 0.02f);
                
                buttonRect.anchorMin = new Vector2(xPos, yPos);
                buttonRect.anchorMax = new Vector2(xPos + buttonWidth, yPos + buttonHeight);
                buttonRect.offsetMin = Vector2.zero;
                buttonRect.offsetMax = Vector2.zero;
                
                int zoneIndex = i;
                button.onClick.AddListener(() => JumpToZone(zoneIndex));
                
                zoneButtons[i] = button;
            }
        }
        
        /// <summary>
        /// Pula para uma zona específica
        /// </summary>
        private void JumpToZone(int zoneIndex)
        {
            if (configLoader != null && configLoader.IsConfigLoaded)
            {
                var config = configLoader.CurrentConfig;
                if (zoneIndex < config.contentZones.Count)
                {
                    var zone = config.contentZones[zoneIndex];
                    SimulatePosition(zone.positionCm);
                    UnityEngine.Debug.Log($"[QUICK TEST] 🎯 Pulando para zona: {zone.name} em {zone.positionCm}cm");
                }
            }
        }
        
        /// <summary>
        /// Atualiza a interface de teste
        /// </summary>
        private void UpdateTestUI()
        {
            if (statusText != null)
            {
                string status = $"Status: ";
                
                if (configLoader != null && configLoader.IsConfigLoaded)
                {
                    var config = configLoader.CurrentConfig;
                    status += $"Config OK ({config.contentZones.Count} zonas) | ";
                }
                else
                {
                    status += "Config ❌ | ";
                }
                
                if (gameManager != null)
                    status += "GameManager OK | ";
                else
                    status += "GameManager ❌ | ";
                
                if (zoneActivator != null)
                    status += "ZoneActivator OK | ";
                else
                    status += "ZoneActivator ❌ | ";
                
                status += $"Pos: {testPositionCm:F1}cm";
                
                statusText.text = status;
            }
            
            if (positionSlider != null)
            {
                positionSlider.value = testPositionCm;
            }
        }
        
        /// <summary>
        /// Callback do slider de posição
        /// </summary>
        private void OnPositionSliderChanged(float value)
        {
            testPositionCm = value;
            SimulatePosition(testPositionCm);
        }
        
        #endregion
        
        #region Utilitários
        
        /// <summary>
        /// Alterna simulação de movimento
        /// </summary>
        [ContextMenu("🔄 Alternar Simulação")]
        public void ToggleSimulation()
        {
            simulateMovement = !simulateMovement;
            UnityEngine.Debug.Log($"[QUICK TEST] 🔄 Simulação de movimento: {(simulateMovement ? "ON" : "OFF")}");
        }
        
        /// <summary>
        /// Reseta posição para início
        /// </summary>
        [ContextMenu("🏠 Reset para Início")]
        public void ResetToStart()
        {
            testPositionCm = 0f;
            SimulatePosition(0f);
            UnityEngine.Debug.Log("[QUICK TEST] 🏠 Reset para posição inicial");
        }
        
        #endregion
    }
}
