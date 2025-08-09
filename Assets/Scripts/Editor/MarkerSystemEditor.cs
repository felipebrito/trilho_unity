using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Trilho.Editor
{
    [CustomEditor(typeof(MarkerSystem))]
    public class MarkerSystemEditor : UnityEditor.Editor
    {
        private MarkerSystem markerSystem;
        private bool showMarkers = true;
        private bool showDebug = true;
        private bool showSettings = true;
        
        private void OnEnable()
        {
            markerSystem = (MarkerSystem)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== MARKER SYSTEM ===", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Settings Section
            showSettings = EditorGUILayout.Foldout(showSettings, "Settings", true);
            if (showSettings)
            {
                EditorGUI.indentLevel++;
                
                SerializedProperty cameraTransform = serializedObject.FindProperty("cameraTransform");
                EditorGUILayout.PropertyField(cameraTransform);
                
                SerializedProperty updateInterval = serializedObject.FindProperty("updateInterval");
                EditorGUILayout.PropertyField(updateInterval);
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Markers Section
            showMarkers = EditorGUILayout.Foldout(showMarkers, "Markers", true);
            if (showMarkers)
            {
                EditorGUI.indentLevel++;
                
                SerializedProperty markersProperty = serializedObject.FindProperty("markers");
                EditorGUILayout.PropertyField(markersProperty, true);
                
                EditorGUILayout.Space();
                
                // Quick Actions
                EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
                
                if (GUILayout.Button("Add New Marker"))
                {
                    AddNewMarker();
                }
                
                if (GUILayout.Button("Create Example Markers"))
                {
                    markerSystem.CriarMarcadoresDeExemplo();
                }
                
                if (GUILayout.Button("Clear All Markers"))
                {
                    markerSystem.LimparMarcadores();
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Debug Section
            showDebug = EditorGUILayout.Foldout(showDebug, "Debug", true);
            if (showDebug)
            {
                EditorGUI.indentLevel++;
                
                SerializedProperty showDebugInfo = serializedObject.FindProperty("showDebugInfo");
                EditorGUILayout.PropertyField(showDebugInfo);
                
                SerializedProperty showMarkerSpheres = serializedObject.FindProperty("showMarkerSpheres");
                EditorGUILayout.PropertyField(showMarkerSpheres);
                
                SerializedProperty showActivationRanges = serializedObject.FindProperty("showActivationRanges");
                EditorGUILayout.PropertyField(showActivationRanges);
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("Toggle Debug Visualization"))
                {
                    markerSystem.ToggleDebugVisualization();
                }
                
                if (GUILayout.Button("Show Marker Info"))
                {
                    markerSystem.MostrarInfoDosMarcadores();
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Status Section
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            var markers = markerSystem.GetMarkers();
            EditorGUILayout.LabelField($"Total Markers: {markers.Count}");
            
            int activeMarkers = 0;
            foreach (var marker in markers)
            {
                if (marker.isActive) activeMarkers++;
            }
            EditorGUILayout.LabelField($"Active Markers: {activeMarkers}");
            
            EditorGUI.indentLevel--;
            
            serializedObject.ApplyModifiedProperties();
        }
        
        private void AddNewMarker()
        {
            var newMarker = new ContentMarker
            {
                markerName = $"Marker_{markerSystem.GetMarkers().Count + 1}",
                worldPositionX = 0f,
                activationRadius = 100f,
                fadeInDuration = 0.5f,
                fadeOutDuration = 0.5f,
                debugColor = Color.yellow
            };
            
            markerSystem.AddMarker(newMarker);
            EditorUtility.SetDirty(markerSystem);
        }
    }
    
    [CustomPropertyDrawer(typeof(ContentMarker))]
    public class ContentMarkerDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            
            // Calculate rects
            var labelRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            var foldoutRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            
            // Draw foldout
            property.isExpanded = EditorGUI.Foldout(foldoutRect, property.isExpanded, label);
            
            if (property.isExpanded)
            {
                EditorGUI.indentLevel++;
                
                var yOffset = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // Marker Settings
                var markerNameRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                var worldPosRect = new Rect(position.x, position.y + yOffset * 2, position.width, EditorGUIUtility.singleLineHeight);
                var radiusRect = new Rect(position.x, position.y + yOffset * 3, position.width, EditorGUIUtility.singleLineHeight);
                
                EditorGUI.PropertyField(markerNameRect, property.FindPropertyRelative("markerName"));
                EditorGUI.PropertyField(worldPosRect, property.FindPropertyRelative("worldPositionX"));
                EditorGUI.PropertyField(radiusRect, property.FindPropertyRelative("activationRadius"));
                
                yOffset += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;
                
                // Content
                var contentRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                var fadeInRect = new Rect(position.x, position.y + yOffset * 2, position.width, EditorGUIUtility.singleLineHeight);
                var fadeOutRect = new Rect(position.x, position.y + yOffset * 3, position.width, EditorGUIUtility.singleLineHeight);
                
                EditorGUI.PropertyField(contentRect, property.FindPropertyRelative("contentToActivate"));
                EditorGUI.PropertyField(fadeInRect, property.FindPropertyRelative("fadeInDuration"));
                EditorGUI.PropertyField(fadeOutRect, property.FindPropertyRelative("fadeOutDuration"));
                
                yOffset += EditorGUIUtility.singleLineHeight * 3 + EditorGUIUtility.standardVerticalSpacing;
                
                // Debug
                var debugColorRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                EditorGUI.PropertyField(debugColorRect, property.FindPropertyRelative("debugColor"));
                
                yOffset += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
                
                // Runtime Status (read-only)
                var isActiveRect = new Rect(position.x, position.y + yOffset, position.width, EditorGUIUtility.singleLineHeight);
                var isInRangeRect = new Rect(position.x, position.y + yOffset * 2, position.width, EditorGUIUtility.singleLineHeight);
                
                var isActive = property.FindPropertyRelative("isActive").boolValue;
                var isInRange = property.FindPropertyRelative("isInRange").boolValue;
                
                EditorGUI.LabelField(isActiveRect, "Is Active", isActive ? "✓" : "✗");
                EditorGUI.LabelField(isInRangeRect, "In Range", isInRange ? "✓" : "✗");
                
                EditorGUI.indentLevel--;
            }
            
            EditorGUI.EndProperty();
        }
        
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!property.isExpanded)
                return EditorGUIUtility.singleLineHeight;
            
            return EditorGUIUtility.singleLineHeight * 12 + EditorGUIUtility.standardVerticalSpacing * 4;
        }
    }
}
