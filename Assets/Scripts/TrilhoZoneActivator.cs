using UnityEngine;
using System.Collections.Generic;

namespace Trilho
{
    public class TrilhoZoneActivator : MonoBehaviour
    {
        public enum PositionReference
        {
            LeftEdge,
            Center
        }
        
        [System.Serializable]
        public class Zone
        {
            public string name = "Zona";
            public float positionCm; // início da zona (gizmo amarelo, borda esquerda da zona)
            public float paddingCm = 0f; // compat
            public GameObject content;
            [HideInInspector] public bool isActive;

            [Header("Posicionamento (opcional)")]
            public bool placeContentAtWorldX = false;
            public float contentOffsetCm = 0f;
            public bool keepUpdatingWhileActive = false;
            public PositionReference reference = PositionReference.LeftEdge;

            [Header("Histerese (opcional)")]
            public float enterPaddingCm = 0f;
            public float exitPaddingCm = 0f;
        }
        
        [Header("Referências")]
        [SerializeField] private TrilhoGameManager trilho;
        
        [Header("Zonas (em cm)")]
        [SerializeField] private List<Zone> zones = new List<Zone>();
        
        [Header("Opções")]
        [SerializeField] private bool hideAllOnStart = true;
        [SerializeField] private bool showDebugLogs = false;
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color windowColor = new Color(0, 1, 1, 0.25f);
        [SerializeField] private Color zoneLineColor = Color.yellow;
        [SerializeField] private Color activeZoneColor = Color.green;
        [SerializeField] private Color zoneAreaColor = new Color(0.5f, 0.8f, 1f, 0.15f);
        [SerializeField] private bool drawGameOverlay = true;
        [SerializeField] private Color overlayTextColor = Color.white;
        [SerializeField] private Color overlayWindowEdgeColor = Color.cyan;
        
        [Header("Fade (CanvasGroup)")]
        [SerializeField] private float fadeInSeconds = 0.35f;
        [SerializeField] private float fadeOutSeconds = 0.35f;
        
        private readonly Dictionary<GameObject, Coroutine> _fadeCoroutines = new Dictionary<GameObject, Coroutine>();
        private float _lastCamCm;
        private int _dir; // -1 = esquerda, 0 = parado, 1 = direita
        
        private void Awake()
        {
            if (trilho == null) trilho = FindObjectOfType<TrilhoGameManager>();
        }
        
        private void Start()
        {
            if (hideAllOnStart)
            {
                HideAll();
            }
            _lastCamCm = trilho != null ? trilho.GetCurrentPositionCm() : 0f;
        }
        
        private void Update()
        {
            if (trilho == null) return;
            float camCm = trilho.GetCurrentPositionCm();
            float delta = camCm - _lastCamCm;
            _dir = Mathf.Abs(delta) < 0.0001f ? 0 : (delta > 0f ? 1 : -1);
            _lastCamCm = camCm;
            float widthCm = Mathf.Max(0f, trilho.GetScreenWidthCm());
            float half = widthCm * 0.5f;
            float leftEdge = camCm - half;
            float rightEdge = camCm + half;
            
            foreach (var z in zones)
            {
                if (z == null) continue;

                float enterPad = z.enterPaddingCm > 0f ? z.enterPaddingCm : z.paddingCm;
                float exitPad = z.exitPaddingCm > 0f ? z.exitPaddingCm : z.paddingCm;

                // Zona é um intervalo: [positionCm, positionCm + larguraTV]
                float zoneStart = z.positionCm;
                float zoneEnd = z.positionCm + widthCm;
                
                // Sobreposição para ENTRAR: janelas se interceptam
                bool overlapForEnter = rightEdge >= (zoneStart - enterPad) && leftEdge <= (zoneEnd + enterPad);
                bool shouldBeActive = z.isActive ? true : overlapForEnter;

                // Histerese para SAIR: depende da direção
                if (z.isActive)
                {
                    if (_dir >= 0)
                    {
                        // Movendo direita: desliga quando a ESQUERDA ultrapassa o fim da zona
                        if (leftEdge > zoneEnd + exitPad) shouldBeActive = false;
                    }
                    else if (_dir < 0)
                    {
                        // Movendo esquerda: desliga quando a DIREITA passa antes do início da zona
                        if (rightEdge < zoneStart - exitPad) shouldBeActive = false;
                    }
                }
                
                if (shouldBeActive != z.isActive)
                {
                    if (shouldBeActive)
                    {
                        if (z.placeContentAtWorldX) PlaceContentAtZoneWorldX(z, widthCm);
                        Fade(z.content, 1f, fadeInSeconds, true);
                    }
                    else
                    {
                        Fade(z.content, 0f, fadeOutSeconds, false);
                    }
                    z.isActive = shouldBeActive;
                    if (showDebugLogs)
                        UnityEngine.Debug.Log($"[ZONES] {(shouldBeActive?"ON":"OFF")} {z.name} @ {z.positionCm}cm | left={leftEdge:F1} right={rightEdge:F1} zone=[{zoneStart:F1},{zoneEnd:F1}] w={widthCm:F1} dir={_dir} enterPad={enterPad} exitPad={exitPad}");
                }
                else if (shouldBeActive && z.isActive && z.placeContentAtWorldX && z.keepUpdatingWhileActive)
                {
                    PlaceContentAtZoneWorldX(z, widthCm);
                }
            }
        }

