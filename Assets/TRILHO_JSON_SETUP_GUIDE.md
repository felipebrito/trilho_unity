# 🚀 Guia Completo - Cena Unity Carregada por JSON

## 📋 Visão Geral

Este guia te ensina como criar uma cena Unity completa que seja **100% configurável através de arquivos JSON**, permitindo que você modifique toda a experiência sem recompilar o projeto.

## 🎯 O que você vai conseguir:

- ✅ **Cena dinâmica** carregada por JSON
- ✅ **Zonas interativas** configuráveis
- ✅ **Conteúdo variado** (vídeos, imagens, texto, apps)
- ✅ **Posicionamento automático** baseado em trilho físico
- ✅ **Transições suaves** entre zonas
- ✅ **Configuração OSC** para controle externo

---

## 🏗️ PASSO 1: Estrutura da Cena Base

### 1.1 Criar a Cena Principal
```
1. Abra Unity
2. File → New Scene
3. Salve como "Trilho-Configurable"
4. Delete todos os GameObjects (exceto Main Camera)
```

### 1.2 Configurar a Câmera
```
1. Selecione Main Camera
2. Position: (0, 0, -10)
3. Projection: Orthographic
4. Size: 5.4 (para 1920x1080)
5. Background: Preto ou sua cor preferida
```

### 1.3 Criar Canvas Principal
```
1. GameObject → UI → Canvas
2. Renomeie para "[CANVAS PRINCIPAL]"
3. Render Mode: World Space
4. Position: (0, 0, 0)
5. Scale: (0.01, 0.01, 0.01) [para 1cm = 1 Unity unit]
```

---

## 🔧 PASSO 2: Scripts Essenciais

### 2.1 TrilhoConfigLoader (Carregador Principal)
```
1. Crie GameObject "[CONFIG LOADER]"
2. Adicione script TrilhoConfigLoader
3. Configure:
   - Auto Load: true
   - Show Debug: true
   - Config File: "trilho_config.json"
```

### 2.2 TrilhoGameManager (Gerenciador do Jogo)
```
1. Crie GameObject "[GAME MANAGER]"
2. Adicione script TrilhoGameManager
3. Configure:
   - Camera: Main Camera
   - Canvas: [CANVAS PRINCIPAL]
   - OSC Connection: (criaremos depois)
```

### 2.3 TrilhoZoneActivator (Ativador de Zonas)
```
1. Crie GameObject "[ZONE ACTIVATOR]"
2. Adicione script TrilhoZoneActivator
3. Configure:
   - Game Manager: [GAME MANAGER]
   - Show Debug: true
```

---

## 📁 PASSO 3: Estrutura de Pastas

### 3.1 Criar StreamingAssets
```
Assets/
├── StreamingAssets/
│   ├── Videos/
│   │   ├── intro.mp4
│   │   ├── produto_1.mp4
│   │   └── demonstracao.mp4
│   ├── Images/
│   │   ├── logo.png
│   │   ├── background.jpg
│   │   └── icons/
│   ├── Text/
│   │   ├── descricao.txt
│   │   └── instrucoes.txt
│   └── Apps/
│       └── simulador.exe
```

### 3.2 Criar Prefabs (Opcional)
```
Assets/
├── Prefabs/
│   ├── UI/
│   │   ├── Button.prefab
│   │   ├── Panel.prefab
│   │   └── Text.prefab
│   └── Content/
│       ├── VideoPlayer.prefab
│       └── ImageDisplay.prefab
```

---

## ⚙️ PASSO 4: Configuração OSC

### 4.1 Criar OSC Connection Asset
```
1. Project Window → Create → OSC Connection
2. Configure:
   - Host: 127.0.0.1
   - Port: 8000
   - Address: /unity
3. Salve como "OSCConnection"
```

### 4.2 Conectar ao GameManager
```
1. Selecione [GAME MANAGER]
2. OSC Connection: OSCConnection
3. Teste a conexão
```

---

## 📝 PASSO 5: Criar o JSON de Configuração

