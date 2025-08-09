using UnityEngine;
using UnityEngine.UI;

namespace Trilho
{
    public class TrilhoTestSystem : MonoBehaviour
    {
        [Header("Teste do Sistema")]
        [SerializeField] private bool testOnStart = true;
        [SerializeField] private KeyCode testKey = KeyCode.T;
        
        private void Start()
        {
            if (testOnStart)
            {
                TestSystem();
            }
        }
        
        private void Update()
        {
            if (Input.GetKeyDown(testKey))
            {
                TestSystem();
            }
        }
        
        [ContextMenu("Testar Sistema")]
        public void TestSystem()
        {
            Debug.Log("=== TESTE DO SISTEMA TRILHO ===");
            
            // Testar Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log("✅ Canvas encontrado!");
                var contents = canvas.GetComponentsInChildren<Image>();
                Debug.Log($"✅ {contents.Length} conteúdos encontrados no Canvas");
                
                foreach (var content in contents)
                {
                    Debug.Log($"   - {content.name}: {content.color}");
                }
            }
            else
            {
                Debug.LogError("❌ Canvas não encontrado!");
            }
            
            // Testar MarkerSystem
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem != null)
            {
                var markers = markerSystem.GetMarkers();
                Debug.Log($"✅ MarkerSystem encontrado com {markers.Count} marcadores");
                
                for (int i = 0; i < markers.Count; i++)
                {
                    var marker = markers[i];
                    Debug.Log($"   - Marcador {i + 1}: {marker.markerName} em {marker.worldPositionX}");
                }
            }
            else
            {
                Debug.LogError("❌ MarkerSystem não encontrado!");
            }
            
            // Testar Câmera
            var camera = Camera.main;
            if (camera != null)
            {
                Debug.Log($"✅ Câmera encontrada em: {camera.transform.position}");
            }
            else
            {
                Debug.LogError("❌ Main Camera não encontrada!");
            }
            
            // Testar TrilhoGameManager
            var gameManager = FindObjectOfType<TrilhoGameManager>();
            if (gameManager != null)
            {
                Debug.Log("✅ TrilhoGameManager encontrado!");
            }
            else
            {
                Debug.LogError("❌ TrilhoGameManager não encontrado!");
            }
            
            Debug.Log("=== FIM DO TESTE ===");
        }
        
        [ContextMenu("Mostrar Conteúdos")]
        public void ShowContents()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var contents = canvas.GetComponentsInChildren<CanvasGroup>();
                foreach (var content in contents)
                {
                    content.alpha = 1f;
                    content.gameObject.SetActive(true);
                    Debug.Log($"Mostrando: {content.name}");
                }
            }
        }
        
        [ContextMenu("Esconder Conteúdos")]
        public void HideContents()
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var contents = canvas.GetComponentsInChildren<CanvasGroup>();
                foreach (var content in contents)
                {
                    content.alpha = 0f;
                    content.gameObject.SetActive(false);
                    Debug.Log($"Escondendo: {content.name}");
                }
            }
        }
        
        [ContextMenu("Mover Câmera para Teste")]
        public void MoveCameraForTest()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                // Mover câmera para posições de teste
                StartCoroutine(TestCameraMovement());
            }
        }
        
        private System.Collections.IEnumerator TestCameraMovement()
        {
            var camera = Camera.main;
            if (camera == null) yield break;
            
            Debug.Log("=== TESTE DE MOVIMENTO DA CÂMERA ===");
            
            // Posição 1: Marcador 1
            camera.transform.position = new Vector3(1000f, 0, -10);
            Debug.Log("Câmera movida para Marcador 1 (1000)");
            yield return new WaitForSeconds(2f);
            
            // Posição 2: Marcador 2
            camera.transform.position = new Vector3(3000f, 0, -10);
            Debug.Log("Câmera movida para Marcador 2 (3000)");
            yield return new WaitForSeconds(2f);
            
            // Posição 3: Marcador 3
            camera.transform.position = new Vector3(5000f, 0, -10);
            Debug.Log("Câmera movida para Marcador 3 (5000)");
            yield return new WaitForSeconds(2f);
            
            // Voltar para posição inicial
            camera.transform.position = new Vector3(0f, 0, -10);
            Debug.Log("Câmera voltou para posição inicial");
            
            Debug.Log("=== FIM DO TESTE DE MOVIMENTO ===");
        }
    }
}

