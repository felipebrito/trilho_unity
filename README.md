# Trilho - Sistema de Configuração Interativa

Sistema de configuração interativa para trilhos digitais com suporte a múltiplos tipos de conteúdo (imagens, vídeos, texto e aplicações).

## 🚀 Funcionalidades

- **Configuração Visual**: Interface web intuitiva para configurar trilhos
- **Múltiplos Tipos de Conteúdo**: Suporte a imagens, vídeos, texto e aplicações
- **Configuração de Câmera**: Ajustes de posição, tamanho e parâmetros da câmera
- **Sistema OSC**: Configuração de comunicação OSC
- **Background Configurável**: Posicionamento e dimensionamento de fundos
- **Zonas de Conteúdo**: Criação e gerenciamento de zonas com posicionamento preciso
- **Exportação Unity**: Geração de pacotes completos para Unity
- **Interface Responsiva**: Funciona em desktop e dispositivos móveis

## 🛠️ Tecnologias

- **Frontend**: HTML5, CSS3, JavaScript (ES6+)
- **Backend**: Unity (C#)
- **Formato de Dados**: JSON
- **Comunicação**: OSC (Open Sound Control)

## 📁 Estrutura do Projeto

```
Trilho/
├── Assets/
│   ├── Scripts/           # Scripts Unity (C#)
│   ├── Scenes/           # Cenas Unity
│   ├── StreamingAssets/  # Configurador Web
│   │   ├── trilho-configurator.html
│   │   ├── trilho-configurator.js
│   │   ├── trilho-configurator-clean.css
│   │   └── trilho_config.json
│   └── Settings/         # Configurações Unity
├── ProjectSettings/       # Configurações do projeto
└── README.md
```

## 🚀 Como Usar

### 1. Configurador Web
1. Abra `Assets/StreamingAssets/trilho-configurator.html` em um navegador
2. Configure os parâmetros do trilho, câmera, OSC e background
3. Crie e configure zonas de conteúdo
4. Salve a configuração localmente ou exporte para Unity

### 2. Unity
1. Abra o projeto no Unity
2. Execute a cena desejada
3. O sistema carregará automaticamente a configuração do JSON

## 📋 Configuração

### Trilho
- **Largura (px)**: Largura da tela em pixels
- **Altura (px)**: Altura da tela em pixels
- **Largura (cm)**: Largura física em centímetros
- **Altura (cm)**: Altura física em centímetros

### Câmera
- **Posição X, Y, Z**: Posição da câmera no espaço 3D
- **Tamanho**: Tamanho da câmera ortográfica

### OSC
- **Host**: Endereço IP do servidor OSC
- **Porta**: Porta de comunicação OSC

### Background
- **Posição X, Y, Z**: Posição do fundo
- **Largura, Altura**: Dimensões do fundo

### Zonas
- **Tipo**: Imagem, Vídeo, Texto ou Aplicação
- **Posição (cm)**: Posição no trilho em centímetros
- **Largura, Altura**: Dimensões da zona
- **Conteúdo**: Arquivo ou texto específico

## 💾 Salvamento e Exportação

- **Salvar**: Salva configuração no localStorage do navegador
- **Salvar JSON**: Download do arquivo de configuração
- **Exportar Unity**: Gera pacote ZIP com JSON e arquivos de mídia

## 🔧 Desenvolvimento

### Estrutura dos Scripts Unity
- `TrilhoConfigLoader.cs`: Carrega e aplica configurações
- `TrilhoGameManager.cs`: Gerencia o sistema principal
- `TrilhoSceneSetup.cs`: Configuração automática da cena
- `TrilhoZoneActivator.cs`: Ativação de zonas de conteúdo

### Personalização
- Modifique `trilho-configurator-clean.css` para alterar o visual
- Edite `trilho-configurator.js` para funcionalidades customizadas
- Ajuste `trilho_config.json` para configurações padrão

## 📱 Compatibilidade

- **Navegadores**: Chrome, Firefox, Safari, Edge (versões modernas)
- **Unity**: 2021.3 LTS ou superior
- **Dispositivos**: Desktop, tablet e mobile

## 🤝 Contribuição

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/AmazingFeature`)
3. Commit suas mudanças (`git commit -m 'Add some AmazingFeature'`)
4. Push para a branch (`git push origin feature/AmazingFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 📞 Suporte

Para suporte técnico ou dúvidas, abra uma issue no GitHub.

---

**Desenvolvido com ❤️ para a comunidade de mídia interativa**

