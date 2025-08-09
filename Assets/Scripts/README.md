# Sistema Trilho Interativo

Este sistema permite criar experiências interativas baseadas na posição física do usuário em um trilho de 600cm, controlando conteúdo em uma tela touch de 42 polegadas.

## Visão Geral

O sistema mapeia posições físicas (0-600cm) para posições virtuais no Unity (0-78.610) e permite definir zonas de ativação que respondem à presença do usuário, exibindo diferentes tipos de conteúdo.

## Componentes Principais

### TrilhoGameManager
O componente central que gerencia todo o sistema:
- Recebe dados de posição via OSC do APARATO
- Controla a câmera no eixo X
- Gerencia zonas de ativação
- Coordena transições entre estados

### ActivationZone
Define uma área no trilho onde conteúdo específico é ativado:
- **startPositionCm/endPositionCm**: Limites da zona em centímetros
- **contentType**: Tipo de conteúdo (Video, Application, Custom, UI)
- **fadeInDuration/fadeOutDuration**: Duração das transições
- **Events**: OnZoneEnter, OnZoneExit, OnZoneStay

### Tipos de Conteúdo

#### 1. Video
- Reproduz vídeos com controles touch
- Suporta VideoClip ou arquivos de vídeo
- Interface completa com play/pause, progresso, volume

#### 2. Application
- Lança aplicações externas
- Pode ocultar Unity durante execução
- Monitoramento de status da aplicação

#### 3. Custom
- Instantia prefabs personalizados
- Máxima flexibilidade para conteúdo específico

#### 4. UI
- Ativa interfaces touch personalizadas
- Integração com Canvas existente

## Configuração Rápida

### 1. Setup Inicial
```csharp
// Adicione TrilhoSetupHelper a qualquer GameObject
// Use o menu de contexto "Setup Trilho System"
```

### 2. Configuração Manual

#### Criando o GameManager:
```csharp
GameObject gameManagerObj = new GameObject("[TRILHO GAME MANAGER]");
TrilhoGameManager manager = gameManagerObj.AddComponent<TrilhoGameManager>();
```

#### Configurando OSC:
1. Configure o OscConnection asset com:
   - Host: 127.0.0.1 (ou IP do APARATO)
   - Port: 8000 (ou porta configurada)
   - Address: /unity

#### Adicionando Zonas:
```csharp
ActivationZone videoZone = new ActivationZone
{
    zoneName = "Video Zone",
    startPositionCm = 30f,
    endPositionCm = 100f,
    contentType = ZoneContentType.Video,
    videoPath = "Videos/meu_video.mp4"
};

manager.AddZone(videoZone);
```

## Estrutura de Pastas

```
StreamingAssets/
├── Videos/          # Arquivos de vídeo
├── Applications/    # Executáveis externos
└── Content/         # Outros recursos
```

## Editor Customizado

O TrilhoGameManagerEditor oferece:
- Interface visual para configurar zonas
- Handles 3D para ajustar posições na Scene View
- Botões de teste em runtime
- Simulação de posição para debug

### Recursos do Editor:
- **Position Simulation**: Teste diferentes posições
- **Zone Visualization**: Visualização 3D das zonas
- **Runtime Status**: Informações em tempo real
- **Quick Test Buttons**: Testes rápidos de zonas

## API Principal

### TrilhoGameManager
```csharp
// Controle de posição
manager.UpdatePosition(float positionCm);
manager.SimulatePosition(float positionCm);

// Gerenciamento de zonas
manager.AddZone(ActivationZone zone);
manager.RemoveZone(ActivationZone zone);
manager.SetZoneActive(string zoneName, bool active);

// Informações de estado
float currentPos = manager.GetCurrentPositionCm();
TrilhoState state = manager.GetCurrentState();
ActivationZone zone = manager.GetCurrentZone();
```

### VideoContentManager
```csharp
// Controle de vídeo
videoManager.LoadVideo(string path);
videoManager.Play();
videoManager.Pause();
videoManager.SetVolume(float volume);
```

### UITransitionManager
```csharp
// Transições
transitionManager.FadeIn(float duration);
transitionManager.ShowWithAnimation(TransitionType.Scale);
transitionManager.MoveIn(Vector3.fromOffset);
```

## Fluxo de Estados

1. **Background**: Estado padrão, exibe conteúdo de fundo
2. **Transitioning**: Durante mudanças entre zonas
3. **InZone**: Usuário dentro de uma zona ativa

## Mapeamento de Posição

```
Posição Física: 0cm ────────────────── 600cm
                 │                      │
Posição Unity:  0.0 ──────────────── 78.610
```

## Debug e Testes

### Simulação de Posição:
```csharp
// No Editor
manager.simulatePosition = true;
manager.simulatedPositionCm = 150f;

// Via código
manager.SimulatePosition(150f);
```

### Visualização 3D:
- Zonas aparecem como cubos coloridos na Scene View
- Zona atual em verde, outras em amarelo
- Posição atual como linha azul

## Eventos

### Eventos de Zona:
```csharp
zone.OnZoneEnter.AddListener(() => Debug.Log("Entrou na zona"));
zone.OnZoneExit.AddListener(() => Debug.Log("Saiu da zona"));
zone.OnZoneStay.AddListener(() => Debug.Log("Permanece na zona"));
```

### Eventos Globais:
```csharp
manager.OnPositionChanged.AddListener(pos => Debug.Log($"Posição: {pos}"));
manager.OnStateChanged.AddListener(state => Debug.Log($"Estado: {state}"));
manager.OnZoneEntered.AddListener(zone => Debug.Log($"Entrou: {zone.zoneName}"));
```

## Integração com APARATO

O sistema recebe dados via OSC no formato:
- **Address**: /unity
- **Data Type**: Float ou Int
- **Value**: Posição em centímetros (0-600)

### Configuração no APARATO:
1. Configure output OSC para Unity
2. Endereço: IP da máquina Unity
3. Porta: 8000 (padrão)
4. Mapping: Encoder value → OSC Float

## Troubleshooting

### Problemas Comuns:

#### OSC não funciona:
- Verifique firewall do Windows
- Confirme IP e porta
- Teste com ferramenta OSC externa

#### Câmera não move:
- Verifique referência de Transform
- Confirme mapeamento de posição
- Ative debug para ver valores

#### Zonas não ativam:
- Verifique limites das zonas
- Confirme posição atual
- Teste com simulação

#### Vídeos não reproduzem:
- Verifique caminho do arquivo
- Confirme formato suportado
- Teste com vídeo de exemplo

### Debug Info:
Ative `showDebugInfo` no GameManager para logs detalhados.

## Performance

### Otimizações:
- Use VideoClips para vídeos frequentes
- Configure transições adequadas
- Otimize UI para touch screen
- Use object pooling para conteúdo dinâmico

### Recomendações:
- Máximo 8-10 zonas simultâneas
- Vídeos em formato H.264
- Resolução adequada para 42" touch
- Transições entre 0.3-1.0 segundos

## Extensibilidade

### Criando Conteúdo Custom:
```csharp
public class MeuContenteCustom : MonoBehaviour
{
    public void OnZoneEnter()
    {
        // Sua lógica aqui
    }
}
```

### Adicionando Tipos de Conteúdo:
1. Extends ZoneContentType enum
2. Implemente lógica no GameManager
3. Crie prefabs específicos

## Suporte

Para dúvidas e suporte, consulte:
- Documentação do Chataigne/APARATO
- Unity Video Player documentation
- OSC Jack package documentation
