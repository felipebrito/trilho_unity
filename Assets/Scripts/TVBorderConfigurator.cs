using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Trilho
{
    public class TVBorderConfigurator : MonoBehaviour
    {
        [Header("Configuração de Bordas")]
        [Tooltip("Transform da BORDA ESQUERDA (criado automaticamente se vazio)")]
        [SerializeField] private Transform leftBorder;
        [Tooltip("Transform da BORDA DIREITA (criado automaticamente se vazio)")]
        [SerializeField] private Transform rightBorder;
        [Tooltip("Altura visual dos marcadores de borda (apenas debug)")]
        [SerializeField] private float borderHeight = 1000f;
        [Tooltip("Espessura visual dos marcadores de borda (apenas debug)")]
        [SerializeField] private float borderThickness = 10f;
        [Tooltip("Cor dos marcadores de borda")]
        [SerializeField] private Color borderColor = Color.red;
        
        [Header("Persistência")]
        [Tooltip("Carregar automaticamente as bordas salvas no início")]
        [SerializeField] private bool loadFromPrefsOnStart = true;
        private string prefsLeftKey;
        private string prefsRightKey;
        
        [Header("Valores Iniciais (opcional)")]
        [Tooltip("Posição X inicial da borda ESQUERDA (usado se não houver PlayerPrefs)")]
        [SerializeField] private float initialLeftX = 0f;
        [Tooltip("Posição X inicial da borda DIREITA (usado se não houver PlayerPrefs)")]
        [SerializeField] private float initialRightX = 2000f;
        [Tooltip("Aplicar os valores iniciais no Start se não houver dados salvos")]
        [SerializeField] private bool applyInitialOnStart = true;
        
        [Header("Entrada / Runtime")]
        [Tooltip("Quando ativo, permite mover as bordas com o teclado (F1 alterna)")]
        [SerializeField] private bool configurationMode = true;
        [Tooltip("Passo de movimento (rápido)")]
        [SerializeField] private float moveStep = 300f;
        [Tooltip("Passo de movimento (fino) ao segurar Shift")]
        [SerializeField] private float fineStep = 30f;
        [Tooltip("Exibir sobreposição de ajuda na tela")]
        [SerializeField] private bool showOverlay = true;

        [Header("Integração Trilho")]
        [Tooltip("Referência ao TrilhoGameManager (detectado automaticamente se vazio)")]
        [SerializeField] private TrilhoGameManager trilho; // opcional; será encontrado automaticamente se vazio
        [Tooltip("Usar largura manual ao sincronizar com o Trilho")]
        [SerializeField] private bool useManualWidth = false;
        [Tooltip("Largura manual em centímetros (override)")]
        [SerializeField] private float manualWidthCm = 75.69f;
        
        private void Awake()
        {
            BuildPrefsKeys();
        }
        
        private void Start()
        {
            EnsureBorders();
            
            // 1) Tentar carregar do PlayerPrefs
            bool loadedFromPrefs = false;
            if (loadFromPrefsOnStart)
            {
                loadedFromPrefs = TryLoadBordersFromPrefs();
            }
            
            // 2) Se não carregou de prefs, aplicar iniciais
            if (!loadedFromPrefs && applyInitialOnStart)
            {
                SetBorderX(leftBorder, initialLeftX);
                SetBorderX(rightBorder, initialRightX);
            }
            
            // 3) Sincroniza a largura calculada/definida com o Trilho
            SyncWidthToTrilho();
        }
        
        private void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            
            // Atalho para ligar/desligar modo
            if (k.f1Key.wasPressedThisFrame) configurationMode = !configurationMode;
            
            if (configurationMode) HandleKeyboard(k);
        }
        
        private void HandleKeyboard(Keyboard k)
        {
            float step = (k.leftShiftKey.isPressed || k.rightShiftKey.isPressed) ? fineStep : moveStep;
            if (leftBorder == null || rightBorder == null) return;
            
            // Movimento com teclas Q/W para borda esquerda
            if (k.qKey.isPressed) MoveBorder(leftBorder, -step * Time.deltaTime);
            if (k.wKey.isPressed) MoveBorder(leftBorder, +step * Time.deltaTime);
            
            // Movimento com teclas O/P para borda direita
            if (k.oKey.isPressed) MoveBorder(rightBorder, -step * Time.deltaTime);
            if (k.pKey.isPressed) MoveBorder(rightBorder, +step * Time.deltaTime);
            
            if (k.enterKey.wasPressedThisFrame) SaveBorders();
            if (k.escapeKey.wasPressedThisFrame) configurationMode = false;
        }
        
        private void MoveBorder(Transform border, float deltaX)
        {
            Vector3 p = border.position;
            p.x += deltaX;
            border.position = p;
        }
        
        private void SetBorderX(Transform border, float x)
        {
            Vector3 p = border.position;
            p.x = x;
            border.position = p;
        }
        
        private void EnsureBorders()
        {
            if (leftBorder == null)
            {
                leftBorder = CreateBorderObject("LeftBorder");
                leftBorder.position = new Vector3(0f, 0f, 0f);
            }
            if (rightBorder == null)
            {
                rightBorder = CreateBorderObject("RightBorder");
                rightBorder.position = new Vector3(2000f, 0f, 0f);
            }
        }
        
        private Transform CreateBorderObject(string name)
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.SetParent(transform);
            go.transform.localScale = new Vector3(borderThickness, borderHeight, borderThickness);
            var renderer = go.GetComponent<Renderer>();
            if (renderer != null) renderer.material.color = borderColor;
            return go.transform;
        }
        
        [ContextMenu("Salvar Bordas")]
        public void SaveBorders()
        {
            if (leftBorder == null || rightBorder == null) return;
            float savedLeftX = leftBorder.position.x;
            float savedRightX = rightBorder.position.x;
            
            // Persistir
            PlayerPrefs.SetFloat(prefsLeftKey, savedLeftX);
            PlayerPrefs.SetFloat(prefsRightKey, savedRightX);
            PlayerPrefs.Save();
            
            #if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
            #endif
            Debug.Log($"[BORDAS] Salvas: Esquerda={savedLeftX:F1} | Direita={savedRightX:F1} | Largura={Mathf.Abs(savedRightX-savedLeftX):F1}");

            // Atualiza largura no Trilho
            SyncWidthToTrilho();
        }
        
        [ContextMenu("Limpar Bordas Salvas (Prefs)")]
        public void ClearSavedPrefs()
        {
            PlayerPrefs.DeleteKey(prefsLeftKey);
            PlayerPrefs.DeleteKey(prefsRightKey);
            PlayerPrefs.Save();
            Debug.Log("[BORDAS] PlayerPrefs limpos para este configurador.");
        }
        
        [ContextMenu("Sincronizar Largura com Trilho")]
        public void SyncWidthToTrilho()
        {
            if (leftBorder == null || rightBorder == null) return;
            if (trilho == null) trilho = FindObjectOfType<TrilhoGameManager>();
            if (trilho == null) return;

            float widthCm;
            if (useManualWidth)
            {
                widthCm = Mathf.Max(0f, manualWidthCm);
            }
            else
            {
                float leftUnity = leftBorder.position.x;
                float rightUnity = rightBorder.position.x;
                float leftCm = trilho.UnityToCm(leftUnity);
                float rightCm = trilho.UnityToCm(rightUnity);
                widthCm = Mathf.Abs(rightCm - leftCm);
            }
            trilho.SetScreenWidthCm(widthCm);
            Debug.Log($"[BORDAS] Largura sincronizada com Trilho: {widthCm:F2}cm");
        }
        
        public float GetWidth()
        {
            if (leftBorder == null || rightBorder == null) return 0f;
            return Mathf.Abs(rightBorder.position.x - leftBorder.position.x);
        }
        
        private void BuildPrefsKeys()
        {
            string scene = SceneManager.GetActiveScene().name;
            string prefix = $"{Application.productName}:{scene}:{gameObject.name}:TVBorders";
            prefsLeftKey = prefix + ":LeftX";
            prefsRightKey = prefix + ":RightX";
        }
        
        private bool TryLoadBordersFromPrefs()
        {
            if (PlayerPrefs.HasKey(prefsLeftKey) && PlayerPrefs.HasKey(prefsRightKey))
            {
                float lx = PlayerPrefs.GetFloat(prefsLeftKey);
                float rx = PlayerPrefs.GetFloat(prefsRightKey);
                EnsureBorders();
                SetBorderX(leftBorder, lx);
                SetBorderX(rightBorder, rx);
                // Assim que carregar, sincroniza a largura
                SyncWidthToTrilho();
                return true;
            }
            return false;
        }
        
        private void OnDrawGizmos()
        {
            if (leftBorder == null || rightBorder == null) return;
            Gizmos.color = Color.yellow;
            Vector3 a = leftBorder.position;
            Vector3 b = rightBorder.position;
            Gizmos.DrawLine(a + Vector3.up * borderHeight * 0.5f, a - Vector3.up * borderHeight * 0.5f);
            Gizmos.DrawLine(b + Vector3.up * borderHeight * 0.5f, b - Vector3.up * borderHeight * 0.5f);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(a, b);
        }
        
        private void OnGUI()
        {
            if (!showOverlay) return;
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
            GUILayout.BeginArea(new Rect(10, 10, 700, 260), GUI.skin.box);
            GUILayout.Label("Configuração de Bordas (Runtime)");
            GUILayout.Label($"Esquerda X: {leftBorder?.position.x:F1} | Direita X: {rightBorder?.position.x:F1} | Largura (Unity): {GetWidth():F1}");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("← E", GUILayout.Width(60))) MoveBorder(leftBorder, -moveStep * Time.deltaTime * 50f);
            if (GUILayout.Button("E →", GUILayout.Width(60))) MoveBorder(leftBorder, +moveStep * Time.deltaTime * 50f);
            GUILayout.Space(20);
            if (GUILayout.Button("← D", GUILayout.Width(60))) MoveBorder(rightBorder, -moveStep * Time.deltaTime * 50f);
            if (GUILayout.Button("D →", GUILayout.Width(60))) MoveBorder(rightBorder, +moveStep * Time.deltaTime * 50f);
            GUILayout.EndHorizontal();
            
            configurationMode = GUILayout.Toggle(configurationMode, configurationMode ? "Modo Configuração ATIVO (F1)" : "Modo Configuração INATIVO (F1)");
            GUILayout.Label("Controles: Q/W move ESQUERDA · O/P move DIREITA (Shift = fino) · Enter salva · ESC sai");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Salvar (Enter)")) SaveBorders();
            if (GUILayout.Button("Enviar Largura ao Trilho")) SyncWidthToTrilho();
            if (GUILayout.Button("Limpar Prefs")) ClearSavedPrefs();
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            useManualWidth = GUILayout.Toggle(useManualWidth, "Usar largura manual (cm)");
            GUILayout.BeginHorizontal();
            GUILayout.Label("Largura manual:", GUILayout.Width(120));
            string manualStr = GUILayout.TextField(manualWidthCm.ToString("F2"), GUILayout.Width(80));
            if (float.TryParse(manualStr, out var parsed)) manualWidthCm = parsed;
            if (GUILayout.Button("Aplicar ao Trilho", GUILayout.Width(150))) SyncWidthToTrilho();
            GUILayout.EndHorizontal();
            
            GUILayout.EndArea();
        }
    }
}
