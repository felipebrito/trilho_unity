using UnityEngine;
using UnityEngine.UI;

namespace Trilho
{
    public class TrilhoSimpleTest : MonoBehaviour
    {
        [Header("Teste Simples")]
        [SerializeField] private bool testOnStart = true;
        [SerializeField] private KeyCode moveLeftKey = KeyCode.LeftArrow;
        [SerializeField] private KeyCode moveRightKey = KeyCode.RightArrow;
        [SerializeField] private float moveSpeed = 1000f;
        
        private Camera mainCamera;
        private MarkerSystem markerSystem;
        
        private void Start()
        {
            mainCamera = Camera.main;
            markerSystem = FindObjectOfType<MarkerSystem>();
            
            if (testOnStart)
            {
                TestSystem();
            }
        }
        
        private void Update()
        {
            HandleMovement();
        }
        
        private void HandleMovement()
        {
            if (mainCamera == null) return;
            
            Vector3 newPosition = mainCamera.transform.position;
            
            if (Input.GetKey(moveLeftKey))
            {
                newPosition.x -= moveSpeed * Time.deltaTime;
                Debug.Log($"Movendo câmera para esquerda: {newPosition.x}");
            }
            
            if (Input.GetKey(moveRightKey))
            {
                newPosition.x += moveSpeed * Time.deltaTime;
                Debug.Log($"Movendo câmera para direita: {newPosition.x}");
            }
            
            mainCamera.transform.position = newPosition;
        }
        
        [ContextMenu("Testar Sistema")]
        public void TestSystem()
        {
            Debug.Log("=== TESTE SIMPLES ===");
            
            // Testar câmera
            if (mainCamera != null)
            {
                Debug.Log($"✅ Câmera: {mainCamera.transform.position}");
            }
            else
            {
                Debug.LogError("❌ Câmera não encontrada!");
            }
            
            // Testar MarkerSystem
            if (markerSystem != null)
            {
                var markers = markerSystem.GetMarkers();
                Debug.Log($"✅ {markers.Count} marcadores encontrados");
            }
            else
            {
                Debug.LogError("❌ MarkerSystem não encontrado!");
            }
            
            // Testar Canvas
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                var contents = canvas.GetComponentsInChildren<Image>();
                Debug.Log($"✅ {contents.Length} conteúdos no Canvas");
            }
            else
            {
                Debug.LogError("❌ Canvas não encontrado!");
            }
            
            Debug.Log("=== FIM DO TESTE ===");
        }
        
        [ContextMenu("Mover para Marcador 1")]
        public void MoveToMarker1()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(1000f, 0, -10);
                Debug.Log("Câmera movida para Marcador 1 (1000)");
            }
        }
        
        [ContextMenu("Mover para Marcador 2")]
        public void MoveToMarker2()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(3000f, 0, -10);
                Debug.Log("Câmera movida para Marcador 2 (3000)");
            }
        }
        
        [ContextMenu("Mover para Marcador 3")]
        public void MoveToMarker3()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(5000f, 0, -10);
                Debug.Log("Câmera movida para Marcador 3 (5000)");
            }
        }
        
        [ContextMenu("Voltar para Posição Inicial")]
        public void MoveToInitialPosition()
        {
            if (mainCamera != null)
            {
                mainCamera.transform.position = new Vector3(0f, 0, -10);
                Debug.Log("Câmera voltou para posição inicial");
            }
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
    }
}
