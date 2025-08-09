using UnityEngine;
using UnityEditor;

namespace Trilho.Editor
{
    [CustomEditor(typeof(TrilhoSetupWizard))]
    public class TrilhoSetupWizardEditor : UnityEditor.Editor
    {
        private TrilhoSetupWizard wizard;
        private bool showWizardInfo = true;
        private bool showQuickActions = true;
        private bool showStepInfo = true;
        private bool showEditorSetup = true;
        
        private void OnEnable()
        {
            wizard = (TrilhoSetupWizard)target;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("=== WIZARD DE CONFIGURAÇÃO TRILHO ===", EditorStyles.boldLabel);
            EditorGUILayout.Space();
            
            // Editor Setup Section
            showEditorSetup = EditorGUILayout.Foldout(showEditorSetup, "Configuração em Editor", true);
            if (showEditorSetup)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.HelpBox(
                    "Configure o sistema diretamente no Editor, sem precisar executar o jogo:\n\n" +
                    "• Configure posições dos marcadores\n" +
                    "• Ajuste raios de ativação\n" +
                    "• Crie conteúdos específicos\n" +
                    "• Teste configurações",
                    MessageType.Info);
                
                EditorGUILayout.Space();
                
                if (GUILayout.Button("🎯 Configurar Marcadores no Editor", GUILayout.Height(30)))
                {
                    SetupMarkersInEditor();
                }
                
                if (GUILayout.Button("📐 Ajustar Posições", GUILayout.Height(30)))
                {
                    AdjustPositionsInEditor();
                }
                
                if (GUILayout.Button("🎨 Criar Conteúdos de Exemplo", GUILayout.Height(30)))
                {
                    CreateExampleContentInEditor();
                }
                
                if (GUILayout.Button("🧪 Testar Configuração", GUILayout.Height(30)))
                {
                    TestConfigurationInEditor();
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Wizard Info
            showWizardInfo = EditorGUILayout.Foldout(showWizardInfo, "Informações do Wizard", true);
            if (showWizardInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.HelpBox(
                    "Este wizard guia você através da configuração do sistema Trilho passo a passo:\n\n" +
                    "1. Configurar bordas das TVs\n" +
                    "2. Posicionar conteúdos\n" +
                    "3. Configurar marcadores\n" +
                    "4. Testar sistema",
                    MessageType.Info);
                
                EditorGUILayout.Space();
                
                SerializedProperty showWizardOnStart = serializedObject.FindProperty("showWizardOnStart");
                EditorGUILayout.PropertyField(showWizardOnStart);
                
                SerializedProperty currentStep = serializedObject.FindProperty("currentStep");
                EditorGUILayout.PropertyField(currentStep);
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Quick Actions
            showQuickActions = EditorGUILayout.Foldout(showQuickActions, "Ações Rápidas", true);
            if (showQuickActions)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🚀 Iniciar Wizard", GUILayout.Height(30)))
                {
                    wizard.StartWizard();
                }
                if (GUILayout.Button("⏭️ Pular Wizard", GUILayout.Height(30)))
                {
                    wizard.SkipWizard();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("✅ Finalizar Wizard", GUILayout.Height(30)))
                {
                    wizard.FinishWizard();
                }
                if (GUILayout.Button("🔄 Reiniciar", GUILayout.Height(30)))
                {
                    wizard.StartWizard();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUILayout.Space();
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("🔄 Reativar Modo Configuração", GUILayout.Height(30)))
                {
                    wizard.ReactivateConfigurationMode();
                }
                if (GUILayout.Button("❌ Sair do Modo Configuração", GUILayout.Height(30)))
                {
                    wizard.ExitConfigurationMode();
                }
                EditorGUILayout.EndHorizontal();
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // Step Info
            showStepInfo = EditorGUILayout.Foldout(showStepInfo, "Informações dos Passos", true);
            if (showStepInfo)
            {
                EditorGUI.indentLevel++;
                
                EditorGUILayout.LabelField("Passos do Wizard:", EditorStyles.boldLabel);
                
                string[] stepTitles = {
                    "1. Configurar Bordas das TVs",
                    "2. Posicionar Conteúdos", 
                    "3. Configurar Marcadores",
                    "4. Testar Sistema"
                };
                
                string[] stepDescriptions = {
                    "Posicione os colisores vermelhos nas bordas das TVs usando as setas do teclado",
                    "Clique onde você quer que os conteúdos apareçam (Verde, Azul, Vermelho)",
                    "Os marcadores são criados automaticamente no sistema",
                    "Teste movendo a câmera e verificando se os conteúdos aparecem"
                };
                
                for (int i = 0; i < stepTitles.Length; i++)
                {
                    EditorGUILayout.BeginVertical("box");
                    EditorGUILayout.LabelField(stepTitles[i], EditorStyles.boldLabel);
                    EditorGUILayout.LabelField(stepDescriptions[i], EditorStyles.wordWrappedLabel);
                    EditorGUILayout.EndVertical();
                }
                
                EditorGUI.indentLevel--;
                EditorGUILayout.Space();
            }
            
            // References
            EditorGUILayout.LabelField("Referências", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            
            SerializedProperty markerSystem = serializedObject.FindProperty("markerSystem");
            EditorGUILayout.PropertyField(markerSystem);
            
            SerializedProperty targetCanvas = serializedObject.FindProperty("targetCanvas");
            EditorGUILayout.PropertyField(targetCanvas);
            
            SerializedProperty sceneCamera = serializedObject.FindProperty("sceneCamera");
            EditorGUILayout.PropertyField(sceneCamera);
            
            EditorGUI.indentLevel--;
            
            // Instructions
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Instruções de Uso", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Controles:\n" +
                "• Teclas 1-4: Navegar entre passos\n" +
                "• Setas: Mover colisores (Passo 1) - APENAS no modo configuração\n" +
                "• ENTER: Próximo passo\n" +
                "• BACKSPACE: Passo anterior\n" +
                "• ESC: Sair do modo configuração\n\n" +
                "Editor:\n" +
                "• Use os botões acima para configuração em tempo de edição\n" +
                "• Configure marcadores diretamente no Inspector",
                MessageType.Info);
            
            serializedObject.ApplyModifiedProperties();
        }
        
        #region Editor Setup Methods
        
        private void SetupMarkersInEditor()
        {
            Debug.Log("=== CONFIGURANDO MARCADORES NO EDITOR ===");
            
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem == null)
            {
                Debug.LogError("MarkerSystem não encontrado! Crie um primeiro.");
                return;
            }
            
            // Clear existing markers
            markerSystem.LimparMarcadores();
            
            // Create example markers
            CreateExampleMarkersInEditor(markerSystem);
            
            Debug.Log("Marcadores configurados no editor!");
        }
        
        private void CreateExampleMarkersInEditor(MarkerSystem markerSystem)
        {
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            // Create 3 example markers
            for (int i = 0; i < 3; i++)
            {
                var marker = new ContentMarker
                {
                    markerName = $"Marcador {i + 1}",
                    worldPositionX = 1000f + (i * 2000f),
                    activationRadius = 200f,
                    fadeInDuration = 0.5f,
                    fadeOutDuration = 0.5f,
                    debugColor = i == 0 ? Color.green : i == 1 ? Color.blue : Color.red
                };
                
                // Create content
                var content = CreateContentInEditor(i, marker.debugColor, canvas);
                marker.contentToActivate = content;
                
                markerSystem.AddMarker(marker);
            }
            
            Debug.Log("3 marcadores de exemplo criados!");
        }
        
        private GameObject CreateContentInEditor(int index, Color color, Canvas canvas)
        {
            var content = new GameObject($"Conteúdo {index + 1}");
            content.transform.SetParent(canvas.transform, false);
            
            var image = content.AddComponent<UnityEngine.UI.Image>();
            image.color = color;
            
            var rectTransform = content.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.sizeDelta = Vector2.zero;
            rectTransform.anchoredPosition = Vector2.zero;
            
            var canvasGroup = content.AddComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;
            content.SetActive(false);
            
            return content;
        }
        
        private void AdjustPositionsInEditor()
        {
            Debug.Log("=== AJUSTANDO POSIÇÕES NO EDITOR ===");
            
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem == null)
            {
                Debug.LogError("MarkerSystem não encontrado!");
                return;
            }
            
            var markers = markerSystem.GetMarkers();
            if (markers.Count == 0)
            {
                Debug.LogWarning("Nenhum marcador encontrado! Configure marcadores primeiro.");
                return;
            }
            
            Debug.Log($"Encontrados {markers.Count} marcadores para ajustar:");
            for (int i = 0; i < markers.Count; i++)
            {
                var marker = markers[i];
                Debug.Log($"Marcador {i + 1}: {marker.markerName} - Posição: {marker.worldPositionX}");
            }
            
            // Select the MarkerSystem in the hierarchy
            Selection.activeGameObject = markerSystem.gameObject;
            EditorGUIUtility.PingObject(markerSystem.gameObject);
            
            Debug.Log("Selecione o MarkerSystem no Inspector para ajustar posições!");
        }
        