### 5.1 Estrutura Básica do JSON
```json
{
  "configName": "Minha Cena Interativa",
  "description": "Cena configurável para demonstração",
  "version": "1.0.0",
  "physicalMinCm": 0,
  "physicalMaxCm": 600,
  "unityMinPosition": 0,
  "unityMaxPosition": 8520,
  "screenWidthInCm": 60,
  "movementSensitivity": 0.5,
  "smoothCameraMovement": true,
  "cameraSmoothing": 5,
  "oscAddress": "/unity",
  "oscPort": 8000,
  "contentZones": []
}
```

### 5.2 Exemplo de Zona de Vídeo
```json
{
  "name": "Apresentação Inicial",
  "description": "Vídeo de boas-vindas",
  "positionCm": 50,
  "paddingCm": 10,
  "contentType": 1,
  "contentFileName": "Videos/intro.mp4",
  "placeContentAtWorldX": true,
  "contentOffsetCm": 0,
  "reference": 0,
  "fadeInSeconds": 1.0,
  "fadeOutSeconds": 0.5,
  "videoSettings": {
    "playOnAwake": true,
    "volume": 1.0,
    "mute": false
  }
}
```

### 5.3 Exemplo de Zona de Imagem
```json
{
  "name": "Logo da Empresa",
  "description": "Exibição do logo",
  "positionCm": 150,
  "paddingCm": 20,
  "contentType": 0,
  "contentFileName": "Images/logo.png",
  "placeContentAtWorldX": true,
  "contentOffsetCm": 0,
  "reference": 1,
  "fadeInSeconds": 0.5,
  "fadeOutSeconds": 0.3,
  "imageSettings": {
    "maintainAspectRatio": true,
    "size": [1920, 1080],
    "tint": [1, 1, 1, 1]
  }
}
```

### 5.4 Exemplo de Zona de Texto
```json
{
  "name": "Instruções",
  "description": "Texto explicativo",
  "positionCm": 250,
  "paddingCm": 15,
  "contentType": 2,
  "textSettings": {
    "text": "Bem-vindo ao nosso sistema interativo!",
    "fontSize": 72,
    "textColor": [1, 1, 1, 1],
    "alignment": 4,
    "fontStyle": 0
  },
  "placeContentAtWorldX": true,
  "contentOffsetCm": 0,
  "reference": 0,
  "fadeInSeconds": 0.8,
  "fadeOutSeconds": 0.4
}
```

### 5.5 Exemplo de Zona de Aplicação
```json
{
  "name": "Simulador 3D",
  "description": "Aplicação externa",
  "positionCm": 350,
  "paddingCm": 25,
  "contentType": 3,
  "appSettings": {
    "applicationPath": "C:/Apps/Simulator.exe",
    "arguments": "-fullscreen -touch",
    "waitForExit": false,
    "hideUnityWhileRunning": true,
    "launchTimeout": 10,
    "autoCloseOnTimeout": true
  },
  "placeContentAtWorldX": true,
  "contentOffsetCm": 0,
  "reference": 0,
  "fadeInSeconds": 0.3,
  "fadeOutSeconds": 0.3
}
```

---

## 🎮 PASSO 6: Testando a Cena

### 6.1 Teste Básico
```
1. Play no Unity
2. Verifique no Console se a configuração foi carregada
3. Use Scene View para ver as zonas criadas
4. Teste movimento da câmera
```

### 6.2 Teste com Controle por Teclado
```
1. Adicione TrilhoKeyboardController à cena
2. Use teclas:
   - ← → : Mover esquerda/direita
   - 0-9 : Pular para zonas específicas
   - R : Reset
   - H : Ajuda
```

### 6.3 Teste com OSC
```
1. Use ferramenta OSC (Chataigne, TouchOSC)
2. Envie valores de 0-600 para /unity
3. Observe movimento da câmera
4. Verifique ativação das zonas
```

---

## 🔄 PASSO 7: Modificando a Cena

