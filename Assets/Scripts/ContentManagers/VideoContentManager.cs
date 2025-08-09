using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;

namespace Trilho.ContentManagers
{
    public class VideoContentManager : MonoBehaviour
    {
        [Header("Video Player")]
        [SerializeField] private VideoPlayer videoPlayer;
        [SerializeField] private RawImage videoDisplay;
        [SerializeField] private RenderTexture videoRenderTexture;
        
        [Header("Audio")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private bool muteOnStart = false;
        
        [Header("Playback Settings")]
        [SerializeField] private bool loopVideo = true;
        [SerializeField] private bool playOnAwake = false;
        [SerializeField] private bool skipOnDrop = true;
        
        [Header("UI Controls")]
        [SerializeField] private Button playPauseButton;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Slider volumeSlider;
        [SerializeField] private Text timeDisplay;
        
        private bool isInitialized = false;
        private bool userInteracting = false;
        
        private void Awake()
        {
            InitializeVideoPlayer();
            SetupUIControls();
        }
        
        private void InitializeVideoPlayer()
        {
            if (videoPlayer == null)
                videoPlayer = GetComponent<VideoPlayer>();
            
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
            
            // Setup video player
            videoPlayer.playOnAwake = playOnAwake;
            videoPlayer.isLooping = loopVideo;
            videoPlayer.skipOnDrop = skipOnDrop;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            
            // Setup render texture
            if (videoRenderTexture == null)
            {
                videoRenderTexture = new RenderTexture(1920, 1080, 16);
                videoRenderTexture.Create();
            }
            
            videoPlayer.targetTexture = videoRenderTexture;
            
            // Setup audio
            if (audioSource == null)
                audioSource = GetComponent<AudioSource>();
            
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
            
            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            videoPlayer.SetTargetAudioSource(0, audioSource);
            
            if (muteOnStart)
                audioSource.mute = true;
            
            // Setup display
            if (videoDisplay != null)
            {
                videoDisplay.texture = videoRenderTexture;
            }
            
            // Events
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.started += OnVideoStarted;
            videoPlayer.loopPointReached += OnVideoLoopPointReached;
            videoPlayer.errorReceived += OnVideoError;
            
            isInitialized = true;
        }
        
        private void SetupUIControls()
        {
            if (playPauseButton != null)
            {
                playPauseButton.onClick.AddListener(TogglePlayPause);
            }
            
            if (progressSlider != null)
            {
                progressSlider.onValueChanged.AddListener(OnProgressSliderChanged);
            }
            
            if (volumeSlider != null)
            {
                volumeSlider.value = audioSource != null ? audioSource.volume : 1f;
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }
        }
        
        private void Update()
        {
            if (isInitialized && videoPlayer.isPlaying)
            {
                UpdateUI();
            }
        }
        
        private void UpdateUI()
        {
            if (!userInteracting && progressSlider != null && videoPlayer.frameCount > 0)
            {
                float progress = (float)videoPlayer.frame / (float)videoPlayer.frameCount;
                progressSlider.value = progress;
            }
            
            if (timeDisplay != null)
            {
                double currentTime = videoPlayer.time;
                double totalTime = videoPlayer.length;
                timeDisplay.text = $"{FormatTime(currentTime)} / {FormatTime(totalTime)}";
            }
        }
        
        private string FormatTime(double timeInSeconds)
        {
            int minutes = Mathf.FloorToInt((float)timeInSeconds / 60f);
            int seconds = Mathf.FloorToInt((float)timeInSeconds % 60f);
            return $"{minutes:00}:{seconds:00}";
        }
        
        #region Public API
        
        public void LoadVideo(string videoPath)
        {
            if (!isInitialized) return;
            
            videoPlayer.url = videoPath;
            videoPlayer.Prepare();
        }
        
        public void LoadVideoClip(VideoClip clip)
        {
            if (!isInitialized) return;
            
            videoPlayer.clip = clip;
            videoPlayer.Prepare();
        }
        
        public void Play()
        {
            if (isInitialized && videoPlayer.isPrepared)
            {
                videoPlayer.Play();
            }
        }
        
        public void Pause()
        {
            if (isInitialized)
            {
                videoPlayer.Pause();
            }
        }
        
        public void Stop()
        {
            if (isInitialized)
            {
                videoPlayer.Stop();
            }
        }
        
        public void TogglePlayPause()
        {
            if (!isInitialized) return;
            
            if (videoPlayer.isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
        
        public void SetVolume(float volume)
        {
            if (audioSource != null)
            {
                audioSource.volume = Mathf.Clamp01(volume);
            }
        }
        
        public void SetMute(bool mute)
        {
            if (audioSource != null)
            {
                audioSource.mute = mute;
            }
        }
        
        public void Seek(float normalizedTime)
        {
            if (isInitialized && videoPlayer.frameCount > 0)
            {
                long targetFrame = (long)(normalizedTime * videoPlayer.frameCount);
                videoPlayer.frame = targetFrame;
            }
        }
        
        public bool IsPlaying => isInitialized && videoPlayer.isPlaying;
        public bool IsPrepared => isInitialized && videoPlayer.isPrepared;
        public double Duration => isInitialized ? videoPlayer.length : 0;
        public double CurrentTime => isInitialized ? videoPlayer.time : 0;
        
        #endregion
        
        #region UI Event Handlers
        
        private void OnProgressSliderChanged(float value)
        {
            if (userInteracting)
            {
                Seek(value);
            }
        }
        
        private void OnVolumeChanged(float value)
        {
            SetVolume(value);
        }
        
        public void OnProgressSliderBeginDrag()
        {
            userInteracting = true;
        }
        
        public void OnProgressSliderEndDrag()
        {
            userInteracting = false;
        }
        
        #endregion
        
        #region Video Player Event Handlers
        
        private void OnVideoPrepared(VideoPlayer player)
        {
            Debug.Log($"Video prepared: {player.url}");
        }
        
        private void OnVideoStarted(VideoPlayer player)
        {
            Debug.Log($"Video started: {player.url}");
        }
        
        private void OnVideoLoopPointReached(VideoPlayer player)
        {
            Debug.Log($"Video loop point reached: {player.url}");
        }
        
        private void OnVideoError(VideoPlayer player, string message)
        {
            Debug.LogError($"Video error: {message}");
        }
        
        #endregion
        
        private void OnDestroy()
        {
            if (videoRenderTexture != null)
            {
                videoRenderTexture.Release();
            }
        }
    }
}
