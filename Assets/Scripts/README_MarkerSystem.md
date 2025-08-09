# Sistema de Marcadores - Trilho

## Visão Geral

O Sistema de Marcadores é uma nova abordagem para ativar conteúdos no Trilho, baseada em marcadores que acompanham a câmera e se movimentam no mundo. Esta abordagem oferece:

- **Lógica simples**: Marcadores fixos no mundo que ativam conteúdos quando a câmera se aproxima
- **Configuração fácil**: Interface visual no Inspector para configurar marcadores
- **Debug visual**: Esferas de debug para visualizar posições e raios de ativação
- **Flexibilidade**: Cada marcador pode ter seu próprio raio de ativação e configurações

## Componentes Principais

### 1. MarkerSystem
O componente principal que gerencia todos os marcadores.

**Características:**
- Gerencia uma lista de `ContentMarker`
- Atualiza a posição da câmera em relação aos marcadores
- Ativa/desativa conteúdos automaticamente
- Fornece visualização de debug

### 2. ContentMarker
Representa um marcador individual no mundo.

**Propriedades:**
- `markerName`: Nome do marcador
- `worldPositionX`: Posição X no mundo Unity
- `activationRadius`: Raio de ativação em unidades Unity
- `contentToActivate`: GameObject do conteúdo a ser ativado
- `fadeInDuration`: Duração do fade in
- `fadeOutDuration`: Duração do fade out
- `debugColor`: Cor para visualização de debug

### 3. MarkerConfiguration
Script auxiliar para facilitar a configuração e migração.

## Como Usar

### 1. Configuração Básica

1. **Adicione o MarkerSystem ao seu GameObject:**
   ```csharp
   var markerSystem = gameObject.AddComponent<MarkerSystem>();
   ```

2. **Configure a câmera:**
   ```csharp
   markerSystem.SetCameraTransform(Camera.main.transform);
   ```

3. **Crie marcadores:**
   ```csharp
   var marker = new ContentMarker
   {
       markerName = "Meu Marcador",
       worldPositionX = 1000f,
       activationRadius = 200f,
       contentToActivate = meuConteudo
   };
   markerSystem.AddMarker(marker);
   ```

### 2. Usando o Inspector

1. Selecione o GameObject com `MarkerSystem`
2. No Inspector, você verá uma interface organizada:
   - **Settings**: Configurações gerais
   - **Markers**: Lista de marcadores (expansível)
   - **Debug**: Opções de debug
   - **Status**: Informações em tempo real

3. Use os botões de "Quick Actions":
   - **Add New Marker**: Adiciona um novo marcador
   - **Create Example Markers**: Cria marcadores de exemplo
   - **Clear All Markers**: Remove todos os marcadores

### 3. Configuração de Marcadores

Cada marcador pode ser expandido no Inspector para mostrar:

- **Marker Settings**: Nome, posição, raio
- **Content**: GameObject do conteúdo, durações de fade
- **Debug**: Cor de debug
- **Runtime Status**: Estado atual (ativo/inativo, no range)

## Migração do Sistema Anterior

### Usando MarkerConfiguration

1. **Adicione o componente:**
   ```csharp
   var config = gameObject.AddComponent<MarkerConfiguration>();
   ```

2. **Execute a migração:**
   - Clique com botão direito no componente
   - Selecione "Migrate from TrilhoGameManager"

3. **Ou use os métodos de contexto:**
   - "Setup Marker System"
   - "Create Example Markers"

### Migração Manual

1. **Crie o MarkerSystem:**
   ```csharp
   var markerSystem = new GameObject("MarkerSystem").AddComponent<MarkerSystem>();
   ```

2. **Converta zonas para marcadores:**
   ```csharp
   // Para cada zona existente
   var marker = new ContentMarker
   {
       markerName = zone.zoneName,
       worldPositionX = ConvertCmToUnity(zone.startPositionCm + (zone.endPositionCm - zone.startPositionCm) / 2f),
       activationRadius = 200f,
       contentToActivate = zone.contentToActivate
   };
   markerSystem.AddMarker(marker);
   ```

## Debug e Visualização

### Esferas de Debug

O sistema cria automaticamente esferas visuais para debug:

- **Esfera pequena**: Posição do marcador
- **Esfera grande**: Raio de ativação
- **Cores**: 
  - Verde: Marcador ativo
  - Cor original: Marcador inativo

### Controles de Debug

- **Toggle Debug Visualization**: Liga/desliga esferas
- **Show Marker Info**: Mostra informações no console
- **Runtime Status**: Mostra estado atual no Inspector

## Exemplos de Uso

### 1. Marcador Simples
```csharp
var marker = new ContentMarker
{
    markerName = "Conteúdo Verde",
    worldPositionX = 1000f,
    activationRadius = 200f,
    contentToActivate = conteudoVerde
};
```

### 2. Marcador com Vídeo
```csharp
var marker = new ContentMarker
{
    markerName = "Vídeo",
    worldPositionX = 5000f,
    activationRadius = 300f,
    contentToActivate = conteudoVideo,
    fadeInDuration = 1f,
    fadeOutDuration = 1f
};
```

### 3. Múltiplos Marcadores
```csharp
// Criar vários marcadores em sequência
float[] positions = { 1000f, 3000f, 5000f };
Color[] colors = { Color.green, Color.blue, Color.red };

for (int i = 0; i < positions.Length; i++)
{
    var marker = new ContentMarker
    {
        markerName = $"Marcador {i + 1}",
        worldPositionX = positions[i],
        activationRadius = 200f,
        debugColor = colors[i]
    };
    markerSystem.AddMarker(marker);
}
```

## Vantagens do Sistema de Marcadores

1. **Simplicidade**: Lógica clara e direta
2. **Configurabilidade**: Interface visual no Inspector
3. **Debug**: Visualização fácil de posições e raios
4. **Performance**: Atualização otimizada com intervalos
5. **Flexibilidade**: Cada marcador independente
6. **Manutenibilidade**: Código organizado e modular

## Comparação com Sistema Anterior

| Aspecto | Sistema Anterior (Zonas) | Sistema de Marcadores |
|---------|--------------------------|----------------------|
| Lógica | Baseada em posição física (cm) | Baseada em posição Unity |
| Configuração | Complexa, múltiplos parâmetros | Simples, interface visual |
| Debug | Limitado | Visualização completa |
| Performance | Atualização constante | Atualização otimizada |
| Flexibilidade | Zonas fixas | Marcadores independentes |

## Próximos Passos

1. **Teste o sistema** com marcadores de exemplo
2. **Configure marcadores** para seu conteúdo específico
3. **Ajuste posições** e raios conforme necessário
4. **Use debug** para visualizar e ajustar
5. **Migre gradualmente** do sistema anterior

## Suporte

Para dúvidas ou problemas:
1. Verifique os logs no console
2. Use as opções de debug
3. Teste com marcadores de exemplo
4. Ajuste configurações no Inspector
