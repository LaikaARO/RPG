# Sistema de UI Arrastável

Este sistema permite que elementos de UI sejam facilmente arrastados pelo usuário. É ideal para janelas de inventário, equipamento, status e outras interfaces que o jogador pode querer reposicionar na tela.

## Componentes Principais

### DraggableUI

Este é o componente principal que torna um elemento de UI arrastável. Adicione-o a qualquer GameObject com um RectTransform para permitir que seja arrastado.

**Propriedades:**
- `Return To Original Position`: Se ativado, o elemento retornará à posição original quando solto.
- `Keep In Parent Bounds`: Se ativado, o elemento ficará restrito aos limites do pai.
- `Drag Speed`: Velocidade de movimento ao arrastar (1 = movimento normal).

### DraggableUIExtension

Classe de extensão que fornece métodos utilitários para trabalhar com elementos de UI arrastáveis:

```csharp
// Tornar um elemento arrastável
gameObject.MakeDraggable(returnToOriginalPosition, keepInParentBounds, dragSpeed);

// Verificar se um elemento é arrastável
bool isDraggable = gameObject.IsDraggable();

// Remover a funcionalidade de arrasto
gameObject.RemoveDraggable();
```

### DraggableUIExample

Exemplo de uso do componente DraggableUI. Adicione este script a um Canvas e configure os elementos que deseja tornar arrastáveis.

### DraggableUIManager

Gerenciador que aplica automaticamente o componente DraggableUI a elementos de UI específicos. Pode encontrar elementos por tag ou usar uma lista específica.

### GameUIManager

Gerenciador de UI do jogo que aplica a funcionalidade de arrasto a janelas específicas como inventário, equipamento, status e habilidades.

## Como Usar

### Método 1: Adicionar o Componente Diretamente

1. Selecione o elemento de UI que deseja tornar arrastável
2. No Inspector, clique em "Add Component"
3. Pesquise por "DraggableUI" e adicione o componente
4. Configure as propriedades conforme necessário

### Método 2: Usar o DraggableUIManager

1. Adicione o script `DraggableUIManager` ao Canvas principal
2. Configure a tag que será usada para identificar elementos arrastáveis
3. Adicione a tag aos elementos que deseja tornar arrastáveis
4. Ou especifique diretamente os elementos na lista "Specific Elements"

### Método 3: Usar o GameUIManager

1. Adicione o script `GameUIManager` ao Canvas principal
2. Atribua as referências para os painéis de UI (inventário, equipamento, etc.)
3. Configure as propriedades de arrasto conforme necessário

### Método 4: Via Código

```csharp
// Tornar um elemento arrastável
GameObject uiElement = GameObject.Find("InventoryPanel");
uiElement.MakeDraggable(false, true, 1.0f);

// Remover a funcionalidade de arrasto
uiElement.RemoveDraggable();

// Resetar a posição
DraggableUI draggable = uiElement.GetComponent<DraggableUI>();
if (draggable != null)
{
    draggable.ResetPosition();
}
```

## Dicas

- Para janelas de UI, é recomendável adicionar o componente DraggableUI ao painel principal, não a elementos individuais dentro da janela.
- Use `Keep In Parent Bounds` para evitar que janelas sejam arrastadas para fora da tela.
- Se quiser que apenas uma área específica da janela seja "arrastável" (como uma barra de título), você pode adicionar um manipulador de arrasto separado e implementar a lógica para mover o painel pai.

## Solução de Problemas

- Se um elemento não estiver arrastável, verifique se ele tem um componente RectTransform.
- Se o elemento estiver sendo arrastado de forma estranha, verifique se o pivot do RectTransform está configurado corretamente.
- Se o elemento não estiver respeitando os limites do pai, verifique se `Keep In Parent Bounds` está ativado e se o pai tem um RectTransform válido.