# 🚀 Guia de Uso - Sistema Trilho Interativo

## 📋 Checklist de Configuração

### ✅ 1. Preparação Inicial
- [ ] Unity 2023.3+ ou 6000.1+ instalado
- [ ] Projeto Unity aberto
- [ ] Pacotes OSC Jack e NDI instalados
- [ ] Scripts do Trilho importados

### ✅ 2. Configuração Básica

#### 2.1 Setup Automático (Recomendado)
```csharp
1. Adicione TrilhoSetupHelper a qualquer GameObject
2. No Inspector, clique com botão direito no script
3. Selecione "Setup Trilho System"
4. Configure as zonas no TrilhoGameManager criado
```

#### 2.2 Setup Manual
```csharp
1. Crie GameObject "[TRILHO GAME MANAGER]"
2. Adicione componente TrilhoGameManager
3. Configure OSC Connection asset
4. Referencie Camera e Canvas
```

### ✅ 3. Configuração OSC

#### 3.1 Unity Side
```
- Host: 127.0.0.1 (ou IP do APARATO)
- Port: 8000
- Address: /unity
```

#### 3.2 APARATO Side
```
- Target IP: IP da máquina Unity
- Target Port: 8000
- OSC Address: /unity
- Data Type: Float ou Int
- Value Range: 0-600 (cm)
```

### ✅ 4. Configuração de Zonas

#### 4.1 Usando o Editor Visual
```
1. Selecione o TrilhoGameManager
2. Expanda "Activation Zones"
3. Clique "Add Zone"
4. Configure:
   - Zone Name: Nome descritivo
   - Start/End Position: Limites em cm
   - Content Type: Video/Application/Custom/UI
   - Fade Durations: Transições
```

#### 4.2 Tipos de Conteúdo

**🎬 Video**
- Path: `StreamingAssets/Videos/nome_video.mp4`
- Formatos: MP4, MOV, AVI
- Resolução: Recomendado 1920x1080

**📱 Application**
- Path: Caminho completo para executável
- Arguments: Parâmetros opcionais
- Auto-close: Configurável

**🎨 Custom**
- Prefab: GameObject personalizado
- Scripts: Componentes customizados

**🖱️ UI**
- Canvas: Interface touch
- Interações: Configuráveis

## 🎮 Testando o Sistema

### 🔧 1. Teste Básico
```csharp
1. Play no Unity
2. Selecione TrilhoGameManager
3. Ative "Simulate Position"
4. Ajuste "Simulated Position Cm"
5. Observe mudanças na Scene View
```

### ⌨️ 2. Controle por Teclado (RECOMENDADO)
```csharp
1. Adicione TrilhoKeyboardController à cena
2. Play no Unity
3. Use os controles:

MOVIMENTO:
← → : Mover para esquerda/direita
Speed: 100 cm/s (configurável)

SALTOS DIRETOS:
0 : Início (0cm)
1 : Zona 1 (65cm)
2 : Zona 2 (185cm)  
3 : Zona 3 (340cm)
4 : Zona 4 (485cm)
5 : Background (550cm)
E : Final (600cm)

OUTROS:
R : Reset para início
H : Mostrar ajuda
I : Informações atuais
Q : Parar simulação
```

### 🔧 3. Teste com TrilhoTestController
```csharp
1. Adicione TrilhoTestController à cena
2. Referencie TrilhoGameManager
3. Use interface visual OU teclado:
   - Botões GUI para zonas
   - Controle por teclado integrado
   - Auto Test automático
   - Slider de posição
```

### 🔧 4. Teste de Interface
```csharp
1. Configure Canvas WorldSpace
2. Teste interações touch
3. Verifique transições
4. Valide responsividade
```

## 📊 Debug e Monitoramento

### 🔍 Informações de Debug
```
- Position: Posição atual em cm
- Unity Position: Posição mapeada
- Current State: Background/InZone/Transitioning
- Current Zone: Nome da zona ativa
- Zone Count: Número de zonas configuradas
```

### 🔍 Logs Importantes
```
"Position: 150.00cm -> Unity: 19.653"
"Entering zone: Video Zone"
"State changed to: InZone"
"OSC message received: 150.0"
```

