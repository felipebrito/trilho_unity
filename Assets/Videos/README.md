# 📹 Trilho Video Configuration Guide

## 🎯 Onde Colocar os Vídeos

### **Opção 1: Assets/Videos (Recomendado para Desenvolvimento)**
```
Assets/Videos/
├── example_video.mp4
├── video1.mp4
├── video2.mp4
└── README.md
```

**Caminho no código:** `"Videos/example_video.mp4"`

### **Opção 2: StreamingAssets (Recomendado para Build Final)**
```
Assets/StreamingAssets/Videos/
├── example_video.mp4
├── video1.mp4
└── video2.mp4
```

**Caminho no código:** `"StreamingAssets/Videos/example_video.mp4"`

## 🔧 Como Configurar

### **1. Para Vídeos em Assets/Videos:**
```csharp
// No TrilhoGameManager ou ActivationZone
videoPath = "Videos/example_video.mp4"
```

### **2. Para Vídeos em StreamingAssets:**
```csharp
// No TrilhoGameManager ou ActivationZone  
videoPath = "StreamingAssets/Videos/example_video.mp4"
```

## 📋 Formatos Suportados

Unity suporta os seguintes formatos de vídeo:
- **MP4** (H.264) - Recomendado
- **MOV** (H.264)
- **AVI** (limitado)
- **WebM** (limitado)

## ⚙️ Configurações Importantes

### **No Unity Inspector:**
1. **Selecione o vídeo** no Project Window
2. **No Inspector**, configure:
   - **Import Settings** → **Video**
   - **Codec**: H.264
   - **Quality**: High
   - **Target Platform**: Standalone

### **No TrilhoGameManager:**
```csharp
[Header("Video Settings")]
[SerializeField] private bool loopVideo = true;
[SerializeField] private bool playOnAwake = false;
[SerializeField] private float videoVolume = 1.0f;
```

## 🚀 Como Testar

### **1. Coloque um vídeo de teste:**
```
Assets/Videos/example_video.mp4
```

### **2. Configure a zona:**
- **Zone Name**: "Video Zone"
- **Video Path**: "Videos/example_video.mp4"
- **Start Position**: 40cm
- **End Position**: 100cm

### **3. Teste no Unity:**
- **Play Mode**
- **Mova para 40-100cm**
- **Vídeo deve aparecer**

## 🔍 Troubleshooting

### **Vídeo não aparece:**
1. ✅ Verifique se o arquivo existe
2. ✅ Verifique o caminho no código
3. ✅ Verifique se o formato é suportado
4. ✅ Verifique as configurações de import

### **Vídeo não carrega:**
1. ✅ Verifique o tamanho do arquivo
2. ✅ Verifique se o codec é H.264
3. ✅ Verifique se o caminho está correto

## 📁 Estrutura Recomendada

```
Assets/
├── Videos/
│   ├── example_video.mp4
│   ├── zone1_video.mp4
│   ├── zone2_video.mp4
│   └── README.md
├── StreamingAssets/
│   └── Videos/
│       └── final_videos/
└── Scripts/
    └── TrilhoGameManager.cs
```

## 🎬 Exemplo de Configuração

```csharp
// No TrilhoGameManager
ActivationZone videoZone = new ActivationZone
{
    zoneName = "Video Zone",
    startPositionCm = 40f,
    endPositionCm = 100f,
    contentType = ZoneContentType.Video,
    videoPath = "Videos/example_video.mp4", // ← Caminho aqui
    isActive = true
};
```

**Dica:** Use caminhos relativos sempre que possível para facilitar a portabilidade do projeto!
