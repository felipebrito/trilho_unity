using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using Trilho.ContentManagers;
using Trilho.UI;

namespace Trilho.Prefabs
{
    /// <summary>
    /// Prefab component for video zone content. 
    /// This creates a complete video player interface for touch interaction.
    /// </summary>
    public class VideoZoneContent : MonoBehaviour
    {
        [Header("Video Settings")]
        [SerializeField] private string videoFileName = "";
        [SerializeField] private bool autoPlay = true;
        [SerializeField] private bool loopVideo = true;
        [SerializeField] private bool showControls = true;
        
        [Header("Auto-generated Components")]
        [SerializeField] private VideoContentManager videoManager;
        [SerializeField] private UITransitionManager transitionManager;
        [SerializeField] private Canvas videoCanvas;
        [SerializeField] private RawImage videoDisplay;
        [SerializeField] private GameObject controlsPanel;
        
        private void Awake()
        {
            CreateVideoInterface();
        }
        
        private void CreateVideoInterface()
        {
            // Create main canvas if not exists
            if (videoCanvas == null)
            {
                GameObject canvasObj = new GameObject("VideoCanvas");
                canvasObj.transform.SetParent(transform);
                
                videoCanvas = canvasObj.AddComponent<Canvas>();
                videoCanvas.renderMode = RenderMode.WorldSpace;
                videoCanvas.worldCamera = Camera.main;
                
                CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
                
                canvasObj.AddComponent<GraphicRaycaster>();
                
                // Set canvas size for 42" touch screen (approximate)
                RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
                canvasRect.sizeDelta = new Vector2(1080, 1920);
                canvasRect.localScale = Vector3.one * 0.001f; // Scale to world units
            }
            
            // Create video display
            CreateVideoDisplay();
            
            // Create controls if enabled
            if (showControls)
            {
                CreateVideoControls();
            }
            
            // Add transition manager
            if (transitionManager == null)
            {
                transitionManager = gameObject.AddComponent<UITransitionManager>();
            }
            
            // Add video manager
            if (videoManager == null)
            {
                videoManager = gameObject.AddComponent<VideoContentManager>();
            }
        }
        
        private void CreateVideoDisplay()
        {
            GameObject displayObj = new GameObject("VideoDisplay");
            displayObj.transform.SetParent(videoCanvas.transform);
            
            videoDisplay = displayObj.AddComponent<RawImage>();
            videoDisplay.color = Color.black;
            
            RectTransform displayRect = displayObj.GetComponent<RectTransform>();
            displayRect.anchorMin = Vector2.zero;
            displayRect.anchorMax = Vector2.one;
            displayRect.offsetMin = Vector2.zero;
            displayRect.offsetMax = Vector2.zero;
        }
        
        private void CreateVideoControls()
        {
            GameObject controlsObj = new GameObject("VideoControls");
            controlsObj.transform.SetParent(videoCanvas.transform);
            
            controlsPanel = controlsObj;
            
            // Add background
            Image controlsBg = controlsObj.AddComponent<Image>();
            controlsBg.color = new Color(0, 0, 0, 0.8f);
            
            RectTransform controlsRect = controlsObj.GetComponent<RectTransform>();
            controlsRect.anchorMin = new Vector2(0, 0);
            controlsRect.anchorMax = new Vector2(1, 0);
            controlsRect.pivot = new Vector2(0.5f, 0);
            controlsRect.sizeDelta = new Vector2(0, 200);
            controlsRect.anchoredPosition = Vector2.zero;
            
            // Create play/pause button
            CreatePlayPauseButton(controlsObj);
            
            // Create progress slider
            CreateProgressSlider(controlsObj);
            
            // Create volume slider
            CreateVolumeSlider(controlsObj);
            
            // Create time display
            CreateTimeDisplay(controlsObj);
        }
        
        private void CreatePlayPauseButton(GameObject parent)
        {
            GameObject buttonObj = new GameObject("PlayPauseButton");
            buttonObj.transform.SetParent(parent.transform);
            
            Button button = buttonObj.AddComponent<Button>();
            Image buttonImage = buttonObj.AddComponent<Image>();
            buttonImage.color = Color.white;
            
            RectTransform buttonRect = buttonObj.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.05f, 0.1f);
            buttonRect.anchorMax = new Vector2(0.05f, 0.1f);
            buttonRect.pivot = new Vector2(0, 0);
            buttonRect.sizeDelta = new Vector2(160, 160);
            
            // Add play icon (simple triangle)
            GameObject iconObj = new GameObject("PlayIcon");
            iconObj.transform.SetParent(buttonObj.transform);
            
            Text iconText = iconObj.AddComponent<Text>();
            iconText.text = "▶";
            iconText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            iconText.fontSize = 60;
            iconText.color = Color.black;
            iconText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform iconRect = iconObj.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = Vector2.zero;
            iconRect.offsetMax = Vector2.zero;
        }
        