### 7.1 Alterar Conteúdo
```
1. Edite o arquivo trilho_config.json
2. Mude nomes, posições, conteúdo
3. Salve o arquivo
4. No Unity, clique com botão direito no [CONFIG LOADER]
5. Selecione "Reload Configuration"
```

### 7.2 Adicionar Novas Zonas
```json
{
  "name": "Nova Zona",
  "positionCm": 450,
  "contentType": 0,
  "contentFileName": "Images/nova_imagem.jpg"
}
```

### 7.3 Modificar Posições
```
1. Altere positionCm no JSON
2. Recarregue configuração
3. Zonas se reposicionam automaticamente
```

---

## 🎨 PASSO 8: Personalização Avançada

### 8.1 Efeitos Visuais
```
1. Adicione Post Processing
2. Configure transições customizadas
3. Use partículas para feedback visual
4. Implemente shaders personalizados
```

### 8.2 Interatividade
```
1. Adicione scripts de interação
2. Implemente touch gestures
3. Configure feedback háptico
4. Adicione sons de interface
```

### 8.3 Animações
```
1. Use Unity Timeline
2. Configure animações por zona
3. Implemente transições suaves
4. Adicione keyframes automáticos
```

---

## 🚀 PASSO 9: Deploy e Produção

### 9.1 Build Final
```
1. File → Build Settings
2. Platform: Windows/Mac/Linux
3. Scenes: Trilho-Configurable
4. Build
```

### 9.2 Configuração de Produção
```
1. Copie trilho_config.json para StreamingAssets
2. Configure IP e porta OSC
3. Teste em hardware real
4. Ajuste posições físicas
```

### 9.3 Manutenção
```
1. Backup das configurações
2. Versionamento dos JSONs
3. Documentação das mudanças
4. Testes regulares
```

---

## 🔧 Solução de Problemas Comuns

### ❌ JSON não carrega
```
✅ Verificar sintaxe JSON
✅ Confirmar caminho do arquivo
✅ Verificar permissões de leitura
✅ Usar JSON validator online
```

### ❌ Zonas não aparecem
```
✅ Verificar positionCm válidos
✅ Confirmar contentType correto
✅ Verificar contentFileName
✅ Recarregar configuração
```

### ❌ Câmera não move
```
✅ Verificar OSC connection
✅ Confirmar mapeamento de posições
✅ Testar com simulação manual
✅ Verificar referências no GameManager
```

### ❌ Conteúdo não exibe
```
✅ Verificar caminhos dos arquivos
✅ Confirmar formatos suportados
✅ Verificar permissões de StreamingAssets
✅ Testar com arquivos simples
```

---

## 📚 Recursos Adicionais

### 🛠️ Ferramentas Úteis
- **JSON Editor**: Visual Studio Code, Notepad++
- **OSC Tools**: Chataigne, TouchOSC, OSC Monitor
- **Video Converters**: HandBrake, FFmpeg
- **Image Editors**: Photoshop, GIMP, Paint.NET

### 📖 Documentação
- Unity Video Player
- Unity UI System
- OSC Jack Package
- Unity StreamingAssets

### 💡 Dicas de Performance
- Vídeos: H.264, 1920x1080, 30fps
- Imagens: PNG/JPG, resolução otimizada
- Transições: 0.3-1.0 segundos
- Zonas simultâneas: máximo 8-10

---

## 🎉 Resultado Final

Com este guia, você terá uma cena Unity **100% configurável via JSON** que permite:

- ✅ **Modificar conteúdo** sem recompilar
- ✅ **Reposicionar zonas** dinamicamente
- ✅ **Adicionar/remover elementos** facilmente
- ✅ **Configurar OSC** para controle externo
- ✅ **Personalizar transições** e efeitos
- ✅ **Deploy flexível** para diferentes usos

**🎯 Sua cena agora é um sistema vivo que evolui através de configuração!**

---

*Para dúvidas ou suporte, consulte a documentação do projeto Trilho ou entre em contato com a equipe de desenvolvimento.*
