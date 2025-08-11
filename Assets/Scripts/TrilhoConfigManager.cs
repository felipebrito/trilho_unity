using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.IO;
using System;
using UnityEngine.Video;

namespace Trilho
{
    public class TrilhoConfigManager : MonoBehaviour
    {
        [Header("Referências")]
        [SerializeField] private TrilhoGameManager gameManager;
        [SerializeField] private TrilhoZoneActivator zoneActivator;
        [SerializeField] private Canvas mainCanvas; // Canvas principal da cena
        [SerializeField] private string configFileName = "trilho_config.json";
        
        [Header("Configuração Atual")]
        [SerializeField] private TrilhoConfig currentConfig;
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private bool showDebugInfo = true;
        
        private string ConfigPath => Path.Combine(Application.streamingAssetsPath, configFileName);
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            if (gameManager == null) gameManager = FindObjectOfType<TrilhoGameManager>();
            if (zoneActivator == null) zoneActivator = FindObjectOfType<TrilhoZoneActivator>();
            
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
                            UnityEngine.Debug.Log($"[CONFIG] Canvas principal encontrado por nome: {mainCanvas.name}");
                        break;
                    }
                }
                
                // Se não encontrou pelo nome, usar o primeiro Canvas encontrado
                if (mainCanvas == null && allCanvases.Length > 0)
                {
                    mainCanvas = allCanvases[0];
                    if (showDebugInfo)
                        UnityEngine.Debug.Log($"[CONFIG] Canvas encontrado (primeiro disponível): {mainCanvas.name}");
                }
                
                if (mainCanvas == null)
                {
                    UnityEngine.Debug.LogError("[CONFIG] Nenhum Canvas encontrado na cena! Configure manualmente o campo 'Main Canvas'.");
                }
            }
        }
        
        private void Start()
        {
            if (autoLoadOnStart)
            {
                LoadConfig();
            }
        }
        
        #endregion
        
        #region Configuração
        
        public void LoadConfig()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                                if (showDebugInfo)
                UnityEngine.Debug.LogWarning($"[CONFIG] Arquivo de configuração não encontrado: {ConfigPath}");
                    return;
                }
                
                string jsonContent = File.ReadAllText(ConfigPath);
                currentConfig = JsonUtility.FromJson<TrilhoConfig>(jsonContent);
                
                if (showDebugInfo)
                    UnityEngine.Debug.Log($"[CONFIG] Configuração carregada: {currentConfig.configName} v{currentConfig.version}");
                
                ApplyConfig();
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao carregar configuração: {e.Message}");
            }
        }
        
        public void SaveConfig()
        {
            try
            {
                // Garantir que StreamingAssets existe
                if (!Directory.Exists(Application.streamingAssetsPath))
                {
                    Directory.CreateDirectory(Application.streamingAssetsPath);
                }
                
                // Atualizar configuração atual
                UpdateConfigFromCurrent();
                
                string jsonContent = JsonUtility.ToJson(currentConfig, true);
                File.WriteAllText(ConfigPath, jsonContent);
                
                if (showDebugInfo)
                    UnityEngine.Debug.Log($"[CONFIG] Configuração salva: {ConfigPath}");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao salvar configuração: {e.Message}");
            }
        }
        
        public void ApplyConfig()
        {
            if (currentConfig == null) return;
            
            try
            {
                // Aplicar configurações do Trilho
                if (gameManager != null)
                {
                    // Usar reflection para acessar campos privados
                    var type = typeof(TrilhoGameManager);
                    
                    var physicalMinField = type.GetField("physicalMinCm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var physicalMaxField = type.GetField("physicalMaxCm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var unityMinField = type.GetField("unityMinPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var unityMaxField = type.GetField("unityMaxPosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var screenWidthField = type.GetField("screenWidthInCm", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var movementSensitivityField = type.GetField("movementSensitivity", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var smoothCameraField = type.GetField("smoothCameraMovement", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var cameraSmoothingField = type.GetField("cameraSmoothing", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var oscAddressField = type.GetField("oscAddress", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var oscNormalizedField = type.GetField("oscValorNormalizado01", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var simulatePositionField = type.GetField("simulatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (physicalMinField != null) physicalMinField.SetValue(gameManager, currentConfig.physicalMinCm);
                    if (physicalMaxField != null) physicalMaxField.SetValue(gameManager, currentConfig.physicalMaxCm);
                    if (unityMinField != null) unityMinField.SetValue(gameManager, currentConfig.unityMinPosition);
                    if (unityMaxField != null) unityMaxField.SetValue(gameManager, currentConfig.unityMaxPosition);
                    if (screenWidthField != null) screenWidthField.SetValue(gameManager, currentConfig.screenWidthInCm);
                    if (movementSensitivityField != null) movementSensitivityField.SetValue(gameManager, currentConfig.movementSensitivity);
                    if (smoothCameraField != null) smoothCameraField.SetValue(gameManager, currentConfig.smoothCameraMovement);
                    if (cameraSmoothingField != null) cameraSmoothingField.SetValue(gameManager, currentConfig.cameraSmoothing);
                    if (oscAddressField != null) oscAddressField.SetValue(gameManager, currentConfig.oscAddress);
                    if (oscNormalizedField != null) oscNormalizedField.SetValue(gameManager, currentConfig.oscValorNormalizado01);
                    if (simulatePositionField != null) simulatePositionField.SetValue(gameManager, currentConfig.simulatePosition);
                    
                    // Aplicar largura da tela
                    gameManager.SetScreenWidthCm(currentConfig.screenWidthInCm);
                }
                
                // Aplicar zonas
                if (zoneActivator != null)
                {
                    ApplyZonesToActivator();
                }
                
                if (showDebugInfo)
                    UnityEngine.Debug.Log($"[CONFIG] Configuração aplicada com sucesso");
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao aplicar configuração: {e.Message}");
            }
        }
        
        private void ApplyZonesToActivator()
        {
            if (zoneActivator == null)
            {
                UnityEngine.Debug.LogError("[CONFIG] TrilhoZoneActivator não encontrado!");
                return;
            }

            // Limpar conteúdos antigos antes de criar novos
            CleanupOldContent();

            try
            {
                var type = typeof(TrilhoZoneActivator);
                var zonesField = type.GetField("zones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (zonesField != null)
                {
                    var zonesList = new List<object>();
                    
                    foreach (var zoneConfig in currentConfig.contentZones)
                    {
                        var content = LoadContentForZone(zoneConfig);
                        if (content != null)
                        {
                            // Criar objeto Zone usando reflection
                            var zoneType = type.GetNestedType("Zone", System.Reflection.BindingFlags.NonPublic);
                            if (zoneType != null)
                            {
                                var zone = Activator.CreateInstance(zoneType);
                                var nameField = zoneType.GetField("name");
                                var positionField = zoneType.GetField("positionCm");
                                var contentField = zoneType.GetField("content");
                                
                                if (nameField != null) nameField.SetValue(zone, zoneConfig.name);
                                if (positionField != null) positionField.SetValue(zone, zoneConfig.positionCm);
                                if (contentField != null) contentField.SetValue(zone, content);
                                
                                zonesList.Add(zone);
                                
                                if (showDebugInfo)
                                    UnityEngine.Debug.Log($"[CONFIG] Zona '{zoneConfig.name}' criada em {zoneConfig.positionCm}cm");
                            }
                        }
                        else
                        {
                            UnityEngine.Debug.LogWarning($"[CONFIG] Falha ao criar conteúdo para zona '{zoneConfig.name}'");
                        }
                    }
                    
                    // Aplicar lista de zonas ao ativador
                    zonesField.SetValue(zoneActivator, zonesList);
                    
                    // IMPORTANTE: Configurar estado inicial correto para fade funcionar
                    foreach (var zone in zonesList)
                    {
                        var contentField = zone.GetType().GetField("content");
                        if (contentField != null)
                        {
                            var content = contentField.GetValue(zone) as GameObject;
                            if (content != null)
                            {
                                // Ativar o GameObject pai (que contém o CanvasGroup)
                                content.SetActive(true);
                                
                                // Configurar CanvasGroup para começar invisível
                                var cg = content.GetComponent<CanvasGroup>();
                                if (cg != null)
                                {
                                    cg.alpha = 0f;
                                }
                                else
                                {
                                    // Se não tiver CanvasGroup, adicionar um
                                    cg = content.AddComponent<CanvasGroup>();
                                    cg.alpha = 0f;
                                }
                                
                                if (showDebugInfo)
                                    UnityEngine.Debug.Log($"[CONFIG] Zona configurada: {content.name} ativa com alpha=0");
                            }
                        }
                    }
                    
                    // Agora chamar HideAll para configurar o estado correto
                    if (zoneActivator != null)
                    {
                        var hideAllMethod = type.GetMethod("HideAll", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                        if (hideAllMethod != null)
                        {
                            hideAllMethod.Invoke(zoneActivator, null);
                            if (showDebugInfo)
                                UnityEngine.Debug.Log("[CONFIG] HideAll() executado - estado inicial configurado");
                        }
                    }
                    
                    if (showDebugInfo)
                        UnityEngine.Debug.Log($"[CONFIG] {zonesList.Count} zonas aplicadas ao ativador com sucesso");
                }
                else
                {
                    UnityEngine.Debug.LogError("[CONFIG] Campo 'zones' não encontrado no TrilhoZoneActivator");
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao aplicar zonas: {e.Message}");
            }
        }
        
        private void CleanupOldContent()
        {
            if (mainCanvas == null) return;
            
            // Encontrar e destruir conteúdos antigos
            var oldContents = mainCanvas.GetComponentsInChildren<Transform>(true);
            foreach (var child in oldContents)
            {
                if (child != mainCanvas.transform && child.name.StartsWith("ZoneContent_"))
                {
                    if (showDebugInfo)
                        UnityEngine.Debug.Log($"[CONFIG] Removendo conteúdo antigo: {child.name}");
                    DestroyImmediate(child.gameObject);
                }
            }
        }
        
        private GameObject LoadContentForZone(ContentZone zoneConfig)
        {
            if (showDebugInfo)
                UnityEngine.Debug.Log($"[CONFIG] Carregando conteúdo para zona '{zoneConfig.name}' (tipo: {zoneConfig.contentType})");
            
            switch (zoneConfig.contentType)
            {
                case ContentType.Image:
                    if (string.IsNullOrEmpty(zoneConfig.contentFileName))
                    {
                        if (showDebugInfo)
                            UnityEngine.Debug.LogWarning($"[CONFIG] Nome do arquivo de imagem vazio para zona '{zoneConfig.name}'");
                        return null;
                    }
                    string imagePath = Path.Combine(Application.streamingAssetsPath, zoneConfig.contentFileName);
                    return CreateImageContent(imagePath, zoneConfig);
                case ContentType.Video:
                    if (string.IsNullOrEmpty(zoneConfig.contentFileName))
                    {
                        if (showDebugInfo)
                            UnityEngine.Debug.LogWarning($"[CONFIG] Nome do arquivo de vídeo vazio para zona '{zoneConfig.name}'");
                        return null;
                    }
                    string videoPath = Path.Combine(Application.streamingAssetsPath, zoneConfig.contentFileName);
                    return CreateVideoContent(videoPath, zoneConfig);
                case ContentType.Text:
                    if (string.IsNullOrEmpty(zoneConfig.textSettings?.text))
                    {
                        if (showDebugInfo)
                            UnityEngine.Debug.LogWarning($"[CONFIG] Texto vazio para zona '{zoneConfig.name}'");
                        return null;
                    }
                    var textContent = CreateTextContent(zoneConfig.textSettings.text, zoneConfig);
                    if (showDebugInfo)
                        UnityEngine.Debug.Log($"[CONFIG] Conteúdo de texto criado para zona '{zoneConfig.name}': {(textContent != null ? "SUCESSO" : "FALHOU")}");
                    return textContent;
                default:
                    if (showDebugInfo)
                        UnityEngine.Debug.LogWarning($"[CONFIG] Tipo de conteúdo desconhecido para zona '{zoneConfig.name}': {zoneConfig.contentType}");
                    return null;
            }
        }
        
        private GameObject CreateImageContent(string imagePath, ContentZone zoneConfig)
        {
            if (!File.Exists(imagePath))
            {
                UnityEngine.Debug.LogWarning($"[CONFIG] Arquivo de imagem não encontrado: {imagePath}");
                return null;
            }
            
            if (mainCanvas == null)
            {
                UnityEngine.Debug.LogError("[CONFIG] Canvas principal não encontrado! Não é possível criar conteúdo de imagem.");
                return null;
            }
            
            // Carregar textura
            byte[] fileData = File.ReadAllBytes(imagePath);
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(fileData);
            
            // Criar GameObject PAI para a zona como filho do Canvas principal
            GameObject zoneContent = new GameObject($"ZoneContent_{zoneConfig.name}");
            zoneContent.transform.SetParent(mainCanvas.transform, false);
            
            // Adicionar RectTransform ao zoneContent
            var rectTransform = zoneContent.AddComponent<RectTransform>();
            
            // Criar fundo colorido como filho (opcional, para zonas de imagem)
            GameObject background = new GameObject("Background");
            background.transform.SetParent(zoneContent.transform, false);
            var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.color = new Color(0.0f, 0.0f, 0.0f, 0.5f); // Fundo semi-transparente
            
            // Obter ou adicionar RectTransform ao fundo (Image já adiciona automaticamente)
            var bgRectTransform = background.GetComponent<RectTransform>();
            if (bgRectTransform == null)
            {
                bgRectTransform = background.AddComponent<RectTransform>();
            }
            
            // Criar container para a imagem como filho
            GameObject imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(zoneContent.transform, false);
            var rawImage = imageContainer.AddComponent<UnityEngine.UI.RawImage>();
            rawImage.texture = texture;
            
            // Adicionar RectTransform ao container de imagem
            var imageRectTransform = imageContainer.AddComponent<RectTransform>();
            
            // Adicionar CanvasGroup ao GameObject pai para controle de fade
            var canvasGroup = zoneContent.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Começa invisível para fade in
            
            // Configurar RectTransform para posicionamento correto baseado na configuração JSON
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(1080f, 1920f);
            
            // Usar a posição da configuração JSON (convertida de cm para unidades Unity)
            float positionInUnity = zoneConfig.positionCm * (8520f / 600f); // Converter de cm para posição Unity
            rectTransform.anchoredPosition = new Vector2(positionInUnity, 0f);
            
            // Configurar RectTransform do fundo para cobrir toda a área
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Configurar RectTransform do container de imagem para centralizar
            imageRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            imageRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            imageRectTransform.pivot = new Vector2(0.5f, 0.5f);
            imageRectTransform.sizeDelta = new Vector2(1000f, 1800f); // Ligeiramente menor que o pai
            imageRectTransform.anchoredPosition = Vector2.zero;
            
            if (showDebugInfo)
                UnityEngine.Debug.Log($"[CONFIG] Imagem criada para zona '{zoneConfig.name}': '{imagePath}' como filho do Canvas principal");
            
            return zoneContent;
        }
        
        private GameObject CreateVideoContent(string videoPath, ContentZone zoneConfig)
        {
            if (!File.Exists(videoPath))
            {
                UnityEngine.Debug.LogWarning($"[CONFIG] Arquivo de vídeo não encontrado: {videoPath}");
                return null;
            }
            
            if (mainCanvas == null)
            {
                UnityEngine.Debug.LogError("[CONFIG] Canvas principal não encontrado! Não é possível criar conteúdo de vídeo.");
                return null;
            }
            
            // Criar GameObject PAI para a zona como filho do Canvas principal
            GameObject zoneContent = new GameObject($"ZoneContent_{zoneConfig.name}");
            zoneContent.transform.SetParent(mainCanvas.transform, false);
            
            // Adicionar RectTransform ao zoneContent
            var rectTransform = zoneContent.AddComponent<RectTransform>();
            
            // Criar fundo colorido como filho (opcional, para zonas de vídeo)
            GameObject background = new GameObject("Background");
            background.transform.SetParent(zoneContent.transform, false);
            var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            backgroundImage.color = new Color(0.0f, 0.0f, 0.0f, 1.0f); // Fundo preto para vídeo
            
            // Obter ou adicionar RectTransform ao fundo (Image já adiciona automaticamente)
            var bgRectTransform = background.GetComponent<RectTransform>();
            if (bgRectTransform == null)
            {
                bgRectTransform = background.AddComponent<RectTransform>();
            }
            
            // Criar container para o vídeo como filho
            GameObject videoContainer = new GameObject("VideoContainer");
            videoContainer.transform.SetParent(zoneContent.transform, false);
            var rawImage = videoContainer.AddComponent<UnityEngine.UI.RawImage>();
            
            // Adicionar RectTransform ao container de vídeo
            var videoRectTransform = videoContainer.AddComponent<RectTransform>();
            
            // Criar VideoPlayer como filho do container
            GameObject videoPlayerObject = new GameObject("VideoPlayer");
            videoPlayerObject.transform.SetParent(videoContainer.transform, false);
            var videoPlayer = videoPlayerObject.AddComponent<VideoPlayer>();
            
            // Configurar VideoPlayer
            videoPlayer.url = videoPath;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            
            // Criar RenderTexture com dimensões corretas
            RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
            renderTexture.Create();
            videoPlayer.targetTexture = renderTexture;
            
            videoPlayer.playOnAwake = true;
            videoPlayer.isLooping = true;
            
            // Configurar RawImage para receber a textura do vídeo
            rawImage.texture = renderTexture;
            
            // Adicionar CanvasGroup ao GameObject pai para controle de fade
            var canvasGroup = zoneContent.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f; // Começa invisível para fade in
            
            // Configurar RectTransform para posicionamento correto baseado na configuração JSON
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(1080f, 1920f);
            
            // Usar a posição da configuração JSON (convertida de cm para unidades Unity)
            float positionInUnity = zoneConfig.positionCm * (8520f / 600f); // Converter de cm para posição Unity
            rectTransform.anchoredPosition = new Vector2(positionInUnity, 0f);
            
            // Configurar RectTransform do fundo para cobrir toda a área
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Configurar RectTransform do container de vídeo para centralizar
            videoRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            videoRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            videoRectTransform.pivot = new Vector2(0.5f, 0.5f);
            videoRectTransform.sizeDelta = new Vector2(1000f, 1800f); // Ligeiramente menor que o pai
            videoRectTransform.anchoredPosition = Vector2.zero;
            
            if (showDebugInfo)
                UnityEngine.Debug.Log($"[CONFIG] Vídeo criado para zona '{zoneConfig.name}': '{videoPath}' como filho do Canvas principal");
            
            return zoneContent;
        }
        
        private GameObject CreateTextContent(string text, ContentZone zoneConfig)
        {
            try
            {
                if (mainCanvas == null)
                {
                    UnityEngine.Debug.LogError("[CONFIG] Canvas principal não encontrado! Não é possível criar conteúdo de texto.");
                    return null;
                }
                
                // Criar GameObject PAI para a zona como filho do Canvas principal
                GameObject zoneContent = new GameObject($"ZoneContent_{zoneConfig.name}");
                zoneContent.transform.SetParent(mainCanvas.transform, false);
                
                // Adicionar RectTransform ao zoneContent
                var rectTransform = zoneContent.AddComponent<RectTransform>();
                
                            // Criar fundo colorido como filho
            GameObject background = new GameObject("Background");
            background.transform.SetParent(zoneContent.transform, false);
            var backgroundImage = background.AddComponent<UnityEngine.UI.Image>();
            
                        // Obter ou adicionar RectTransform ao fundo (Image já adiciona automaticamente)
            var bgRectTransform = background.GetComponent<RectTransform>();
            if (bgRectTransform == null)
            {
                bgRectTransform = background.AddComponent<RectTransform>();
            }
            
            // Configurar cor do fundo
            if (zoneConfig.imageSettings?.tint != null)
            {
                backgroundImage.color = zoneConfig.imageSettings.tint;
            }
            else
            {
                backgroundImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            }
            
            // Criar texto como filho
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(zoneContent.transform, false);
            var textComponent = textObject.AddComponent<Text>();
            
            // Obter ou adicionar RectTransform ao texto (Text já adiciona automaticamente)
            var textRectTransform = textObject.GetComponent<RectTransform>();
            if (textRectTransform == null)
            {
                textRectTransform = textObject.AddComponent<RectTransform>();
            }
                
                // Configurar Text
                textComponent.text = text;
                textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                textComponent.fontSize = zoneConfig.textSettings?.fontSize ?? 48;
                textComponent.color = Color.white;
                textComponent.alignment = TextAnchor.MiddleCenter;
                
                // Adicionar CanvasGroup ao GameObject pai para controle de fade
                var canvasGroup = zoneContent.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f; // Começa invisível para fade in
                
            // Configurar RectTransform para posicionamento correto baseado na configuração JSON
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = new Vector2(1080f, 1920f);
            
            // Usar a posição da configuração JSON (convertida de cm para unidades Unity)
            float positionInUnity = zoneConfig.positionCm * (8520f / 600f); // Converter de cm para posição Unity
            rectTransform.anchoredPosition = new Vector2(positionInUnity, 0f);
            
            // Configurar RectTransform do fundo para cobrir toda a área
            bgRectTransform.anchorMin = Vector2.zero;
            bgRectTransform.anchorMax = Vector2.one;
            bgRectTransform.offsetMin = Vector2.zero;
            bgRectTransform.offsetMax = Vector2.zero;
            
            // Configurar RectTransform do texto para centralizar
            textRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            textRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            textRectTransform.pivot = new Vector2(0.5f, 0.5f);
            textRectTransform.sizeDelta = new Vector2(1000f, 1800f); // Ligeiramente menor que o pai
            textRectTransform.anchoredPosition = Vector2.zero;
                
                if (showDebugInfo)
                    UnityEngine.Debug.Log($"[CONFIG] Texto criado para zona '{zoneConfig.name}': '{text}' como filho do Canvas principal");
                
                return zoneContent;
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao criar texto para zona '{zoneConfig.name}': {e.Message}");
                return null;
            }
        }
        
        private void UpdateConfigFromCurrent()
        {
            if (currentConfig == null) return;
            
            try
            {
                // Atualizar configurações do Trilho
                if (gameManager != null)
                {
                    currentConfig.screenWidthInCm = gameManager.GetScreenWidthCm();
                }
                
                // Atualizar zonas do ativador
                if (zoneActivator != null)
                {
                    UpdateZonesFromActivator();
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao atualizar configuração: {e.Message}");
            }
        }
        
        private void UpdateZonesFromActivator()
        {
            if (zoneActivator == null || currentConfig == null) return;
            
            try
            {
                // Usar reflection para acessar a lista de zonas
                var type = typeof(TrilhoZoneActivator);
                var zonesField = type.GetField("zones", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                
                if (zonesField != null)
                {
                    var zonesList = zonesField.GetValue(zoneActivator) as List<TrilhoZoneActivator.Zone>;
                    if (zonesList != null)
                    {
                        currentConfig.contentZones.Clear();
                        
                        foreach (var zone in zonesList)
                        {
                            var zoneConfig = new ContentZone
                            {
                                name = zone.name,
                                positionCm = zone.positionCm,
                                paddingCm = zone.paddingCm,
                                enterPaddingCm = zone.enterPaddingCm,
                                exitPaddingCm = zone.exitPaddingCm,
                                placeContentAtWorldX = zone.placeContentAtWorldX,
                                contentOffsetCm = zone.contentOffsetCm,
                                reference = (PositionReference)zone.reference,
                                keepUpdatingWhileActive = zone.keepUpdatingWhileActive
                            };
                            
                            currentConfig.contentZones.Add(zoneConfig);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"[CONFIG] Erro ao atualizar zonas: {e.Message}");
            }
        }
        
        #endregion
        
        #region Utilitários
        
        public TrilhoConfig GetCurrentConfig() => currentConfig;
        
        public void SetCurrentConfig(TrilhoConfig config)
        {
            currentConfig = config;
            ApplyConfig();
        }
        
        public void CreateDefaultConfig()
        {
            currentConfig = new TrilhoConfig
            {
                configName = "Projeto Trilho",
                version = "1.0",
                description = "Configuração padrão do Trilho",
                physicalMinCm = 0f,
                physicalMaxCm = 600f,
                unityMinPosition = 0f,
                unityMaxPosition = 8520f,
                screenWidthInCm = 60f,
                movementSensitivity = 0.5f,
                smoothCameraMovement = true,
                cameraSmoothing = 5f,
                oscAddress = "/unity",
                oscValorNormalizado01 = true,
                oscPort = 9000,
                contentZones = new List<ContentZone>()
            };
            
            if (showDebugInfo)
                UnityEngine.Debug.Log("[CONFIG] Configuração padrão criada");
        }
        
        #endregion
        
        #region Context Menu
        
        [ContextMenu("Carregar Configuração")]
        public void LoadConfigContext() => LoadConfig();
        
        [ContextMenu("Salvar Configuração")]
        public void SaveConfigContext() => SaveConfig();
        
        [ContextMenu("Aplicar Configuração")]
        public void ApplyConfigContext() => ApplyConfig();
        
        [ContextMenu("Criar Configuração Padrão")]
        public void CreateDefaultConfigContext() => CreateDefaultConfig();
        
        #endregion
    }
}
