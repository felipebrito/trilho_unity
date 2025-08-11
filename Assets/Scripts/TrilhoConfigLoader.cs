using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;

namespace Trilho
{
    [System.Serializable]
    public class TrilhoConfig
    {
        [Header("Configurações Gerais")]
        public string configName = "Configuração Padrão";
        public string description = "";
        public string version = "1.0.0";
        public DateTime lastModified = DateTime.Now;
        
        [Header("Configurações do Trilho")]
        public float physicalMinCm = 0f;
        public float physicalMaxCm = 600f;
        public float unityMinPosition = 0f;
        public float unityMaxPosition = 8520f;
        public float screenWidthInCm = 60f;
        public float movementSensitivity = 0.5f;
        
        [Header("Configurações da Câmera")]
        public bool smoothCameraMovement = true;
        public float cameraSmoothing = 5f;
        
        [Header("Configurações OSC")]
        public string oscAddress = "/unity";
        public bool oscValorNormalizado01 = true;
        public int oscPort = 9000;
        
        [Header("Zonas de Conteúdo")]
        public List<ContentZone> contentZones = new List<ContentZone>();
        
        [Header("Configurações de Debug")]
        public bool showDebugInfo = true;
        public bool simulatePosition = true;
    }
    
    [System.Serializable]
    public class ContentZone
    {
        public string name = "Nova Zona";
        public string description = "";
        public float positionCm = 0f; // Posição em cm do início do trilho
        public float paddingCm = 0f; // Padding para ativação/desativação
        public float enterPaddingCm = 0f; // Padding específico para entrada
        public float exitPaddingCm = 0f; // Padding específico para saída
        
        [Header("Tipo de Conteúdo")]
        public ContentType contentType = ContentType.Image;
        public string contentFileName = ""; // Nome do arquivo na pasta StreamingAssets
        public string contentPath = ""; // Caminho relativo para o arquivo
        
        [Header("Posicionamento")]
        public bool placeContentAtWorldX = false;
        public float contentOffsetCm = 0f;
        public bool keepUpdatingWhileActive = false;
        public PositionReference reference = PositionReference.LeftEdge;
        
        [Header("Transições")]
        public float fadeInSeconds = 0.35f;
        public float fadeOutSeconds = 0.35f;
        
        [Header("Configurações Específicas")]
        public ImageContentSettings imageSettings = new ImageContentSettings();
        public VideoContentSettings videoSettings = new VideoContentSettings();
        public TextContentSettings textSettings = new TextContentSettings();
        public ApplicationContentSettings appSettings = new ApplicationContentSettings();
    }
    
    [System.Serializable]
    public enum ContentType
    {
        Image,
        Video,
        Text,
        Application
    }
    
    [System.Serializable]
    public enum PositionReference
    {
        LeftEdge,
        Center
    }
    
    [System.Serializable]
    public class ImageContentSettings
    {
        public bool maintainAspectRatio = true;
        public Vector2 size = new Vector2(1920, 1080);
        public Color tint = Color.white;
    }
    
    [System.Serializable]
    public class VideoContentSettings
    {
        public bool loop = true;
        public bool playOnAwake = false;
        public float volume = 1f;
        public bool mute = false;
    }
    
    [System.Serializable]
    public class TextContentSettings
    {
        public string text = "Texto da zona";
        public int fontSize = 48;
        public Color textColor = Color.white;
        public TextAnchor alignment = TextAnchor.MiddleCenter;
        public FontStyle fontStyle = FontStyle.Normal;
    }
    
    [System.Serializable]
    public class ApplicationContentSettings
    {
        public string applicationPath = "";
        public string arguments = "";
        public bool waitForExit = false;
        public bool hideUnityWhileRunning = true;
        public float launchTimeout = 10f;
        public bool autoCloseOnTimeout = false;
    }
    
    public class TrilhoConfigLoader : MonoBehaviour
    {
        [Header("Configuração")]
        [Tooltip("Nome do arquivo de configuração na pasta StreamingAssets")]
        [SerializeField] private string configFileName = "trilho_config.json";
        [Tooltip("Carregar automaticamente no Start")]
        [SerializeField] private bool autoLoadOnStart = true;
        [Tooltip("Mostrar logs de debug")]
        [SerializeField] private bool showDebugInfo = true;
        
