using UnityEngine;
using UnityEditor;

namespace Trilho.Setup
{
    /// <summary>
    /// Script de setup automático para cenas Trilho configuráveis via JSON
    /// </summary>
    public class TrilhoSceneSetup : MonoBehaviour
    {
        [Header("Setup Automático")]
        [SerializeField] private bool autoSetupOnStart = false;
        [SerializeField] private bool showSetupInfo = true;
        
        [Header("Configurações da Cena")]
        [SerializeField] private string sceneName = "Trilho-Configurable";
        [SerializeField] private string configFileName = "trilho_config.json";
        
        [Header("Referências (Preenchidas Automaticamente)")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private Canvas mainCanvas;
        [SerializeField] private TrilhoConfigLoader configLoader;
        [SerializeField] private TrilhoGameManager gameManager;
        [SerializeField] private TrilhoZoneActivator zoneActivator;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (autoSetupOnStart)
            {
                SetupScene();
            }
        }
        
        #endregion
        
        #region Setup Automático
        
        /// <summary>
        /// Configura automaticamente toda a cena Trilho
        /// </summary>
        [ContextMenu("🔧 Setup Automático da Cena")]
        public void SetupScene()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 🚀 Iniciando setup automático da cena Trilho...");
            
            try
            {
                // 1. Configurar câmera
                SetupCamera();
                
                // 2. Criar canvas principal
                SetupCanvas();
                
                // 3. Criar scripts essenciais
                SetupScripts();
                
                // 4. Configurar OSC
                SetupOSC();
                
                // 5. Verificar StreamingAssets
                CheckStreamingAssets();
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Setup da cena concluído com sucesso!");
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SETUP] ❌ Erro durante setup: {e.Message}");
            }
        }
        
        /// <summary>
        /// Configura a câmera principal
        /// </summary>
        private void SetupCamera()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 📷 Configurando câmera principal...");
            
            // Encontrar ou criar câmera principal
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraGO = new GameObject("Main Camera");
                mainCamera = cameraGO.AddComponent<Camera>();
                cameraGO.tag = "MainCamera";
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Câmera principal criada");
            }
            
            // Configurar câmera para trilho (baseado na cena correta)
            mainCamera.transform.position = new Vector3(0, 0, -700);
            mainCamera.orthographic = true;
            mainCamera.orthographicSize = 960f; // Valor correto da cena
            mainCamera.backgroundColor = Color.black;
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            
            // Configurar câmera para World Space Canvas (igual à cena correta)
            mainCamera.cullingMask = -1; // Everything
            
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] ✅ Câmera configurada para trilho");
        }
        
        /// <summary>
        /// Configura o canvas principal
        /// </summary>
        private void SetupCanvas()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 🎨 Configurando canvas principal...");
            
            // Encontrar ou criar canvas
            mainCanvas = FindObjectOfType<Canvas>();
            if (mainCanvas == null)
            {
                GameObject canvasGO = new GameObject("[CANVAS PRINCIPAL]");
                mainCanvas = canvasGO.AddComponent<Canvas>();
                
                // Configurar CanvasScaler para World Space (igual à cena correta)
                var canvasScaler = canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
                canvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
                canvasScaler.dynamicPixelsPerUnit = 1f;
                canvasScaler.referencePixelsPerUnit = 100f;
                
                canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Canvas principal criado");
            }
            
            // Configurar canvas para trilho (baseado na cena funcional)
            mainCanvas.renderMode = RenderMode.WorldSpace;
            mainCanvas.transform.position = new Vector3(0f, 0f, 0f); // Posição correta da cena funcional
            mainCanvas.transform.localScale = new Vector3(1f, 1f, 1f); // Escala correta da cena funcional
            
            // Configurar RectTransform do Canvas para as dimensões corretas
            var canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
            if (canvasRectTransform != null)
            {
                canvasRectTransform.sizeDelta = new Vector2(1080f, 1920f);
                canvasRectTransform.anchoredPosition = Vector2.zero;
                canvasRectTransform.anchorMin = new Vector2(0f, 0f); // Anchors corretos da cena funcional
                canvasRectTransform.anchorMax = new Vector2(0f, 0f); // Anchors corretos da cena funcional
                canvasRectTransform.pivot = new Vector2(0.5f, 0.5f);
            }
            
            // Configurar CanvasScaler se existir (igual à cena correta)
            var existingCanvasScaler = mainCanvas.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (existingCanvasScaler != null)
            {
                existingCanvasScaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ConstantPixelSize;
                existingCanvasScaler.dynamicPixelsPerUnit = 1f;
                existingCanvasScaler.referencePixelsPerUnit = 100f;
            }
            
            // Configurar background do canvas
            SetupCanvasBackground();
            
            // Configurar EventSystem para World Space Canvas
            SetupEventSystem();
            
            // Configurar Input System se disponível
            SetupInputSystem();
            
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] ✅ Canvas configurado para trilho");
        }
        
        /// <summary>
        /// Configura o EventSystem para World Space Canvas
        /// </summary>
        private void SetupEventSystem()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 🎯 Configurando EventSystem...");
            
            // Verificar se já existe um EventSystem
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemGO = new GameObject("EventSystem");
                eventSystem = eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Usar InputSystemUIInputModule para compatibilidade com Input System Package
                try
                {
                    eventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    if (showSetupInfo)
                        UnityEngine.Debug.Log("[SETUP] ✅ EventSystem criado com InputSystemUIInputModule");
                }
                catch (System.Exception)
                {
                    // Fallback para StandaloneInputModule se Input System não estiver disponível
                    eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    if (showSetupInfo)
                        UnityEngine.Debug.Log("[SETUP] ✅ EventSystem criado com StandaloneInputModule (fallback)");
                }
            }
            else
            {
                // SOLUÇÃO DEFINITIVA: Desabilitar EventSystem antigo e criar um novo
                UnityEngine.Debug.Log("[SETUP] ⚠️ EventSystem existente detectado - criando um novo limpo...");
                
                // Desabilitar EventSystem antigo
                eventSystem.enabled = false;
                eventSystem.gameObject.SetActive(false);
                
                // Criar novo EventSystem limpo
                GameObject newEventSystemGO = new GameObject("EventSystem_NEW");
                var newEventSystem = newEventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                
                // Usar InputSystemUIInputModule para compatibilidade com Input System Package
                try
                {
                    newEventSystemGO.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
                    if (showSetupInfo)
                        UnityEngine.Debug.Log("[SETUP] ✅ Novo EventSystem criado com InputSystemUIInputModule");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogWarning($"[SETUP] ⚠️ InputSystemUIInputModule não disponível: {e.Message}");
                    // Fallback para StandaloneInputModule se Input System não estiver disponível
                    newEventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                    if (showSetupInfo)
                        UnityEngine.Debug.Log("[SETUP] ✅ Novo EventSystem criado com StandaloneInputModule (fallback)");
                }
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ EventSystem antigo desabilitado, novo criado");
            }
        }
        

        
        /// <summary>
        /// Configura o Input System se disponível
        /// </summary>
        private void SetupInputSystem()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 🎮 Configurando Input System...");
            
            try
            {
                // Verificar se o Input System está disponível
                var inputSystemType = System.Type.GetType("UnityEngine.InputSystem.InputSystem");
                if (inputSystemType != null)
                {
                    // Verificar se há Input Actions configuradas
                    var inputActions = FindObjectOfType<UnityEngine.InputSystem.InputActionAsset>();
                    if (inputActions != null)
                    {
                        if (showSetupInfo)
                            UnityEngine.Debug.Log("[SETUP] ✅ Input Actions encontradas");
                    }
                    else
                    {
                        if (showSetupInfo)
                            UnityEngine.Debug.Log("[SETUP] ⚠️ Nenhuma Input Action encontrada. Configure-as manualmente.");
                    }
                    
                    if (showSetupInfo)
                        UnityEngine.Debug.Log("[SETUP] ✅ Input System configurado");
                }
                else
                {
                    if (showSetupInfo)
                        UnityEngine.Debug.Log("[SETUP] ⚠️ Input System não está disponível");
                }
            }
            catch (System.Exception e)
            {
                if (showSetupInfo)
                    UnityEngine.Debug.LogWarning($"[SETUP] ⚠️ Erro ao configurar Input System: {e.Message}");
            }
        }
        
        /// <summary>
        /// Configura o RectTransform do background com posicionamento dinâmico
        /// </summary>
        /// <param name="rectTransform">RectTransform do background</param>
        /// <param name="type">Tipo do background ("novo" ou "existente")</param>
        private void ConfigureBackgroundRectTransform(RectTransform rectTransform, string type)
        {
            // Configurar anchors e pivot
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            
            // Dimensões do background (pode ser configurado via JSON)
            float backgroundWidth = 9600f; // Valor corrigido para 9600
            float backgroundHeight = 1920f; // Valor corrigido para 1920 (sem decimais para NDI)
            float screenWidth = 1080f;
            
            // Calcular posição X automaticamente: (Background Width / 2) - (Screen Width / 2)
            // Esta fórmula garante que o background seja posicionado corretamente
            // Posição = (9600 / 2) - (1080 / 2) = 4800 - 540 = 4260
            float calculatedPositionX = (backgroundWidth / 2f) - (screenWidth / 2f);
            
            rectTransform.sizeDelta = new Vector2(backgroundWidth, backgroundHeight);
            rectTransform.anchoredPosition = new Vector2(calculatedPositionX, 0f);
            
            if (showSetupInfo)
                UnityEngine.Debug.Log($"[SETUP] 📐 Background {type} configurado: {backgroundWidth}x{backgroundHeight} na posição ({calculatedPositionX}, 0)");
        }
        
        /// <summary>
        /// Configura o background do canvas
        /// </summary>
        private void SetupCanvasBackground()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 🖼️ Configurando background do canvas...");
            
            // Verificar se já existe um background
            UnityEngine.UI.Image backgroundImage = mainCanvas.GetComponentInChildren<UnityEngine.UI.Image>();
            if (backgroundImage == null)
            {
                // Criar GameObject para o background
                GameObject backgroundGO = new GameObject("[BACKGROUND]");
                backgroundGO.transform.SetParent(mainCanvas.transform, false);
                
                // Adicionar componente Image
                backgroundImage = backgroundGO.AddComponent<UnityEngine.UI.Image>();
                
                // Configurar como background (cobrir toda a tela)
                backgroundImage.type = UnityEngine.UI.Image.Type.Simple;
                backgroundImage.raycastTarget = false; // Não interceptar cliques
                
                // Obter ou adicionar RectTransform (Image já adiciona automaticamente)
                var rectTransform = backgroundGO.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    rectTransform = backgroundGO.AddComponent<RectTransform>();
                }
                
                // Configurar background com as dimensões e posição calculadas dinamicamente
                ConfigureBackgroundRectTransform(rectTransform, "novo");
                
                // Definir cor padrão (preto)
                backgroundImage.color = Color.black;
                
                // Mover para o final da hierarquia (atrás de tudo)
                backgroundImage.transform.SetAsFirstSibling();
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Background do canvas criado");
            }
            else
            {
                // Configurar background existente com as configurações calculadas dinamicamente
                var existingRectTransform = backgroundImage.GetComponent<RectTransform>();
                if (existingRectTransform != null)
                {
                    ConfigureBackgroundRectTransform(existingRectTransform, "existente");
                }
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Background do canvas já existe e foi configurado");
            }
            
            // Aplicar configurações de background do JSON se disponível
            ApplyBackgroundSettings(backgroundImage);
        }
        
        /// <summary>
        /// Aplica as configurações de background do JSON
        /// </summary>
        private void ApplyBackgroundSettings(UnityEngine.UI.Image backgroundImage)
        {
            if (configLoader == null || configLoader.CurrentConfig == null) return;
            
            try
            {
                // Usar reflection para acessar as configurações de background
                var backgroundField = configLoader.CurrentConfig.GetType().GetField("backgroundSettings");
                if (backgroundField != null)
                {
                    var backgroundSettings = backgroundField.GetValue(configLoader.CurrentConfig);
                    if (backgroundSettings != null)
                    {
                        // Aplicar cor de fundo
                        var colorField = backgroundSettings.GetType().GetField("color");
                        if (colorField != null)
                        {
                            var colorArray = (float[])colorField.GetValue(backgroundSettings);
                            if (colorArray != null && colorArray.Length >= 4)
                            {
                                backgroundImage.color = new Color(colorArray[0], colorArray[1], colorArray[2], colorArray[3]);
                            }
                        }
                        
                        // Aplicar imagem de fundo se especificada
                        var contentPathField = backgroundSettings.GetType().GetField("contentPath");
                        if (contentPathField != null)
                        {
                            var contentPath = (string)contentPathField.GetValue(backgroundSettings);
                            if (!string.IsNullOrEmpty(contentPath))
                            {
                                LoadBackgroundImage(backgroundImage, contentPath);
                            }
                        }
                        
                        if (showSetupInfo)
                            UnityEngine.Debug.Log("[SETUP] ✅ Configurações de background aplicadas");
                    }
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"[SETUP] ⚠️ Erro ao aplicar configurações de background: {e.Message}");
            }
        }
        
        /// <summary>
        /// Carrega imagem de fundo do arquivo
        /// </summary>
        private void LoadBackgroundImage(UnityEngine.UI.Image backgroundImage, string imagePath)
        {
            try
            {
                string fullPath = System.IO.Path.Combine(Application.streamingAssetsPath, imagePath);
                if (System.IO.File.Exists(fullPath))
                {
                    // Carregar imagem como byte array
                    byte[] imageData = System.IO.File.ReadAllBytes(fullPath);
                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(imageData);
                    
                    // Criar sprite e aplicar
                    Sprite sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                    backgroundImage.sprite = sprite;
                    
                    if (showSetupInfo)
                        UnityEngine.Debug.Log($"[SETUP] ✅ Imagem de fundo carregada: {imagePath}");
                }
                else
                {
                    UnityEngine.Debug.LogWarning($"[SETUP] ⚠️ Arquivo de imagem de fundo não encontrado: {fullPath}");
                }
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"[SETUP] ❌ Erro ao carregar imagem de fundo: {e.Message}");
            }
        }
        
        /// <summary>
        /// Configura os scripts essenciais
        /// </summary>
        private void SetupScripts()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] ⚙️ Configurando scripts essenciais...");
            
            // 1. TrilhoConfigLoader
            SetupConfigLoader();
            
            // 2. TrilhoGameManager
            SetupGameManager();
            
            // 3. TrilhoZoneActivator
            SetupZoneActivator();
            
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] ✅ Scripts essenciais configurados");
        }
        
        /// <summary>
        /// Configura o TrilhoConfigLoader
        /// </summary>
        private void SetupConfigLoader()
        {
            // Encontrar ou criar config loader
            configLoader = FindObjectOfType<TrilhoConfigLoader>();
            if (configLoader == null)
            {
                GameObject loaderGO = new GameObject("[CONFIG LOADER]");
                configLoader = loaderGO.AddComponent<TrilhoConfigLoader>();
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ TrilhoConfigLoader criado");
            }
            
            // Configurar loader
            configLoader.transform.SetParent(transform);
        }
        
        /// <summary>
        /// Configura o TrilhoGameManager
        /// </summary>
        private void SetupGameManager()
        {
            // Encontrar ou criar game manager
            gameManager = FindObjectOfType<TrilhoGameManager>();
            if (gameManager == null)
            {
                GameObject managerGO = new GameObject("[GAME MANAGER]");
                gameManager = managerGO.AddComponent<TrilhoGameManager>();
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ TrilhoGameManager criado");
            }
            
            // Configurar referências
            if (mainCamera != null)
            {
                // Usar reflection para acessar campos privados
                var cameraField = typeof(TrilhoGameManager).GetField("mainCamera", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (cameraField != null)
                    cameraField.SetValue(gameManager, mainCamera);
            }
            
            if (mainCanvas != null)
            {
                var canvasField = typeof(TrilhoGameManager).GetField("mainCanvas", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (canvasField != null)
                    canvasField.SetValue(gameManager, mainCanvas);
            }
            
            gameManager.transform.SetParent(transform);
        }
        
        /// <summary>
        /// Configura o TrilhoZoneActivator
        /// </summary>
        private void SetupZoneActivator()
        {
            // Encontrar ou criar zone activator
            zoneActivator = FindObjectOfType<TrilhoZoneActivator>();
            if (zoneActivator == null)
            {
                GameObject activatorGO = new GameObject("[ZONE ACTIVATOR]");
                zoneActivator = activatorGO.AddComponent<TrilhoZoneActivator>();
                
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ TrilhoZoneActivator criado");
            }
            
            // Configurar referências
            if (gameManager != null)
            {
                var gameManagerField = typeof(TrilhoZoneActivator).GetField("gameManager", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (gameManagerField != null)
                    gameManagerField.SetValue(zoneActivator, gameManager);
            }
            
            zoneActivator.transform.SetParent(transform);
        }
        
        /// <summary>
        /// Configura a conexão OSC
        /// </summary>
        private void SetupOSC()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 📡 Configurando conexão OSC...");
            
            // Verificar se já existe OSC Connection
            var oscConnections = Resources.FindObjectsOfTypeAll<ScriptableObject>();
            bool hasOSC = false;
            
            foreach (var obj in oscConnections)
            {
                if (obj.name.Contains("OSC") || obj.name.Contains("Osc"))
                {
                    hasOSC = true;
                    break;
                }
            }
            
            if (!hasOSC)
            {
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ⚠️ Nenhuma conexão OSC encontrada. Crie uma manualmente.");
            }
            else
            {
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Conexão OSC encontrada");
            }
        }
        
        /// <summary>
        /// Verifica a estrutura de StreamingAssets
        /// </summary>
        private void CheckStreamingAssets()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 📁 Verificando estrutura de StreamingAssets...");
            
            string streamingPath = Application.streamingAssetsPath;
            
            // Verificar se StreamingAssets existe
            if (!System.IO.Directory.Exists(streamingPath))
            {
                System.IO.Directory.CreateDirectory(streamingPath);
                if (showSetupInfo)
                    UnityEngine.Debug.Log("[SETUP] ✅ Pasta StreamingAssets criada");
            }
            
            // Verificar se o arquivo de configuração existe
            string configPath = System.IO.Path.Combine(streamingPath, configFileName);
            if (!System.IO.File.Exists(configPath))
            {
                if (showSetupInfo)
                    UnityEngine.Debug.Log($"[SETUP] ⚠️ Arquivo de configuração não encontrado: {configFileName}");
                UnityEngine.Debug.Log("[SETUP] 💡 Use o arquivo trilho_config_example.json como base");
            }
            else
            {
                if (showSetupInfo)
                    UnityEngine.Debug.Log($"[SETUP] ✅ Arquivo de configuração encontrado: {configFileName}");
            }
            
            // Verificar pastas de conteúdo
            string[] contentFolders = { "Videos", "Images", "Text", "Apps" };
            foreach (string folder in contentFolders)
            {
                string folderPath = System.IO.Path.Combine(streamingPath, folder);
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                    if (showSetupInfo)
                        UnityEngine.Debug.Log($"[SETUP] ✅ Pasta criada: {folder}");
                }
            }
        }
        
        #endregion
        
        #region Utilitários
        
        /// <summary>
        /// Limpa todos os objetos de setup
        /// </summary>
        [ContextMenu("🗑️ Limpar Setup")]
        public void ClearSetup()
        {
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] 🗑️ Limpando setup...");
            
            // Destruir objetos de setup
            if (configLoader != null) DestroyImmediate(configLoader.gameObject);
            if (gameManager != null) DestroyImmediate(gameManager.gameObject);
            if (zoneActivator != null) DestroyImmediate(zoneActivator.gameObject);
            
            // Resetar referências
            configLoader = null;
            gameManager = null;
            zoneActivator = null;
            
            if (showSetupInfo)
                UnityEngine.Debug.Log("[SETUP] ✅ Setup limpo");
        }
        
        /// <summary>
        /// Verifica o status atual da cena
        /// </summary>
        [ContextMenu("🔍 Verificar Status")]
        public void CheckStatus()
        {
            UnityEngine.Debug.Log("[SETUP] 🔍 Status da Cena Trilho:");
            UnityEngine.Debug.Log($"  📷 Câmera: {(mainCamera != null ? "✅" : "❌")}");
            UnityEngine.Debug.Log($"  🎨 Canvas: {(mainCanvas != null ? "✅" : "❌")}");
            
            // Verificar background
            if (mainCanvas != null)
            {
                UnityEngine.UI.Image backgroundImage = mainCanvas.GetComponentInChildren<UnityEngine.UI.Image>();
                UnityEngine.Debug.Log($"  🖼️ Background: {(backgroundImage != null ? "✅" : "❌")}");
                if (backgroundImage != null && backgroundImage.sprite != null)
                {
                    UnityEngine.Debug.Log($"  🖼️ Imagem de Fundo: ✅ ({backgroundImage.sprite.name})");
                }
            }
            
            UnityEngine.Debug.Log($"  ⚙️ ConfigLoader: {(configLoader != null ? "✅" : "❌")}");
            UnityEngine.Debug.Log($"  🎮 GameManager: {(gameManager != null ? "✅" : "❌")}");
            UnityEngine.Debug.Log($"  🎯 ZoneActivator: {(zoneActivator != null ? "✅" : "❌")}");
            
            // Verificar StreamingAssets
            string configPath = System.IO.Path.Combine(Application.streamingAssetsPath, configFileName);
            UnityEngine.Debug.Log($"  📁 Config: {(System.IO.File.Exists(configPath) ? "✅" : "❌")}");
            
            // Verificar pastas de conteúdo
            string streamingPath = Application.streamingAssetsPath;
            string[] contentFolders = { "Videos", "Images", "Texts", "Apps" };
            foreach (string folder in contentFolders)
            {
                string folderPath = System.IO.Path.Combine(streamingPath, folder);
                UnityEngine.Debug.Log($"  📁 {folder}: {(System.IO.Directory.Exists(folderPath) ? "✅" : "❌")}");
            }
        }
        
        #endregion
        
        #region Editor
        
        #if UNITY_EDITOR
        
        /// <summary>
        /// Setup automático no editor
        /// </summary>
        [MenuItem("Trilho/Setup Cena Automático")]
        public static void EditorSetupScene()
        {
            // Encontrar ou criar TrilhoSceneSetup
            TrilhoSceneSetup setup = FindObjectOfType<TrilhoSceneSetup>();
            if (setup == null)
            {
                GameObject setupGO = new GameObject("[TRILHO SETUP]");
                setup = setupGO.AddComponent<TrilhoSceneSetup>();
                UnityEngine.Debug.Log("[SETUP] ✅ TrilhoSceneSetup criado no editor");
            }
            
            setup.SetupScene();
            UnityEngine.Debug.Log("[SETUP] 🎉 Setup da cena concluído! Verifique o Console para detalhes.");
        }
        
        /// <summary>
        /// Limpar setup no editor
        /// </summary>
        [MenuItem("Trilho/Limpar Setup")]
        public static void EditorClearSetup()
        {
            TrilhoSceneSetup setup = FindObjectOfType<TrilhoSceneSetup>();
            if (setup != null)
            {
                setup.ClearSetup();
                UnityEngine.Debug.Log("[SETUP] 🗑️ Setup limpo no editor");
            }
        }
        
        #endif
        
        #endregion
    }
}
