using UnityEngine;
using UnityEngine.UI;

namespace Trilho
{
    public class TrilhoContentTest : MonoBehaviour
    {
        [Header("Teste de Conteúdos")]
        [SerializeField] private bool createTestContent = true;
        
        private void Start()
        {
            if (createTestContent)
            {
                CreateSimpleTestContent();
            }
        }
        
        [ContextMenu("Criar Conteúdo de Teste")]
        public void CreateSimpleTestContent()
        {
            Debug.Log("=== CRIANDO CONTEÚDO DE TESTE ===");
            
            // Criar Canvas se não existir
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 1000;
                
                var scaler = canvasGO.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                
                canvasGO.AddComponent<GraphicRaycaster>();
                
                Debug.Log("✅ Canvas criado");
            }
            
            // Criar conteúdo simples
            var content = new GameObject("Teste Conteúdo");
            content.transform.SetParent(canvas.transform, false);
            
            var image = content.AddComponent<Image>();
            image.color = Color.red;
            
            var rectTransform = content.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            // Adicionar texto
            var textGO = new GameObject("Texto");
            textGO.transform.SetParent(content.transform, false);
            var text = textGO.AddComponent<Text>();
            text.text = "TESTE - DEVERIA APARECER NA GAME VIEW";
            text.color = Color.white;
            text.fontSize = 100;
            text.alignment = TextAnchor.MiddleCenter;
            
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.sizeDelta = Vector2.zero;
            textRect.anchoredPosition = Vector2.zero;
            
            Debug.Log("✅ Conteúdo de teste criado - VERIFIQUE A GAME VIEW!");
        }
        
        [ContextMenu("Verificar Canvas")]
        public void CheckCanvas()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var contents = canvas.GetComponentsInChildren<Image>();
                Debug.Log($"✅ Canvas encontrado com {contents.Length} conteúdos");
                
                foreach (var content in contents)
                {
                    Debug.Log($"   - {content.name}: {content.color}");
                }
            }
            else
            {
                Debug.LogError("❌ Canvas não encontrado!");
            }
        }
        
        [ContextMenu("Limpar Console")]
        public void ClearConsole()
        {
            Debug.ClearDeveloperConsole();
            Debug.Log("=== CONSOLE LIMPO ===");
        }
    }
}

