# Como Configurar o Trilho - Guia Prático

## 🚀 Configuração Rápida (2 opções)

### Opção 1: Configuração em Runtime (5 minutos)
1. **Crie um GameObject** na cena
2. **Adicione o componente** `TrilhoSetupWizard`
3. **Clique "Iniciar Wizard"** no Inspector
4. **Siga o wizard visual** passo a passo

### Opção 2: Configuração Visual no Editor (Recomendada)
1. **Crie um GameObject** na cena
2. **Adicione o componente** `TrilhoEditorConfigurator`
3. **Configure posições** com feedback visual e numérico
4. **Aplique configurações** com um clique

## 🆕 Configuração de Cena Limpa

### Passo 1: Setup Automático (Recomendado)
1. **Crie um GameObject** vazio
2. **Adicione o componente** `TrilhoQuickSetup`
3. **Execute o jogo** - setup automático acontece
4. **Verifique o console** para logs de progresso

### Passo 2: Setup Manual (Opcional)
1. **Crie um GameObject** chamado "Trilho Manager"
2. **Adicione os componentes**:
   - `TrilhoEditorConfigurator` (configuração visual)
   - `MarkerSystem` (sistema de marcadores)
   - `TrilhoGameManager` (gerenciador principal)

### Passo 2: Canvas e UI
1. **Crie um Canvas** (se não existir)
2. **Configure o Canvas**:
   - Render Mode: Screen Space - Overlay
   - UI Scale Mode: Scale With Screen Size
   - Reference Resolution: 1920x1080

### Passo 3: Câmera
1. **Selecione a Main Camera**
2. **Configure a posição**: X: 0, Y: 0, Z: -10
3. **Verifique se está ativa**

### Passo 4: Configuração Rápida
1. **Selecione o "Trilho Manager"**
2. **No Inspector do TrilhoEditorConfigurator**:
   - Clique **"Criar Exemplo"** (cria 3 marcadores)
   - Clique **"Aplicar Marcadores"** (aplica ao sistema)
   - Ajuste **leftColliderX** e **rightColliderX**
   - Clique **"Aplicar Posições"** (aplica colisores)

### Passo 5: Teste
1. **Execute o jogo** (Play)
2. **Verifique o console** - deve mostrar "SETUP RÁPIDO CONCLUÍDO"
3. **Use as setas** para mover a câmera
4. **Verifique** se os conteúdos aparecem na Game View

## 🎯 O que Deveria Acontecer

### **Na Game View você deveria ver:**
- ✅ **3 conteúdos coloridos** (Verde, Azul, Vermelho)
- ✅ **Texto identificando** cada conteúdo
- ✅ **Transparência** (alpha = 0.3) para ver todos
- ✅ **Conteúdos mudam** conforme você move a câmera

### **No Console você deveria ver:**
```
=== SETUP RÁPIDO TRILHO ===
Criando Canvas...
Canvas criado com sucesso!
Câmera configurada na posição: (0.0, 0.0, -10.0)
Trilho Manager criado!
MarkerSystem adicionado!
TrilhoGameManager adicionado!
TrilhoGameManager desabilitado para evitar interferência!
Componentes essenciais criados!
3 marcadores de exemplo criados!
3 conteúdos de exemplo criados e VISÍVEIS!
=== DEBUG VISUAL CRIADO ===
=== SETUP RÁPIDO CONCLUÍDO ===
```

### **Na Scene View você deveria ver:**
- ✅ **Esferas coloridas** nos marcadores
- ✅ **Cilindros transparentes** mostrando raios
- ✅ **Texto 3D** identificando marcadores
- ✅ **Linha amarela** da posição da câmera

## ✅ Checklist de Cena Limpa

### Componentes Obrigatórios:
- [ ] **TrilhoEditorConfigurator** (configuração visual)
- [ ] **MarkerSystem** (sistema de marcadores)
- [ ] **TrilhoGameManager** (gerenciador principal)
- [ ] **Canvas** (interface de usuário)
- [ ] **Main Camera** (câmera principal)

### Configurações:
- [ ] **Canvas configurado** (Screen Space - Overlay)
- [ ] **Câmera posicionada** (Z: -10)
- [ ] **Marcadores criados** (3 exemplos)
- [ ] **Colisores configurados** (posições X)
- [ ] **Sistema testado** (executar jogo)

