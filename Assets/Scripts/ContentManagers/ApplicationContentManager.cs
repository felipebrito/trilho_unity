using System.Collections;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;

namespace Trilho.ContentManagers
{
    public class ApplicationContentManager : MonoBehaviour
    {
        [Header("Application Settings")]
        [SerializeField] private string applicationPath = "";
        [SerializeField] private string applicationArguments = "";
        [SerializeField] private bool waitForExit = false;
        [SerializeField] private bool hideUnityWhileRunning = true;
        
        [Header("UI")]
        [SerializeField] private Button launchButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Text statusText;
        [SerializeField] private GameObject loadingIndicator;
        
        [Header("Timeout Settings")]
        [SerializeField] private float launchTimeout = 10f;
        [SerializeField] private bool autoCloseOnTimeout = false;
        
        private Process currentProcess;
        private bool isApplicationRunning = false;
        private Coroutine statusCheckCoroutine;
        
        private void Awake()
        {
            SetupUI();
        }
        
        private void SetupUI()
        {
            if (launchButton != null)
            {
                launchButton.onClick.AddListener(LaunchApplication);
            }
            
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseApplication);
                closeButton.gameObject.SetActive(false);
            }
            
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(false);
            }
            
            UpdateStatusText("Ready to launch");
        }
        
        #region Public API
        
        public void LaunchApplication()
        {
            if (isApplicationRunning)
            {
                UnityEngine.Debug.LogWarning("Application is already running");
                return;
            }
            
            if (string.IsNullOrEmpty(applicationPath))
            {
                UnityEngine.Debug.LogError("Application path is not set");
                UpdateStatusText("Error: No application path set");
                return;
            }
            
            StartCoroutine(LaunchApplicationCoroutine());
        }
        
        public void LaunchApplication(string path, string arguments = "")
        {
            applicationPath = path;
            applicationArguments = arguments;
            LaunchApplication();
        }
        
        public void CloseApplication()
        {
            if (!isApplicationRunning || currentProcess == null)
            {
                UnityEngine.Debug.LogWarning("No application is currently running");
                return;
            }
            
            try
            {
                if (!currentProcess.HasExited)
                {
                    currentProcess.Kill();
                    currentProcess.WaitForExit(5000); // Wait up to 5 seconds
                }
                
                OnApplicationClosed();
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogError($"Error closing application: {e.Message}");
                UpdateStatusText($"Error closing: {e.Message}");
            }
        }
        
        public void SetApplicationPath(string path)
        {
            applicationPath = path;
        }
        
        public void SetApplicationArguments(string arguments)
        {
            applicationArguments = arguments;
        }
        
        public bool IsApplicationRunning => isApplicationRunning;
        public string CurrentApplicationPath => applicationPath;
        
        #endregion
        
        private IEnumerator LaunchApplicationCoroutine()
        {
            UpdateStatusText("Launching application...");
            ShowLoadingIndicator(true);
            
            bool launchFailed = false;
            string errorMessage = "";
            
            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = applicationPath,
                    Arguments = applicationArguments,
                    UseShellExecute = true,
                    CreateNoWindow = false
                };
                
                currentProcess = Process.Start(startInfo);
                
                if (currentProcess == null)
                {
                    launchFailed = true;
                    errorMessage = "Failed to start process";
                }
            }
            catch (System.Exception e)
            {
                launchFailed = true;
                errorMessage = e.Message;
            }
            
            if (launchFailed)
            {
                UnityEngine.Debug.LogError($"Failed to launch application: {errorMessage}");
                UpdateStatusText($"Launch failed: {errorMessage}");
                OnApplicationLaunchFailed();
                ShowLoadingIndicator(false);
                yield break;
            }
            
            // Wait a bit to see if the process starts successfully
            yield return new WaitForSeconds(1f);
            
            try
            {
                if (currentProcess.HasExited)
                {
                    int exitCode = currentProcess.ExitCode;
                    launchFailed = true;
                    errorMessage = $"Application exited immediately with code: {exitCode}";
                }
            }
            catch (System.Exception e)
            {
                launchFailed = true;
                errorMessage = e.Message;
            }
            
            if (launchFailed)
            {
                UnityEngine.Debug.LogError($"Failed to launch application: {errorMessage}");
                UpdateStatusText($"Launch failed: {errorMessage}");
                OnApplicationLaunchFailed();
                ShowLoadingIndicator(false);
                yield break;
            }
            
            OnApplicationLaunched();
            
            // Start monitoring the process
            if (statusCheckCoroutine != null)
            {
                StopCoroutine(statusCheckCoroutine);
            }
            statusCheckCoroutine = StartCoroutine(MonitorApplicationStatus());
            
            // Hide Unity window if configured
            if (hideUnityWhileRunning)
            {
                SetUnityWindowVisibility(false);
            }
            
            ShowLoadingIndicator(false);
        }
        
        private IEnumerator MonitorApplicationStatus()
        {
            float elapsedTime = 0f;
            
            while (isApplicationRunning && currentProcess != null)
            {
                yield return new WaitForSeconds(0.5f);
                elapsedTime += 0.5f;
                
                try
                {
                    if (currentProcess.HasExited)
                    {
                        OnApplicationClosed();
                        break;
                    }
                    
                    // Check for timeout
                    if (autoCloseOnTimeout && elapsedTime >= launchTimeout)
                    {
                        UnityEngine.Debug.LogWarning("Application timeout reached, closing...");
                        CloseApplication();
                        break;
                    }
                    
                    UpdateStatusText($"Running... ({elapsedTime:F0}s)");
                }
                catch (System.Exception e)
                {
                    UnityEngine.Debug.LogError($"Error monitoring application: {e.Message}");
                    OnApplicationClosed();
                    break;
                }
            }
        }
        
        private void OnApplicationLaunched()
        {
            isApplicationRunning = true;
            UpdateStatusText("Application running");
            
            if (launchButton != null)
                launchButton.gameObject.SetActive(false);
            
            if (closeButton != null)
                closeButton.gameObject.SetActive(true);
            
            UnityEngine.Debug.Log($"Application launched: {applicationPath}");
        }
        
        private void OnApplicationClosed()
        {
            isApplicationRunning = false;
            currentProcess = null;
            
            UpdateStatusText("Application closed");
            
            if (launchButton != null)
                launchButton.gameObject.SetActive(true);
            
            if (closeButton != null)
                closeButton.gameObject.SetActive(false);
            
            // Show Unity window again
            if (hideUnityWhileRunning)
            {
                SetUnityWindowVisibility(true);
            }
            
            if (statusCheckCoroutine != null)
            {
                StopCoroutine(statusCheckCoroutine);
                statusCheckCoroutine = null;
            }
            
            UnityEngine.Debug.Log("Application closed");
        }
        
        private void OnApplicationLaunchFailed()
        {
            isApplicationRunning = false;
            currentProcess = null;
            
            if (launchButton != null)
                launchButton.gameObject.SetActive(true);
            
            if (closeButton != null)
                closeButton.gameObject.SetActive(false);
        }
        
        private void UpdateStatusText(string message)
        {
            if (statusText != null)
            {
                statusText.text = message;
            }
            
            UnityEngine.Debug.Log($"Application Manager: {message}");
        }
        
        private void ShowLoadingIndicator(bool show)
        {
            if (loadingIndicator != null)
            {
                loadingIndicator.SetActive(show);
            }
        }
        
        private void SetUnityWindowVisibility(bool visible)
        {
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
            try
            {
                var unityWindow = GetActiveWindow();
                ShowWindow(unityWindow, visible ? 9 : 0); // 9 = SW_RESTORE, 0 = SW_HIDE
            }
            catch (System.Exception e)
            {
                UnityEngine.Debug.LogWarning($"Could not change window visibility: {e.Message}");
            }
#else
            UnityEngine.Debug.Log($"Window visibility change not supported on this platform. Visible: {visible}");
#endif
        }
        
#if UNITY_STANDALONE_WIN && !UNITY_EDITOR
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern System.IntPtr GetActiveWindow();
        
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowWindow(System.IntPtr hWnd, int nCmdShow);
#endif
        
        private void OnDestroy()
        {
            if (isApplicationRunning)
            {
                CloseApplication();
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            // Handle Unity pause/resume
            if (!pauseStatus && hideUnityWhileRunning && isApplicationRunning)
            {
                // Unity is resuming, but we want to keep it hidden while app is running
                SetUnityWindowVisibility(false);
            }
        }
    }
}
