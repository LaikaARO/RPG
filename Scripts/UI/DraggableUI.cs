using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Componente que permite arrastar elementos de UI.
/// Adicione este componente a qualquer GameObject com um componente RectTransform para torná-lo arrastável.
/// </summary>
public class DraggableUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Configurações de Arrasto")]
    [Tooltip("Se verdadeiro, o elemento retornará à posição original quando solto")]
    [SerializeField] private bool returnToOriginalPosition = false;
    
    [Tooltip("Se verdadeiro, o elemento ficará restrito aos limites do pai")]
    [SerializeField] private bool keepInParentBounds = true;
    
    [Tooltip("Velocidade de movimento ao arrastar (1 = movimento normal)")]
    [SerializeField] private float dragSpeed = 1.0f;
    
    // Referência ao RectTransform do elemento
    private RectTransform rectTransform;
    
    // Posição original antes de começar a arrastar
    private Vector2 originalPosition;
    
    // Referência ao RectTransform pai (para restrição de limites)
    private RectTransform parentRectTransform;
    
    // Offset do ponto de clique em relação ao pivot do elemento
    private Vector2 dragOffset;
    
    private void Awake()
    {
        // Obter referência ao RectTransform
        rectTransform = GetComponent<RectTransform>();
        
        // Verificar se o RectTransform existe
        if (rectTransform == null)
        {
            Debug.LogError($"O objeto {gameObject.name} não possui um RectTransform. Adicionando um RectTransform.");
            rectTransform = gameObject.AddComponent<RectTransform>();
        }
        
        // Obter referência ao RectTransform pai, se existir
        if (transform.parent != null)
        {
            parentRectTransform = transform.parent.GetComponent<RectTransform>();
        }
        
        // Salvar a posição original
        originalPosition = rectTransform.anchoredPosition;
    }
    
    /// <summary>
    /// Chamado quando o usuário começa a arrastar o elemento
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Salvar a posição original antes de começar a arrastar
        originalPosition = rectTransform.anchoredPosition;
        
        // Calcular o offset entre o ponto de clique e o pivot do elemento
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform, eventData.position, eventData.pressEventCamera, out dragOffset);
    }
    
    /// <summary>
    /// Chamado enquanto o usuário arrasta o elemento
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (rectTransform == null) return;
        
        // Converter a posição do mouse para posição local no pai
        Vector2 localPointerPosition;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRectTransform, eventData.position, eventData.pressEventCamera, out localPointerPosition))
        {
            // Calcular a nova posição considerando o offset e a velocidade de arrasto
            Vector2 newPosition = localPointerPosition - dragOffset * dragSpeed;
            
            // Aplicar a nova posição
            rectTransform.anchoredPosition = newPosition;
            
            // Restringir aos limites do pai, se necessário
            if (keepInParentBounds && parentRectTransform != null)
            {
                KeepInParentBounds();
            }
        }
    }
    
    /// <summary>
    /// Chamado quando o usuário termina de arrastar o elemento
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        // Se configurado para retornar à posição original, fazer isso
        if (returnToOriginalPosition)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
    
    /// <summary>
    /// Mantém o elemento dentro dos limites do pai
    /// </summary>
    private void KeepInParentBounds()
    {
        if (parentRectTransform == null) return;
        
        // Obter as dimensões do elemento e do pai
        Vector2 parentSize = parentRectTransform.rect.size;
        Vector2 elementSize = rectTransform.rect.size;
        
        // Calcular os limites considerando o pivot e a escala
        float pivotX = rectTransform.pivot.x;
        float pivotY = rectTransform.pivot.y;
        
        float minX = -parentSize.x * parentRectTransform.pivot.x + elementSize.x * pivotX;
        float maxX = parentSize.x * (1 - parentRectTransform.pivot.x) - elementSize.x * (1 - pivotX);
        float minY = -parentSize.y * parentRectTransform.pivot.y + elementSize.y * pivotY;
        float maxY = parentSize.y * (1 - parentRectTransform.pivot.y) - elementSize.y * (1 - pivotY);
        
        // Restringir a posição aos limites calculados
        Vector2 currentPosition = rectTransform.anchoredPosition;
        currentPosition.x = Mathf.Clamp(currentPosition.x, minX, maxX);
        currentPosition.y = Mathf.Clamp(currentPosition.y, minY, maxY);
        
        // Aplicar a posição restrita
        rectTransform.anchoredPosition = currentPosition;
    }
    
    /// <summary>
    /// Reseta a posição do elemento para a posição original
    /// </summary>
    public void ResetPosition()
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }
}