using UnityEngine;

namespace Trilho.Examples
{
    public class TrilhoWizardExample : MonoBehaviour
    {
        [Header("Wizard Reference")]
        [SerializeField] private TrilhoSetupWizard wizard;
        
        [Header("Example Settings")]
        [SerializeField] private bool autoStartWizard = true;
        [SerializeField] private bool showInstructions = true;
        
        private void Start()
        {
            if (autoStartWizard)
            {
                StartWizardExample();
            }
        }
        
        [ContextMenu("Iniciar Exemplo do Wizard")]
        public void StartWizardExample()
        {
            Debug.Log("=== EXEMPLO DO WIZARD TRILHO ===");
            
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
            
            // Show instructions
            if (showInstructions)
            {
                ShowInstructions();
            }
            
            // Start wizard
            wizard.StartWizard();
            
            Debug.Log("=== EXEMPLO INICIADO ===");
        }
        
        private void ShowInstructions()
        {
            Debug.Log("=== INSTRUÇÕES DO WIZARD ===");
            Debug.Log("1. O wizard aparecerá na tela");
            Debug.Log("2. Siga os passos na ordem:");
            Debug.Log("   - Passo 1: Configure as bordas das TVs");
            Debug.Log("   - Passo 2: Posicione os conteúdos");
            Debug.Log("   - Passo 3: Configure os marcadores");
            Debug.Log("   - Passo 4: Teste o sistema");
            Debug.Log("3. Use as teclas 1-4 para navegar");
            Debug.Log("4. Use as setas para mover colisores");
            Debug.Log("5. Pressione ENTER para próximo passo");
            Debug.Log("===============================");
        }
        
        [ContextMenu("Mostrar Controles")]
        public void ShowControls()
        {
            Debug.Log("=== CONTROLES DO WIZARD ===");
            Debug.Log("Teclas 1-4: Navegar entre passos");
            Debug.Log("Setas: Mover colisores (Passo 1)");
            Debug.Log("ENTER: Próximo passo");
            Debug.Log("BACKSPACE: Passo anterior");
            Debug.Log("Mouse: Clicar em marcadores (Passo 2)");
            Debug.Log("===========================");
        }
        
        [ContextMenu("Reiniciar Wizard")]
        public void RestartWizard()
        {
            if (wizard != null)
            {
                wizard.StartWizard();
                Debug.Log("Wizard reiniciado!");
            }
        }
        
        [ContextMenu("Finalizar Wizard")]
        public void FinishWizard()
        {
            if (wizard != null)
            {
                wizard.FinishWizard();
                Debug.Log("Wizard finalizado!");
            }
        }
        
        private void OnGUI()
        {
            // Simple UI for testing
            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label("=== WIZARD TRILHO - EXEMPLO ===");
            
            if (GUILayout.Button("🚀 Iniciar Wizard"))
            {
                StartWizardExample();
            }
            
            if (GUILayout.Button("📋 Mostrar Controles"))
            {
                ShowControls();
            }
            
            if (GUILayout.Button("🔄 Reiniciar"))
            {
                RestartWizard();
            }
            
            if (GUILayout.Button("✅ Finalizar"))
            {
                FinishWizard();
            }
            
            GUILayout.Label("Use as teclas 1-4 para navegar");
            GUILayout.Label("Use as setas para mover colisores");
            
            GUILayout.EndArea();
        }
    }
}