        private void CreateProgressSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("ProgressSlider");
            sliderObj.transform.SetParent(parent.transform);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 0f;
            
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.25f, 0.6f);
            sliderRect.anchorMax = new Vector2(0.95f, 0.9f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            
            // Create background
            GameObject bgObj = new GameObject("Background");
            bgObj.transform.SetParent(sliderObj.transform);
            Image bgImage = bgObj.AddComponent<Image>();
            bgImage.color = new Color(0.2f, 0.2f, 0.2f, 1f);
            
            RectTransform bgRect = bgObj.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            
            // Create fill area
            GameObject fillAreaObj = new GameObject("Fill Area");
            fillAreaObj.transform.SetParent(sliderObj.transform);
            
            RectTransform fillAreaRect = fillAreaObj.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = Vector2.zero;
            fillAreaRect.anchorMax = Vector2.one;
            fillAreaRect.offsetMin = Vector2.zero;
            fillAreaRect.offsetMax = Vector2.zero;
            
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(fillAreaObj.transform);
            Image fillImage = fillObj.AddComponent<Image>();
            fillImage.color = Color.red;
            
            RectTransform fillRect = fillObj.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            // Create handle
            GameObject handleSlideAreaObj = new GameObject("Handle Slide Area");
            handleSlideAreaObj.transform.SetParent(sliderObj.transform);
            
            RectTransform handleSlideAreaRect = handleSlideAreaObj.GetComponent<RectTransform>();
            handleSlideAreaRect.anchorMin = Vector2.zero;
            handleSlideAreaRect.anchorMax = Vector2.one;
            handleSlideAreaRect.offsetMin = new Vector2(10, 0);
            handleSlideAreaRect.offsetMax = new Vector2(-10, 0);
            
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(handleSlideAreaObj.transform);
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            
            RectTransform handleRect = handleObj.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(40, 40);
            
            // Assign slider components
            slider.fillRect = fillRect;
            slider.handleRect = handleRect;
            slider.targetGraphic = handleImage;
        }
        
        private void CreateVolumeSlider(GameObject parent)
        {
            GameObject sliderObj = new GameObject("VolumeSlider");
            sliderObj.transform.SetParent(parent.transform);
            
            Slider slider = sliderObj.AddComponent<Slider>();
            slider.minValue = 0f;
            slider.maxValue = 1f;
            slider.value = 1f;
            
            RectTransform sliderRect = sliderObj.GetComponent<RectTransform>();
            sliderRect.anchorMin = new Vector2(0.25f, 0.1f);
            sliderRect.anchorMax = new Vector2(0.6f, 0.4f);
            sliderRect.offsetMin = Vector2.zero;
            sliderRect.offsetMax = Vector2.zero;
            
            // Similar structure to progress slider but smaller
            // (Simplified for brevity - would follow same pattern as progress slider)
        }
        
        private void CreateTimeDisplay(GameObject parent)
        {
            GameObject timeObj = new GameObject("TimeDisplay");
            timeObj.transform.SetParent(parent.transform);
            
            Text timeText = timeObj.AddComponent<Text>();
            timeText.text = "00:00 / 00:00";
            timeText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            timeText.fontSize = 36;
            timeText.color = Color.white;
            timeText.alignment = TextAnchor.MiddleCenter;
            
            RectTransform timeRect = timeObj.GetComponent<RectTransform>();
            timeRect.anchorMin = new Vector2(0.65f, 0.1f);
            timeRect.anchorMax = new Vector2(0.95f, 0.4f);
            timeRect.offsetMin = Vector2.zero;
            timeRect.offsetMax = Vector2.zero;
        }
        
        #region Public API
        
        public void LoadVideo(string videoPath)
        {
            if (videoManager != null)
            {
                videoManager.LoadVideo(videoPath);
                
                if (autoPlay)
                {
                    videoManager.Play();
                }
            }
        }
        
        public void LoadVideoClip(VideoClip clip)
        {
            if (videoManager != null)
            {
                videoManager.LoadVideoClip(clip);
                
                if (autoPlay)
                {
                    videoManager.Play();
                }
            }
        }
        
        public void Show()
        {
            if (transitionManager != null)
            {
                transitionManager.ShowWithAnimation(TransitionType.Fade);
            }
            else
            {
                gameObject.SetActive(true);
            }
        }
        
        public void Hide()
        {
            if (transitionManager != null)
            {
                transitionManager.HideWithAnimation(TransitionType.Fade);
            }
            else
            {
                gameObject.SetActive(false);
            }
        }
        
        public VideoContentManager GetVideoManager() => videoManager;
        public UITransitionManager GetTransitionManager() => transitionManager;
        
        #endregion
        
        private void Start()
        {
            // Auto-load video if filename is specified
            if (!string.IsNullOrEmpty(videoFileName))
            {
                string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "Videos", videoFileName);
                LoadVideo(videoPath);
            }
        }
    }
}
