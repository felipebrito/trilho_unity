using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Trilho.UI
{
    public class UITransitionManager : MonoBehaviour
    {
        [Header("Transition Settings")]
        [SerializeField] private float defaultFadeDuration = 0.5f;
        [SerializeField] private float defaultScaleDuration = 0.3f;
        [SerializeField] private float defaultMoveDuration = 0.4f;
        [SerializeField] private AnimationCurve defaultEaseCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        [Header("Components")]
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private GraphicRaycaster graphicRaycaster;
        
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private Coroutine currentTransition;
        
        private void Awake()
        {
            // Get components if not assigned
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            
            if (rectTransform == null)
                rectTransform = GetComponent<RectTransform>();
            
            if (graphicRaycaster == null)
                graphicRaycaster = GetComponent<GraphicRaycaster>();
            
            // Store original values
            if (rectTransform != null)
            {
                originalScale = rectTransform.localScale;
                originalPosition = rectTransform.anchoredPosition;
            }
        }
        
        #region Fade Transitions
        
        public void FadeIn(float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultFadeDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            currentTransition = StartCoroutine(FadeCoroutine(1f, duration, onComplete));
        }
        
        public void FadeOut(float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultFadeDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            currentTransition = StartCoroutine(FadeCoroutine(0f, duration, onComplete));
        }
        
        public void FadeTo(float targetAlpha, float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultFadeDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            currentTransition = StartCoroutine(FadeCoroutine(targetAlpha, duration, onComplete));
        }
        
        private IEnumerator FadeCoroutine(float targetAlpha, float duration, System.Action onComplete)
        {
            if (canvasGroup == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            float startAlpha = canvasGroup.alpha;
            float elapsedTime = 0f;
            
            // Enable interaction if fading in
            if (targetAlpha > 0.5f)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float easedT = defaultEaseCurve.Evaluate(t);
                
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, easedT);
                yield return null;
            }
            
            canvasGroup.alpha = targetAlpha;
            
            // Disable interaction if faded out
            if (targetAlpha <= 0.1f)
            {
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            
            currentTransition = null;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Scale Transitions
        
        public void ScaleIn(float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultScaleDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            currentTransition = StartCoroutine(ScaleCoroutine(Vector3.zero, originalScale, duration, onComplete));
        }
        
        public void ScaleOut(float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultScaleDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            currentTransition = StartCoroutine(ScaleCoroutine(originalScale, Vector3.zero, duration, onComplete));
        }
        
        public void ScaleTo(Vector3 targetScale, float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultScaleDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            Vector3 startScale = rectTransform != null ? rectTransform.localScale : Vector3.one;
            currentTransition = StartCoroutine(ScaleCoroutine(startScale, targetScale, duration, onComplete));
        }
        
        private IEnumerator ScaleCoroutine(Vector3 startScale, Vector3 targetScale, float duration, System.Action onComplete)
        {
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float easedT = defaultEaseCurve.Evaluate(t);
                
                rectTransform.localScale = Vector3.Lerp(startScale, targetScale, easedT);
                yield return null;
            }
            
            rectTransform.localScale = targetScale;
            currentTransition = null;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Movement Transitions
        
        public void MoveIn(Vector3 fromOffset, float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultMoveDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            Vector3 startPos = originalPosition + fromOffset;
            currentTransition = StartCoroutine(MoveCoroutine(startPos, originalPosition, duration, onComplete));
        }
        
        public void MoveOut(Vector3 toOffset, float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultMoveDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            Vector3 targetPos = originalPosition + toOffset;
            Vector3 startPos = rectTransform != null ? rectTransform.anchoredPosition : originalPosition;
            currentTransition = StartCoroutine(MoveCoroutine(startPos, targetPos, duration, onComplete));
        }
        
        public void MoveTo(Vector3 targetPosition, float duration = -1f, System.Action onComplete = null)
        {
            if (duration < 0) duration = defaultMoveDuration;
            
            if (currentTransition != null)
                StopCoroutine(currentTransition);
            
            Vector3 startPos = rectTransform != null ? rectTransform.anchoredPosition : originalPosition;
            currentTransition = StartCoroutine(MoveCoroutine(startPos, targetPosition, duration, onComplete));
        }
        
        private IEnumerator MoveCoroutine(Vector3 startPos, Vector3 targetPos, float duration, System.Action onComplete)
        {
            if (rectTransform == null)
            {
                onComplete?.Invoke();
                yield break;
            }
            
            float elapsedTime = 0f;
            
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                float t = elapsedTime / duration;
                float easedT = defaultEaseCurve.Evaluate(t);
                
                rectTransform.anchoredPosition = Vector3.Lerp(startPos, targetPos, easedT);
                yield return null;
            }
            
            rectTransform.anchoredPosition = targetPos;
            currentTransition = null;
            onComplete?.Invoke();
        }
        
        #endregion
        
        #region Combined Transitions
        
        public void ShowWithAnimation(TransitionType type = TransitionType.Fade, float duration = -1f, System.Action onComplete = null)
        {
            gameObject.SetActive(true);
            
            switch (type)
            {
                case TransitionType.Fade:
                    if (canvasGroup != null) canvasGroup.alpha = 0f;
                    FadeIn(duration, onComplete);
                    break;
                    
                case TransitionType.Scale:
                    if (rectTransform != null) rectTransform.localScale = Vector3.zero;
                    ScaleIn(duration, onComplete);
                    break;
                    
                case TransitionType.SlideFromLeft:
                    MoveIn(Vector3.left * 1000f, duration, onComplete);
                    break;
                    
                case TransitionType.SlideFromRight:
                    MoveIn(Vector3.right * 1000f, duration, onComplete);
                    break;
                    
                case TransitionType.SlideFromTop:
                    MoveIn(Vector3.up * 1000f, duration, onComplete);
                    break;
                    
                case TransitionType.SlideFromBottom:
                    MoveIn(Vector3.down * 1000f, duration, onComplete);
                    break;
                    
                default:
                    onComplete?.Invoke();
                    break;
            }
        }
        
        public void HideWithAnimation(TransitionType type = TransitionType.Fade, float duration = -1f, System.Action onComplete = null)
        {
            System.Action hideComplete = () =>
            {
                gameObject.SetActive(false);
                onComplete?.Invoke();
            };
            
            switch (type)
            {
                case TransitionType.Fade:
                    FadeOut(duration, hideComplete);
                    break;
                    
                case TransitionType.Scale:
                    ScaleOut(duration, hideComplete);
                    break;
                    
                case TransitionType.SlideFromLeft:
                    MoveOut(Vector3.left * 1000f, duration, hideComplete);
                    break;
                    
                case TransitionType.SlideFromRight:
                    MoveOut(Vector3.right * 1000f, duration, hideComplete);
                    break;
                    
                case TransitionType.SlideFromTop:
                    MoveOut(Vector3.up * 1000f, duration, hideComplete);
                    break;
                    
                case TransitionType.SlideFromBottom:
                    MoveOut(Vector3.down * 1000f, duration, hideComplete);
                    break;
                    
                default:
                    hideComplete();
                    break;
            }
        }
        
        #endregion
        
        #region Utility Methods
        
        public void StopCurrentTransition()
        {
            if (currentTransition != null)
            {
                StopCoroutine(currentTransition);
                currentTransition = null;
            }
        }
        
        public void ResetToOriginal()
        {
            StopCurrentTransition();
            
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 1f;
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            if (rectTransform != null)
            {
                rectTransform.localScale = originalScale;
                rectTransform.anchoredPosition = originalPosition;
            }
        }
        
        public void SetInteractable(bool interactable)
        {
            if (canvasGroup != null)
            {
                canvasGroup.interactable = interactable;
                canvasGroup.blocksRaycasts = interactable;
            }
        }
        
        public bool IsTransitioning => currentTransition != null;
        
        #endregion
    }
    
    public enum TransitionType
    {
        Fade,
        Scale,
        SlideFromLeft,
        SlideFromRight,
        SlideFromTop,
        SlideFromBottom
    }
}