        private void PlaceContentAtZoneWorldX(Zone z, float screenWidthCm)
        {
            if (z?.content == null || trilho == null) return;
            float baseCm = z.reference == PositionReference.LeftEdge
                ? z.positionCm + screenWidthCm * 0.5f
                : z.positionCm;
            float targetCm = baseCm + z.contentOffsetCm;
            float xUnity = trilho.CmToUnity(targetCm);
            var t = z.content.transform;
            var p = t.position;
            p.x = xUnity;
            t.position = p;
        }
        
        private void Fade(GameObject go, float target, float duration, bool setActiveBefore)
        {
            if (go == null) return;
            if (!_fadeCoroutines.ContainsKey(go)) _fadeCoroutines[go] = null;
            if (_fadeCoroutines[go] != null) StopCoroutine(_fadeCoroutines[go]);
            _fadeCoroutines[go] = StartCoroutine(FadeRoutine(go, target, duration, setActiveBefore));
        }
        
        private System.Collections.IEnumerator FadeRoutine(GameObject go, float target, float duration, bool setActiveBefore)
        {
            var cg = go.GetComponent<CanvasGroup>();
            if (cg == null) cg = go.AddComponent<CanvasGroup>();
            if (setActiveBefore) go.SetActive(true);
            float start = cg.alpha;
            float t = 0f;
            duration = Mathf.Max(0.001f, duration);
            while (t < 1f)
            {
                t += Time.deltaTime / duration;
                cg.alpha = Mathf.Lerp(start, target, t);
                yield return null;
            }
            cg.alpha = target;
            if (!setActiveBefore) go.SetActive(false);
        }
        
        private void OnDrawGizmos()
        {
            if (!drawGizmos) return;
            if (trilho == null) return;
            
            float camCm = trilho.GetCurrentPositionCm();
            float widthCm = Mathf.Max(0f, trilho.GetScreenWidthCm());
            float half = widthCm * 0.5f;
            float startCm = camCm - half;
            float endCm = camCm + half;
            
            float startX = trilho.CmToUnity(startCm);
            float endX = trilho.CmToUnity(endCm);
            
            // Janela
            Gizmos.color = windowColor;
            Vector3 center = new Vector3((startX + endX) * 0.5f, 0f, 0f);
            Vector3 size = new Vector3(Mathf.Abs(endX - startX), 2000f, 1f);
            Gizmos.DrawCube(center, size);
            
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(new Vector3(startX, -1000f, 0f), new Vector3(startX, 1000f, 0f));
            Gizmos.DrawLine(new Vector3(endX, -1000f, 0f), new Vector3(endX, 1000f, 0f));
            
            // Zonas: retângulo da largura da TV começando em positionCm
            foreach (var z in zones)
            {
                if (z == null) continue;
                float zx0 = trilho.CmToUnity(z.positionCm);
                float zx1 = trilho.CmToUnity(z.positionCm + widthCm);
                Gizmos.color = zoneAreaColor;
                Vector3 zc = new Vector3((zx0 + zx1) * 0.5f, 0f, 0f);
                Vector3 zs = new Vector3(Mathf.Abs(zx1 - zx0), 1800f, 1f);
                Gizmos.DrawCube(zc, zs);
                
                // linhas de borda esquerda e direita da zona
                Gizmos.color = z.isActive ? activeZoneColor : zoneLineColor;
                Gizmos.DrawLine(new Vector3(zx0, -1200f, 0f), new Vector3(zx0, 1200f, 0f));
                Gizmos.DrawLine(new Vector3(zx1, -1200f, 0f), new Vector3(zx1, 1200f, 0f));
            }
        }
        