## 🎯 Configuração Detalhada

### 1. Preparação Inicial

#### Opção A: Configuração Automática
```csharp
// 1. Crie um GameObject
var wizardObj = new GameObject("TrilhoSetupWizard");

// 2. Adicione o componente
var wizard = wizardObj.AddComponent<TrilhoSetupWizard>();

// 3. Inicie o wizard
wizard.StartWizard();
```

#### Opção B: Configuração Manual
1. **Selecione um GameObject** na hierarquia
2. **Clique "Add Component"**
3. **Procure por "TrilhoSetupWizard"**
4. **Clique "Iniciar Wizard"** no Inspector

### 2. Configuração em Editor (Recomendada)

#### Passo 1: Setup Automático
1. **Crie um GameObject** vazio
2. **Adicione o componente** `TrilhoQuickSetup`
3. **Execute o jogo** - setup automático acontece
4. **Verifique o console** para logs de progresso

#### Passo 2: Configuração Manual (Opcional)
1. **Crie um GameObject** na hierarquia
2. **Adicione o componente** `TrilhoEditorConfigurator`
3. **No Inspector**, clique **"Criar Exemplo"** para criar marcadores
4. **Ajuste posições** diretamente nos campos numéricos
5. **Clique "Aplicar Marcadores"** para aplicar

#### Passo 2: Configurar Colisores
1. **No Inspector do TrilhoEditorConfigurator**
2. **Ajuste** `leftColliderX` e `rightColliderX`
3. **Clique "Aplicar Posições"** para aplicar
4. **Veja feedback visual** na Scene View

#### Passo 3: Visualização
1. **Ative** `showPositionHandles` no Inspector
2. **Veja gizmos** na Scene View
3. **Ajuste posições** com feedback visual
4. **Use "Carregar da Cena"** para capturar posições atuais

#### Passo 2: Ajustar Posições
1. **Clique "Ajustar Posições"** no Inspector
2. **Selecione o MarkerSystem** na hierarquia
3. **No Inspector**, ajuste:
   - `worldPositionX`: Posição no mundo
   - `activationRadius`: Raio de ativação
   - `fadeInDuration`: Duração do fade in
   - `fadeOutDuration`: Duração do fade out

#### Passo 3: Criar Conteúdos
1. **Clique "Criar Conteúdos de Exemplo"** no Inspector
2. **3 conteúdos** serão criados no Canvas
3. **Associe conteúdos** aos marcadores no MarkerSystem

#### Passo 4: Testar Configuração
1. **Clique "Testar Configuração"** no Inspector
2. **Verifique os logs** no console
3. **Execute o jogo** para testar funcionalidade

### 2. Configuração das Bordas

#### Visualização
```
[TV 1] [TV 2] [TV 3]
   |     |     |
   v     v     v
[Colisor] [Colisor] [Colisor]
```

#### Como Fazer
1. **Ative o modo configuração** (acontece automaticamente)
2. **Use as setas** para mover os colisores vermelhos
3. **Posicione** um colisor na borda esquerda de cada TV
4. **Posicione** outro colisor na borda direita de cada TV
5. **Pressione ENTER** para confirmar

#### Dicas
- Os colisores são **vermelhos** e **altos**
- Mova-os **lentamente** com as setas
- Posicione **exatamente** nas bordas das TVs

### 3. Posicionamento de Conteúdos

#### Visualização
```
[TV 1] [TV 2] [TV 3]
   |     |     |
   v     v     v
[Verde] [Azul] [Vermelho]
```

#### Como Fazer
1. **Clique** onde quer que o conteúdo verde apareça
2. **Clique** onde quer que o conteúdo azul apareça
3. **Clique** onde quer que o vídeo apareça
4. **Pressione ENTER** para continuar

#### Dicas
- **Verde** = Conteúdo simples (imagem/texto)
- **Azul** = Conteúdo interativo
- **Vermelho** = Vídeo
- Clique **exatamente** onde quer o conteúdo

### 4. Configuração dos Marcadores

#### O que Acontece Automaticamente
1. **Marcadores são criados** nas posições clicadas
2. **Raios de ativação** são definidos (200 unidades)
3. **Conteúdos são criados** no Canvas
4. **Sistema é configurado** automaticamente

