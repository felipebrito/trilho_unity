using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace Trilho
{
    public class TrilhoQuickSetup : MonoBehaviour
    {
        [Header("Setup Rápido")]
        [SerializeField] private bool autoSetupOnStart = true;
        
        [Header("Configurações")]
        [SerializeField] private float leftColliderX = 0f;
        [SerializeField] private float rightColliderX = 2000f;
        [SerializeField] private Vector3 cameraPosition = new Vector3(0, 0, -10);
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                QuickSetup();
            }
        }
        
        [ContextMenu("Setup Rápido")]
        public void QuickSetup()
        {
            Debug.Log("=== SETUP RÁPIDO TRILHO ===");
            
            // 1. Criar Canvas
            CreateCanvas();
            
            // 2. Configurar câmera
            SetupCamera();
            
            // 3. Criar Trilho Manager
            CreateTrilhoManager();
            
            // 4. Criar marcadores de exemplo
            CreateExampleMarkers();
            
            // 5. Criar conteúdos de exemplo
            CreateExampleContents();
            
            // 6. Criar debug visual
            CreateDebugVisual();
            
            Debug.Log("✅ SETUP CONCLUÍDO - Verifique a Game View!");
        }
        
        private void CreateCanvas()
        {
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
                
                // Criar EventSystem se não existir
                if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var eventSystemGO = new GameObject("EventSystem");
                    eventSystemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    eventSystemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
        }
        
        private void SetupCamera()
        {
            var camera = Camera.main;
            if (camera != null)
            {
                camera.transform.position = cameraPosition;
            }
        }
        
        private void CreateTrilhoManager()
        {
            // Criar Trilho Manager se não existir
            var trilhoManager = GameObject.Find("Trilho Manager");
            if (trilhoManager == null)
            {
                trilhoManager = new GameObject("Trilho Manager");
            }
            
            // Adicionar MarkerSystem
            var markerSystem = trilhoManager.GetComponent<MarkerSystem>();
            if (markerSystem == null)
            {
                markerSystem = trilhoManager.AddComponent<MarkerSystem>();
            }
            
            // Adicionar TrilhoGameManager (desabilitado por padrão)
            var gameManager = trilhoManager.GetComponent<TrilhoGameManager>();
            if (gameManager == null)
            {
                gameManager = trilhoManager.AddComponent<TrilhoGameManager>();
            }
            
            // Desabilitar o TrilhoGameManager para evitar interferência
            gameManager.enabled = false;
        }
        
        private void CreateExampleMarkers()
        {
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem == null) return;
            
            // Limpar marcadores existentes
            markerSystem.LimparMarcadores();
            
            // Garantir opções de debug discretas
            markerSystem.SetDebugOptions(showInfo: false, showSpheres: true, showRanges: true);
            
            // Criar 3 marcadores de exemplo (sem conteúdo ainda)
            var colors = new[] { Color.green, Color.blue, Color.red };
            for (int i = 0; i < 3; i++)
            {
                var marker = new ContentMarker
                {
                    markerName = $"Marcador {i + 1}",
                    worldPositionX = 1000f + (i * 2000f),
                    activationRadius = 200f,
                    fadeInDuration = 0.5f,
                    fadeOutDuration = 0.5f,
                    debugColor = colors[i]
                };
                
                markerSystem.AddMarker(marker);
            }
        }
        
        private void CreateExampleContents()
        {
            var canvas = FindObjectOfType<Canvas>();
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (canvas == null || markerSystem == null) return;
            
            var markers = markerSystem.GetMarkers();
            if (markers.Count == 0) return;
            
            // Para cada marcador, crie e vincule um conteúdo correspondente
            for (int i = 0; i < markers.Count; i++)
            {
                var marker = markers[i];
                var content = new GameObject($"Conteúdo {i + 1}");
                content.transform.SetParent(canvas.transform, false);
                
                var image = content.AddComponent<Image>();
                image.color = marker.debugColor;
                
                var rectTransform = content.GetComponent<RectTransform>();
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
                
                var canvasGroup = content.AddComponent<CanvasGroup>();
                canvasGroup.alpha = 0f; // oculto inicialmente
                content.SetActive(false); // só aparece quando o marcador ativar
                
                // Adicionar texto para identificar
                var textGO = new GameObject("Texto");
                textGO.transform.SetParent(content.transform, false);
                var text = textGO.AddComponent<Text>();
                text.text = marker.markerName;
                text.color = Color.white;
                text.fontSize = 72;
                text.alignment = TextAnchor.MiddleCenter;
                
                var textRect = textGO.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.sizeDelta = Vector2.zero;
                textRect.anchoredPosition = Vector2.zero;
                
                // Vincular conteúdo ao marcador
                marker.contentToActivate = content;
            }
        }
        
        [ContextMenu("Testar Sistema")]
        public void TestSystem()
        {
            Debug.Log("=== TESTE DO SISTEMA ===");
            
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem != null)
            {
                var markers = markerSystem.GetMarkers();
                Debug.Log($"Encontrados {markers.Count} marcadores:");
                
                for (int i = 0; i < markers.Count; i++)
                {
                    var marker = markers[i];
                    Debug.Log($"✅ Marcador {i + 1}: {marker.markerName}");
                    Debug.Log($"   Posição: {marker.worldPositionX}");
                    Debug.Log($"   Conteúdo: {(marker.contentToActivate != null ? marker.contentToActivate.name : "Nenhum")}");
                }
            }
            else
            {
                Debug.LogError("MarkerSystem não encontrado!");
            }
            
            var canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Debug.Log("✅ Canvas encontrado e configurado!");
            }
            else
            {
                Debug.LogError("Canvas não encontrado!");
            }
            
            var camera = Camera.main;
            if (camera != null)
            {
                Debug.Log($"✅ Câmera configurada: {camera.transform.position}");
            }
            else
            {
                Debug.LogError("Main Camera não encontrada!");
            }
        }
        
        [ContextMenu("Criar Colisores de Borda")]
        public void CreateBorderColliders()
        {
            var borderColliders = GameObject.Find("Colisores das Bordas");
            if (borderColliders == null)
            {
                borderColliders = new GameObject("Colisores das Bordas");
            }
            
            // Criar colisor esquerdo
            var leftCollider = borderColliders.transform.Find("Colisor Esquerdo");
            if (leftCollider == null)
            {
                var leftColliderGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                leftColliderGO.name = "Colisor Esquerdo";
                leftColliderGO.transform.SetParent(borderColliders.transform);
                leftColliderGO.transform.position = new Vector3(leftColliderX, 0, 0);
                leftColliderGO.transform.localScale = new Vector3(10, 100, 10);
                
                var renderer = leftColliderGO.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = Color.red;
            }
            
            // Criar colisor direito
            var rightCollider = borderColliders.transform.Find("Colisor Direito");
            if (rightCollider == null)
            {
                var rightColliderGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rightColliderGO.name = "Colisor Direito";
                rightColliderGO.transform.SetParent(borderColliders.transform);
                rightColliderGO.transform.position = new Vector3(rightColliderX, 0, 0);
                rightColliderGO.transform.localScale = new Vector3(10, 100, 10);
                
                var renderer = rightColliderGO.GetComponent<Renderer>();
                if (renderer != null)
                    renderer.material.color = Color.red;
            }
            
            Debug.Log("Colisores de borda criados!");
        }

        [ContextMenu("Criar Debug Visual")]
        public void CreateDebugVisual()
        {
            // Criar esferas de debug para marcadores
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem != null)
            {
                var markers = markerSystem.GetMarkers();
                
                for (int i = 0; i < markers.Count; i++)
                {
                    var marker = markers[i];
                    
                    // Criar esfera de debug
                    var debugSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    debugSphere.name = $"Debug_Sphere_Marcador_{i + 1}";
                    debugSphere.transform.position = new Vector3(marker.worldPositionX, 0, 0);
                    debugSphere.transform.localScale = Vector3.one * 50f;
                    
                    var renderer = debugSphere.GetComponent<Renderer>();
                    if (renderer != null)
                        renderer.material.color = marker.debugColor;
                    
                    // Criar cilindro para mostrar raio
                    var debugRange = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    debugRange.name = $"Debug_Range_Marcador_{i + 1}";
                    debugRange.transform.position = new Vector3(marker.worldPositionX, 0, 0);
                    debugRange.transform.localScale = new Vector3(marker.activationRadius * 2, 1, marker.activationRadius * 2);
                    
                    var rangeRenderer = debugRange.GetComponent<Renderer>();
                    if (rangeRenderer != null)
                    {
                        rangeRenderer.material.color = new Color(marker.debugColor.r, marker.debugColor.g, marker.debugColor.b, 0.3f);
                    }
                    
                    // Criar texto 3D
                    var textGO = new GameObject($"Debug_Text_Marcador_{i + 1}");
                    textGO.transform.position = new Vector3(marker.worldPositionX, 100, 0);
                    
                    var textMesh = textGO.AddComponent<TextMesh>();
                    textMesh.text = marker.markerName;
                    textMesh.fontSize = 24;
                    textMesh.color = marker.debugColor;
                    textMesh.alignment = TextAlignment.Center;
                }
            }
            
            // Criar linha de debug para câmera
            var camera = Camera.main;
            if (camera != null)
            {
                var cameraDebug = new GameObject("Debug_Camera_Position");
                cameraDebug.transform.position = camera.transform.position;
                
                var lineRenderer = cameraDebug.AddComponent<LineRenderer>();
                lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
                lineRenderer.startColor = Color.yellow;
                lineRenderer.endColor = Color.yellow;
                lineRenderer.startWidth = 10f;
                lineRenderer.endWidth = 10f;
                lineRenderer.positionCount = 2;
                lineRenderer.SetPosition(0, camera.transform.position);
                lineRenderer.SetPosition(1, camera.transform.position + Vector3.forward * 1000f);
            }
        }
    }
}
