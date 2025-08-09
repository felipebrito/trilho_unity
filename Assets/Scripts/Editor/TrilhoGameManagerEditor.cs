using UnityEngine;
using UnityEditor;
using Trilho;

[CustomEditor(typeof(TrilhoGameManager))]
public class TrilhoGameManagerEditor : UnityEditor.Editor
{
    private SerializedProperty activationZones;
    private SerializedProperty showDebugInfo;
    private SerializedProperty simulatePosition;
    private SerializedProperty simulatedPositionCm;
    private SerializedProperty movementSensitivity;
    
    private bool showZoneDetails = true;
    private bool showPositionSettings = true;
    private bool showDebugSettings = true;
    
    private void OnEnable()
    {
        activationZones = serializedObject.FindProperty("activationZones");
        showDebugInfo = serializedObject.FindProperty("showDebugInfo");
        simulatePosition = serializedObject.FindProperty("simulatePosition");
        simulatedPositionCm = serializedObject.FindProperty("simulatedPositionCm");
        movementSensitivity = serializedObject.FindProperty("movementSensitivity");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        TrilhoGameManager manager = (TrilhoGameManager)target;
        
        // Header
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trilho Interactive System", EditorStyles.boldLabel);
        EditorGUILayout.Space();
        
        // Current Status (Runtime only)
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Runtime Status", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Current Position (cm)", manager.GetCurrentPositionCm());
            EditorGUILayout.FloatField("Unity Position", manager.GetCurrentUnityPosition());
            EditorGUILayout.EnumPopup("Current State", manager.GetCurrentState());
            
