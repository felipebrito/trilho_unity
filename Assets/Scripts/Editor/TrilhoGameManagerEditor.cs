using UnityEngine;
using UnityEditor;
using Trilho;

[CustomEditor(typeof(TrilhoGameManager))]
public class TrilhoGameManagerEditor : UnityEditor.Editor
{
    private SerializedProperty showDebugInfo;
    private SerializedProperty simulatePosition;
    private SerializedProperty simulatedPositionCm;
    private SerializedProperty movementSensitivity;

    private bool showPositionSettings = true;
    private bool showDebugSettings = true;

    private void OnEnable()
    {
        showDebugInfo = serializedObject.FindProperty("showDebugInfo");
        simulatePosition = serializedObject.FindProperty("simulatePosition");
        simulatedPositionCm = serializedObject.FindProperty("simulatedPositionCm");
        movementSensitivity = serializedObject.FindProperty("movementSensitivity");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        TrilhoGameManager manager = (TrilhoGameManager)target;

        // Cabeçalho
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Trilho - Gerenciador", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        // Status em runtime
        if (Application.isPlaying)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField("Status de Execução", EditorStyles.boldLabel);
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.FloatField("Posição Atual (cm)", manager.GetCurrentPositionCm());
            EditorGUILayout.FloatField("Posição Unity (X)", manager.GetCurrentUnityPosition());
            EditorGUILayout.FloatField("Largura da TV (cm)", manager.GetScreenWidthCm());
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
        }

        // Configurações (mostra os campos padrão do MonoBehaviour, já organizados no script)
        showPositionSettings = EditorGUILayout.Foldout(showPositionSettings, "Configurações", true);
        if (showPositionSettings)
        {
            EditorGUILayout.BeginVertical("box");
            DrawDefaultInspector();
            EditorGUILayout.EndVertical();
        }

        // Debug/Simulação
        showDebugSettings = EditorGUILayout.Foldout(showDebugSettings, "Depuração e Simulação", true);
        if (showDebugSettings)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.PropertyField(showDebugInfo);
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Simulação de Posição", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(simulatePosition);

            if (simulatePosition.boolValue)
            {
                EditorGUILayout.PropertyField(simulatedPositionCm);
                EditorGUILayout.PropertyField(movementSensitivity);

                if (Application.isPlaying)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("Ir para 100cm")) manager.SimulatePosition(100f);
                    if (GUILayout.Button("Ir para 300cm")) manager.SimulatePosition(300f);
                    if (GUILayout.Button("Parar")) manager.StopSimulation();
                    EditorGUILayout.EndHorizontal();
                }
            }

            EditorGUILayout.EndVertical();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
