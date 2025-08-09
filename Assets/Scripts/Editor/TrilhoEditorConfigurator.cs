using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Trilho
{
    public class TrilhoEditorConfigurator : MonoBehaviour
    {
        [Header("Configuração Visual")]
        [SerializeField] private bool showPositionHandles = true;
        [SerializeField] private bool showDistanceLabels = true;
        [SerializeField] private bool showMarkerPositions = true;
        
        [Header("Posições dos Colisores")]
        [SerializeField] public float leftColliderX = 0f;
        [SerializeField] public float rightColliderX = 2000f;
        
        [Header("Marcadores")]
        [SerializeField] public List<MarkerPosition> markerPositions = new List<MarkerPosition>();
        
        [System.Serializable]
        public class MarkerPosition
        {
            public string name;
            public float positionX;
            public float activationRadius = 200f;
            public Color debugColor = Color.green;
        }
        
        private GameObject leftCollider;
        private GameObject rightCollider;
        private MarkerSystem markerSystem;
        
        private void OnEnable()
        {
            FindColliders();
            FindMarkerSystem();
            LoadCurrentPositions();
        }
        
        private void FindColliders()
        {
            var borderColliders = GameObject.Find("Colisores das Bordas");
            if (borderColliders != null)
            {
                leftCollider = borderColliders.transform.Find("Colisor Esquerdo")?.gameObject;
                rightCollider = borderColliders.transform.Find("Colisor Direito")?.gameObject;
            }
        }
        
        private void FindMarkerSystem()
        {
            markerSystem = FindObjectOfType<MarkerSystem>();
        }
        
        private void LoadCurrentPositions()
        {
            if (leftCollider != null)
                leftColliderX = leftCollider.transform.position.x;
            if (rightCollider != null)
                rightColliderX = rightCollider.transform.position.x;
        }
        
        [ContextMenu("Aplicar Posições dos Colisores")]
        public void ApplyColliderPositions()
        {
            if (leftCollider != null)
            {
                Vector3 pos = leftCollider.transform.position;
                pos.x = leftColliderX;
                leftCollider.transform.position = pos;
                Debug.Log($"[EDITOR] Colisor Esquerdo aplicado: {leftColliderX}");
            }
            
            if (rightCollider != null)
            {
                Vector3 pos = rightCollider.transform.position;
                pos.x = rightColliderX;
                rightCollider.transform.position = pos;
                Debug.Log($"[EDITOR] Colisor Direito aplicado: {rightColliderX}");
            }
        }
        
        [ContextMenu("Aplicar Posições dos Marcadores")]
        public void ApplyMarkerPositions()
        {
            if (markerSystem == null)
            {
                Debug.LogError("MarkerSystem não encontrado!");
                return;
            }
            
            markerSystem.LimparMarcadores();
            
            foreach (var markerPos in markerPositions)
            {
                var marker = new ContentMarker
                {
                    markerName = markerPos.name,
                    worldPositionX = markerPos.positionX,
                    activationRadius = markerPos.activationRadius,
                    fadeInDuration = 0.5f,
                    fadeOutDuration = 0.5f,
                    debugColor = markerPos.debugColor
                };
                
                markerSystem.AddMarker(marker);
            }
            
            Debug.Log($"[EDITOR] {markerPositions.Count} marcadores aplicados!");
        }
        
        [ContextMenu("Carregar Posições Atuais")]
        public void LoadCurrentPositionsFromScene()
        {
            LoadCurrentPositions();
            
            if (markerSystem != null)
            {
                var markers = markerSystem.GetMarkers();
                markerPositions.Clear();
                
                foreach (var marker in markers)
                {
                    markerPositions.Add(new MarkerPosition
                    {
                        name = marker.markerName,
                        positionX = marker.worldPositionX,
                        activationRadius = marker.activationRadius,
                        debugColor = marker.debugColor
                    });
                }
                
                Debug.Log($"[EDITOR] {markerPositions.Count} posições carregadas!");
            }
        }
        
        [ContextMenu("Criar Marcadores de Exemplo")]
        public void CreateExampleMarkers()
        {
            markerPositions.Clear();
            
            markerPositions.Add(new MarkerPosition
            {
                name = "Marcador Verde",
                positionX = 1000f,
                activationRadius = 200f,
                debugColor = Color.green
            });
            
            markerPositions.Add(new MarkerPosition
            {
                name = "Marcador Azul",
                positionX = 3000f,
                activationRadius = 200f,
                debugColor = Color.blue
            });
            
            markerPositions.Add(new MarkerPosition
            {
                name = "Marcador Vermelho",
                positionX = 5000f,
                activationRadius = 200f,
                debugColor = Color.red
            });
            
            Debug.Log("[EDITOR] 3 marcadores de exemplo criados!");
        }
        
        private void OnDrawGizmos()
        {
            if (!showPositionHandles) return;
            
            // Draw collider positions
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(new Vector3(leftColliderX, 0, 0), Vector3.one * 100);
            Gizmos.DrawWireCube(new Vector3(rightColliderX, 0, 0), Vector3.one * 100);
            
            // Draw distance
            if (showDistanceLabels)
            {
                float distance = Mathf.Abs(rightColliderX - leftColliderX);
                Vector3 centerPos = new Vector3((leftColliderX + rightColliderX) * 0.5f, 100, 0);
                
                #if UNITY_EDITOR
                Handles.color = Color.yellow;
                Handles.Label(centerPos, $"Distância: {distance:F1}");
                Handles.DrawLine(new Vector3(leftColliderX, 0, 0), new Vector3(rightColliderX, 0, 0));
                #endif
            }
            
            // Draw marker positions
            if (showMarkerPositions)
            {
                foreach (var markerPos in markerPositions)
                {
                    Gizmos.color = markerPos.debugColor;
                    Gizmos.DrawWireSphere(new Vector3(markerPos.positionX, 0, 0), markerPos.activationRadius);
                    
                    #if UNITY_EDITOR
                    Handles.color = markerPos.debugColor;
                    Handles.Label(new Vector3(markerPos.positionX, markerPos.activationRadius + 50, 0), 
                        $"{markerPos.name}\n{markerPos.positionX:F1}");
                    #endif
                }
            }
        }
    }
    
    [CustomEditor(typeof(TrilhoEditorConfigurator))]
    public class TrilhoEditorConfiguratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            TrilhoEditorConfigurator configurator = (TrilhoEditorConfigurator)target;
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== CONFIGURADOR VISUAL TRILHO ===", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Visual settings
            EditorGUILayout.LabelField("Configuração Visual", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            SerializedProperty showPositionHandles = serializedObject.FindProperty("showPositionHandles");
            EditorGUILayout.PropertyField(showPositionHandles);
            
            SerializedProperty showDistanceLabels = serializedObject.FindProperty("showDistanceLabels");
            EditorGUILayout.PropertyField(showDistanceLabels);
            
            SerializedProperty showMarkerPositions = serializedObject.FindProperty("showMarkerPositions");
            EditorGUILayout.PropertyField(showMarkerPositions);
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // Collider positions
            EditorGUILayout.LabelField("Posições dos Colisores", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            SerializedProperty leftColliderX = serializedObject.FindProperty("leftColliderX");
            EditorGUILayout.PropertyField(leftColliderX);
            
            SerializedProperty rightColliderX = serializedObject.FindProperty("rightColliderX");
            EditorGUILayout.PropertyField(rightColliderX);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Aplicar Posições"))
            {
                configurator.ApplyColliderPositions();
            }
            if (GUILayout.Button("Carregar da Cena"))
            {
                configurator.LoadCurrentPositionsFromScene();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // Marker positions
            EditorGUILayout.LabelField("Posições dos Marcadores", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            SerializedProperty markerPositions = serializedObject.FindProperty("markerPositions");
            EditorGUILayout.PropertyField(markerPositions, true);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Aplicar Marcadores"))
            {
                configurator.ApplyMarkerPositions();
            }
            if (GUILayout.Button("Criar Exemplo"))
            {
                configurator.CreateExampleMarkers();
            }
            EditorGUILayout.EndHorizontal();
            
            EditorGUI.indentLevel--;
            EditorGUILayout.Space();
            
            // Quick actions
            EditorGUILayout.LabelField("Ações Rápidas", EditorStyles.boldLabel);
            
            if (GUILayout.Button("🎯 Configuração Completa", GUILayout.Height(30)))
            {
                configurator.CreateExampleMarkers();
                configurator.ApplyMarkerPositions();
                configurator.ApplyColliderPositions();
            }
            
            if (GUILayout.Button("📊 Mostrar Estatísticas", GUILayout.Height(30)))
            {
                ShowStatistics(configurator);
            }
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void ShowStatistics(TrilhoEditorConfigurator configurator)
        {
            float distance = Mathf.Abs(configurator.rightColliderX - configurator.leftColliderX);
            
            Debug.Log("=== ESTATÍSTICAS DA CONFIGURAÇÃO ===");
            Debug.Log($"Distância entre colisores: {distance:F1}");
            Debug.Log($"Posição esquerda: {configurator.leftColliderX:F1}");
            Debug.Log($"Posição direita: {configurator.rightColliderX:F1}");
            Debug.Log($"Número de marcadores: {configurator.markerPositions.Count}");
            
            foreach (var marker in configurator.markerPositions)
            {
                Debug.Log($"- {marker.name}: {marker.positionX:F1} (raio: {marker.activationRadius})");
            }
        }
    }
}
