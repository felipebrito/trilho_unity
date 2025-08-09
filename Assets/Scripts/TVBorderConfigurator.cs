using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Trilho
{
    public class TVBorderConfigurator : MonoBehaviour
    {
        [Header("Configuração de Bordas")]
        [SerializeField] private Transform leftBorder;
        [SerializeField] private Transform rightBorder;
        [SerializeField] private float borderHeight = 1000f;
        [SerializeField] private float borderThickness = 10f;
        [SerializeField] private Color borderColor = Color.red;
        
        [Header("Persistência")]
        [SerializeField] private bool loadFromPrefsOnStart = true;
        private string prefsLeftKey;
        private string prefsRightKey;
        
        // Follow removido para manter papel do script apenas como calibrador
        
        [Header("Valores Iniciais (opcional)")]
        [SerializeField] private float initialLeftX = 0f;
        [SerializeField] private float initialRightX = 2000f;
        [SerializeField] private bool applyInitialOnStart = true;
        
        [Header("Zonas/Conteúdos")]
        [SerializeField] private List<SimpleZone> zones = new List<SimpleZone>();
        [SerializeField] private float activationPadding = 0f; // margem adicional nas bordas
        [SerializeField] private bool hideZoneContentsOnStart = true;
        
        [System.Serializable]
        public class SimpleZone
        {
            public string zoneName = "Zona";
            public float positionX;
            public GameObject content; // Conteúdo a ativar quando a zona está entre as bordas
            [HideInInspector] public bool isActive;
        }
        
        [Header("Geração Automática de Zonas (Opcional)")]
        [SerializeField] private int numberOfZones = 3;
        
        [Header("Entrada / Runtime")]
        [SerializeField] private bool configurationMode = true;
        [SerializeField] private float moveStep = 300f; // passo maior para ficar visível
        [SerializeField] private float fineStep = 30f;
        [SerializeField] private bool showOverlay = true;

        [Header("Integração Trilho")]
        [SerializeField] private TrilhoGameManager trilho; // opcional; será encontrado automaticamente se vazio
        [SerializeField] private bool useManualWidth = false;
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
            
            // 3) Garantir conteúdos das zones ocultos no início, se desejado
            if (hideZoneContentsOnStart)
            {
                InitializeZonesHidden();
            }
            
            // 4) Criar exemplos apenas se nenhuma zona foi definida
            if (zones.Count == 0)
            {
                zones.Add(new SimpleZone { zoneName = "Zona 1", positionX = 1000f });
                zones.Add(new SimpleZone { zoneName = "Zona 2", positionX = 3000f });
                zones.Add(new SimpleZone { zoneName = "Zona 3", positionX = 5000f });
            }

            // Sincroniza a largura inicial com o Trilho (após carregar/aplicar)
            SyncWidthToTrilho();
        }
        
        private void Update()
        {
            var k = Keyboard.current;
            if (k == null) return;
            
            // Atalho para ligar/desligar modo
            if (k.f1Key.wasPressedThisFrame) configurationMode = !configurationMode;
            
            if (configurationMode) HandleKeyboard(k);
            
            UpdateSimpleZonesActivation();
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
            if (k.cKey.wasPressedThisFrame) ComputeZonesFromBorders();
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
        
        private void InitializeZonesHidden()
        {
            foreach (var z in zones)
            {
                if (z?.content == null) continue;
                var cg = z.content.GetComponent<CanvasGroup>();
                if (cg == null) cg = z.content.AddComponent<CanvasGroup>();
                cg.alpha = 0f;
                z.content.SetActive(false);
                z.isActive = false;
            }
        }
        
        public void ApplyInitial()
        {
            EnsureBorders();
            SetBorderX(leftBorder, initialLeftX);
            SetBorderX(rightBorder, initialRightX);
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
        
        [ContextMenu("Gerar Zonas Iguais entre as Bordas")] 
        public void ComputeZonesFromBorders()
        {
            if (leftBorder == null || rightBorder == null)
            {
                Debug.LogError("[BORDAS] Bordas não configuradas.");
                return;
            }
            
            float leftX = leftBorder.position.x;
            float rightX = rightBorder.position.x;
            if (rightX <= leftX)
            {
                Debug.LogError("[BORDAS] A borda direita deve estar à direita da borda esquerda.");
                return;
            }
            
            float width = rightX - leftX;
            float zoneWidth = width / Mathf.Max(1, numberOfZones);
            
            zones.Clear();
            for (int i = 0; i < numberOfZones; i++)
            {
                float centerX = leftX + (i + 0.5f) * zoneWidth;
                zones.Add(new SimpleZone { zoneName = $"Zona {i + 1}", positionX = centerX });
            }
            Debug.Log($"[BORDAS] {numberOfZones} zonas geradas para {width:F1} de largura.");
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
            Debug.Log($"[BORDAS] Largura sincronizada com Trilho: {widthCm:F1}cm");
        }
        
        private void UpdateSimpleZonesActivation()
        {
            if (leftBorder == null || rightBorder == null) return;
            float minX = Mathf.Min(leftBorder.position.x, rightBorder.position.x) - activationPadding;
            float maxX = Mathf.Max(leftBorder.position.x, rightBorder.position.x) + activationPadding;
            
            foreach (var zone in zones)
            {
                if (zone == null) continue;
                bool shouldBeActive = zone.positionX >= minX && zone.positionX <= maxX;
                if (shouldBeActive != zone.isActive)
                {
                    if (shouldBeActive) ActivateContent(zone.content); else DeactivateContent(zone.content);
                    zone.isActive = shouldBeActive;
                }
            }
        }
        
        private void ActivateContent(GameObject content)
        {
            if (content == null) return;
            var cg = content.GetComponent<CanvasGroup>();
            if (cg == null) cg = content.AddComponent<CanvasGroup>();
            cg.alpha = 1f;
            content.SetActive(true);
        }
        
        private void DeactivateContent(GameObject content)
        {
            if (content == null) return;
            var cg = content.GetComponent<CanvasGroup>();
            if (cg == null) cg = content.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            content.SetActive(false);
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
            
            if (zones != null)
            {
                Gizmos.color = Color.green;
                foreach (var z in zones)
                {
                    Gizmos.DrawSphere(new Vector3(z.positionX, 0f, 0f), 25f);
                }
            }
        }
        
        private void OnGUI()
        {
            if (!showOverlay) return;
            GUI.backgroundColor = new Color(0f, 0f, 0f, 0.6f);
            GUILayout.BeginArea(new Rect(10, 10, 660, 220), GUI.skin.box);
            GUILayout.Label("Configuração de Bordas (Runtime)");
            GUILayout.Label($"Esquerda X: {leftBorder?.position.x:F1} | Direita X: {rightBorder?.position.x:F1} | Largura: {GetWidth():F1}");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("← E", GUILayout.Width(60))) MoveBorder(leftBorder, -moveStep * Time.deltaTime * 50f);
            if (GUILayout.Button("E →", GUILayout.Width(60))) MoveBorder(leftBorder, +moveStep * Time.deltaTime * 50f);
            GUILayout.Space(20);
            if (GUILayout.Button("← D", GUILayout.Width(60))) MoveBorder(rightBorder, -moveStep * Time.deltaTime * 50f);
            if (GUILayout.Button("D →", GUILayout.Width(60))) MoveBorder(rightBorder, +moveStep * Time.deltaTime * 50f);
            GUILayout.EndHorizontal();
            
            configurationMode = GUILayout.Toggle(configurationMode, configurationMode ? "Modo Configuração ATIVO (F1)" : "Modo Configuração INATIVO (F1)");
            GUILayout.Label("Controles: Q/W move ESQUERDA · O/P move DIREITA (Shift = fino) · Enter salva · C gera zonas · ESC sai");
            GUILayout.Label($"Zonas: {zones?.Count ?? 0} (ativa quando posição está entre as bordas)");
            
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Salvar (Enter)")) SaveBorders();
            if (GUILayout.Button("Gerar Zonas Iguais (C)")) ComputeZonesFromBorders();
            if (GUILayout.Button("Enviar Largura ao Trilho")) SyncWidthToTrilho();
            if (GUILayout.Button("Aplicar Iniciais")) ApplyInitial();
            if (GUILayout.Button("Limpar Prefs")) ClearSavedPrefs();
            GUILayout.EndHorizontal();

            // Manual width override controls
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