            var currentZone = manager.GetCurrentZone();
            EditorGUILayout.TextField("Current Zone", currentZone?.zoneName ?? "None");
            EditorGUI.EndDisabledGroup();
            
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }
        
        // Position Settings
        showPositionSettings = EditorGUILayout.Foldout(showPositionSettings, "Position Settings", true);
        if (showPositionSettings)
        {
            EditorGUILayout.BeginVertical("box");
            DrawDefaultInspector();
            EditorGUILayout.EndVertical();
        }
        
        // Debug Settings
        showDebugSettings = EditorGUILayout.Foldout(showDebugSettings, "Debug Settings", true);
        if (showDebugSettings)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(showDebugInfo);
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Position Simulation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(simulatePosition);
            
            if (simulatePosition.boolValue)
            {
                EditorGUILayout.PropertyField(simulatedPositionCm);
                EditorGUILayout.PropertyField(movementSensitivity);
                
                if (Application.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Test Zone 1 (130cm)"))
                        manager.SimulatePosition(130f);
                    if (GUILayout.Button("Test Zone 2 (240cm)"))
                        manager.SimulatePosition(240f);
                    if (GUILayout.Button("Test Zone 3 (370cm)"))
                        manager.SimulatePosition(370f);
                    EditorGUILayout.EndHorizontal();
                    
                    if (GUILayout.Button("Stop Simulation"))
                        manager.StopSimulation();
                }
            }
            
            EditorGUILayout.EndVertical();
        }
        
        // Activation Zones
        EditorGUILayout.Space();
        showZoneDetails = EditorGUILayout.Foldout(showZoneDetails, "Activation Zones", true);
        if (showZoneDetails)
        {
            EditorGUILayout.BeginVertical("box");
            
            // Zone list header
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Zones", EditorStyles.boldLabel);
            if (GUILayout.Button("Add Zone", GUILayout.Width(80)))
            {
                activationZones.arraySize++;
                var newZone = activationZones.GetArrayElementAtIndex(activationZones.arraySize - 1);
                SetDefaultZoneValues(newZone, activationZones.arraySize);
            }
            EditorGUILayout.EndHorizontal();
            
            // Draw zones
            for (int i = 0; i < activationZones.arraySize; i++)
            {
                SerializedProperty zone = activationZones.GetArrayElementAtIndex(i);
                DrawZone(zone, i);
            }
            
            EditorGUILayout.EndVertical();
        }
        
        serializedObject.ApplyModifiedProperties();
    }
    
    private void DrawZone(SerializedProperty zone, int index)
    {
        EditorGUILayout.Space();
        EditorGUILayout.BeginVertical("box");
        
        SerializedProperty zoneName = zone.FindPropertyRelative("zoneName");
        SerializedProperty startPos = zone.FindPropertyRelative("startPositionCm");
        SerializedProperty endPos = zone.FindPropertyRelative("endPositionCm");
        SerializedProperty isActive = zone.FindPropertyRelative("isActive");
        SerializedProperty contentToActivate = zone.FindPropertyRelative("contentToActivate");
        
        // Zone header
        EditorGUILayout.BeginHorizontal();
        
        string foldoutLabel = $"{zoneName.stringValue} ({startPos.floatValue}-{endPos.floatValue}cm)";
        zone.isExpanded = EditorGUILayout.Foldout(zone.isExpanded, foldoutLabel, true);
        
        GUI.color = isActive.boolValue ? Color.green : Color.red;
        isActive.boolValue = GUILayout.Toggle(isActive.boolValue, "", GUILayout.Width(20));
        GUI.color = Color.white;
        
        GUI.color = Color.red;
        if (GUILayout.Button("X", GUILayout.Width(25)))
        {
            activationZones.DeleteArrayElementAtIndex(index);
            return;
        }
        GUI.color = Color.white;
        
        EditorGUILayout.EndHorizontal();
        
        // Zone details
        if (zone.isExpanded)
        {
            EditorGUI.indentLevel++;
            
            // Basic settings
            EditorGUILayout.PropertyField(zoneName);
            EditorGUILayout.PropertyField(startPos);
            EditorGUILayout.PropertyField(endPos);
            
            // Validate zone range
            if (startPos.floatValue >= endPos.floatValue)
            {
                EditorGUILayout.HelpBox("Start position must be less than end position!", MessageType.Warning);
            }
            
            // Content settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Content", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(contentToActivate);
            
            if (contentToActivate.objectReferenceValue == null)
            {
                EditorGUILayout.HelpBox("Assign a GameObject to activate when entering this zone.", MessageType.Info);
            }
            
            // Transition settings
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Transitions", EditorStyles.boldLabel);
            SerializedProperty fadeIn = zone.FindPropertyRelative("fadeInDuration");
            SerializedProperty fadeOut = zone.FindPropertyRelative("fadeOutDuration");
            
            EditorGUILayout.PropertyField(fadeIn);
            EditorGUILayout.PropertyField(fadeOut);
            
            // Test buttons (runtime only)
            if (Application.isPlaying)
            {
                EditorGUILayout.Space();
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Test Enter"))
                {
                    float testPosition = (startPos.floatValue + endPos.floatValue) / 2f;
                    ((TrilhoGameManager)target).SimulatePosition(testPosition);
                }
                if (GUILayout.Button("Test Exit"))
                {
                    ((TrilhoGameManager)target).SimulatePosition(endPos.floatValue + 10f);
                }
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUI.indentLevel--;
        }
        
        EditorGUILayout.EndVertical();
    }
    
    private void SetDefaultZoneValues(SerializedProperty zone, int zoneIndex)
    {
        zone.FindPropertyRelative("zoneName").stringValue = $"Zona {zoneIndex}";
        zone.FindPropertyRelative("startPositionCm").floatValue = (zoneIndex - 1) * 100f;
        zone.FindPropertyRelative("endPositionCm").floatValue = zoneIndex * 100f;
        zone.FindPropertyRelative("isActive").boolValue = true;
        zone.FindPropertyRelative("fadeInDuration").floatValue = 0.5f;
        zone.FindPropertyRelative("fadeOutDuration").floatValue = 0.5f;
    }
    
    private void OnSceneGUI()
    {
        TrilhoGameManager manager = (TrilhoGameManager)target;
        
        if (!showDebugInfo.boolValue) return;
        
        // Draw zone handles in scene view
        Handles.color = Color.yellow;
        
        for (int i = 0; i < activationZones.arraySize; i++)
        {
            SerializedProperty zone = activationZones.GetArrayElementAtIndex(i);
            SerializedProperty startPos = zone.FindPropertyRelative("startPositionCm");
            SerializedProperty endPos = zone.FindPropertyRelative("endPositionCm");
            SerializedProperty zoneName = zone.FindPropertyRelative("zoneName");
            SerializedProperty isActive = zone.FindPropertyRelative("isActive");
            
            if (!isActive.boolValue) continue;
            
            float startUnity = MapPositionToUnity(startPos.floatValue, manager);
            float endUnity = MapPositionToUnity(endPos.floatValue, manager);
            
            // Draw zone boundaries
            Vector3 startPoint = new Vector3(startUnity, 0, 0);
            Vector3 endPoint = new Vector3(endUnity, 0, 0);
            
            Handles.DrawLine(startPoint + Vector3.up * 2f, startPoint - Vector3.up * 2f);
            Handles.DrawLine(endPoint + Vector3.up * 2f, endPoint - Vector3.up * 2f);
            
            // Draw zone label
            Vector3 labelPos = Vector3.Lerp(startPoint, endPoint, 0.5f) + Vector3.up * 2.5f;
            Handles.Label(labelPos, zoneName.stringValue);
            
            // Interactive handles
            EditorGUI.BeginChangeCheck();
            
            Vector3 newStartPoint = Handles.FreeMoveHandle(startPoint, 0.5f, Vector3.zero, Handles.CubeHandleCap);
            Vector3 newEndPoint = Handles.FreeMoveHandle(endPoint, 0.5f, Vector3.zero, Handles.CubeHandleCap);
            
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Move Zone Boundary");
                
                startPos.floatValue = MapUnityToPosition(newStartPoint.x, manager);
                endPos.floatValue = MapUnityToPosition(newEndPoint.x, manager);
                
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
    
    private float MapPositionToUnity(float positionCm, TrilhoGameManager manager)
    {
        // Access the private fields through reflection or use public getters
        // For now, using the known values
        float physicalMin = 0f;
        float physicalMax = 600f;
        float unityMin = 0f;
        float unityMax = 8520f;
        
        float normalizedPosition = (positionCm - physicalMin) / (physicalMax - physicalMin);
        return Mathf.Lerp(unityMin, unityMax, normalizedPosition);
    }
    
    private float MapUnityToPosition(float unityPosition, TrilhoGameManager manager)
    {
        // Reverse mapping
        float physicalMin = 0f;
        float physicalMax = 600f;
        float unityMin = 0f;
        float unityMax = 8520f;
        
        float normalizedPosition = (unityPosition - unityMin) / (unityMax - unityMin);
        return Mathf.Lerp(physicalMin, physicalMax, normalizedPosition);
    }
}