        private static Texture2D _lineTex;
        private static void DrawRect(Rect r, Color c)
        {
            if (_lineTex == null)
            {
                _lineTex = new Texture2D(1, 1);
                _lineTex.SetPixel(0, 0, Color.white);
                _lineTex.Apply();
            }
            var old = GUI.color;
            GUI.color = c;
            GUI.DrawTexture(r, _lineTex);
            GUI.color = old;
        }
        
        private void OnGUI()
        {
            if (!drawGameOverlay) return;
            if (trilho == null) return;
            
            float camCm = trilho.GetCurrentPositionCm();
            float widthCm = Mathf.Max(0f, trilho.GetScreenWidthCm());
            float half = widthCm * 0.5f;
            float start = camCm - half;
            float end   = camCm + half;
            
            DrawRect(new Rect(0, 0, 2, Screen.height), overlayWindowEdgeColor);
            DrawRect(new Rect(Screen.width - 2, 0, 2, Screen.height), overlayWindowEdgeColor);
            
            // Zonas como faixas na Game view
            foreach (var z in zones)
            {
                if (z == null) continue;
                float zs = z.positionCm;
                float ze = z.positionCm + widthCm;
                float ts = Mathf.InverseLerp(start, end, zs);
                float te = Mathf.InverseLerp(start, end, ze);
                if (te >= 0f && ts <= 1f)
                {
                    float xs = Mathf.Clamp01(ts) * Screen.width;
                    float xe = Mathf.Clamp01(te) * Screen.width;
                    DrawRect(new Rect(xs, 0, Mathf.Max(2f, xe - xs), Screen.height), new Color(zoneAreaColor.r, zoneAreaColor.g, zoneAreaColor.b, 0.25f));
                }
                // Linhas das bordas esquerda e direita da zona
                float tLeft = Mathf.InverseLerp(start, end, zs);
                if (tLeft >= 0f && tLeft <= 1f)
                {
                    float xL = tLeft * Screen.width;
                    DrawRect(new Rect(xL - 1, 0, 2, Screen.height), z.isActive ? activeZoneColor : zoneLineColor);
                }
                float tRight = Mathf.InverseLerp(start, end, ze);
                if (tRight >= 0f && tRight <= 1f)
                {
                    float xR = tRight * Screen.width;
                    DrawRect(new Rect(xR - 1, 0, 2, Screen.height), z.isActive ? activeZoneColor : zoneLineColor);
                }
            }
            
            var old = GUI.color;
            GUI.color = new Color(0, 0, 0, 0.6f);
            GUILayout.BeginArea(new Rect(10, 10, 460, 180), GUI.skin.box);
            GUI.color = overlayTextColor;
            GUILayout.Label($"Janela (cm): {start:F1} — {end:F1}  |  Largura: {widthCm:F1}");
            GUILayout.Label($"Câmera: {camCm:F1}cm");
            foreach (var z in zones)
            {
                if (z == null) continue;
                string st = z.isActive ? "ON" : "off";
                float zoneStart = z.positionCm;
                float zoneEnd = z.positionCm + widthCm;
                GUILayout.Label($"{z.name}: zona [{zoneStart:F1},{zoneEnd:F1}]cm [{st}]");
            }
            GUILayout.EndArea();
            GUI.color = old;
        }
        
        [ContextMenu("Ocultar Todos")]
        public void HideAll()
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
    }
}
