using UnityEngine;

namespace Trilho.Examples
{
    public class TrilhoQuickSetup : MonoBehaviour
    {
        [Header("Setup Components")]
        [SerializeField] private TrilhoSetupWizard wizard;
        [SerializeField] private MarkerSystem markerSystem;
        [SerializeField] private Canvas targetCanvas;
        
        [Header("Quick Setup")]
        [SerializeField] private bool autoSetupOnStart = true;
        [SerializeField] private bool showInstructions = true;
        
        private void Start()
        {
            if (autoSetupOnStart)
            {
                QuickSetup();
            }
        }
        
        [ContextMenu("Configuração Rápida")]
        public void QuickSetup()
        {
            Debug.Log("=== CONFIGURAÇÃO RÁPIDA TRILHO ===");
            
            // Step 1: Create Wizard
            CreateWizard();
            
            // Step 2: Show Instructions
            if (showInstructions)
            {
                ShowSetupInstructions();
            }
            
            // Step 3: Start Wizard
            StartWizard();
            
            Debug.Log("=== CONFIGURAÇÃO INICIADA ===");
        }
        
        private void CreateWizard()
        {
            // Find or create wizard
            if (wizard == null)
            {
                wizard = FindObjectOfType<TrilhoSetupWizard>();
                if (wizard == null)
                {
                    Debug.Log("Criando TrilhoSetupWizard...");
                    var wizardObj = new GameObject("TrilhoSetupWizard");
                    wizard = wizardObj.AddComponent<TrilhoSetupWizard>();
                }
            }
            
            // Find or create MarkerSystem
            if (markerSystem == null)
            {
                markerSystem = FindObjectOfType<MarkerSystem>();
                if (markerSystem == null)
                {
                    Debug.Log("Criando MarkerSystem...");
                    var markerSystemObj = new GameObject("MarkerSystem");
                    markerSystem = markerSystemObj.AddComponent<MarkerSystem>();
                }
            }
            
            // Find Canvas
            if (targetCanvas == null)
            {
                targetCanvas = FindObjectOfType<Canvas>();
                if (targetCanvas == null)
                {
                    Debug.LogWarning("Canvas não encontrado! Crie um Canvas primeiro.");
                }
            }
            
            Debug.Log("Componentes encontrados/criados com sucesso!");
        }
        
        private void ShowSetupInstructions()
        {
            Debug.Log("=== INSTRUÇÕES DE CONFIGURAÇÃO ===");
            Debug.Log("");
            Debug.Log("1. O wizard aparecerá na tela");
            Debug.Log("2. Siga os 4 passos:");
            Debug.Log("   Passo 1: Configure as bordas das TVs");
            Debug.Log("   Passo 2: Posicione os conteúdos");
            Debug.Log("   Passo 3: Configure os marcadores");
            Debug.Log("   Passo 4: Teste o sistema");
            Debug.Log("");
            Debug.Log("3. Controles:");
            Debug.Log("   - Setas: Mover colisores (Passo 1)");
            Debug.Log("   - Mouse: Clicar para posicionar (Passo 2)");
            Debug.Log("   - ENTER: Próximo passo");
            Debug.Log("   - ESC: Sair do modo configuração");
            Debug.Log("");
            Debug.Log("4. Após configurar, teste movendo a câmera");
            Debug.Log("================================");
        }
        
        private void StartWizard()
        {
            if (wizard != null)
            {
                wizard.StartWizard();
            }
            else
            {
                Debug.LogError("Wizard não encontrado! Execute 'Configuração Rápida' primeiro.");
            }
        }
        
        [ContextMenu("Mostrar Status")]
        public void ShowStatus()
        {
            Debug.Log("=== STATUS DA CONFIGURAÇÃO ===");
            
            if (wizard != null)
                Debug.Log("✅ TrilhoSetupWizard: Encontrado");
            else
                Debug.Log("❌ TrilhoSetupWizard: Não encontrado");
                
            if (markerSystem != null)
                Debug.Log("✅ MarkerSystem: Encontrado");
            else
                Debug.Log("❌ MarkerSystem: Não encontrado");
                
            if (targetCanvas != null)
                Debug.Log("✅ Canvas: Encontrado");
            else
                Debug.Log("❌ Canvas: Não encontrado");
                
            Debug.Log("=============================");
        }
        
        [ContextMenu("Reiniciar Configuração")]
        public void RestartSetup()
        {
            Debug.Log("Reiniciando configuração...");
            QuickSetup();
        }
        
        [ContextMenu("Limpar Configuração")]
        public void ClearSetup()
        {
            Debug.Log("=== LIMPANDO CONFIGURAÇÃO ===");
            
            // Clear wizard
            if (wizard != null)
            {
                wizard.ExitConfigurationMode();
            }
            
            // Clear marker system
            if (markerSystem != null)
            {
                markerSystem.LimparMarcadores();
            }
            
            Debug.Log("Configuração limpa!");
        }
        
        private void OnGUI()
        {
            // Simple UI for quick setup
            GUILayout.BeginArea(new Rect(10, 10, 350, 250));
            GUILayout.Label("=== CONFIGURAÇÃO RÁPIDA TRILHO ===");
            
            if (GUILayout.Button("🚀 Configuração Rápida", GUILayout.Height(30)))
            {
                QuickSetup();
            }
            
            if (GUILayout.Button("📋 Mostrar Status", GUILayout.Height(30)))
            {
                ShowStatus();
            }
            
            if (GUILayout.Button("🔄 Reiniciar", GUILayout.Height(30)))
            {
                RestartSetup();
            }
            
            if (GUILayout.Button("🗑️ Limpar", GUILayout.Height(30)))
            {
                ClearSetup();
            }
            
            GUILayout.Space(10);
            GUILayout.Label("Instruções:");
            GUILayout.Label("1. Clique 'Configuração Rápida'");
            GUILayout.Label("2. Siga os 4 passos do wizard");
            GUILayout.Label("3. Use as setas para mover colisores");
            GUILayout.Label("4. Clique onde quer os conteúdos");
            GUILayout.Label("5. Teste movendo a câmera");
            
            GUILayout.EndArea();
        }
    }
}
