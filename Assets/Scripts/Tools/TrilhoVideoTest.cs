using UnityEngine;
using UnityEngine.Video;

namespace Trilho.Tools
{
    /// <summary>
    /// Script simples para testar reprodução de vídeo
    /// </summary>
    public class TrilhoVideoTest : MonoBehaviour
    {
        [Header("Video Test")]
        [SerializeField] private string videoPath = "Videos/video.mp4";
        [SerializeField] private bool testOnStart = false;
        [SerializeField] private bool loopVideo = true;
        
        private VideoPlayer videoPlayer;
        
        private void Start()
        {
            if (testOnStart)
            {
                TestVideo();
            }
        }
        
        [ContextMenu("Test Video")]
        public void TestVideo()
        {
            UnityEngine.Debug.Log("=== TESTING VIDEO PLAYBACK ===");
            
            // Check if video file exists
            string fullPath = Application.dataPath + "/" + videoPath;
            bool fileExists = System.IO.File.Exists(fullPath);
            
            UnityEngine.Debug.Log($"Video path: {videoPath}");
            UnityEngine.Debug.Log($"Full path: {fullPath}");
            UnityEngine.Debug.Log($"File exists: {fileExists}");
            
            if (fileExists)
            {
                CreateVideoPlayer();
            }
            else
            {
                UnityEngine.Debug.LogError("❌ Video file not found!");
                UnityEngine.Debug.Log($"Please place a video file at: {fullPath}");
            }
            
            UnityEngine.Debug.Log("=============================");
        }
        
        private void CreateVideoPlayer()
        {
            // Create video player
            if (videoPlayer == null)
            {
                videoPlayer = gameObject.AddComponent<VideoPlayer>();
            }
            
            // Configure video player
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = loopVideo;
            videoPlayer.renderMode = VideoRenderMode.CameraFarPlane;
            videoPlayer.targetCamera = Camera.main;
            
            // Set video URL
            string videoUrl = "file://" + Application.dataPath + "/" + videoPath;
            videoPlayer.url = videoUrl;
            
            UnityEngine.Debug.Log($"Video URL: {videoUrl}");
            
            // Start playing
            videoPlayer.Play();
            UnityEngine.Debug.Log("✓ Video started playing!");
        }
        
        [ContextMenu("Stop Video")]
        public void StopVideo()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
                UnityEngine.Debug.Log("Video stopped");
            }
        }
        
        [ContextMenu("Pause Video")]
        public void PauseVideo()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Pause();
                UnityEngine.Debug.Log("Video paused");
            }
        }
        
        [ContextMenu("Resume Video")]
        public void ResumeVideo()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Play();
                UnityEngine.Debug.Log("Video resumed");
            }
        }
        
        private void OnDestroy()
        {
            if (videoPlayer != null)
            {
                videoPlayer.Stop();
            }
        }
    }
}