#### Ajustes Manuais (Opcional)
1. **Selecione o MarkerSystem** na hierarquia
2. **Ajuste os raios** no Inspector
3. **Configure os conteúdos** específicos
4. **Teste** movendo a câmera

### 5. Teste e Ajustes

#### Como Testar
1. **Mova a câmera** com as setas
2. **Observe** se os conteúdos aparecem
3. **Verifique** se aparecem no local correto
4. **Ajuste** posições se necessário

#### Ajustes Comuns
- **Conteúdo não aparece**: Aumente o raio de ativação
- **Conteúdo aparece no lugar errado**: Reposicione o marcador
- **Múltiplos conteúdos aparecem**: Diminua os raios de ativação

## 🎮 Controles Durante a Configuração

### Modo Configuração (Ativo)
```
Setas: Mover colisores
1-4: Navegar entre passos
ENTER: Próximo passo
BACKSPACE: Passo anterior
ESC: Sair do modo configuração
```

### Modo Normal (Inativo)
```
Setas: Mover câmera (normal)
Outros controles: Funcionam normalmente
```

## 🔧 Configuração Avançada

### 1. Múltiplos Conteúdos por TV
```csharp
// Crie marcadores adicionais
var extraMarker = new ContentMarker
{
    markerName = "Conteúdo Extra",
    worldPositionX = 2500f,
    activationRadius = 150f,
    contentToActivate = meuConteudoExtra
};
markerSystem.AddMarker(extraMarker);
```

### 2. Conteúdos Específicos
```csharp
// Configure vídeos
var videoMarker = new ContentMarker
{
    markerName = "Vídeo Específico",
    worldPositionX = 5000f,
    activationRadius = 300f,
    contentToActivate = meuVideoPlayer
};
```

### 3. Ajustes de Performance
```csharp
// Configure intervalos de atualização
markerSystem.updateInterval = 0.05f; // Mais responsivo
markerSystem.updateInterval = 0.2f;  // Mais eficiente
```

## 🚨 Solução de Problemas

### Problema: Wizard não aparece
**Solução:**
1. Verifique se o Canvas existe
2. Clique "Iniciar Wizard" manualmente
3. Verifique os logs no console

### Problema: Colisores não se movem
**Solução:**
1. Verifique se está no Passo 1
2. Use as setas (não mouse)
3. Verifique se o modo configuração está ativo

### Problema: Conteúdos não aparecem
**Solução:**
1. Verifique se os marcadores foram criados
2. Aumente os raios de ativação
3. Verifique se a câmera está se movendo

### Problema: Setas não funcionam
**Solução:**
1. Pressione ESC para sair do modo configuração
2. Use "Reativar Modo Configuração" se necessário
3. Verifique se não há outros scripts capturando as setas

## 📋 Checklist de Configuração

### ✅ Preparação
- [ ] TrilhoSetupWizard adicionado
- [ ] Canvas configurado
- [ ] Câmera configurada

### ✅ Passo 1 - Bordas
- [ ] Colisores criados
- [ ] Bordas esquerda posicionadas
- [ ] Bordas direita posicionadas
- [ ] ENTER pressionado

### ✅ Passo 2 - Conteúdos
- [ ] Conteúdo verde posicionado
- [ ] Conteúdo azul posicionado
- [ ] Vídeo posicionado
- [ ] ENTER pressionado

### ✅ Passo 3 - Marcadores
- [ ] Marcadores criados automaticamente
- [ ] Raios de ativação configurados
- [ ] Conteúdos associados
- [ ] ENTER pressionado

### ✅ Passo 4 - Teste
- [ ] Câmera se move
- [ ] Conteúdos aparecem
- [ ] Posições corretas
- [ ] Sistema funcionando

## 🎉 Configuração Concluída!

Após seguir estes passos, você terá:

✅ **Sistema Trilho configurado**
✅ **Marcadores posicionados**
✅ **Conteúdos funcionando**
✅ **Controles organizados**
✅ **Interface intuitiva**

### Próximos Passos
1. **Teste** o sistema movendo a câmera
2. **Ajuste** posições se necessário
3. **Configure** conteúdos específicos
4. **Use** o MarkerSystem para ajustes finos

O sistema está pronto para uso! 🚀
