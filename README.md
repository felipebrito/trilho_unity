# Trilho (Unity)

Sistema simples para ativação de conteúdos por zonas em centímetros, sincronizado com a largura real da TV.

## Requisitos
- Unity 6.1 (URP) ou superior
- Pacote Input System habilitado (ou use o `TrilhoUniversalInputController` com Legacy)

## Como baixar
```bash
git clone https://github.com/felipebrito/trilho_unity.git
cd trilho_unity
```
Abra o projeto no Unity.

## Cena
Abra a cena: `Assets/Scenes/Trilho-v2.unity`

## Configuração rápida
1) TVBorderConfigurator (na GameObject `--SETUP--`)
- Ajuste as bordas (Q/W esquerda, O/P direita). Enter salva em PlayerPrefs.
- Opcional: marque “Usar largura manual (cm)” e defina, por exemplo, 75.69.
- Clique “Aplicar ao Trilho/Enviar Largura ao Trilho”.

2) TrilhoGameManager
- Responsável por posição do trilho, mapeamento cm↔Unity e movimento da câmera.
- Se quiser simular, marque “Simulação de Posição” e use as setas (ou os botões do inspector).

3) TrilhoZoneActivator (no mesmo `--SETUP--`)
- Adicione zonas. `positionCm` é o INÍCIO da zona; a faixa da zona = `positionCm` até `positionCm + largura da TV`.
- Marque `placeContentAtWorldX` para posicionar o conteúdo automaticamente no X correto; use `contentOffsetCm` se precisar ajuste fino.
- A ativação ocorre quando a janela da TV sobrepõe a faixa da zona; a saída usa histerese por direção (não apaga até a borda oposta passar).
- Ative `drawGameOverlay` para ver na Game View a janela (linhas ciano) e as duas linhas da zona.

## Teste rápido
- Dê Play.
- Use setas esquerda/direita para mover (se simulação estiver ativa).
- Quando a janela cobrir uma zona, o conteúdo liga com fade; desliga quando a borda oposta sai da faixa.

## Controles (teste)
- `TrilhoKeyboardController`: setas para mover; teclas 0–5 para saltos rápidos.
- `TrilhoUniversalInputController`: funciona com Input System ou Legacy (auto‑detecção).

## Dicas de debug
- Se a ativação parecer “adiantada/atrasada”, confirme a “Largura da TV (cm)” no `TrilhoGameManager`.
- Use o overlay do `TrilhoZoneActivator` para ver:
  - Janela (cm): início/fim
  - Duas linhas da zona: início e fim (início + largura)
- Ajuste `enterPaddingCm`/`exitPaddingCm` na zona para folga.

## Estrutura principal
- `TVBorderConfigurator`: calibração das bordas e envio da largura ao Trilho.
- `TrilhoGameManager`: mapeamento cm↔Unity, posição do trilho, câmera.
- `TrilhoZoneActivator`: ativação por zonas (faixa = início + largura da TV), overlays e fades.

## Problemas comuns
- “Nada ativa”: verifique se a largura foi aplicada ao Trilho e se `drawGameOverlay` está ativo.
- “Desliga cedo”: aumente `exitPaddingCm` da zona; confirme que a faixa da zona cobre o conteúdo desejado.
- “Teclado não responde”: ative “Simulação” no `TrilhoGameManager` ou use os scripts de exemplo.

