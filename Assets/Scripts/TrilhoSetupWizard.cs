using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Trilho
{
    public class TrilhoSetupWizard : MonoBehaviour
    {
        [Header("Wizard Settings")]
        [SerializeField] private bool showWizardOnStart = true;
        [SerializeField] private int currentStep = 0;
        
        [Header("References")]
        [SerializeField] private MarkerSystem markerSystem;
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private Camera sceneCamera;
        
        [Header("Visual Elements")]
        [SerializeField] private GameObject borderColliders;
        [SerializeField] private GameObject contentPlaceholders;
        [SerializeField] private GameObject wizardUI;
        
        [Header("Step Configuration")]
        [SerializeField] private List<WizardStep> steps = new List<WizardStep>();
        
        // Runtime variables
        private bool isWizardActive = false;
        private GameObject leftBorder;
        private GameObject rightBorder;
        private List<GameObject> contentMarkers = new List<GameObject>();
        
        [System.Serializable]
        public class WizardStep
        {
            public string title;
            public string description;
            public string buttonText;
            public System.Action action;
        }
        
        private void Start()
        {
            if (showWizardOnStart)
            {
                InitializeWizard();
            }
        }
        
        #region Wizard Initialization
        
        [ContextMenu("Iniciar Wizard")]
        public void InitializeWizard()
        {
            Debug.Log("=== WIZARD DE CONFIGURAÇÃO TRILHO ===");
            
            // Find required components
            if (markerSystem == null)
                markerSystem = FindObjectOfType<MarkerSystem>();
                
            if (targetCanvas == null)
                targetCanvas = FindObjectOfType<Canvas>();
                
            if (sceneCamera == null)
                sceneCamera = Camera.main;
            
            // Create visual elements
            CreateVisualElements();
            
            // Setup steps
            SetupWizardSteps();
            
            // Show first step
            currentStep = 0;
            ShowCurrentStep();
            
            isWizardActive = true;
            
            Debug.Log("=== MODO CONFIGURAÇÃO ATIVADO ===");
            Debug.Log("As setas do teclado agora são usadas para configuração.");
            Debug.Log("Use as teclas 1-4 para navegar entre os passos.");
            Debug.Log("Pressione ESC para sair do modo configuração.");
        }
        
        private void CreateVisualElements()
        {
            // Create border colliders container
            if (borderColliders == null)
            {
                borderColliders = new GameObject("Colisores das Bordas");
            }
            
            // Create content placeholders container
            if (contentPlaceholders == null)
            {
                contentPlaceholders = new GameObject("Marcadores de Conteúdo");
            }
            
            // Create wizard UI
            if (wizardUI == null)
            {
                CreateWizardUI();
            }
        }
        
        private void CreateWizardUI()
        {
            wizardUI = new GameObject("Wizard UI");
            var canvas = wizardUI.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            
            var canvasScaler = wizardUI.AddComponent<CanvasScaler>();
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            
            var graphicRaycaster = wizardUI.AddComponent<GraphicRaycaster>();
            
            // Create background
            var background = new GameObject("Background");
            background.transform.SetParent(wizardUI.transform, false);
            var backgroundImage = background.AddComponent<Image>();
            backgroundImage.color = new Color(0, 0, 0, 0.8f);
            
            var backgroundRect = background.GetComponent<RectTransform>();
            backgroundRect.anchorMin = Vector2.zero;
            backgroundRect.anchorMax = Vector2.one;
            backgroundRect.sizeDelta = Vector2.zero;
            backgroundRect.anchoredPosition = Vector2.zero;
            
            // Create panel
            var panel = new GameObject("Panel");
            panel.transform.SetParent(wizardUI.transform, false);
            var panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.2f, 0.2f, 0.2f, 0.9f);
            
            var panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.1f, 0.1f);
            panelRect.anchorMax = new Vector2(0.9f, 0.9f);
            panelRect.sizeDelta = Vector2.zero;
            panelRect.anchoredPosition = Vector2.zero;
            
            // Create title
            var title = new GameObject("Title");
            title.transform.SetParent(panel.transform, false);
            var titleText = title.AddComponent<Text>();
            titleText.text = "Wizard de Configuração Trilho";
            titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            titleText.fontSize = 36;
            titleText.color = Color.white;
            titleText.alignment = TextAnchor.UpperCenter;
            
            var titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.8f);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.sizeDelta = Vector2.zero;
            titleRect.anchoredPosition = Vector2.zero;
            
            // Create description
            var description = new GameObject("Description");
            description.transform.SetParent(panel.transform, false);
            var descText = description.AddComponent<Text>();
            descText.text = "Clique em 'Iniciar' para começar a configuração";
            descText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            descText.fontSize = 18;
            descText.color = Color.white;
            descText.alignment = TextAnchor.MiddleCenter;
            
            var descRect = description.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0.1f, 0.4f);
            descRect.anchorMax = new Vector2(0.9f, 0.7f);
            descRect.sizeDelta = Vector2.zero;
            descRect.anchoredPosition = Vector2.zero;
            
            // Create buttons
            CreateWizardButton("Iniciar", new Vector2(0.3f, 0.2f), StartWizard);
            CreateWizardButton("Pular", new Vector2(0.7f, 0.2f), SkipWizard);
            
            wizardUI.SetActive(false);
        }
        
        private void CreateWizardButton(string text, Vector2 position, System.Action action)
        {
            var button = new GameObject($"Button_{text}");
            button.transform.SetParent(wizardUI.transform.Find("Panel"), false);
            
            var buttonImage = button.AddComponent<Image>();
            buttonImage.color = new Color(0.3f, 0.6f, 0.9f, 1f);
            
            var buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(position.x - 0.15f, position.y - 0.05f);
            buttonRect.anchorMax = new Vector2(position.x + 0.15f, position.y + 0.05f);
            buttonRect.sizeDelta = Vector2.zero;
            buttonRect.anchoredPosition = Vector2.zero;
            
            var buttonComponent = button.AddComponent<Button>();
            buttonComponent.onClick.AddListener(() => action?.Invoke());
            
            var buttonText = new GameObject("Text");
            buttonText.transform.SetParent(button.transform, false);
            var textComponent = buttonText.AddComponent<Text>();
            textComponent.text = text;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            textComponent.fontSize = 16;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            var textRect = buttonText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
        }
        
        #endregion
        
        #region Wizard Steps
        
        private void SetupWizardSteps()
        {
            steps.Clear();
            
            steps.Add(new WizardStep
            {
                title = "Passo 1: Configurar Bordas das TVs",
                description = "Posicione os colisores vermelhos nas bordas das TVs. Use as setas do teclado para mover.",
                buttonText = "Próximo",
                action = () => NextStep()
            });
            
            steps.Add(new WizardStep
            {
                title = "Passo 2: Posicionar Conteúdos",
                description = "Clique onde você quer que os conteúdos apareçam. Use as cores para identificar.",
                buttonText = "Próximo",
                action = () => NextStep()
            });
            
            steps.Add(new WizardStep
            {
                title = "Passo 3: Configurar Marcadores",
                description = "Ajuste os raios de ativação dos marcadores conforme necessário.",
                buttonText = "Próximo",
                action = () => NextStep()
            });
            
            steps.Add(new WizardStep
            {
                title = "Passo 4: Testar Sistema",
                description = "Teste o sistema movendo a câmera e verificando se os conteúdos aparecem corretamente.",
                buttonText = "Finalizar",
                action = () => FinishWizard()
            });
        }
        
        private void ShowCurrentStep()
        {
            if (currentStep >= 0 && currentStep < steps.Count)
            {
                var step = steps[currentStep];
                UpdateWizardUI(step.title, step.description, step.buttonText);
                
                switch (currentStep)
                {
                    case 0:
                        ShowBorderSetup();
                        break;
                    case 1:
                        ShowContentPlacement();
                        break;
                    case 2:
                        ShowMarkerConfiguration();
                        break;
                    case 3:
                        ShowTesting();
                        break;
                }
            }
        }
        
        private void NextStep()
        {
            currentStep++;
            if (currentStep < steps.Count)
            {
                ShowCurrentStep();
            }
            else
            {
                FinishWizard();
            }
        }
        
        private void PreviousStep()
        {
            currentStep--;
            if (currentStep >= 0)
            {
                ShowCurrentStep();
            }
        }
        
        #endregion
        
        #region Step Implementations
        
        private void ShowBorderSetup()
        {
            Debug.Log("=== PASSO 1: CONFIGURAR BORDAS DAS TVs ===");
            
            // Create border colliders
            CreateBorderColliders();
            
            // Show instructions
            ShowInstructions("Use as setas do teclado para mover os colisores vermelhos para as bordas das TVs.\n" +
                          "Pressione ENTER quando estiver satisfeito com as posições.");
        }
        
        private void CreateBorderColliders()
        {
            // Create left border
            leftBorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            leftBorder.name = "Colisor Esquerdo";
            leftBorder.transform.SetParent(borderColliders.transform);
            leftBorder.transform.position = new Vector3(-500, 0, 10);
            leftBorder.transform.localScale = new Vector3(50, 2000, 50);
            
            var leftRenderer = leftBorder.GetComponent<Renderer>();
            leftRenderer.material.color = Color.red;
            
            // Create right border
            rightBorder = GameObject.CreatePrimitive(PrimitiveType.Cube);
            rightBorder.name = "Colisor Direito";
            rightBorder.transform.SetParent(borderColliders.transform);
            rightBorder.transform.position = new Vector3(500, 0, 10);
            rightBorder.transform.localScale = new Vector3(50, 2000, 50);
            
            var rightRenderer = rightBorder.GetComponent<Renderer>();
            rightRenderer.material.color = Color.red;
            
            Debug.Log("Colisores criados! Use as setas para movê-los.");
        }
        
        private void ShowContentPlacement()
        {
            Debug.Log("=== PASSO 2: POSICIONAR CONTEÚDOS ===");
            
            // Clear previous content markers
            ClearContentMarkers();
            
            // Create content placeholders
            CreateContentPlaceholders();
            
            ShowInstructions("Clique onde você quer que os conteúdos apareçam.\n" +
                          "Verde = Conteúdo 1, Azul = Conteúdo 2, Vermelho = Vídeo");
        }
        
        private void CreateContentPlaceholders()
        {
            Color[] colors = { Color.green, Color.blue, Color.red };
            string[] names = { "Conteúdo Verde", "Conteúdo Azul", "Conteúdo Vídeo" };
            
            for (int i = 0; i < 3; i++)
            {
                var marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker.name = names[i];
                marker.transform.SetParent(contentPlaceholders.transform);
                marker.transform.position = new Vector3(1000 + i * 2000, 0, 10);
                marker.transform.localScale = Vector3.one * 100;
                
                var renderer = marker.GetComponent<Renderer>();
                renderer.material.color = colors[i];
                
                // Add click handler
                var clickHandler = marker.AddComponent<ContentPlaceholder>();
                clickHandler.Initialize(this, i, colors[i]);
                
                contentMarkers.Add(marker);
            }
        }
        
        private void ClearContentMarkers()
        {
            foreach (var marker in contentMarkers)
            {
                if (marker != null)
                    DestroyImmediate(marker);
            }
            contentMarkers.Clear();
        }
        
        private void ShowMarkerConfiguration()
        {
            Debug.Log("=== PASSO 3: CONFIGURAR MARCADORES ===");
            
            // Create markers in MarkerSystem
            CreateMarkersInSystem();
            
            ShowInstructions("Os marcadores foram criados no sistema.\n" +
                          "Ajuste os raios de ativação no Inspector se necessário.");
        }
        
        private void CreateMarkersInSystem()
        {
            if (markerSystem == null) return;
            
            // Clear existing markers
            markerSystem.LimparMarcadores();
            
            // Create markers based on content placeholders
            for (int i = 0; i < contentMarkers.Count; i++)
            {
                var marker = contentMarkers[i];
                if (marker == null) continue;
                
                var contentMarker = new ContentMarker
                {
                    markerName = $"Marcador {i + 1}",
                    worldPositionX = marker.transform.position.x,
                    activationRadius = 200f,
                    fadeInDuration = 0.5f,
                    fadeOutDuration = 0.5f,
                    debugColor = marker.GetComponent<Renderer>().material.color
                };
                
                // Create content
                var content = CreateContentForMarker(i, marker.GetComponent<Renderer>().material.color);
                contentMarker.contentToActivate = content;
                
                markerSystem.AddMarker(contentMarker);
            }
            
            Debug.Log($"Criados {contentMarkers.Count} marcadores no sistema!");
        }
        
        private GameObject CreateContentForMarker(int index, Color color)
        {
            if (targetCanvas == null) return null;
            
            var content = new GameObject($"Conteúdo {index + 1}");
            content.transform.SetParent(targetCanvas.transform, false);
            
            var image = content.AddComponent<Image>();
            image.color = color;
            
            var rectTransform = content.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var canvasGroup = content.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            content.SetActive(false);
            
            return content;
        }
        
        private void ShowTesting()
        {
            Debug.Log("=== PASSO 4: TESTAR SISTEMA ===");
            
            ShowInstructions("Teste o sistema:\n" +
                          "1. Use as setas para mover a câmera\n" +
                          "2. Verifique se os conteúdos aparecem\n" +
                          "3. Ajuste posições se necessário");
        }
        
        #endregion
        
        #region UI Updates
        
        private void UpdateWizardUI(string title, string description, string buttonText)
        {
            if (wizardUI == null) return;
            
            var titleText = wizardUI.transform.Find("Panel/Title").GetComponent<Text>();
            var descText = wizardUI.transform.Find("Panel/Description").GetComponent<Text>();
            
            if (titleText != null) titleText.text = title;
            if (descText != null) descText.text = description + "\n\n[MODO CONFIGURAÇÃO ATIVO - Setas do teclado em uso]";
            
            wizardUI.SetActive(true);
        }
        
        private void ShowInstructions(string message)
        {
            Debug.Log($"[WIZARD] {message}");
        }
        
        #endregion
        
        #region Public Methods
        
        [ContextMenu("Iniciar Wizard")]
        public void StartWizard()
        {
            InitializeWizard();
        }
        
        [ContextMenu("Reativar Modo Configuração")]
        public void ReactivateConfigurationMode()
        {
            Debug.Log("=== REATIVANDO MODO CONFIGURAÇÃO ===");
            
            // Show visual elements
            if (borderColliders != null)
                borderColliders.SetActive(true);
            if (contentPlaceholders != null)
                contentPlaceholders.SetActive(true);
            
            isWizardActive = true;
            ShowCurrentStep();
            
            Debug.Log("Modo configuração reativado! As setas do teclado estão disponíveis.");
        }
        
        [ContextMenu("Pular Wizard")]
        public void SkipWizard()
        {
            Debug.Log("Wizard pulado. Use os métodos de contexto para configuração manual.");
            if (wizardUI != null)
                wizardUI.SetActive(false);
        }
        
        [ContextMenu("Finalizar Wizard")]
        public void FinishWizard()
        {
            Debug.Log("=== WIZARD FINALIZADO ===");
            Debug.Log("Sistema configurado com sucesso!");
            Debug.Log("Use o MarkerSystem para ajustes finais.");
            
            ExitConfigurationMode();
        }
        
        [ContextMenu("Sair do Modo Configuração")]
        public void ExitConfigurationMode()
        {
            Debug.Log("=== SAINDO DO MODO CONFIGURAÇÃO ===");
            Debug.Log("As setas do teclado agora funcionam normalmente.");
            
            if (wizardUI != null)
                wizardUI.SetActive(false);
                
            isWizardActive = false;
            
            // Hide visual elements
            if (borderColliders != null)
                borderColliders.SetActive(false);
            if (contentPlaceholders != null)
                contentPlaceholders.SetActive(false);
        }
        
        #endregion
        
        #region Input Handling
        
        private void Update()
        {
            if (!isWizardActive) return;
            
            HandleWizardInput();
        }
        
        private void HandleWizardInput()
        {
            // Only handle input when wizard is active
            if (!isWizardActive) return;
            
            // Step navigation
            if (Input.GetKeyDown(KeyCode.Alpha1)) 
            {
                currentStep = 0;
                ShowCurrentStep();
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) 
            {
                currentStep = 1;
                ShowCurrentStep();
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) 
            {
                currentStep = 2;
                ShowCurrentStep();
            }
            if (Input.GetKeyDown(KeyCode.Alpha4)) 
            {
                currentStep = 3;
                ShowCurrentStep();
            }
            
            // Border movement - only in step 0
            if (currentStep == 0 && isWizardActive)
            {
                HandleBorderMovement();
            }
            
            // Step navigation
            if (Input.GetKeyDown(KeyCode.Return))
            {
                NextStep();
            }
            
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                PreviousStep();
            }
            
            // Exit configuration mode
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                ExitConfigurationMode();
            }
        }
        
        private void HandleBorderMovement()
        {
            float moveSpeed = 50f;
            
            if (leftBorder != null)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                    leftBorder.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
                if (Input.GetKey(KeyCode.RightArrow))
                    leftBorder.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
            
            if (rightBorder != null)
            {
                if (Input.GetKey(KeyCode.LeftArrow))
                    rightBorder.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
                if (Input.GetKey(KeyCode.RightArrow))
                    rightBorder.transform.Translate(Vector3.right * moveSpeed * Time.deltaTime);
            }
        }
        
        #endregion
    }
    
    // Helper class for content placeholders
    public class ContentPlaceholder : MonoBehaviour
    {
        private TrilhoSetupWizard wizard;
        private int index;
        private Color color;
        
        public void Initialize(TrilhoSetupWizard wizard, int index, Color color)
        {
            this.wizard = wizard;
            this.index = index;
            this.color = color;
        }
        
        private void OnMouseDown()
        {
            Debug.Log($"Marcador {index + 1} clicado! Posição: {transform.position}");
        }
    }
}