        private void CreateExampleContentInEditor()
        {
            Debug.Log("=== CRIANDO CONTEÚDOS DE EXEMPLO NO EDITOR ===");
            
            var canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Canvas não encontrado!");
                return;
            }
            
            // Create example contents
            CreateContentInEditor(0, Color.green, canvas);
            CreateContentInEditor(1, Color.blue, canvas);
            CreateContentInEditor(2, Color.red, canvas);
            
            Debug.Log("3 conteúdos de exemplo criados!");
        }
        
        private void TestConfigurationInEditor()
        {
            Debug.Log("=== TESTANDO CONFIGURAÇÃO NO EDITOR ===");
            
            var markerSystem = FindObjectOfType<MarkerSystem>();
            if (markerSystem == null)
            {
                Debug.LogError("MarkerSystem não encontrado!");
                return;
            }
            
            var markers = markerSystem.GetMarkers();
            Debug.Log($"Testando {markers.Count} marcadores:");
            
            for (int i = 0; i < markers.Count; i++)
            {
                var marker = markers[i];
                Debug.Log($"✅ Marcador {i + 1}: {marker.markerName}");
                Debug.Log($"   Posição: {marker.worldPositionX}");
                Debug.Log($"   Raio: {marker.activationRadius}");
                Debug.Log($"   Conteúdo: {(marker.contentToActivate != null ? marker.contentToActivate.name : "Nenhum")}");
            }
            
            Debug.Log("Configuração testada! Execute o jogo para testar funcionalidade.");
        }
        
