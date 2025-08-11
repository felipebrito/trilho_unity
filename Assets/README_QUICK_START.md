# 🚀 Trilho - Início Rápido

## ⚡ Setup em 5 Minutos

### 1. 🎯 Setup Automático (Recomendado)
```
1. Abra Unity
2. Crie nova cena ou abra uma existente
3. Menu: Trilho → Setup Cena Automático
4. ✅ Pronto! Sua cena está configurada
```

### 2. 🔧 Setup Manual
```
1. Crie GameObject "[TRILHO SETUP]"
2. Adicione script TrilhoSceneSetup
3. Clique com botão direito → "Setup Automático da Cena"
```

### 3. 📁 Estrutura Criada Automaticamente
```
[CANVAS PRINCIPAL] - Canvas World Space
[CONFIG LOADER] - TrilhoConfigLoader
[GAME MANAGER] - TrilhoGameManager  
[ZONE ACTIVATOR] - TrilhoZoneActivator
```

### 4. 🎮 Teste Rápido
```
1. Adicione TrilhoQuickTest à cena
2. Play no Unity
3. Use interface de teste para verificar funcionamento
4. Slider para simular movimento
5. Botões para pular entre zonas
```

---

## 📋 Arquivos Essenciais

### ✅ Já Criados
- `trilho_config_example.json` - Configuração de exemplo
- `TRILHO_JSON_SETUP_GUIDE.md` - Guia completo
- `TrilhoSceneSetup.cs` - Setup automático
- `TrilhoQuickTest.cs` - Teste rápido

### 📁 Pastas Criadas
```
Assets/StreamingAssets/
├── Videos/          - Vídeos MP4
├── Images/          - Imagens PNG/JPG
├── Text/            - Arquivos de texto
└── Apps/            - Aplicações externas
```

---

## 🎯 Próximos Passos

### 1. 📝 Personalizar JSON
```
1. Copie trilho_config_example.json
2. Renomeie para trilho_config.json
3. Edite zonas, posições, conteúdo
4. Salve e recarregue no Unity
```

### 2. 🎨 Adicionar Conteúdo
```
1. Coloque vídeos em Videos/
2. Coloque imagens em Images/
3. Atualize JSON com novos arquivos
4. Recarregue configuração
```

### 3. 🔌 Configurar OSC
```
1. Crie OSC Connection Asset
2. Configure IP/Porta
3. Conecte ao GameManager
4. Teste com ferramenta OSC
```

---

## 🚨 Solução de Problemas

### ❌ "Configuração não carregada"
```
✅ Verificar se trilho_config.json existe
✅ Verificar sintaxe JSON
✅ Usar JSON validator online
✅ Recarregar configuração
```

### ❌ "Zonas não aparecem"
```
✅ Verificar positionCm válidos (0-600)
✅ Confirmar contentType correto
✅ Verificar contentFileName
✅ Recarregar configuração
```

### ❌ "Câmera não move"
```
✅ Verificar referências no GameManager
✅ Testar com simulação manual
✅ Verificar Console para erros
```

---

## 🎮 Controles de Teste

### 🖱️ Interface de Teste
- **Slider**: Simula posição no trilho
- **Botões**: Pula para zonas específicas
- **Status**: Mostra estado dos componentes

### ⌨️ Teclas de Atalho
- **R**: Reset para posição inicial
- **H**: Mostrar ajuda
- **0-9**: Pular para zonas

---

## 📚 Recursos Adicionais

### 🔧 Scripts de Setup
- `TrilhoSceneSetup.cs` - Setup automático completo
- `TrilhoQuickTest.cs` - Teste e debug
- `TrilhoConfigLoader.cs` - Carregador de configuração

### 📖 Documentação
- `TRILHO_JSON_SETUP_GUIDE.md` - Guia completo passo a passo
- `TRILHO_USAGE_GUIDE.md` - Guia de uso geral

### 🎯 Exemplos
- `trilho_config_example.json` - Configuração completa de exemplo
- Cena de demonstração com 7 zonas diferentes

---

## 🎉 Resultado

Com este setup, você terá uma cena Unity **100% configurável via JSON** que permite:

- ✅ **Modificar conteúdo** sem recompilar
- ✅ **Reposicionar zonas** dinamicamente  
- ✅ **Adicionar/remover elementos** facilmente
- ✅ **Configurar OSC** para controle externo
- ✅ **Personalizar transições** e efeitos
- ✅ **Deploy flexível** para diferentes usos

**🎯 Sua cena agora é um sistema vivo que evolui através de configuração!**

---

## 🆘 Suporte

### 🔍 Verificar Status
```
1. Selecione [TRILHO SETUP]
2. Clique com botão direito
3. "Verificar Status"
4. Ver Console para detalhes
```

### 🗑️ Limpar Setup
```
1. Menu: Trilho → Limpar Setup
2. Ou: Clique direito → "Limpar Setup"
3. Remove todos os objetos de setup
```

### 📞 Ajuda Adicional
- Consulte `TRILHO_JSON_SETUP_GUIDE.md` para detalhes
- Verifique Console para mensagens de debug
- Use TrilhoQuickTest para diagnóstico

---

*🎮 **Boa diversão criando sua cena configurável!** 🎮*
