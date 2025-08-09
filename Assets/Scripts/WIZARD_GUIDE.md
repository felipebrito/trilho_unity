# Wizard de Configuração Trilho - Guia de Uso

## Visão Geral

O **TrilhoSetupWizard** é um sistema visual passo a passo que facilita a configuração do Trilho de forma intuitiva e em português. Ele guia você através de 4 passos simples para configurar todo o sistema.

## Como Usar

### 1. Iniciar o Wizard

**Opção A - Automático:**
- Adicione `TrilhoSetupWizard` a um GameObject
- Marque "Show Wizard On Start" no Inspector
- Execute o projeto

**Opção B - Manual:**
- Clique com botão direito no componente
- Selecione "Iniciar Wizard"

**Opção C - Via Código:**
```csharp
var wizard = FindObjectOfType<TrilhoSetupWizard>();
wizard.StartWizard();
```

### 2. Passos do Wizard

#### Passo 1: Configurar Bordas das TVs
- **Objetivo**: Definir onde estão as bordas das TVs
- **Como fazer**: 
  - Use as **setas do teclado** para mover os colisores vermelhos
  - Posicione um colisor na borda esquerda da TV
  - Posicione outro colisor na borda direita da TV
  - Pressione **ENTER** quando estiver satisfeito

#### Passo 2: Posicionar Conteúdos
- **Objetivo**: Definir onde os conteúdos devem aparecer
- **Como fazer**:
  - Clique onde você quer que cada conteúdo apareça
  - **Verde** = Conteúdo 1
  - **Azul** = Conteúdo 2  
  - **Vermelho** = Vídeo
  - Pressione **ENTER** para continuar

#### Passo 3: Configurar Marcadores
- **Objetivo**: Criar os marcadores no sistema
- **Como fazer**:
  - O sistema cria automaticamente os marcadores
  - Ajuste os raios de ativação no Inspector se necessário
  - Pressione **ENTER** para continuar

#### Passo 4: Testar Sistema
- **Objetivo**: Verificar se tudo está funcionando
- **Como fazer**:
  - Use as setas para mover a câmera
  - Verifique se os conteúdos aparecem nos locais corretos
  - Ajuste posições se necessário
  - Pressione **ENTER** para finalizar

## Controles

### Modo Configuração (Ativo)
- **Teclas 1-4**: Pular para um passo específico
- **ENTER**: Próximo passo
- **BACKSPACE**: Passo anterior
- **ESC**: Sair do modo configuração

### Passo 1 - Bordas
- **Setas**: Mover colisores vermelhos (apenas no modo configuração)
- **ENTER**: Confirmar posições

### Passo 2 - Conteúdos
- **Mouse**: Clicar onde quer os conteúdos
- **ENTER**: Confirmar posições

### Passo 3 - Configuração
- **Inspector**: Ajustar raios de ativação
- **ENTER**: Continuar

### Passo 4 - Teste
- **Setas**: Mover câmera (apenas fora do modo configuração)
- **ENTER**: Finalizar

### Modo Normal (Inativo)
- **Setas**: Funcionam normalmente para outros controles
- **Wizard**: Use os botões do Inspector para reativar

## Modo de Configuração

### Ativação
- O wizard ativa automaticamente o **modo configuração**
- As setas do teclado são **capturadas** para configuração
- Outros controles ficam **desabilitados** durante a configuração

### Indicadores Visuais
- **Wizard UI**: Mostra "[MODO CONFIGURAÇÃO ATIVO]"
- **Colisores**: Visíveis apenas no modo configuração
- **Marcadores**: Visíveis apenas no modo configuração

### Desativação
- **ESC**: Sai do modo configuração
- **Finalizar Wizard**: Sai automaticamente
- **Setas voltam a funcionar** normalmente

## Interface Visual

### Wizard UI
- **Título**: Mostra o passo atual
- **Descrição**: Instruções detalhadas + indicador de modo
- **Botões**: Ações disponíveis

### Elementos Visuais
- **Colisores Vermelhos**: Bordas das TVs (Passo 1)
- **Esferas Coloridas**: Marcadores de conteúdo (Passo 2)
- **Esferas de Debug**: Raios de ativação (Passo 3)

## Exemplo Prático

### 1. Preparação
```csharp
// Adicione o wizard a um GameObject
var wizardObj = new GameObject("TrilhoSetupWizard");
var wizard = wizardObj.AddComponent<TrilhoSetupWizard>();
```

### 2. Configuração
```csharp
// Configure as referências
wizard.markerSystem = FindObjectOfType<MarkerSystem>();
wizard.targetCanvas = FindObjectOfType<Canvas>();
wizard.sceneCamera = Camera.main;
```

### 3. Execução
```csharp
// Inicie o wizard
wizard.StartWizard();
```

## Solução de Problemas

### Wizard não aparece
- Verifique se o Canvas está configurado
- Certifique-se de que o wizard está ativo
- Use o método `StartWizard()` manualmente

### Colisores não se movem
- Verifique se está no Passo 1
- Use as setas do teclado (não mouse)
- Verifique se os colisores foram criados

### Conteúdos não aparecem
- Verifique se os marcadores foram criados
- Ajuste os raios de ativação
- Verifique se a câmera está se movendo

### Erro de referência
- Configure as referências no Inspector
- Use "Auto-assign" se disponível
- Verifique se os componentes existem

## Dicas

### Para Configuração Rápida
1. Use as teclas 1-4 para pular passos
2. Configure apenas o essencial primeiro
3. Teste e ajuste depois

### Para Configuração Precisa
1. Siga todos os passos na ordem
2. Teste cada passo antes de continuar
3. Ajuste os raios de ativação conforme necessário

### Para Configuração Avançada
1. Use o Inspector para ajustes finos
2. Configure múltiplos marcadores
3. Use o sistema de debug para visualizar

## Integração com Sistema Anterior

O wizard é compatível com o sistema anterior:

```csharp
// Use ambos os sistemas
var integration = FindObjectOfType<TrilhoIntegration>();
integration.SetupIntegration();

// Ou use apenas o wizard
var wizard = FindObjectOfType<TrilhoSetupWizard>();
wizard.StartWizard();
```

## Próximos Passos

Após usar o wizard:

1. **Teste o sistema** movendo a câmera
2. **Ajuste posições** se necessário
3. **Configure conteúdos** específicos
4. **Use o MarkerSystem** para ajustes finos

## Suporte

Para dúvidas ou problemas:

1. **Verifique os logs** no console
2. **Use os métodos de contexto** no Inspector
3. **Teste com o exemplo** fornecido
4. **Consulte a documentação** completa

O wizard torna a configuração do Trilho muito mais simples e intuitiva! 🎉