        [Header("Referências")]
        [SerializeField] private TrilhoZoneActivator zoneActivator;
        [SerializeField] private Canvas mainCanvas; // Canvas principal da cena
        
        private TrilhoConfig currentConfig;
        private string configFilePath;
        
        public TrilhoConfig CurrentConfig => currentConfig;
        public bool IsConfigLoaded => currentConfig != null;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Encontrar o Canvas principal da cena automaticamente
            if (mainCanvas == null)
            {
                // Primeiro tentar encontrar pelo nome específico
                Canvas[] allCanvases = FindObjectsOfType<Canvas>();
                foreach (var canvas in allCanvases)
                {
                    if (canvas.name.Contains("CANVAS PRINCIPAL") || canvas.name.Contains("Canvas"))
                    {
                        mainCanvas = canvas;
                        if (showDebugInfo)
                            UnityEngine.Debug.Log($"[TRILHO CONFIG] Canvas principal encontrado por nome: {mainCanvas.name}");
                        break;
                    }
                }
                
                // Se não encontrou pelo nome, usar o primeiro Canvas encontrado
                if (mainCanvas == null && allCanvases.Length > 0)
                {
                    mainCanvas = allCanvases[0];
                    if (showDebugInfo)
                        UnityEngine.Debug.Log($"[TRILHO CONFIG] Canvas encontrado (primeiro disponível): {mainCanvas.name}");
                }
                
                if (mainCanvas == null)
                {
                    UnityEngine.Debug.LogError("[TRILHO CONFIG] Nenhum Canvas encontrado na cena! Configure manualmente o campo 'Main Canvas'.");
                }
            }
            
            // Encontrar referências automaticamente se não estiverem definidas
            if (zoneActivator == null)
                zoneActivator = FindObjectOfType<TrilhoZoneActivator>();
            
            // Construir caminho para o arquivo de configuração
            configFilePath = Path.Combine(Application.streamingAssetsPath, configFileName);
        }
        