### 🔍 Scene View Debug
```
- Zonas: Cubos amarelos/verdes
- Posição Atual: Linha azul vertical
- Handles: Arrastar para ajustar limites
- Gizmos: Visualização 3D das zonas
```

## 🎯 Casos de Uso Comuns

### 📱 1. Zona de Vídeo
```csharp
Zone Name: "Apresentação Produto"
Start: 50cm, End: 150cm
Content Type: Video
Video Path: "Videos/produto_demo.mp4"
Fade In: 1.0s, Fade Out: 0.5s
```

### 💻 2. Zona de Aplicação
```csharp
Zone Name: "Simulador 3D"
Start: 200cm, End: 300cm
Content Type: Application
App Path: "C:/Apps/Simulator.exe"
Arguments: "-fullscreen -touch"
```

### 🎨 3. Zona Interativa
```csharp
Zone Name: "Configurador"
Start: 350cm, End: 450cm
Content Type: Custom
Prefab: ConfiguratorUI
Custom Scripts: TouchInteraction
```

## 📁 Estrutura de Arquivos

```
StreamingAssets/
├── Videos/
│   ├── intro.mp4
│   ├── produto_1.mp4
│   └── demonstracao.mp4
├── Applications/
│   ├── simulator.exe
│   └── configurator.exe
└── Content/
    ├── textures/
    └── data/
```

## 🔧 Solução de Problemas

### ❌ OSC não funciona
```
✅ Verificar firewall Windows
✅ Confirmar IP e porta
✅ Testar com ferramenta OSC externa
✅ Verificar logs Unity Console
```

### ❌ Câmera não move
```
✅ Verificar referência Transform
✅ Conferir mapeamento posição
✅ Ativar debug para logs
✅ Testar simulação manual
```

### ❌ Zonas não ativam
```
✅ Verificar limites das zonas
✅ Confirmar posição atual
✅ Testar com TrilhoTestController
✅ Verificar estado isActive
```

### ❌ Vídeos não reproduzem
```
✅ Verificar caminho arquivo
✅ Confirmar formato suportado
✅ Testar com codec H.264
✅ Verificar resolução/bitrate
```

### ❌ Performance baixa
```
✅ Otimizar vídeos (resolução/codec)
✅ Reduzir número de zonas ativas
✅ Usar object pooling
✅ Configurar transições mais rápidas
```

## 📈 Otimizações Avançadas

### 🚀 Performance
```csharp
- Max 8-10 zonas simultâneas
- Vídeos H.264, 1920x1080, 30fps
- Transições 0.3-1.0 segundos
- Use VideoClips para vídeos frequentes
```

### 🚀 Memória
```csharp
- Descarregar vídeos não utilizados
- Object pooling para UI dinâmica
- Limpar cache periodicamente
```

### 🚀 Responsividade
```csharp
- Smooth camera movement: true
- Camera smoothing: 5.0
- Update rate: 60fps
- OSC buffer size: otimizado
```

## 🔄 Workflow de Produção

### 1️⃣ Pré-produção
```
1. Definir zonas e conteúdo
2. Criar storyboard
3. Preparar assets (vídeos, apps)
4. Configurar hardware (trilho, encoder)
```

### 2️⃣ Desenvolvimento
```
1. Setup Unity projeto
2. Configurar zonas
3. Implementar conteúdo customizado
4. Testes unitários
```

### 3️⃣ Integração
```
1. Conectar APARATO
2. Calibrar posições físicas
3. Ajustar mapeamento
4. Testes integração
```

### 4️⃣ Deploy
```
1. Build Unity player
2. Configurar máquina produção
3. Setup OSC conexão
4. Testes finais
```

## 📞 Suporte e Recursos

### 📚 Documentação
- Unity Video Player docs
- OSC Jack package docs
- Chataigne/APARATO manual

### 🛠️ Ferramentas Debug
- TrilhoTestController
- Scene View visualization
- Unity Console logs
- OSC monitor tools

### 💡 Extensões Sugeridas
- Unity Timeline para sequências
- Cinemachine para câmeras avançadas
- Unity Analytics para tracking
- Custom shaders para efeitos

---

**🎉 Sistema Trilho Interativo - Desenvolvido para máxima flexibilidade e performance!**