        #endregion
        
        #region Editor Movement Tools
        
        private void OnSceneGUI()
        {
            if (wizard == null) return;
            
            // Draw wizard info in scene view
            Handles.BeginGUI();
            
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== WIZARD TRILHO ===");
            
            if (GUILayout.Button("Iniciar Wizard"))
            {
                wizard.StartWizard();
            }
            
            if (GUILayout.Button("Configurar no Editor"))
            {
                SetupMarkersInEditor();
            }
            
            GUILayout.Label("Use as teclas 1-4 para navegar");
            GUILayout.Label("Use as setas para mover colisores");
            
            GUILayout.EndArea();
            
            Handles.EndGUI();
            
            // Editor movement tools
            DrawEditorMovementTools();
        }
        
        private void DrawEditorMovementTools()
        {
            // Find border colliders
            var borderColliders = GameObject.Find("Colisores das Bordas");
            if (borderColliders == null) return;
            
            var leftCollider = borderColliders.transform.Find("Colisor Esquerdo");
            var rightCollider = borderColliders.transform.Find("Colisor Direito");
            
            if (leftCollider == null || rightCollider == null) return;
            
            // Draw position handles
            EditorGUI.BeginChangeCheck();
            
            Vector3 leftPos = leftCollider.position;
            Vector3 rightPos = rightCollider.position;
            
            // Left collider handle
            Handles.color = Color.red;
            leftPos = Handles.PositionHandle(leftPos, Quaternion.identity);
            
            // Right collider handle
            Handles.color = Color.red;
            rightPos = Handles.PositionHandle(rightPos, Quaternion.identity);
            
            // Draw position labels
            Handles.Label(leftPos + Vector3.up * 50, $"Esquerdo: {leftPos.x:F1}");
            Handles.Label(rightPos + Vector3.up * 50, $"Direito: {rightPos.x:F1}");
            
            // Update positions if changed
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(leftCollider, "Move Left Collider");
                Undo.RecordObject(rightCollider, "Move Right Collider");
                
                leftCollider.position = leftPos;
                rightCollider.position = rightPos;
                
                // Log position changes
                Debug.Log($"[EDITOR] Colisor Esquerdo: {leftPos.x:F1}");
                Debug.Log($"[EDITOR] Colisor Direito: {rightPos.x:F1}");
            }
            
            // Draw distance between colliders
            float distance = Mathf.Abs(rightPos.x - leftPos.x);
            Vector3 centerPos = (leftPos + rightPos) * 0.5f;
            Handles.color = Color.yellow;
            Handles.Label(centerPos + Vector3.up * 100, $"Distância: {distance:F1}");
            
            // Draw line between colliders
            Handles.color = Color.yellow;
            Handles.DrawLine(leftPos, rightPos);
        }
        
        #endregion
    }
}
