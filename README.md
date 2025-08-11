# Trilho - Sistema de Trilhos Digitais Interativos

Sistema completo para criação de trilhos digitais interativos que respondem ao movimento físico de um dispositivo móvel. Integra hardware de medição (encoder) com software de configuração e Unity para experiências imersivas.

## 🎯 O que é o Trilho?

O Trilho é um sistema que permite criar **experiências interativas baseadas em movimento físico**. Imagine uma exposição onde o visitante empurra uma TV ou dispositivo móvel ao longo de um trilho, e o conteúdo muda automaticamente conforme a posição.

### Como Funciona na Prática:
1. **Hardware** ([trilhoencoder](https://github.com/felipebrito/trilhoencoder)) mede a posição em tempo real
2. **Configurador Web** permite criar e configurar as experiências
3. **Unity** executa a experiência e responde ao movimento físico

## 🔧 Componentes do Sistema

### 1. Hardware - TrilhoEncoder
- **ESP32** com encoder rotativo
- **Medição precisa**: 1 rotação = 20cm, 4000 pulsos por rotação
- **WiFi AP**: Cria rede própria para comunicação
- **Transmissão UDP**: Envia posição em tempo real para Unity
- **Interface Web**: Monitoramento e configuração do hardware

**Repositório**: [felipebrito/trilhoencoder](https://github.com/felipebrito/trilhoencoder)

### 2. Configurador Web
- **Interface intuitiva** para configurar experiências
- **Definição de zonas** com posicionamento preciso em centímetros
- **Múltiplos tipos de conteúdo**: imagens, vídeos, texto, aplicações
- **Visualização em tempo real** do trilho e zonas
- **Exportação** para Unity

### 3. Unity (Runtime)
- **Carregamento automático** da configuração
- **Resposta em tempo real** ao movimento físico
- **Ativação de zonas** conforme posição do dispositivo
- **Transições suaves** entre conteúdos

## 🚀 Fluxo de Trabalho Completo

### Passo 1: Configurar o Hardware
1. **Conecte o ESP32** ao encoder rotativo
2. **Carregue o código** do [trilhoencoder](https://github.com/felipebrito/trilhoencoder)
3. **Conecte-se** à rede WiFi "EncoderESP32" (senha: 12345678)
4. **Verifique** a transmissão UDP na porta 8888

### Passo 2: Criar a Experiência
1. **Abra o configurador**: `Assets/StreamingAssets/trilho-configurator.html`
2. **Configure o trilho**:
   - Dimensões físicas (cm) e da tela (px)
   - Posição da câmera Unity
   - Parâmetros OSC para comunicação
3. **Crie zonas de conteúdo**:
   - **Posição (cm)**: Onde no trilho a zona começa
   - **Tipo**: Imagem, Vídeo, Texto ou Aplicação
   - **Conteúdo**: Arquivo específico ou texto
   - **Dimensões**: Largura e altura da zona

### Passo 3: Testar no Unity
1. **Abra o projeto** no Unity
2. **Execute a cena** (ex: `Trilho-Configurable.unity`)
3. **O sistema automaticamente**:
   - Carrega a configuração do JSON
   - Configura a câmera e posições
   - Cria as zonas de conteúdo
   - Conecta ao hardware via UDP

### Passo 4: Experiência Interativa
- **Mova o dispositivo** ao longo do trilho físico
- **O hardware detecta** a posição e envia via UDP
- **Unity recebe** a posição e ativa as zonas correspondentes
- **Conteúdo muda** automaticamente conforme movimento

## 📋 Configuração Detalhada

### Trilho (Dimensões Físicas)
```json
{
  "trilho": {
    "widthPx": 1920,        // Largura da tela em pixels
    "heightPx": 1080,       // Altura da tela em pixels
    "widthCm": 600,         // Largura física do trilho em cm
    "heightCm": 108         // Altura física em cm
  }
}
```

### Câmera Unity
```json
{
  "camera": {
    "positionX": 0,         // Posição X da câmera
    "positionY": 0,         // Posição Y da câmera
    "positionZ": -10,       // Posição Z da câmera
    "size": 5.4             // Tamanho da câmera ortográfica
  }
}
```

### Zonas de Conteúdo
```json
{
  "contentZones": [
    {
      "id": 1,
      "name": "Entrada Principal",
      "type": "TEXT",
      "positionCm": 50,     // Começa aos 50cm do trilho
      "widthCm": 100,       // Zona tem 100cm de largura
      "heightCm": 80,       // Zona tem 80cm de altura
      "content": "Bem-vindo ao Trilho!",
      "fontSize": 58,
      "textColor": "#FFFFFF"
    }
  ]
}
```

## 🔌 Comunicação Hardware-Software

### Protocolo UDP
- **IP**: 192.168.4.255 (broadcast) ou IP específico
- **Porta**: 8888
- **Formato**: JSON
- **Frequência**: 100ms

### Dados Transmitidos
```json
{
  "encoder": {
    "pulses": 1234,         // Pulsos do encoder
    "distance": 6.17,       // Distância em cm
    "timestamp": 12345      // Timestamp
  }
}
```

### Unity Recebe e Processa
1. **UDP Listener** captura dados do hardware
2. **Conversão** de pulsos para posição em cm
3. **Mapeamento** para posição Unity
4. **Ativação** das zonas correspondentes

## 🎮 Controles e Teste

### Simulação (Sem Hardware)
- **Teclas setas**: Movem a câmera virtualmente
- **Teclas 0-5**: Saltos rápidos para posições específicas
- **Overlay visual**: Mostra zonas e posição atual

### Hardware Real
- **Movimento físico**: Empurre o dispositivo ao longo do trilho
- **Feedback visual**: Conteúdo muda em tempo real
- **Precisão**: Resolução de 0.05mm (4000 pulsos/rotação)

## 🛠️ Desenvolvimento e Customização

### Scripts Unity Principais
- **`TrilhoConfigLoader.cs`**: Carrega configuração JSON e cria objetos
- **`TrilhoGameManager.cs`**: Gerencia posição, câmera e mapeamento cm↔Unity
- **`TrilhoSceneSetup.cs`**: Configuração automática da cena
- **`TrilhoZoneActivator.cs`**: Ativação de zonas baseada em posição

### Configurador Web
- **`trilho-configurator.js`**: Lógica principal e gerenciamento de dados
- **`trilho-configurator-clean.css`**: Estilos e tema visual
- **`trilho-configurator.html`**: Interface e estrutura

### Estrutura de Arquivos
```
Assets/
├── Scripts/
│   ├── TrilhoConfigLoader.cs      # Carregamento de configuração
│   ├── TrilhoGameManager.cs       # Gerenciamento principal
│   ├── TrilhoSceneSetup.cs        # Setup automático
│   └── TrilhoZoneActivator.cs     # Ativação de zonas
├── StreamingAssets/
│   ├── trilho-configurator.html   # Interface web
│   ├── trilho-configurator.js     # Lógica JavaScript
│   ├── trilho-configurator-clean.css # Estilos
│   └── trilho_config.json        # Configuração padrão
└── Scenes/
    └── Trilho-Configurable.unity  # Cena principal
```

## 🚨 Troubleshooting

### Hardware Não Responde
1. **Verifique WiFi**: Conecte-se à rede "EncoderESP32"
2. **Teste UDP**: Use `examples/udp_receiver_example.py`
3. **Firewall**: Desabilite temporariamente no Windows
4. **IP específico**: Configure `udpTargetIP` no código

### Unity Não Carrega Configuração
1. **Verifique JSON**: Valide sintaxe em `trilho_config.json`
2. **Caminho StreamingAssets**: Certifique-se que está correto
3. **Console Unity**: Verifique erros de carregamento
4. **Scripts**: Confirme que todos estão anexados aos GameObjects

### Zonas Não Ativam
1. **Posição da câmera**: Verifique se está alinhada com o trilho
2. **Dimensões**: Confirme largura/altura em cm
3. **Overlay**: Ative `drawGameOverlay` para debug visual
4. **Mapeamento**: Verifique conversão cm↔Unity

## 📱 Casos de Uso

### Exposições Interativas
- **Museus**: Conteúdo muda conforme movimento
- **Feiras**: Informações específicas por posição
- **Showrooms**: Produtos diferentes em cada zona

### Educação
- **Timeline histórica**: Eventos em ordem cronológica
- **Anatomia**: Partes do corpo em diferentes posições
- **Geografia**: Países/regiões ao longo do trilho

### Entretenimento
- **Jogos**: Níveis diferentes por posição
- **Narrativas**: Histórias que se desenrolam com movimento
- **Experiências imersivas**: Realidade aumentada física

## 🤝 Contribuição

1. **Fork** o projeto
2. **Crie uma branch** para sua feature
3. **Desenvolva** e teste
4. **Abra um Pull Request**

## 📄 Licença

MIT License - veja [LICENSE](LICENSE) para detalhes.

## 🔗 Links Relacionados

- **Hardware**: [trilhoencoder](https://github.com/felipebrito/trilhoencoder) - ESP32 com encoder
- **Documentação**: [TRILHO_JSON_SETUP_GUIDE.md](Assets/TRILHO_JSON_SETUP_GUIDE.md)
- **Guia Rápido**: [README_QUICK_START.md](Assets/README_QUICK_START.md)

---

**Desenvolvido para criar experiências interativas que conectam o mundo físico ao digital** 🚀