        private void Start()
        {
            if (autoLoadOnStart)
            {
                LoadConfiguration();
            }
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Carrega a configuração do arquivo JSON
        /// </summary>
        public bool LoadConfiguration()
        {
            try
            {
                if (!File.Exists(configFilePath))
                {
                                if (showDebugInfo)
                UnityEngine.Debug.LogWarning($"[TRILHO CONFIG] Arquivo de configuração não encontrado: {configFilePath}");
                    
                    // Criar configuração padrão
                    CreateDefaultConfiguration();
                    return false;
                }
                
                string jsonContent = File.ReadAllText(configFilePath);
                currentConfig = JsonUtility.FromJson<TrilhoConfig>(jsonContent);
                
                            if (showDebugInfo)
                UnityEngine.Debug.Log($"[TRILHO CONFIG] Configuração carregada: {currentConfig.configName}");
                
                // Aplicar configuração aos componentes
                ApplyConfiguration();
                
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[TRILHO CONFIG] Erro ao carregar configuração: {e.Message}");
                CreateDefaultConfiguration();
                return false;
            }
        }
        
        /// <summary>
        /// Salva a configuração atual em arquivo JSON
        /// </summary>
        public bool SaveConfiguration()
        {
            try
            {
                if (currentConfig == null)
                {
                    UnityEngine.Debug.LogWarning("[TRILHO CONFIG] Nenhuma configuração para salvar");
                    return false;
                }
                
                currentConfig.lastModified = DateTime.Now;
                string jsonContent = JsonUtility.ToJson(currentConfig, true);
                File.WriteAllText(configFilePath, jsonContent);
                
                if (showDebugInfo)
                    UnityEngine.Debug.Log($"[TRILHO CONFIG] Configuração salva: {configFilePath}");
                
                return true;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[TRILHO CONFIG] Erro ao salvar configuração: {e.Message}");
                return false;
            }
        }
        
        /// <summary>
        /// Cria uma nova configuração padrão
        /// </summary>
        public void CreateDefaultConfiguration()
        {
            currentConfig = new TrilhoConfig();
            
            // Adicionar algumas zonas de exemplo
            currentConfig.contentZones.Add(new ContentZone
            {
                name = "Zona de Exemplo",
                description = "Zona de exemplo para demonstração",
                positionCm = 100f,
                contentType = ContentType.Image,
                contentFileName = "example_image.png"
            });
            
                            if (showDebugInfo)
                    UnityEngine.Debug.Log("[TRILHO CONFIG] Configuração padrão criada");
        }
        
        /// <summary>
        /// Aplica a configuração atual aos componentes do Trilho
        /// </summary>
        public void ApplyConfiguration()
        {
            if (currentConfig == null || zoneActivator == null)
                return;
            
            try
            {
                // Aplicar configurações ao TrilhoGameManager
                // gameManager.SetScreenWidthCm(currentConfig.screenWidthInCm); // REMOVIDO
                
                // Aplicar configurações ao TrilhoZoneActivator
                if (zoneActivator != null)
                {
                    ApplyZonesToActivator();
                }
                
                if (showDebugInfo)
                    UnityEngine.Debug.Log("[TRILHO CONFIG] Configuração aplicada aos componentes");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[TRILHO CONFIG] Erro ao aplicar configuração: {e.Message}");
            }
        }
        
        /// <summary>
        /// Recarrega a configuração do arquivo
        /// </summary>
        public void ReloadConfiguration()
        {
            LoadConfiguration();
        }
        
        #endregion
        
        #region Private Methods
        
        private void ApplyZonesToActivator()
        {
            if (zoneActivator == null || currentConfig == null) return;
            
            // Verificar se contentZones não é null
            if (currentConfig.contentZones == null)
            {
                UnityEngine.Debug.LogWarning("[TRILHO CONFIG] contentZones é null, criando lista vazia");
                currentConfig.contentZones = new List<ContentZone>();
                return;
            }
            
            // Limpar conteúdos antigos antes de criar novos
            CleanupOldContent();
            
            // Converter ContentZone para Zone do TrilhoZoneActivator
            var zones = new List<TrilhoZoneActivator.Zone>();
            
            foreach (var contentZone in currentConfig.contentZones)
            {
                // Verificar se contentZone não é null
                if (contentZone == null)
                {
                    UnityEngine.Debug.LogWarning("[TRILHO CONFIG] ContentZone null encontrado, pulando...");
                    continue;
                }
                
                // Verificar se campos obrigatórios não são null
                if (string.IsNullOrEmpty(contentZone.name))
                {
                    UnityEngine.Debug.LogWarning("[TRILHO CONFIG] ContentZone sem nome encontrado, pulando...");
                    continue;
                }
                
                var zone = new TrilhoZoneActivator.Zone
                {
                    name = contentZone.name ?? "Zona Sem Nome",
                    positionCm = contentZone.positionCm,
                    paddingCm = contentZone.paddingCm,
                    enterPaddingCm = contentZone.enterPaddingCm,
                    exitPaddingCm = contentZone.exitPaddingCm,
                    placeContentAtWorldX = contentZone.placeContentAtWorldX,
                    contentOffsetCm = contentZone.contentOffsetCm,
                    keepUpdatingWhileActive = contentZone.keepUpdatingWhileActive,
                    reference = (TrilhoZoneActivator.PositionReference)contentZone.reference
                };
                
                // Criar GameObject para o conteúdo baseado no tipo
                zone.content = CreateContentGameObject(contentZone);
                
                zones.Add(zone);
            }
            
            // Aplicar zonas ao ativador usando reflexão (já que zones é privado)
            var zonesField = typeof(TrilhoZoneActivator).GetField("zones", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (zonesField != null)
            {
                zonesField.SetValue(zoneActivator, zones);
                if (showDebugInfo)
                    UnityEngine.Debug.Log($"[TRILHO CONFIG] {zones.Count} zonas aplicadas ao ativador");
            }
        }
        
        private void CleanupOldContent()
        {
            if (mainCanvas == null) return;
            
            // Encontrar e destruir conteúdos antigos
            var oldContents = mainCanvas.GetComponentsInChildren<Transform>(true);
            foreach (var child in oldContents)
            {
                if (child != mainCanvas.transform && child.name.StartsWith("Content_"))
                {
                    if (showDebugInfo)
                        UnityEngine.Debug.Log($"[TRILHO CONFIG] Removendo conteúdo antigo: {child.name}");
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        private GameObject CreateContentGameObject(ContentZone contentZone)
        {
            // Verificar se contentZone não é null
            if (contentZone == null)
            {
                UnityEngine.Debug.LogError("[TRILHO CONFIG] ContentZone é null! Não é possível criar conteúdo.");
                return null;
            }
            
            if (mainCanvas == null)
            {
                UnityEngine.Debug.LogError("[TRILHO CONFIG] Canvas principal não encontrado! Não é possível criar conteúdo.");
                return null;
            }
            
            var zoneName = string.IsNullOrEmpty(contentZone.name) ? "ZonaSemNome" : contentZone.name;
            var go = new GameObject($"Content_{zoneName}");
            
            // Definir o Canvas como pai do conteúdo
            go.transform.SetParent(mainCanvas.transform, false);
            
            // Adicionar RectTransform para funcionar com o Canvas
            var rectTransform = go.AddComponent<RectTransform>();
            
            // Adicionar CanvasGroup para controle de fade
            var canvasGroup = go.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Começa invisível para fade in
            
            // Configurar RectTransform para posicionamento correto baseado na configuração JSON
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(1080f, 1920f);
            
            // Usar a posição da configuração JSON (convertida de cm para unidades Unity)
            float positionInUnity = contentZone.positionCm * (8520f / 600f); // Converter de cm para posição Unity
            rectTransform.anchoredPosition = new Vector2(positionInUnity, 0f);
            
            switch (contentZone.contentType)
            {
                case ContentType.Image:
                    CreateImageContent(go, contentZone);
                    break;
                case ContentType.Video:
                    CreateVideoContent(go, contentZone);
                    break;
                case ContentType.Text:
                    CreateTextContent(go, contentZone);
                    break;
                case ContentType.Application:
                    CreateApplicationContent(go, contentZone);
                    break;
            }
            
            return go;
        }
        
        private void CreateImageContent(GameObject go, ContentZone contentZone)
        {
            // Verificar se contentZone não é null
            if (contentZone == null)
            {
                UnityEngine.Debug.LogError("[TRILHO CONFIG] ContentZone é null em CreateImageContent!");
                return;
            }
            
            // Criar fundo colorido com transparência para zonas de imagem
            var background = new GameObject("Background");
            background.transform.SetParent(go.transform, false);
            var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            
            // Gerar cor aleatória baseada no nome da zona para diferenciar visualmente
            var zoneColor = GenerateZoneColor(contentZone.name);
            backgroundImage.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.7f); // Fundo colorido com transparência
            
            // Obter ou adicionar RectTransform ao fundo (Image já adiciona automaticamente)
            var bgRectTransform = background.GetComponent<RectTransform>();
            if (bgRectTransform == null)
            {
                bgRectTransform = background.AddComponent<RectTransform>();
            }
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Criar container para a imagem
            var imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(go.transform, false);
            var image = imageContainer.AddComponent<UnityEngine.UI.Image>();
            
            // Obter ou adicionar RectTransform ao container de imagem (Image já adiciona automaticamente)
            var imageRectTransform = imageContainer.GetComponent<RectTransform>();
            if (imageRectTransform == null)
            {
                imageRectTransform = imageContainer.AddComponent<RectTransform>();
            }
            
            // Verificar se imageSettings não é null e usar valores padrão se necessário
            var imageSettings = contentZone.imageSettings ?? new ImageContentSettings();
            
            // Carregar textura se o arquivo existir
            string imagePath = Path.Combine(Application.streamingAssetsPath, contentZone.contentFileName ?? "");
            if (File.Exists(imagePath))
            {
                byte[] imageData = File.ReadAllBytes(imagePath);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
                image.sprite = sprite;
            }
            
            // Aplicar configurações
            if (imageSettings.maintainAspectRatio)
            {
                var aspectRatioFitter = imageContainer.AddComponent<UnityEngine.UI.AspectRatioFitter>();
                aspectRatioFitter.aspectMode = UnityEngine.UI.AspectRatioFitter.AspectMode.FitInParent;
            }
            
            image.color = imageSettings.tint;
            
            // Configurar RectTransform do container de imagem para centralizar
            imageRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            imageRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            imageRectTransform.pivot = new Vector2(0.5f, 0.5f);
            imageRectTransform.sizeDelta = new Vector2(1000f, 1800f); // Ligeiramente menor que o pai
            imageRectTransform.anchoredPosition = Vector2.zero;
        }
        
        private void CreateVideoContent(GameObject go, ContentZone contentZone)
        {
            // Verificar se contentZone não é null
            if (contentZone == null)
            {
                UnityEngine.Debug.LogError("[TRILHO CONFIG] ContentZone é null em CreateVideoContent!");
                return;
            }
            
            // Criar fundo colorido com transparência para zonas de vídeo
            var background = new GameObject("Background");
            background.transform.SetParent(go.transform, false);
            var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            
            // Gerar cor aleatória baseada no nome da zona para diferenciar visualmente
            var zoneColor = GenerateZoneColor(contentZone.name);
            backgroundImage.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.7f); // Fundo colorido com transparência
            
            // Obter ou adicionar RectTransform ao fundo (Image já adiciona automaticamente)
            var bgRectTransform = background.GetComponent<RectTransform>();
            if (bgRectTransform == null)
            {
                bgRectTransform = background.AddComponent<RectTransform>();
            }
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Criar container para o vídeo
            var videoContainer = new GameObject("VideoContainer");
            videoContainer.transform.SetParent(go.transform, false);
            var rawImage = videoContainer.AddComponent<UnityEngine.UI.RawImage>();
            
            // Obter ou adicionar RectTransform ao container de vídeo (RawImage já adiciona automaticamente)
            var videoRectTransform = videoContainer.GetComponent<RectTransform>();
            if (videoRectTransform == null)
            {
                videoRectTransform = videoContainer.AddComponent<RectTransform>();
            }
            
            // Criar VideoPlayer como filho do container
            var videoPlayerObject = new GameObject("VideoPlayer");
            videoPlayerObject.transform.SetParent(videoContainer.transform, false);
            var videoPlayer = videoPlayerObject.AddComponent<UnityEngine.Video.VideoPlayer>();
            
            // Verificar se videoSettings não é null e usar valores padrão se necessário
            var videoSettings = contentZone.videoSettings ?? new VideoContentSettings();
            
            // Configurar vídeo
            string videoPath = Path.Combine(Application.streamingAssetsPath, contentZone.contentFileName ?? "");
            if (File.Exists(videoPath))
            {
                videoPlayer.url = videoPath;
                videoPlayer.isLooping = videoSettings.loop;
                videoPlayer.playOnAwake = videoSettings.playOnAwake;
                videoPlayer.SetDirectAudioVolume(0, videoSettings.volume);
                videoPlayer.SetDirectAudioMute(0, videoSettings.mute);
                
                // Conectar vídeo à imagem
                videoPlayer.renderMode = UnityEngine.Video.VideoRenderMode.RenderTexture;
                var renderTexture = new RenderTexture(1920, 1080, 24);
                renderTexture.Create();
                videoPlayer.targetTexture = renderTexture;
                rawImage.texture = renderTexture;
            }
            
            // Configurar RectTransform do container de vídeo para centralizar
            videoRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            videoRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            videoRectTransform.pivot = new Vector2(0.5f, 0.5f);
            videoRectTransform.sizeDelta = new Vector2(1000f, 1800f); // Ligeiramente menor que o pai
            videoRectTransform.anchoredPosition = Vector2.zero;
        }
        
        private void CreateTextContent(GameObject go, ContentZone contentZone)
        {
            // Verificar se contentZone e suas configurações não são null
            if (contentZone == null)
            {
                UnityEngine.Debug.LogError("[TRILHO CONFIG] ContentZone é null em CreateTextContent!");
                return;
            }
            
            // Criar fundo colorido
            var background = new GameObject("Background");
            background.transform.SetParent(go.transform, false);
            var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            
            // Obter ou adicionar RectTransform ao fundo (Image já adiciona automaticamente)
            var bgRectTransform = background.GetComponent<RectTransform>();
            if (bgRectTransform == null)
            {
                bgRectTransform = background.AddComponent<RectTransform>();
            }
            
            // Gerar cor aleatória baseada no nome da zona para diferenciar visualmente
            var zoneColor = GenerateZoneColor(contentZone.name);
            backgroundImage.color = new Color(zoneColor.r, zoneColor.g, zoneColor.b, 0.7f); // Fundo colorido com transparência
            
            // Configurar RectTransform do fundo para cobrir toda a área do objeto pai
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Criar texto como filho separado
            var textObject = new GameObject("Text");
            textObject.transform.SetParent(go.transform, false);
            var text = textObject.AddComponent<UnityEngine.UI.Text>();
            
            // Obter ou adicionar RectTransform ao texto (Text já adiciona automaticamente)
            var textRectTransform = textObject.GetComponent<RectTransform>();
            if (textRectTransform == null)
            {
                textRectTransform = textObject.AddComponent<RectTransform>();
            }
            
            // Verificar se textSettings não é null e usar valores padrão se necessário
            var textSettings = contentZone.textSettings ?? new TextContentSettings();
            
            // Configurar texto
            text.text = textSettings.text ?? "Texto da zona";
            text.fontSize = textSettings.fontSize;
            text.color = Color.black; // Texto preto para garantir visibilidade
            text.alignment = textSettings.alignment;
            text.fontStyle = textSettings.fontStyle;
            
            // Usar fonte padrão do sistema
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            
            // Configurar RectTransform do texto para centralizar
            textRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);
            textRectTransform.sizeDelta = new Vector2(1000f, 1800f); // Ligeiramente menor que o pai
            textRectTransform.anchoredPosition = Vector2.zero;
        }
        
        private void CreateApplicationContent(GameObject go, ContentZone contentZone)
        {
            // Verificar se contentZone não é null
            if (contentZone == null)
            {
                UnityEngine.Debug.LogError("[TRILHO CONFIG] ContentZone é null em CreateApplicationContent!");
                return;
            }
            
            // Adicionar gerenciador de aplicação
            var appManager = go.AddComponent<ContentManagers.ApplicationContentManager>();
            
            // Verificar se appSettings não é null e usar valores padrão se necessário
            var settings = contentZone.appSettings ?? new ApplicationContentSettings();
            
            // Configurar aplicação
            appManager.SetApplicationPath(settings.applicationPath ?? "");
            appManager.SetApplicationArguments(settings.arguments ?? "");
        }
        
        /// <summary>
        /// Gera uma cor única baseada no nome da zona para diferenciação visual
        /// </summary>
        /// <param name="zoneName">Nome da zona</param>
        /// <returns>Cor única para a zona</returns>
        private Color GenerateZoneColor(string zoneName)
        {
            if (string.IsNullOrEmpty(zoneName))
                return new Color(0.5f, 0.5f, 0.5f, 0.7f); // Cor padrão cinza
            
            // Gerar hash do nome para criar cor consistente
            int hash = zoneName.GetHashCode();
            var random = new System.Random(hash);
            
            // Cores vibrantes e diferentes para cada zona
            float r = (float)random.NextDouble() * 0.8f + 0.2f; // 0.2 a 1.0
            float g = (float)random.NextDouble() * 0.8f + 0.2f; // 0.2 a 1.0
            float b = (float)random.NextDouble() * 0.8f + 0.2f; // 0.2 a 1.0
            
            return new Color(r, g, b);
        }
        
        #endregion
        
        #region Editor Support
        
        #if UNITY_EDITOR
        [ContextMenu("Carregar Configuração")]
        private void EditorLoadConfiguration()
        {
            LoadConfiguration();
        }
        
        [ContextMenu("Salvar Configuração")]
        private void EditorSaveConfiguration()
        {
            SaveConfiguration();
        }
        
        [ContextMenu("Criar Configuração Padrão")]
        private void EditorCreateDefaultConfiguration()
        {
            CreateDefaultConfiguration();
        }
        #endif
        
        #endregion
    }
}
